using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace DbAssertions
{
    /// <summary>
    /// DatabasesクラスのAssertionクラス
    /// </summary>
    public class DatabaseAssertions :
        ReferenceTypeAssertions<Database, DatabaseAssertions>
    {
        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="instance"></param>
        public DatabaseAssertions(Database instance)
        {
            Subject = instance;
        }

        protected override string Context => "file";

        /// <summary>
        /// データベースが期待結果ファイルと一致するかどうか評価する
        /// </summary>
        /// <param name="expectedFileInfo"></param>
        /// <param name="directoryInfo"></param>
        /// <param name="compareResult"></param>
        /// <param name="setupCompletionTime"></param>
        /// <param name="lifeCycleColumns"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<DatabaseAssertions> BeAsExpected(
            FileInfo expectedFileInfo,
            DirectoryInfo directoryInfo,
            CompareResult compareResult,
            DateTime setupCompletionTime,
            IEnumerable<LifeCycleColumn>? lifeCycleColumns = null, 
            string because = "", 
            params object[] becauseArgs)
        {
            Subject.Compare(
                expectedFileInfo,
                directoryInfo, 
                compareResult, 
                setupCompletionTime, 
                new HashSet<LifeCycleColumn>(lifeCycleColumns ?? Enumerable.Empty<LifeCycleColumn>()),
                because, 
                becauseArgs);

            return new(this);
        }
    }
}