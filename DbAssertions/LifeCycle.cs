namespace DbAssertions
{
    /// <summary>
    /// カラムのライフサイクルタイプ
    /// </summary>
    public enum LifeCycle
    {
        /// <summary>
        /// 実行時ではなく、日次で更新される値は無条件に一致とする。
        /// これを増やしすぎるとテストにならないので要注意。
        /// </summary>
        Daily,
        /// <summary>
        /// 実行の都度に更新される値。
        /// ExpectedDatabaseファイルにColumn.TimeAfterStartの値が記載されていた場合、actualの
        /// 値がデータベースを設定した時刻以降であれば一致として判断する。
        /// </summary>
        Runtime
    }
}