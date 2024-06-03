#load "build/helpers.cake"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var isStable = EnvironmentVariable("GITHUB_REF")?.StartsWith("refs/tags/v") ?? false;

///////////////////////////////////////////////////////////////////////////////
// VERSIONING
///////////////////////////////////////////////////////////////////////////////

var packageVersion = string.Empty;
#load "build/version.cake"

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./src/FlexNamer.sln");
var solution = ParseSolution(solutionPath);
var projects = GetProjects(solutionPath, configuration);
var artifacts = "./dist/";
var testResultsPath = MakeAbsolute(Directory(artifacts + "./test-results"));
var plugins = new List<string> { "FlexNamer.Personal" };

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
	packageVersion = BuildVersion(fallbackVersion);
	if (FileExists("./build/.dotnet/dotnet.exe")) {
		Information("Using local install of `dotnet` SDK!");
		Context.Tools.RegisterFile("./build/.dotnet/dotnet.exe");
	}
	if (isStable) {
		Information("Publishing images as 'stable' (if publishing is enabled)!");
	}
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	// Clean solution directories.
	foreach(var path in projects.AllProjectPaths)
	{
		Information("Cleaning {0}", path);
		CleanDirectories(path + "/**/bin/" + configuration);
		CleanDirectories(path + "/**/obj/" + configuration);
	}
	foreach (var proj in projects.AllProjects) {
		Information(proj.Type);
	}
	Information("Cleaning common files...");
	CleanDirectory(artifacts);
});

Task("Restore")
	.Does(() =>
{
	// Restore all NuGet packages.
	Information("Restoring solution...");
	foreach (var project in projects.AllProjectPaths) {
		DotNetRestore(project.FullPath);
	}
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	var settings = new DotNetBuildSettings {
		Configuration = configuration,
		NoIncremental = true,
		ArgumentCustomization = args => args.Append($"/p:Version={packageVersion}").Append("/p:AssemblyVersion=1.0.0.0")
	};
	DotNetBuild(solutionPath, settings);
});

Task("Run-Unit-Tests")
	.IsDependentOn("Build")
	.Does(() =>
{
	CreateDirectory(testResultsPath);
	if (projects.TestProjects.Any()) {

		var settings = new DotNetTestSettings {
			Configuration = configuration
		};

		foreach(var project in projects.TestProjects) {
			DotNetTest(project.Path.FullPath, settings);
		}
	}
});

Task("Publish-Runtime")
	.IsDependentOn("Build")
	.Does(() =>
{
	var projectDir = $"{artifacts}publish";
	CreateDirectory(projectDir);

	foreach (var project in projects.SourceProjects)
	{
		var projPath = project.Path.FullPath;
		Information("Publishing {0}", project.Name);
		DotNetPublish(projPath, new DotNetPublishSettings {
			OutputDirectory = $"{projectDir}/{project.Name}/dotnet-any",
			Configuration = configuration,
			PublishSingleFile = false,
			SelfContained = false,
			PublishTrimmed = false,
			NoBuild = true,
			NoRestore = true,
			ArgumentCustomization = args => args
				.Append("/p:UseAppHost=false")
				.Append("/p:StandalonePublish=true")
				.Append($"/p:Version={packageVersion}")
				.Append("/p:AssemblyVersion=1.0.0.0")
				.Append("/p:NoWarn=\"IL2104 IL2072 IL2087\"")
		});
		var runtimes = new[] { "win-x64", "linux-x64", "linux-arm64"};
		// plugins don't get native builds
		if (!plugins.Contains(project.Name)) {
			foreach (var runtime in runtimes) {
				var runtimeDir = $"{projectDir}/{project.Name}/{runtime}";
				// CreateDirectory(runtimeDir);
				Information("Publishing {0} for {1} runtime", project.Name, runtime);
				var settings = new DotNetPublishSettings {
					Runtime = runtime,
					Configuration = configuration,
					OutputDirectory = runtimeDir,
					PublishSingleFile = true,
					SelfContained = true,
					ArgumentCustomization = args => args
						.Append($"/p:Version={packageVersion}")
						.Append("/p:AssemblyVersion=1.0.0.0")
						// .Append("/p:AssemblyName=fn")
				};
				DotNetPublish(projPath, settings);
				CreateDirectory($"{artifacts}archive");
				if (DirectoryExists(runtimeDir)) {
					Zip(runtimeDir, $"{artifacts}archive/{project.Name.Split('.').Last().ToLower()}-{runtime}.zip");
				}
			}
		}
	}
});

Task("Publish-Plugins")
	.IsDependentOn("Build")
	.WithCriteria(() => !HasEnvironmentVariable("GITHUB_REF"))
	.Does(() =>
{
	var pluginsDir = $"{artifacts}plugins";
	CreateDirectory(pluginsDir);
	foreach(var project in projects.SourceProjects.Where(p => plugins.Contains(p.Name))) {
		DotNetPublish(project.Path.FullPath, new DotNetPublishSettings {
			OutputDirectory = pluginsDir + "/" + project.Name,
			Configuration = configuration,
			PublishSingleFile = false,
			SelfContained = false,
			PublishTrimmed = false,
			NoBuild = true,
			NoRestore = true,
		});
	}
	CreateDirectory($"{artifacts}archive");
	Zip(pluginsDir, $"{artifacts}archive/flex-namer-plugins.zip");
});

Task("Publish-Personal")
    .IsDependentOn("Publish")
	.IsDependentOn("Publish-Plugins")
	.WithCriteria(() => !HasEnvironmentVariable("GITHUB_REF"))
	.WithCriteria(() => IsRunningOnWindows())
	.Does(() =>
{
	var personalDir = $"{artifacts}personal";
	var pluginsDir = $"{personalDir}/Plugins";
	CreateDirectory(personalDir);
	CreateDirectory(pluginsDir);
	CopyDirectory($"{artifacts}plugins", pluginsDir);
	CopyFiles($"{artifacts}publish/FlexNamer/win-x64/*.exe", personalDir);
// 	foreach(var project in projects.SourceProjects.Where(p => plugins.Contains(p.Name))) {
// 		DotNetPublish(project.Path.FullPath, new DotNetPublishSettings {
// 			OutputDirectory = pluginsDir + "/" + project.Name,
// 			Configuration = configuration,
// 			PublishSingleFile = false,
// 			SelfContained = false,
// 			PublishTrimmed = false,
// 			NoBuild = true,
// 			NoRestore = true,
// 		});
// 	}
});

Task("NuGet")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Building NuGet package");
    CreateDirectory(artifacts + "package/");
    var packSettings = new DotNetPackSettings {
        Configuration = configuration,
        NoBuild = false,
        OutputDirectory = $"{artifacts}package",
        ArgumentCustomization = args => args
            .Append($"/p:Version=\"{packageVersion}\"")
            .Append("/p:NoWarn=\"NU1701 NU1602\"")
    };
    /*foreach(var project in projects.SourceProjectPaths) {
        Information($"Packing {project.GetDirectoryName()}...");
        DotNetPack(project.FullPath, packSettings);
    }*/
    DotNetPack(solutionPath, packSettings);
});

#load "build/publish.cake"

Task("Default")
	.IsDependentOn("Build");

Task("Publish")
	.IsDependentOn("Publish-Runtime")
	.IsDependentOn("Publish-Plugins")
	.IsDependentOn("NuGet");

Task("Release")
	.IsDependentOn("Publish")
	.IsDependentOn("Publish-NuGet-Package");

RunTarget(target);