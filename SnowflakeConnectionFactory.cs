using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Snowflake.Data.Client;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace DbUp.Snowflake
{
    public class SnowflakeConnectionFactory : IConnectionFactory
    {
        public string Database { get; }
        public string Warehouse { get; }

        public string ConnectionString { get; }
        public SnowflakeConnectionFactory(string connectionString)
        {
            var connection = connectionString.Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                {
                    var keyValue = x.Split('=');
                    return new KeyValuePair<string, string>(keyValue[0].ToLowerInvariant(), keyValue[1]);
                })
                .ToDictionary(x => x.Key, y => y.Value);

            if (!(connection.ContainsKey("database") || connection.ContainsKey("db")))
                throw new ArgumentException("Connection string does not have database parameter");
            if (!connection.ContainsKey("warehouse"))
                throw new ArgumentException("Connection string does not have warehouse parameter");

            ConnectionString = connectionString;
            Database = connection.ContainsKey("database") ? connection["database"] : connection["db"];
            Warehouse = connection["warehouse"];
        }
        public IDbConnection CreateConnection(IUpgradeLog upgradeLog, DatabaseConnectionManager databaseConnectionManager)
        {
            var connection = new SnowflakeDbConnection(ConnectionString);
            connection.Open();
            using var useWHcommand = connection.CreateCommand();
            useWHcommand.CommandText = $"use warehouse {Warehouse}";
            useWHcommand.ExecuteNonQuery();
            using var useDBcommand = connection.CreateCommand();
            useDBcommand.CommandText = $"use database {Database}";
            useDBcommand.ExecuteScalar();
            return connection;
        }
    }
}
