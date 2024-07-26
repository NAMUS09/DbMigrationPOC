namespace Cetas.DbMigration.Core.Configuration
{
    /// <summary>
    /// Represents default values related to configuration services
    /// </summary>
    public static partial class CetasConfigurationDefaults
    {
        /// <summary>
        /// Gets the path to file that contains app settings
        /// </summary>
        public static string AppSettingsFilePath => "appsettings.json";

        /// <summary>
        /// Gets the path to file that contains app settings for specific hosting environment
        /// </summary>
        /// <remarks>0 - Environment name</remarks>
        public static string AppSettingsEnvironmentFilePath => "appsettings.{0}.json";
    }
}