namespace DbAssertions
{
    /// <summary>
    /// カラム型
    /// </summary>
    public enum ColumnType : byte
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
        DateTime = 61
    }
}