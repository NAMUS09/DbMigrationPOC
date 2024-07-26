namespace Cetas.DbMigration.Data
{
    /// <summary>
    /// Represents a data provider
    /// </summary>
    public partial interface IDataProvider
    {
        #region Methods

        /// <summary>
        /// Create the database
        /// </summary>
        /// <param name="collation">Collation</param>
        /// <param name="triesToConnect">Count of tries to connect to the database after creating; set 0 if no need to connect after creating</param>
        void CreateDatabase(string collation = "", int triesToConnect = 10);

        /// <summary>
        /// Initialize database
        /// </summary>
        void InitializeDatabase();

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <returns>Returns true if the database exists.</returns>
        bool DatabaseExists();

        /// <summary>
        /// Build the connection string
        /// </summary>
        /// <param name="ConnectionString">Connection string info</param>
        /// <returns>Connection string</returns>
        string BuildConnectionString(IConnectionStringInfo connectionString);


        /// <summary>
        /// Gets the name of a foreign key
        /// </summary>
        /// <param name="foreignTable">Foreign key table</param>
        /// <param name="foreignColumn">Foreign key column name</param>
        /// <param name="primaryTable">Primary table</param>
        /// <param name="primaryColumn">Primary key column name</param>
        /// <returns>Name of a foreign key</returns>
        string CreateForeignKeyName(string foreignTable, string foreignColumn, string primaryTable, string primaryColumn);

        /// <summary>
        /// Gets the name of an index
        /// </summary>
        /// <param name="targetTable">Target table name</param>
        /// <param name="targetColumn">Target column name</param>
        /// <returns>Name of an index</returns>
        string GetIndexName(string targetTable, string targetColumn);

        #endregion

        #region Properties

        /// <summary>
        /// Gets allowed a limit input value of the data for hashing functions, returns 0 if not limited
        /// </summary>
        int SupportedLengthOfBinaryHash { get; }

        /// <summary>
        /// Gets a value indicating whether this data provider supports backup
        /// </summary>
        bool BackupSupported { get; }

        #endregion
    }
}