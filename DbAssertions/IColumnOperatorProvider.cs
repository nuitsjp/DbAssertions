namespace DbAssertions;

public interface IColumnOperatorProvider
{
    IColumnOperator HostName { get; }
    IColumnOperator Random { get; }
    IColumnOperator Ignore { get; }
    bool TryGetColumnOperator(string label, out IColumnOperator columnOperator);
}