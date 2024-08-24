using System;
using System.Collections.Generic;
using System.Linq;
using DbUp;
using DbUp.Builder;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Snowflake;
using Snowflake.Data.Client;

// ReSharper disable once CheckNamespace
/// <summary>
/// Configuration extension methods for Snowflake.
/// </summary>
public static class SnowflakeExtensions
{
    /// <summary>
    /// Creates an upgrade engine builder for Snowflake databases.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionString">Snowflake database connection string.</param>
    /// <returns>
    /// A builder for a database upgrader designed for Snowflake databases.
    /// </returns>
    public static UpgradeEngineBuilder SnowflakeDatabase(this SupportedDatabases supported, string connectionString)
    {
        var connectionOptions = new SnowflakeDbConnectionStringBuilder { ConnectionString = connectionString };
        var schema = connectionOptions["schema"] as string;
        return SnowflakeDatabase(new SnowflakeConnectionManager(connectionString), schema);
    }

    /// <summary>
    /// Creates an upgrade engine builder for Snowflake databases.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionString">Snowflake database connection string.</param>
    /// <param name="schema">The schema in which to check for changes</param>
    /// <returns>
    /// A builder for a database upgrader designed for Snowflake databases.
    /// </returns>
    public static UpgradeEngineBuilder SnowflakeDatabase(this SupportedDatabases supported, string connectionString, string schema)
    {
        return SnowflakeDatabase(new SnowflakeConnectionManager(connectionString), schema);
    }


    /// <summary>
    /// Creates an upgrade engine builder for Snowflake databases.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionManager">The <see cref="SnowflakeDatabaseConnectionManager"/> to be used during a database upgrade.</param>
    /// <returns>
    /// A builder for a database upgrader designed for Snowflake databases.
    /// </returns>
    public static UpgradeEngineBuilder SnowflakeDatabase(this SupportedDatabases supported, IConnectionManager connectionManager)
        => SnowflakeDatabase(connectionManager);

    /// <summary>
    /// Creates an upgrade engine builder for Snowflake databases.
    /// </summary>
    /// <param name="connectionManager">The <see cref=" SnowflakeConnectionManager"/> to be used during a database upgrade.</param>
    /// <returns>
    /// A builder for a database upgrader designed for Snowflake databases.
    /// </returns>
    public static UpgradeEngineBuilder SnowflakeDatabase(IConnectionManager connectionManager)
        => SnowflakeDatabase(connectionManager, null);

    /// <summary>
    /// Creates an upgrade engine builder for Snowflake databases.
    /// </summary>
    /// <param name="connectionManager">The <see cref=" SnowflakeConnectionManager"/> to be used during a database upgrade.</param>
    /// <param name="schema">The schema in which to check for changes</param>
    /// <returns>
    /// A builder for a database upgrader designed for Snowflake databases.
    /// </returns>
    public static UpgradeEngineBuilder SnowflakeDatabase(IConnectionManager connectionManager, string schema)
    {
        var builder = new UpgradeEngineBuilder();
        builder.Configure(c => c.ConnectionManager = connectionManager);
        builder.Configure(c => c.ScriptExecutor = new SnowflakeScriptExecutor(() => c.ConnectionManager, () => c.Log, schema, () => c.VariablesEnabled, c.ScriptPreprocessors, () => c.Journal));
        builder.JournalToSnowflakeTable(schema, "schema_versions");
        return builder;
    }

    /// <summary>
    /// Ensures that the database specified in the connection string exists.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns></returns>
    public static void SnowflakeDatabase(this SupportedDatabasesForEnsureDatabase supported, string connectionString)
    {
        SnowflakeDatabase(supported, connectionString, new ConsoleUpgradeLog());
    }

    /// <summary>
    /// Ensures that the database specified in the connection string exists.
    /// </summary>
    /// <param name="supported">Fluent helper type.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="logger">The <see cref="DbUp.Engine.Output.IUpgradeLog"/> used to record actions.</param>
    /// <returns></returns>
    public static void SnowflakeDatabase(this SupportedDatabasesForEnsureDatabase supported, string connectionString, IUpgradeLog logger)
    {
        // @TODO implement
    }

    /// <summary>
    /// Tracks the list of executed scripts in a Snowflake table.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="table">The table.</param>
    /// <returns></returns>
    public static UpgradeEngineBuilder JournalToSnowflakeTable(this UpgradeEngineBuilder builder, string schema, string table)
    {
        builder.Configure(c => c.Journal = new SnowflakeTableJournal(() => c.ConnectionManager, () => c.Log, schema, table));
        return builder;
    }

    public static UpgradeEngineBuilder WithSnowflakeVariables(this UpgradeEngineBuilder builder, IDictionary<string, string> variables)
    {
        var preprocessor = new SnowflakePreprocessor(variables);
        builder.Configure(c => c.ScriptPreprocessors.Add(preprocessor));
        return builder;
    }
}
