using System.IO;

namespace DbAssertions
{
    internal interface ITableReader
    {
        IRow[] ReadAllRows(FileInfo fileInfo);
    }
}