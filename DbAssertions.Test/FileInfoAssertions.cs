using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace DbAssertions.Test
{
    /// <summary>
    /// ファイルに対するアサーションクラス
    /// </summary>
    public class FileInfoAssertions :
        ReferenceTypeAssertions<FileInfo, FileInfoAssertions>
    {
        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="instance"></param>
        public FileInfoAssertions(FileInfo instance) : base(instance)
        {
        }

        protected override string Identifier => "file";

        /// <summary>
        /// ファイルが期待値と一致するかどうか判定する
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<FileInfoAssertions> Be(
            FileInfo expected,
            string because = "",
            params object[] becauseArgs)
        {
            try
            {
                if (Equals(Subject.ReadAllText(), expected.ReadAllText()))
                {
                    return new AndConstraint<FileInfoAssertions>(this);
                }

                throw new AssertionFailedException(
                    string.Format(
                        string.IsNullOrEmpty(because)
                            ? "{0} と {1} のファイル内容が不一致です。"
                            : because,
                        Subject.FullName,
                        expected.FullName));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}