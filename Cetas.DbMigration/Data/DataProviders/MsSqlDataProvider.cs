using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Cetas.DbMigration.Data.DataProviders
{
    public class MsSqlDataProvider : BaseDataProvider, IDataProvider
    {
        protected virtual SqlConnectionStringBuilder GetConnectionStringBuilder()
        {
            var connectionString = DataSettingsManager.LoadSettings().ConnectionString;

            return new SqlConnectionStringBuilder(connectionString);
        }

        protected static string GetCurrentConnectionString()
        {
            return DataSettingsManager.LoadSettings().ConnectionString;
        }


        /// <summary>
        /// Gets a connection to the database for a current data provider
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Connection to a database</returns>
        private DbConnection GetInternalDbConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            return new SqlConnection(connectionString);
        }

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <returns>Returns true if the database exists.</returns>
        public bool DatabaseExists()
        {
            try
            {
                using var connection = GetInternalDbConnection(GetCurrentConnectionString());
                //just try to connect
                connection.Open();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual string BuildConnectionString(IConnectionStringInfo connectionString)
        {
            if (connectionString is null)
                throw new ArgumentNullException(nameof(connectionString));

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = connectionString.ServerName,
                InitialCatalog = connectionString.DatabaseName,
                PersistSecurityInfo = false,
                IntegratedSecurity = connectionString.IntegratedSecurity,
                TrustServerCertificate = true
            };

            if (!connectionString.IntegratedSecurity)
            {
                builder.UserID = connectionString.Username;
                builder.Password = connectionString.Password;
            }

            return builder.ConnectionString;
        }


        /// <summary>
        /// Create the database
        /// </summary>
        /// <param name="collation">Collation</param>
        /// <param name="triesToConnect">Count of tries to connect to the database after creating; set 0 if no need to connect after creating</param>
        public virtual void CreateDatabase(string collation = "", int triesToConnect = 10)
        {
            if (DatabaseExists())
                return;

            var builder = GetConnectionStringBuilder();

            //gets database name
            var databaseName = builder.InitialCatalog;

            //now create connection string to 'master' dabatase. It always exists.
            builder.InitialCatalog = "master";

            using (DbConnection connection = GetInternalDbConnection(builder.ConnectionString))
            {
                var query = $"CREATE DATABASE [{databaseName}]";
                if (!string.IsNullOrWhiteSpace(collation))
                    query = $"{query} COLLATE {collation}";

                var command = connection.CreateCommand();
                command.CommandText = query;
                command.Connection?.Open();

                command.ExecuteNonQuery();
            }

            //try connect
            if (triesToConnect <= 0)
                return;

            //sometimes on slow servers (hosting) there could be situations when database requires some time to be created.
            //but we have already started creation of tables and sample data.
            //as a result there is an exception thrown and the installation process cannot continue.
            //that's why we are in a cycle of "triesToConnect" times trying to connect to a database with a delay of one second.
            for (var i = 0; i <= triesToConnect; i++)
            {
                if (i == triesToConnect)
                    throw new Exception("Unable to connect to the new database. Please try one more time");

                if (!DatabaseExists())
                    Thread.Sleep(1000);
                else
                    break;
            }
        }

        /// <summary>
        /// Gets the name of a foreign key
        /// </summary>
        /// <param name="foreignTable">Foreign key table</param>
        /// <param name="foreignColumn">Foreign key column name</param>
        /// <param name="primaryTable">Primary table</param>
        /// <param name="primaryColumn">Primary key column name</param>
        /// <returns>Name of a foreign key</returns>
        public virtual string CreateForeignKeyName(string foreignTable, string foreignColumn, string primaryTable, string primaryColumn)
        {
            return $"FK_{foreignTable}_{foreignColumn}_{primaryTable}_{primaryColumn}";
        }

        /// <summary>
        /// Gets the name of an index
        /// </summary>
        /// <param name="targetTable">Target table name</param>
        /// <param name="targetColumn">Target column name</param>
        /// <returns>Name of an index</returns>
        public virtual string GetIndexName(string targetTable, string targetColumn)
        {
            return $"IX_{targetTable}_{targetColumn}";
        }

        /// <summary>
        /// Gets allowed a limit input value of the data for hashing functions, returns 0 if not limited
        /// </summary>
        public int SupportedLengthOfBinaryHash { get; } = 8000;

        /// <summary>
        /// Gets a value indicating whether this data provider supports backup
        /// </summary>
        public virtual bool BackupSupported => true;

    }


}
