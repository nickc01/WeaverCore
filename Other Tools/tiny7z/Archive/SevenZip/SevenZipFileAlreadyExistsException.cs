namespace pdj.tiny7z.Archive
{
    public class SevenZipFileAlreadyExistsException : SevenZipException
    {
        internal SevenZipFileAlreadyExistsException(SevenZipArchiveFile file)
            : base($"File `{file.Name}` already exists.")
        {
        }
    }
}
