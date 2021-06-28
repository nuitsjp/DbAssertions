using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
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
        protected override IList<Table> GetTables()
        {
            using var connection = OpenConnection();

            var query = @$"
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
            return connection.Query<Table>(query).ToList();
        }

        /// <summary>
        /// テーブルの列を取得する。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected override IList<Column> GetTableColumns(Table table)
        {
            using var connection = OpenConnection();

            var query = @"
select
	schemas.name as SchemaName,
	columns.name as ColumnName,
	system_type_id as SystemTypeId
from
	sys.columns
	inner join sys.tables
		on	columns.object_id = tables.object_id
	inner join sys.schemas
		on	tables.schema_id = schemas.schema_id
where
	tables.name = @TableName
order by
	column_id
";

            return connection
                .Query(
                    query,
                    new { TableName = table.TableName })
                .Select(x =>
                {
                    return new Column(
                        DatabaseName,
                        x.SchemaName,
                        table.TableName,
                        (string)x.ColumnName,
                        (byte)x.SystemTypeId switch
                        {
                            (byte)ColumnType.VarBinary => ColumnType.VarBinary,
                            (byte)ColumnType.DateTime => ColumnType.DateTime,
                            _ => ColumnType.Other
                        });
                })
                .ToList();
        }

        protected override IList<string> GetPrimaryKeys(Table table)
        {
            using var connection = OpenConnection();

            var query = @"
select
	columns.name as Name
from
	sys.tables
	inner join sys.key_constraints
		on tables.object_id = key_constraints.parent_object_id AND key_constraints.type = 'PK'
	inner join sys.index_columns
		on key_constraints.parent_object_id = index_columns.object_id
		and key_constraints.unique_index_id  = index_columns.index_id
	inner join sys.columns
		on index_columns.object_id = columns.object_id
		and index_columns.column_id = columns.column_id
where
	tables.name = @TableName
order by
	index_columns.key_ordinal";

            return connection
                .Query(
                    query,
                    new { TableName = table.TableName })
                .Select(x => (string)x.Name)
                .ToList();
        }

    }
}
