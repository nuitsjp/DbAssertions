namespace DbAssertions.SqlServer
{
    /// <summary>
    /// カラム型
    /// </summary>
    public enum SqlColumnType : byte
    {
        /// <summary>
        /// その他
        /// </summary>
        Other,

        /// <summary>
        /// バイナリー
        /// </summary>
        VarBinary = 165,

        /// <summary>
        /// 日時
        /// </summary>
        DateTime = 61,

        /// <summary>
        /// datetime2
        /// </summary>
        DateTime2 = 42
    }
}