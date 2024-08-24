using System;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support;

namespace DbUp.Snowflake
{
    public class SnowflakeTableJournal : TableJournal
    {
        public string FqUnquotedSchemaTableName { get; }

        /// <summary>
        ///     Creates a new Snowflake table journal.
        /// </summary>
        /// <param name="connectionManager">The Snowflake connection manager.</param>
        /// <param name="logger">The upgrade logger.</param>
        /// <param name="schema">The name of the schema the journal is stored in.</param>
        /// <param name="tableName">The name of the journal table.</param>
        public SnowflakeTableJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger, string schema, string tableName)
            : base(connectionManager, logger, new SnowflakeObjectParser(), schema, tableName)
        {
            FqUnquotedSchemaTableName = $"{SchemaTableSchema}.{UnquotedSchemaTableName}";
        }

        protected override string GetInsertJournalEntrySql(string scriptName, string applied)
        {
            return $"insert into {FqUnquotedSchemaTableName} (ScriptName, Applied) values (?, ?)";
        }

        protected override string GetJournalEntriesSql()
        {
            return $"select ScriptName from {FqUnquotedSchemaTableName} order by ScriptName";
        }

        /// <summary>Verify, using database-specific queries, if the table exists in the database.</summary>
        /// <returns>1 if table exists, 0 otherwise</returns>
        protected override string DoesTableExistSql()
        {
            var sql = $"select count(*) from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '{UnquotedSchemaTableName.ToUpper()}'";
            if (!string.IsNullOrEmpty(SchemaTableSchema))
                sql += $" and TABLE_SCHEMA = '{SchemaTableSchema.ToUpper()}'";

            return sql;
        }

        protected override string CreateSchemaTableSql(string quotedPrimaryKeyName)
        {
            return $@"CREATE TABLE {FqUnquotedSchemaTableName}
            (
                schemaversionsid int identity constraint {UnquoteSqlObjectName(quotedPrimaryKeyName)} primary key,
                scriptname varchar(255),
                applied timestamp
            )";
        }
    }
}
