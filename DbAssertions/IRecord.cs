namespace DbAssertions
{
    public interface IRecord
    {
        object this[Column column] { get; }
    }
}