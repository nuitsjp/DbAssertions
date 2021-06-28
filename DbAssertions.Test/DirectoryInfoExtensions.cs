using System.IO;

namespace DbAssertions.Test
{
    /// <summary>
    /// DirectoryInfo拡張メソッドクラス
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// DirectoryInfoAssertionsを生成する
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static DirectoryInfoAssertions Should(this DirectoryInfo instance)
        {
            return new (instance);
        }
    }
}