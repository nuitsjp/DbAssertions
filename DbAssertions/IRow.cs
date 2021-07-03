namespace DbAssertions
{
    public interface IRow
    {
        object this[Column column] { get; }
    }
}