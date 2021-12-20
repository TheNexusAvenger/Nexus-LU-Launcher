using System;
using System.Collections.Generic;
using System.Linq;

namespace NLUL.Core.Client.Archive
{
    public static class ArchiveResolver
    {
        /// <summary>
        /// Types to try for reading archives.
        /// </summary>
        private static readonly List<Func<string, ClientArchive>> ArchiveTypes = new List<Func<string, ClientArchive>>()
        {
            (archiveLocation) => new RarFileArchive(archiveLocation),
            (archiveLocation) => new ZipFileArchive(archiveLocation),
        };

        /// <summary>
        /// Returns the archive for the given file. Returns null if it can't be read or doesn't contain a client.
        /// </summary>
        /// <param name="archiveLocation">Location of the archive.</param>
        /// <returns>The archive for the given file.</returns>
        public static ClientArchive GetArchive(string archiveLocation)
        {
            return ArchiveTypes.Select(archiveCreator => archiveCreator(archiveLocation)).FirstOrDefault(archive => archive.ContainsClient());
        }
    }
}