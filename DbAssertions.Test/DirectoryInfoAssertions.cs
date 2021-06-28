using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace DbAssertions.Test
{
    /// <summary>
    /// ディレクトリとディレクトリ内のファイルが一致するか再帰的に評価するためのアサーションクラス
    /// </summary>
    public class DirectoryInfoAssertions :
        ReferenceTypeAssertions<DirectoryInfo, DirectoryInfoAssertions>
    {
        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="instance"></param>
        public DirectoryInfoAssertions(DirectoryInfo instance)
        {
            Subject = instance;
        }

        protected override string Context => "directory";

        /// <summary>
        /// 比較対象のディレクトリが同じコンテンツを保持しているかどうか評価する
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="ignoreNumbersInFileName"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<DirectoryInfoAssertions> HaveSameContents(
            DirectoryInfo expected,
            bool ignoreNumbersInFileName = false,
            string because = "",
            params object[] becauseArgs)
        {
            HasSameFiles(Subject, expected, ignoreNumbersInFileName, because, becauseArgs);
            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        /// <summary>
        /// 比較対象のディレクトリが同じコンテンツを保持しているかどうか再帰的に評価する
        /// </summary>
        /// <param name="directoryA"></param>
        /// <param name="directoryB"></param>
        /// <param name="ignoreNumbersInFileName"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        private static void HasSameFiles(
            DirectoryInfo directoryA,
            DirectoryInfo directoryB,
            bool ignoreNumbersInFileName,
            string because = "",
            params object[] becauseArgs)
        {
            // 同一名ファイルの存在チェック
            var directoryAFiles = directoryA.GetFiles().OrderBy(x => x.Name).ToArray();
            var directoryBFiles = directoryB.GetFiles().OrderBy(x => x.Name).ToArray();
            directoryAFiles.Select(x => x.Name)
                .Should().Equal(directoryBFiles.Select(x => x.Name), because, becauseArgs);

            // 同一名フォルダの存在チェック
            directoryA.GetDirectories().Select(x => x.Name)
                .Should().BeEquivalentTo(directoryB.GetDirectories().Select(x => x.Name), because, becauseArgs);

            // ファイルの内容チェック
            for (int i = 0; i < directoryAFiles.Length; i++)
            {
                var fileA = directoryAFiles[i];
                var fileB = directoryBFiles[i];
                fileA.Should().Be(fileB);
            }

            // サブフォルダの再帰的評価
            directoryA.GetDirectories().ToList()
                .ForEach(subA => HasSameFiles(subA, directoryB.GetDirectory(subA.Name), ignoreNumbersInFileName));
        }
    }
}