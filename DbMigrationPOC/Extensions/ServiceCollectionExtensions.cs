using Cetas.DbMigration.Core;
using Cetas.DbMigration.Core.Configuration;
using Cetas.DbMigration.Core.Infrastructure;
using System.Net;
using System.Threading.RateLimiting;

namespace DbMigrationPOC.Extensions
{
    /// <summary>
    /// Represents extensions of IServiceCollection
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure base application settings
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="builder">A builder for web applications and services</param>
        public static void ConfigureApplicationSettings(this IServiceCollection services,
            WebApplicationBuilder builder)
        {
            //let the operating system decide what TLS protocol version to use
            //see https://docs.microsoft.com/dotnet/framework/network-programming/tls
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

            //create default file provider
            CommonHelper.DefaultFileProvider = new CetasFileProvider(builder.Environment);

            //register type finder
            var typeFinder = new WebAppTypeFinder();
            Singleton<ITypeFinder>.Instance = typeFinder;
            services.AddSingleton<ITypeFinder>(typeFinder);

            //add configuration parameters
            var configurations = typeFinder
                .FindClassesOfType<IConfig>()
                .Select(configType => (IConfig)Activator.CreateInstance(configType)!)
                .ToList();

            foreach (var config in configurations)
                builder.Configuration.GetSection(config.Name).Bind(config, options => options.BindNonPublicProperties = true);

            var appSettings = AppSettingsHelper.SaveAppSettings(configurations, CommonHelper.DefaultFileProvider, false);
            services.AddSingleton(appSettings);
        }


        /// <summary>
        /// Add services to the application and configure service provider
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="builder">A builder for web applications and services</param>
        public static void ConfigureApplicationServices(this IServiceCollection services,
            WebApplicationBuilder builder)
        {
            //add accessor to HttpContext
            services.AddHttpContextAccessor();

            services.AddControllers();

            //initialize plugins
            //var mvcCoreBuilder = services.AddMvcCore();
            //var pluginConfig = new PluginConfig();
            //builder.Configuration.GetSection(nameof(PluginConfig)).Bind(pluginConfig, options => options.BindNonPublicProperties = true);
            //mvcCoreBuilder.PartManager.InitializePlugins(pluginConfig);

            //create engine and configure service provider
            var engine = EngineContext.Create();

            builder.Services.AddRateLimiter(options =>
            {
                var settings = Singleton<AppSettings>.Instance.Get<CommonConfig>();

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = settings.PermitLimit,
                            QueueLimit = settings.QueueCount,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.RejectionStatusCode = settings.RejectionStatusCode;
            });


            engine.ConfigureServices(services, builder.Configuration);
        }

        /// <summary>
        /// Register HttpContextAccessor
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}
