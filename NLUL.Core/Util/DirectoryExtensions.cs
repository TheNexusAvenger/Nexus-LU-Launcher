using System.IO;

namespace NLUL.Core.Util
{
    public static class DirectoryExtensions
    {
        /// <summary>
        /// Copies a directory.
        /// Modified from: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        /// <param name="sourceDirectoryName">Source directory to copy from.</param>
        /// <param name="destinationDirectoryName">Destination directory to copy to.</param>
        public static void Copy(string sourceDirectoryName, string destinationDirectoryName)
        {
            // Get the subdirectories for the specified directory.
            var directory = new DirectoryInfo(sourceDirectoryName);
            var directories = directory.GetDirectories();
        
            // Create the destination if it doesn't exist.
            Directory.CreateDirectory(destinationDirectoryName);

            // Copy the files to the new location.
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                file.CopyTo(Path.Combine(destinationDirectoryName, file.Name), false);
            }

            // Copy the subdirectories.
            foreach (var subDirectory in directories)
            {
                Copy(subDirectory.FullName, Path.Combine(destinationDirectoryName, subDirectory.Name));
            }
        }

        /// <summary>
        /// Moves a directory.
        /// </summary>
        /// <param name="sourceDirectoryName">Source directory to move from.</param>
        /// <param name="destinationDirectoryName">Destination directory to move to.</param>
        public static void Move(string sourceDirectoryName, string destinationDirectoryName)
        {
            // First attempt to move the directory, then copy try to copy the directory.
            // Directory.Move does not work between volumes.
            try
            {
                Directory.Move(sourceDirectoryName, destinationDirectoryName);
            }
            catch (IOException)
            {
                Copy(sourceDirectoryName, destinationDirectoryName);
                Directory.Delete(sourceDirectoryName, true);
            }
        }
    }
}