namespace FlexNamer;

public static class CoreExtensions
{
	public static void Rename(this FileInfo file, string targetFileName) {
		file.MoveTo(Path.Combine(file.Directory?.FullName ?? Path.GetFileName(file.FullName), targetFileName.EndsWith(file.Extension) ? targetFileName : targetFileName + file.Extension));
	}

	public static string Format(this FileNameTarget result, string? separator = null) {
		separator ??= " - ";
		if (result.Site == null && result.Key == null) return result.Name;
		return $"{result.Site}{separator}{result.Key}{separator}{result.Name}";
	}

	public static string ToTitleCase(this string s) {
		return GetTextInfo().ToTitleCase(s);
	}

	public static System.Globalization.TextInfo GetTextInfo() => Thread.CurrentThread.CurrentCulture.TextInfo;
}