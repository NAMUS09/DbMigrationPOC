using Cetas.DbMigration.Core;
using Cetas.DbMigration.Core.Configuration;
using Cetas.DbMigration.Core.Infrastructure;
using Cetas.DbMigration.Data;
using Cetas.DbMigration.Helper.Security;
using DbMigrationPOC.Models.Install;

namespace DbMigrationPOC.EndPoints
{
    public class Install : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder routes)
        {

            //var database = routes.MapGroup("/api/v1/database");

            // Create Database End Point

            routes.MapPost("/createDatabase", (InstallModel installModel, ICetasFileProvider cetasFileProvider) =>
            {

                //Consider granting access rights to the resource to the ASP.NET request identity. 
                //ASP.NET has a base process identity 
                //(typically {MACHINE}\ASPNET on IIS 5 or Network Service on IIS 6 and IIS 7, 
                //and the configured application pool identity on IIS 7.5) that is used if the application is not impersonating.
                //If the application is impersonating via <identity impersonate="true"/>, 
                //the identity will be the anonymous user (typically IUSR_MACHINENAME) or the authenticated request user.

                //validate permissions
                var dirsToCheck = cetasFileProvider.GetDirectoriesWrite();
                var response = string.Empty;
                foreach (var dir in dirsToCheck)
                    if (!cetasFileProvider.CheckPermissions(dir, false, true, true, false))
                        response = string.Format("The '{0}' account is not granted with Modify permission on folder '{1}'. Please configure these permissions.", CurrentOSUser.FullName, dir);

                var filesToCheck = cetasFileProvider.GetFilesWrite();
                foreach (var file in filesToCheck)
                {
                    if (!cetasFileProvider.FileExists(file))
                        continue;

                    if (!cetasFileProvider.CheckPermissions(file, false, true, true, true))
                        response = string.Format("The '{0}' account is not granted with Modify permission on file '{1}'. Please configure these permissions.", CurrentOSUser.FullName, file);
                }

                if (!string.IsNullOrEmpty(response))
                    return response;

                var model = installModel;
                var connectionStringSettings = new InstallModel()
                { ServerName = "CIT218\\SQL2019", DatabaseName = "Test", IntegratedSecurity = true };

                try
                {
                    var dataProvider = DataProviderManager.GetDataProvider(DataProviderType.SqlServer);
                    var connectionString = dataProvider.BuildConnectionString(connectionStringSettings);

                    DataSettingsManager.SaveSettings(new DataConfig()
                    {
                        ConnectionString = connectionString,
                    }, cetasFileProvider);

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
                    DataSettingsManager.SaveSettings(new DataConfig(), cetasFileProvider);
                    return "Failed to create dataBase";
                }

            })
            .WithDisplayName("Create Database")
            .WithOpenApi();

            // Restart Application End Point

            routes.MapGet("/restart", (Lazy<IWebHelper> webHelper) =>
            {
                if (DataSettingsManager.IsDatabaseInstalled())
                    return Results.Ok(new { Success = false, Message = "Database  is already installed!!" });

                //restart application
                webHelper.Value.RestartAppDomain();

                return Results.Ok(new { Success = true, Message = "Application is restarting!!" });
            })
            .WithDisplayName("Restart Application")
            .WithOpenApi();

        }
    }
}
