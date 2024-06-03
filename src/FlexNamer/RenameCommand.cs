using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
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
		[CommandArgument(0, "[DIR]")]
		[Description("The folder to search for file(s) to rename.")]
		public string Directory { get; set; } = Environment.CurrentDirectory;

		[CommandOption("-f|--filter")]
		[Description("Wildcard pattern to match file(s) to, such as \"*.txt\" or \"Export*.*\".")]
		public string? Pattern { get; set; }

		// [CommandOption("-a|--all")]
		[Description("Includes files, even when they contain excluded words.")]
		internal bool IncludeAll { get; set; } = false;

		[CommandOption("-d|--directory")]
		[Description("Uses the current directory name instead of the input file name as the basis for renaming.")]
		public bool UseDirectoryName { get; set; } = false;

		[CommandOption("-b|--batch")]
		[Description("Enables renaming of multiple files at once. Otherwise, renaming will only proceed if exactly one file is matched.")]
		public bool BatchMode { get; set; } = false;

		[CommandOption("-z|--iso")]
		[Description("Instructs naming formats to prefer ISO date format to \"friendlier\" date formats.")]
		public bool UseISODateFormat { get; set; } = false;

		[CommandOption("--dry-run")]
		[Description("Does not perform any actual renaming, instead only simulating the rename operation.")]
		public bool DryRun { get; set; } = false;
		
		[CommandOption("--non-interactive")]
		[Description("Forces the command to proceed without any user input. This will bypass any prompts and skip formats that require user interaction")]
		public bool NonInteractive { get; set; } = false;

		[CommandOption("-s|--separator")]
		[Description("The separator to use between the components of a target name (where applicable).")]
		public string Separator { get; set; } = " - ";

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