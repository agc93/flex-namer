using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace FlexNamer;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class RenameCommand : Command<RenameCommand.Settings> {
	private readonly List<INamingFormat> _formats;

	public RenameCommand(IEnumerable<INamingFormat> formats) {
		_formats = formats.OrderBy(f => f.Priority).ToList();
	}
	private static List<string> ExcludedWords => ["sample"];

	public override int Execute(CommandContext context, Settings settings) {
		var di = new DirectoryInfo(settings.Directory);
		if (!di.Exists) {
			Console.WriteLine("The given directory does not exist!");
			return 404;
		}

		if (settings.NonInteractive && _formats.Any(f => !f.SupportsNonInteractive)) {
			AnsiConsole.WriteLine(
				"[WARNING]: Some naming formats require interaction, but you are running in non-interactive mode! These formats will be skipped!");
		}
		
		var files = di
			.GetFiles(settings.Pattern ?? "*")
			.Where(f => settings.IncludeAll || !ExcludedWords.Any(e => f.Name.Contains(e)))
			.ToList();
		switch (files.Count) {
			case 0:
				Console.WriteLine("No matching files found, please widen filters!");
				return 404;
			case > 1 when !settings.BatchMode:
				Console.WriteLine("Multiple matching files found, please narrow filters");
				return 412;
		}

		
		foreach (var matchedFile in files) {
			var file = new FileInfo(matchedFile.FullName);
			var matchName = settings.UseDirectoryName
				? file.Directory!.Name
				: file.Name.Replace(file.Extension, string.Empty);
			FileSystemInfo matchCandidate = settings.UseDirectoryName
				? file.Directory!
				: file;
			FileNameTarget? target = null;
			for (var i = 0; i < _formats.Count && target == null; i++) {
				var fmt = _formats[i];
				var matches = (settings.NonInteractive == false || fmt.SupportsNonInteractive) && fmt.Matches(matchName);
				if (matches) {
					target = fmt.Rename(matchCandidate, new RenameOptions(!settings.UseISODateFormat));
				}
			}
			if (target == null) {
				AnsiConsole.WriteLine("Could not match a file pattern for renaming!");
			} else {
				var finalPath = target.Format(settings.Separator);
				if (settings.UseDirectoryName && !finalPath.EndsWith(file.Extension)) {
					finalPath = $"{finalPath}{file.Extension}";
				}
				AnsiConsole.WriteLine("Renaming '{0}' to '{1}'", matchName, finalPath);
				if (settings.DryRun) {
					AnsiConsole.WriteLine("Dry run enabled! Skipping rename of '{0}' to '{1}'", matchName, finalPath);
				} else {
					file.Rename(finalPath);
				}
			}
		}

		return 200;
	}

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
	public sealed class Settings : CommandSettings {
		[CommandArgument(0, "[DIR]")] public string Directory { get; set; } = Environment.CurrentDirectory;

		[CommandOption("-f|--filter")] public string? Pattern { get; set; } = "*.mp4";

		[CommandOption("-a|--all")] public bool IncludeAll { get; set; } = false;

		[CommandOption("-d|--directory")] public bool UseDirectoryName { get; set; } = false;

		[CommandOption("-b|--batch")] public bool BatchMode { get; set; } = false;

		[CommandOption("-z|--iso")] public bool UseISODateFormat { get; set; } = false;

		[CommandOption("--dry-run")] public bool DryRun { get; set; } = false;
		
		[CommandOption("--non-interactive")] public bool NonInteractive { get; set; } = false;

		[CommandOption("-s|--separator")] public string Separator { get; set; } = " - ";

		public override ValidationResult Validate() {
			if (File.Exists(Directory) && !System.IO.Directory.Exists(Directory)) {
				Pattern = Path.GetFileName(Directory);
				Directory = Path.GetDirectoryName(Directory) ??
				            throw new InvalidOperationException("Invalid directory provided!");
			}

			return base.Validate();
		}
	}
}