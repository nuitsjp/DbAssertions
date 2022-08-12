namespace DbAssertions;

public interface IColumnOperatorProvider
{
    bool TryGetColumnOperator(string label, out IColumnOperator columnOperator);
}