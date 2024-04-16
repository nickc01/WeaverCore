namespace pdj.tiny7z.Archive
{
    public class SevenZipPasswordRequiredException : SevenZipException
    {
        internal SevenZipPasswordRequiredException()
            : base("No password provided. Encrypted stream requires password.")
        {
        }
    }
}
