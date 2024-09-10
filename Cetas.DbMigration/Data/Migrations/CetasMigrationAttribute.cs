using FluentMigrator;
using System.Globalization;

namespace Cetas.DbMigration.Data.Migrations
{
    /// <summary>
    /// Attribute for a migration
    /// </summary>
    public partial class CetasMigrationAttribute : MigrationAttribute
    {
        #region Utils

        protected static long GetVersion(string dateTime)
        {
            return DateTime.ParseExact(dateTime, CetasMigrationDefaults.DateFormats, CultureInfo.InvariantCulture).Ticks;
        }

        protected static long GetVersion(string dateTime, UpdateMigrationType migrationType)
        {
            return GetVersion(dateTime) + (int)migrationType;
        }

        protected static string GetDescription(string cetasVersion, UpdateMigrationType migrationType)
        {
            return string.Format(CetasMigrationDefaults.UpdateMigrationDescription, cetasVersion, migrationType.ToString());
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the CetasMigrationAttribute class
        /// </summary>
        /// <param name="dateTime">The migration date time string to convert on version</param>
        /// <param name="targetMigrationProcess">The target migration process</param>
        public CetasMigrationAttribute(string dateTime, MigrationProcessType targetMigrationProcess = MigrationProcessType.NoMatter) :
            base(GetVersion(dateTime), null)
        {
            TargetMigrationProcess = targetMigrationProcess;
        }

        /// <summary>
        /// Initializes a new instance of the CetasMigrationAttribute class
        /// </summary>
        /// <param name="dateTime">The migration date time string to convert on version</param>
        /// <param name="description">The migration description</param>
        /// <param name="targetMigrationProcess">The target migration process</param>
        public CetasMigrationAttribute(string dateTime, string description, MigrationProcessType targetMigrationProcess = MigrationProcessType.NoMatter) :
            base(GetVersion(dateTime), description)
        {
            TargetMigrationProcess = targetMigrationProcess;
        }

        /// <summary>
        /// Initializes a new instance of the CetasMigrationAttribute class
        /// </summary>
        /// <param name="dateTime">The migration date time string to convert on version</param>
        /// <param name="appVersion">app full version</param>
        /// <param name="migrationType">The migration type</param>
        /// <param name="targetMigrationProcess">The target migration process</param>
        public CetasMigrationAttribute(string dateTime, string appVersion, UpdateMigrationType migrationType, MigrationProcessType targetMigrationProcess = MigrationProcessType.NoMatter) :
            base(GetVersion(dateTime, migrationType), GetDescription(appVersion, migrationType))
        {
            TargetMigrationProcess = targetMigrationProcess;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Target migration process
        /// </summary>
        public MigrationProcessType TargetMigrationProcess { get; set; }

        #endregion
    }
}
