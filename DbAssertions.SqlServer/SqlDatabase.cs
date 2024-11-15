﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
#if NETSTANDARD2_0
using System.Runtime.Loader;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Types;
#else
using System.Data.SqlClient;
#endif

namespace DbAssertions.SqlServer
{
    /// <summary>
    /// Database of SQL Server.
    /// </summary>
    public class SqlDatabase : Database
    {
        /// <summary>
        /// Provider of IColumnOperator.
        /// </summary>
        private readonly IColumnOperatorProvider _columnOperatorProvider;

        static SqlDatabase()
        {
#if NETSTANDARD2_0
            AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;
#endif

        }

#if NETSTANDARD2_0
        static Assembly OnAssemblyResolve(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        {
            try
            {
                AssemblyLoadContext.Default.Resolving -= OnAssemblyResolve;
                return assemblyLoadContext.LoadFromAssemblyName(assemblyName);
            }
            catch
            {
                if (assemblyName.Name == "Microsoft.SqlServer.Types")
                    return typeof(SqlGeography).Assembly;
                throw;
            }
            finally
            {
                AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;
            }
        }
#endif

        /// <summary>
        /// Create instance.
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlDatabase(string connectionString) : 
            this(
                connectionString,
                new ColumnOperatorProvider(
                    new HostNameColumnOperator(),
                    new RandomColumnOperator(),
                    new IgnoreColumnOperator()))
        {
        }

        /// <summary>
        /// Create instance.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="columnOperatorProvider"></param>
        public SqlDatabase(string connectionString, IColumnOperatorProvider columnOperatorProvider)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            Server = builder.DataSource;
            UserId = builder.UserID;
            Password = builder.Password;
            DatabaseName = builder.InitialCatalog;
            ConnectionString = connectionString;
            _columnOperatorProvider = columnOperatorProvider;
        }

        /// <summary>
        /// Get server.
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// Get user ID.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Get password.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Get database name.
        /// </summary>
        public override string DatabaseName { get; }

        /// <summary>
        /// Get connection string.
        /// </summary>
        public override string ConnectionString { get; }

        /// <summary>
        /// Open connection of database.
        /// </summary>
        /// <returns></returns>
        public override IDbConnection OpenConnection()
        {
            SqlConnection connection = new(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Get all user tables in the database.
        /// </summary>
        /// <returns></returns>
        protected override List<Table> GetTables(IDbAssertionsConfig config)
        {
            using var connection = OpenConnection();

            var columns = GetTableColumns(connection, config);

            using var command = connection.CreateCommand();
            command.CommandText = @$"
use {DatabaseName};

select 
	'{DatabaseName}' as [DatabaseName],
	schema_name(schema_id) as [SchemaName],
	name as [TableName]
from 
	sys.objects 
where 
	type = 'U'
order by 
	[DatabaseName],
	[SchemaName],
	[TableName];";
            using var reader = command.ExecuteReader();
            List<Table> tables = new();
            while (reader.Read())
            {
                var schemaName = (string) reader["SchemaName"];
                var tableName = (string) reader["TableName"];
                tables.Add(
                    new Table(
                        schemaName,
                        tableName,
                        columns
                            .Where(x => x.SchemaName == schemaName && x.TableName == tableName)
                            .ToList()
                    ));
            }

            return tables;
        }

        /// <summary>
        /// Get table columns.
        /// </summary>
        /// <returns></returns>
        private List<Column> GetTableColumns(IDbConnection connection, IDbAssertionsConfig config)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @$"
use {DatabaseName};

select
	schemas.name as SchemaName,
	tables.name as TableName,
	columns.name as ColumnName,
	system_type_id as SystemTypeId,
	case
		when index_columns.OBJECT_ID is not null then convert(bit, 1)
		else convert(bit, 0)
	end as IsPrimaryKey,
	case
		when index_columns.key_ordinal is not null then index_columns.key_ordinal
		else 0
	end as PrimaryKeyOrdinal
from
	sys.columns
	inner join sys.tables
		on	columns.object_id = tables.object_id
	inner join sys.schemas
		on	tables.schema_id = schemas.schema_id
	left outer join sys.key_constraints as pk_constraints
		on tables.object_id = pk_constraints.parent_object_id AND pk_constraints.type = 'PK'
	left outer join sys.index_columns
		on pk_constraints.parent_object_id = index_columns.object_id
		and pk_constraints.unique_index_id  = index_columns.index_id
		and index_columns.object_id = columns.object_id
		and index_columns.column_id = columns.column_id
order by
	columns.column_id
";
            using var reader = command.ExecuteReader();

            List<Column> columns = new();
            while (reader.Read())
            {
                var schemaName = (string)reader["SchemaName"];
                var tableName = (string)reader["TableName"];
                var columnName = (string)reader["ColumnName"];
                var columnType = (byte) reader["SystemTypeId"] switch
                {
                    (byte) SqlColumnType.VarBinary => ColumnType.VarBinary,
                    (byte) SqlColumnType.DateTime => ColumnType.DateTime,
                    (byte) SqlColumnType.DateTime2 => ColumnType.DateTime,
                    _ => ColumnType.Other
                };
                var columnOperator = config.GetColumnOperator(DatabaseName, schemaName, tableName, columnName, columnType);
                columns.Add(
                    new Column(
                        DatabaseName,
                        schemaName,
                        tableName,
                        columnName,
                        columnType,
                        Convert.ToBoolean(reader["IsPrimaryKey"]),
                        (int)reader["PrimaryKeyOrdinal"],
                        columnOperator,
                        _columnOperatorProvider));
            }

            return columns;
        }
    }
}
