using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#if NET40
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

namespace DbAssertions.SqlServer
{
    public class SqlDatabase : Database
    {
        public SqlDatabase(string server, string databaseName, string userId, string password) : base(databaseName)
        {
            Server = server;
            UserId = userId;
            Password = password;
        }

        /// <summary>
        /// サーバー
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// パスワード
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// 接続文字列を取得する
        /// </summary>
        public override string ConnectionString => new SqlConnectionStringBuilder
        {
            DataSource = Server,
            UserID = UserId,
            Password = Password,
            InitialCatalog = DatabaseName
        }.ToString();

        public override IDbConnection OpenConnection()
        {
            SqlConnection connection = new(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// データベースのすべてのユーザーテーブルを取得する。
        /// </summary>
        /// <returns></returns>
        protected override List<Table> GetTables()
        {
            using var connection = OpenConnection();
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
                tables.Add(
                    new Table(
                        (string) reader["DatabaseName"],
                        (string) reader["SchemaName"],
                        (string) reader["TableName"]
                    ));
            }

            return tables;
        }

        /// <summary>
        /// テーブルの列を取得する。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected override List<Column> GetTableColumns(Table table)
        {
            using var connection = OpenConnection(); 
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
where
	schemas.name = @SchemaName
	and tables.name = @TableName
order by
	columns.column_id
";
            var schemaNameParameter = command.CreateParameter();
            schemaNameParameter.ParameterName = "SchemaName";
            schemaNameParameter.Value = table.SchemaName;
            command.Parameters.Add(schemaNameParameter);

            var tableNameParameter = command.CreateParameter();
            tableNameParameter.ParameterName = "TableName";
            tableNameParameter.Value = table.TableName;
            command.Parameters.Add(tableNameParameter);

            using var reader = command.ExecuteReader();

            List<Column> columns = new();
            while (reader.Read())
            {
                columns.Add(
                    new Column(
                        DatabaseName,
                        (string) reader["SchemaName"],
                        table.TableName,
                        (string) reader["ColumnName"],
                        (byte) reader["SystemTypeId"] switch
                        {
                            (byte) ColumnType.VarBinary => ColumnType.VarBinary,
                            (byte) ColumnType.DateTime => ColumnType.DateTime,
                            _ => ColumnType.Other
                        },
                        Convert.ToBoolean(reader["IsPrimaryKey"]),
                        (int) reader["PrimaryKeyOrdinal"]));
            }

            return columns;
        }
    }
}
