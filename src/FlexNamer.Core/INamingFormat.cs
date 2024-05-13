namespace FlexNamer;

public interface INamingFormat {
	int Priority => 100;
	bool Matches(string inputFileName);
	FileNameTarget? Rename(FileSystemInfo targetFile, RenameOptions options);
}