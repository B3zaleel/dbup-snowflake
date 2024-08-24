﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DbUp.Engine.Transactions;
using Snowflake.Data.Client;

namespace DbUp.Snowflake
{
    /// <summary>
    /// Manages Snowflake database connections.
    /// </summary>
    public class SnowflakeConnectionManager : DatabaseConnectionManager
    {
        /// <summary>
        /// Creates a new Snowflake database connection.
        /// </summary>
        /// <param name="connectionString">The Snowflake connection string.</param>
        public SnowflakeConnectionManager(string connectionString) : base(upgradeLog => new SnowflakeDbConnection(connectionString))
        {
        }

        /// <summary>
        /// Splits the statements in the script using the ";" character.
        /// </summary>
        /// <param name="scriptContents">The contents of the script to split.</param>
        public override IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            var scriptStatements =
                Regex.Split(scriptContents, ";(?=(?:[^']*'[^']*')*[^']*$)")
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .ToArray();

            return scriptStatements;
        }

    }
}
