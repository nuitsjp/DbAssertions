namespace DbAssertions.SqlServer
{
    /// <summary>
    /// Column type.
    /// </summary>
    public enum SqlColumnType : byte
    {
        /// <summary>
        /// varbinary.
        /// </summary>
        VarBinary = 165,

        /// <summary>
        /// datetime.
        /// </summary>
        DateTime = 61,

        /// <summary>
        /// datetime2
        /// </summary>
        DateTime2 = 42
    }
}