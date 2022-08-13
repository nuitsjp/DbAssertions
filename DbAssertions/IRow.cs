namespace DbAssertions
{
    /// <summary>
    /// Row of table.
    /// </summary>
    public interface IRow
    {
        /// <summary>
        /// Get the cell in the specified column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        object this[Column column] { get; }
    }
}