namespace FlexNamer;

public interface INamingFormat {
	
	/// <summary>
	/// Represents the priority of a format, where the lowest priority wins. FlexNamer will use the first format it
	/// finds that matches a given file, starting at priority 1.
	/// </summary>
	/// <remarks>
	/// If your format matches quite broadly, consider setting the priority higher to ensure you don't inadvertently
	/// catch unintended file names.
	/// </remarks>
	int Priority => 100;
	
	/// <summary>
	/// Settings this to false indicates that your file name format requires user intervention during renaming. This
	/// would usually only be used if your format asks for user input.
	/// </summary>
	/// <remarks>
	/// If this is set to false, FlexNamer will skip your format entirely if the user specifies the "--non-interactive"
	/// CLI option.
	/// </remarks>
	bool SupportsNonInteractive => true;
	
	/// <summary>
	/// Checks if a given file name is supported by this naming format. Use this to have your format only called for
	/// file names that it can successfully handle.
	/// </summary>
	/// <remarks>
	/// Make this as narrow as possible, but it may also be called quite often, so it should complete quickly.
	/// </remarks>
	/// <param name="inputFileName">The file name being used as input for the current operation.</param>
	/// <returns>true if the current format can handle this file name, otherwise false.</returns>
	bool Matches(string inputFileName);
	
	/// <summary>
	/// Creates a target file name for the given input file. This method should not rename the actual file, and should 
	/// respect any provided options.
	/// </summary>
	/// <param name="targetFile">The current input file name.</param>
	/// <param name="options">User-provided options that can be used to fine-tune the target name.</param>
	/// <returns>A target file name to rename the current file to. If only the `Name` property is set, it will be used
	/// verbatim. Otherwise, the components of the name will be formatted according to the user's options. If your
	/// format cannot provide a valid name for the current file, return null and FlexNamer will skip your format.
	/// </returns>
	FileNameTarget? Rename(FileSystemInfo targetFile, RenameOptions options);
}