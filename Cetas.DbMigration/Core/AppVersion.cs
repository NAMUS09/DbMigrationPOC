namespace Cetas.DbMigration.Core
{
    /// <summary>
    /// Represents app version
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// Gets the major store version
        /// </summary>
        public const string CURRENT_VERSION = "1.10";

        /// <summary>
        /// Gets the minor store version
        /// </summary>
        public const string MINOR_VERSION = "0";

        /// <summary>
        /// Gets the full store version
        /// </summary>
        public const string FULL_VERSION = CURRENT_VERSION + "." + MINOR_VERSION;
    }
}
