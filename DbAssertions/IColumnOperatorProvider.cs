namespace DbAssertions;

/// <summary>
/// IColumnOperator Provider
/// </summary>
public interface IColumnOperatorProvider
{
    /// <summary>
    /// IColumnOperator for hostname columns.
    /// </summary>
    IColumnOperator HostName { get; }

    /// <summary>
    /// IColumnOperator for random columns.
    /// </summary>
    IColumnOperator Random { get; }

    /// <summary>
    /// IColumnOperator for Ignore columns.
    /// </summary>
    IColumnOperator Ignore { get; }

    /// <summary>
    /// Get the IColumnOperator.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="columnOperator"></param>
    /// <returns></returns>
    bool TryGetColumnOperator(string label, out IColumnOperator columnOperator);
}