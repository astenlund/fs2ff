// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace fs2ff
{
    public class UpdateInformation
    {
        public UpdateInformation(Version version, Uri downloadLink)
        {
            Version = version;
            DownloadLink = downloadLink;
        }

        public Version Version { get; }

        public Uri DownloadLink { get; }
    }
}
