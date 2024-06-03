namespace FlexNamer;

public interface INamingFormat {
	int Priority => 100;
	bool SupportsNonInteractive => true;
	bool Matches(string inputFileName);
	FileNameTarget? Rename(FileSystemInfo targetFile, RenameOptions options);
}