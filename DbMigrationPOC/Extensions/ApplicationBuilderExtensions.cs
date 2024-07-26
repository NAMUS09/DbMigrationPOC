using Cetas.DbMigration.Core.Infrastructure;
using Cetas.DbMigration.Data;
using Cetas.DbMigration.Data.Migrations;
using System.Reflection;

namespace DbMigrationPOC.Extensions
{
    /// <summary>
    /// Represents extensions of IApplicationBuilder
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Configure the application HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public static void ConfigureRequestPipeline(this IApplicationBuilder application)
        {
            EngineContext.Current.ConfigureRequestPipeline(application);
        }

        public static void StartEngine(this IApplicationBuilder application)
        {
            var engine = EngineContext.Current;

            //further actions are performed only when the database is installed
            if (DataSettingsManager.IsDatabaseInstalled())
            {
                //log application start
                //engine.Resolve<ILogger>().Information("Application started");

                //install and update plugins
                //var pluginService = engine.Resolve<IPluginService>();
                //pluginService.InstallPluginsAsync().Wait();
                //pluginService.UpdatePluginsAsync().Wait();

                //update nopCommerce core and db
                var migrationManager = engine.Resolve<IMigrationManager>();
                var assembly = Assembly.GetAssembly(typeof(ApplicationBuilderExtensions))!;
                migrationManager.ApplyUpMigrations(assembly, MigrationProcessType.Update);
                assembly = Assembly.GetAssembly(typeof(IMigrationManager))!;
                migrationManager.ApplyUpMigrations(assembly, MigrationProcessType.Update);

                //var taskScheduler = engine.Resolve<ITaskScheduler>();
                //taskScheduler.InitializeAsync().Wait();
                //taskScheduler.StartScheduler();
            }
        }
    }
}
