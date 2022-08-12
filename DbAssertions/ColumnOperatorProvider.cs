﻿using System;

namespace DbAssertions;

public class ColumnOperatorProvider : IColumnOperatorProvider
{
    public static readonly IColumnOperatorProvider Default = 
        new ColumnOperatorProvider(
            new HostNameColumnOperator(),
            new RandomColumnOperator(),
            new IgnoreColumnOperator());

    public ColumnOperatorProvider(IColumnOperator hostName, IColumnOperator random, IColumnOperator ignore)
    {
        HostName = hostName;
        Random = random;
        Ignore = ignore;
    }

    public IColumnOperator HostName { get; }
    public IColumnOperator Random { get; }
    public IColumnOperator Ignore { get; }

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


        columnOperator = new InvalidLabelColumnOperator();
        return false;
    }

    private class InvalidLabelColumnOperator : IColumnOperator
    {
        public string ToExpected(Column column, int rowNumber, string firstCell, string secondCell)
        {
            throw new InvalidOperationException();
        }

        public bool Compare(string expectedCell, string actualCell, DateTime timeBeforeStart)
        {
            throw new InvalidOperationException();
        }
    }
}