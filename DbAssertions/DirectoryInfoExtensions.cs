using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// DirectoryInfo extension method class.
    /// </summary>
    internal static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Get a sub directory.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        internal static DirectoryInfo GetDirectory(this DirectoryInfo directoryInfo, string relativePath)
            => new(Path.Combine(directoryInfo.FullName, relativePath));

        /// <summary>
        /// Get file on relative paths from DirectoryInfo.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        internal static FileInfo GetFile(this DirectoryInfo directoryInfo, string relativePath)
            => new(Path.Combine(directoryInfo.FullName, relativePath));

        /// <summary>
        /// Re-create the directory.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        internal static DirectoryInfo ReCreate(this DirectoryInfo directoryInfo)
        {
            directoryInfo.ForceDelete(true).Create();
            return directoryInfo;
        }

        /// <summary>
        /// If called immediately after deleting or creating the target directory, the existence check does not work correctly in the file cache.
        /// Therefore, it is forcibly deleted without question.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="recurse"></param>
        /// <returns></returns>
        internal static DirectoryInfo ForceDelete(this DirectoryInfo directoryInfo, bool recurse = false)
        {
            try
            {
                directoryInfo.Delete(true);
            }
            catch (DirectoryNotFoundException)
            {
                // ignore
            }

            return directoryInfo;
        }

        /// <summary>
        /// Returns true if the directory does not exist.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        internal static bool NotExist(this DirectoryInfo directoryInfo) => !directoryInfo.Exists;
    }
}