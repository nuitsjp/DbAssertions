using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// DirectoryInfo拡張メソッドクラス
    /// </summary>
    internal static class DirectoryInfoExtensions
    {
        internal static DirectoryInfo GetDirectory(this DirectoryInfo directoryInfo, string relativePath)
            => new(Path.Combine(directoryInfo.FullName, relativePath));

        internal static FileInfo GetFile(this DirectoryInfo directoryInfo, string relativePath)
            => new(Path.Combine(directoryInfo.FullName, relativePath));

        internal static DirectoryInfo ReCreate(this DirectoryInfo directoryInfo)
        {
            directoryInfo.ForceDelete(true).Create();
            return directoryInfo;
        }

        internal static DirectoryInfo ForceDelete(this DirectoryInfo directoryInfo, bool recurse = false)
        {
            try
            {
                // 対象ディレクトリを削除や生成した直後に呼ぶと、存在チェックがファイルキャッシュで正しく動作しない
                // そのため問答無用で強制的に削除する
                directoryInfo.Delete(true);
            }
            catch (DirectoryNotFoundException)
            {
                // 例外を握りつぶす
            }

            return directoryInfo;
        }

        internal static bool NotExist(this DirectoryInfo directoryInfo) => !directoryInfo.Exists;
    }
}