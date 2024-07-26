using Cetas.DbMigration.Core.Configuration;
using Cetas.DbMigration.Core.Infrastructure;
using Newtonsoft.Json;

namespace Cetas.DbMigration.Data
{
    public partial class DataSettingsManager
    {

        private static bool? _databaseIsInstalled;

        public DataSettingsManager()
        {
        }

        /// <summary>
        /// Gets data settings from the old json file (dataSettings.json)
        /// </summary>
        /// <param name="data">Old json file data</param>
        /// <returns>Data settings</returns>
        protected static DataConfig LoadDataSettingsFromOldJsonFile(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            var jsonDataSettings = JsonConvert.DeserializeAnonymousType(data,
                new { DataConnectionString = "", DataProvider = DataProviderType.SqlServer, SQLCommandTimeout = "" });
            var dataSettings = new DataConfig
            {
                ConnectionString = jsonDataSettings.DataConnectionString,
                DataProvider = jsonDataSettings.DataProvider,
                SQLCommandTimeout = int.TryParse(jsonDataSettings.SQLCommandTimeout, out var result) ? result : null
            };

            return dataSettings;
        }



        public static DataConfig LoadSettings(bool reload = false)
        {
            if (!reload && Singleton<DataConfig>.Instance is not null)
                return Singleton<DataConfig>.Instance;

            Singleton<DataConfig>.Instance = Singleton<AppSettings>.Instance.Get<DataConfig>();

            return Singleton<DataConfig>.Instance;

        }


        /// <summary>
        /// Save data settings
        /// </summary>
        /// <param name="dataSettings">Data settings</param>
        /// <param name="fileProvider">File provider</param>
        public static void SaveSettings(DataConfig dataSettings, ICetasFileProvider fileProvider)
        {
            AppSettingsHelper.SaveAppSettings(new List<IConfig> { dataSettings }, fileProvider);
            LoadSettings(reload: true);
        }

        public static bool IsDatabaseInstalled()
        {
            _databaseIsInstalled ??= !string.IsNullOrEmpty(LoadSettings()?.ConnectionString);

            return _databaseIsInstalled.Value;
        }

    }
}