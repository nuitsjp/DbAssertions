using System.IO;

namespace DbAssertions
{
    /// <summary>
    /// DirectoryInfo拡張メソッドクラス
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        public static DirectoryInfo GetDirectory(this DirectoryInfo directoryInfo, string relativePath)
            => new(Path.Combine(directoryInfo.FullName, relativePath));

        public static FileInfo GetFile(this DirectoryInfo directoryInfo, string relativePath)
            => new(Path.Combine(directoryInfo.FullName, relativePath));

        public static DirectoryInfo ReCreate(this DirectoryInfo directoryInfo)
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
            directoryInfo.Create();
            return directoryInfo;
        }

        public static bool NotExist(this DirectoryInfo directoryInfo) => !directoryInfo.Exists;
    }
}