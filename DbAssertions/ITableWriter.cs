using System;

namespace DbAssertions
{
    internal interface ITableWriter : IDisposable
    {
        void Write(IRow row);
    }
}