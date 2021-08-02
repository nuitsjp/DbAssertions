using System;

namespace DbAssertions
{
    public interface ISpecificColumnType
    {
        string ToExpected(Column column, int rowNumber, string firstCell, string secondCell);

        bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart);
    }
}