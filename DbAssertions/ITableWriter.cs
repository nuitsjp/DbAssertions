using System;
using System.IO;

namespace DbAssertions
{
    internal interface ITableWriter : IDisposable
    {
        FileInfo FileInfo { get; }
        void Write(IRow row);
    }
}