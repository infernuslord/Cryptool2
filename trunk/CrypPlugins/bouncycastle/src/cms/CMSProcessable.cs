using System;
using System.IO;

namespace Org.BouncyCastle.Cms
{
    public interface CmsProcessable
    {
        /**
        * generic routine to copy out the data we want processed - the Stream
        * passed in will do the handling on it's own.
        * <p>
        * Note: this routine may be called multiple times.</p>
        */
        void Write(Stream outStream);

        object GetContent();
    }
}
