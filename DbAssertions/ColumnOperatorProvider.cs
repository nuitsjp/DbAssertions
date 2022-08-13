using System;

namespace DbAssertions;

/// <summary>
/// Provide IColumnOperator.
/// </summary>
public class ColumnOperatorProvider : IColumnOperatorProvider
{
    /// <summary>
    /// Default provider.
    /// </summary>
    public static readonly IColumnOperatorProvider Default = 
        new ColumnOperatorProvider(
            new HostNameColumnOperator(),
            new RandomColumnOperator(),
            new IgnoreColumnOperator());

    /// <summary>
    /// Create instance.
    /// </summary>
    /// <param name="hostName"></param>
    /// <param name="random"></param>
    /// <param name="ignore"></param>
    public ColumnOperatorProvider(IColumnOperator hostName, IColumnOperator random, IColumnOperator ignore)
    {
        HostName = hostName;
        Random = random;
        Ignore = ignore;
    }

    /// <summary>
    /// Get host name provider.
    /// </summary>
    public IColumnOperator HostName { get; }

    /// <summary>
    /// Get random provider.
    /// </summary>
    public IColumnOperator Random { get; }

    /// <summary>
    /// Get ignore provider.
    /// </summary>
    public IColumnOperator Ignore { get; }

    /// <summary>
    /// Get IColumnOperator.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="columnOperator"></param>
    /// <returns></returns>
    public bool TryGetColumnOperator(string label, out IColumnOperator columnOperator)
    {
        if (Equals(label, HostNameColumnOperator.DefaultLabel))
        {
            columnOperator = HostName;
            return true;
        }
        if (Equals(label, RandomColumnOperator.DefaultLabel))
        {
            columnOperator = Random;
            return true;
        }
        if (Equals(label, IgnoreColumnOperator.DefaultLabel))
        {
            columnOperator = Ignore;
            return true;
        }


        columnOperator = default!;
        return false;
    }
}