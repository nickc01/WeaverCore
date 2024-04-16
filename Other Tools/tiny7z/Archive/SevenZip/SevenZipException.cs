using pdj.tiny7z.Common;
using System;

namespace pdj.tiny7z.Archive
{
    public class SevenZipException : Exception
    {
        internal SevenZipException(string message)
            : base(message)
        {
        }
    }
}
