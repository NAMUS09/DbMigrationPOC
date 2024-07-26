using Asp.Versioning;
using Cetas.DbMigration.Core.Configuration;
using Cetas.DbMigration.Core.Infrastructure;
using Cetas.DbMigration.Data;
using DbMigrationPOC.Models.Install;
using Microsoft.AspNetCore.Mvc;

namespace DbMigrationPOC.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class InstallController : ControllerBase
    {
        protected readonly ICetasFileProvider _cetasFileProvider;

        public InstallController(ICetasFileProvider cetasFileProvider)
        {
            _cetasFileProvider = cetasFileProvider;
        }

        [HttpPost]
        [Route("Index")]
        public virtual string Index(InstallModel model)
        {
            var connectionStringSettings = new InstallModel()
            { ServerName = "CIT218\\SQL2019", DatabaseName = "Test", IntegratedSecurity = true };

            try
            {
                var dataProvider = DataProviderManager.GetDataProvider(DataProviderType.SqlServer);
                var connectionString = dataProvider.BuildConnectionString(connectionStringSettings);

                DataSettingsManager.SaveSettings(new DataConfig()
                {
                    ConnectionString = connectionString,
                }, _cetasFileProvider);

                //create database
                dataProvider.CreateDatabase();

                //initialize database
                dataProvider.InitializeDatabase();

                return "DataBase created successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //clear provider settings if something got wrong
                DataSettingsManager.SaveSettings(new DataConfig(), _cetasFileProvider);
                return "Failed to create dataBase";
            }
        }
    }
}
