﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;

namespace Cetas.DbMigration.Core.Infrastructure
{
    /// <summary>
    /// Represents Cetas engine
    /// </summary>
    public partial class CetasEngine : IEngine
    {
        #region Utilities

        /// <summary>
        /// Get IServiceProvider
        /// </summary>
        /// <returns>IServiceProvider</returns>
        protected virtual IServiceProvider GetServiceProvider(IServiceScope? scope = null)
        {
            if (scope == null)
            {
                var accessor = ServiceProvider?.GetService<IHttpContextAccessor>();
                var context = accessor?.HttpContext;
                return context?.RequestServices ?? ServiceProvider!;
            }
            return scope.ServiceProvider;
        }


        /// <summary>
        /// Run startup tasks
        /// </summary>
        protected virtual void RunStartupTasks()
        {
            //find startup tasks provided by other assemblies
            var typeFinder = Singleton<ITypeFinder>.Instance;
            var startupTasks = typeFinder.FindClassesOfType<IStartupTask>();

            //create and sort instances of startup tasks
            //we startup this interface even for not installed plugins. 
            //otherwise, DbContext initializers won't run and a plugin installation won't work
            var instances = startupTasks
                .Select(startupTask => (IStartupTask)Activator.CreateInstance(startupTask)!)
                .OrderBy(startupTask => startupTask.Order);

            //execute tasks
            foreach (var task in instances)
                task.Execute();
        }

        protected virtual Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyFullName = args.Name;

            //check for assembly already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyFullName);
            if (assembly != null)
                return assembly;

            //get assembly from TypeFinder
            var typeFinder = Singleton<ITypeFinder>.Instance;
            assembly = typeFinder?.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyFullName);

            return assembly ?? AssemblyResolver.GetAssemblyByFullName(assemblyFullName);
        }


        #endregion

        #region Methods


        /// <summary>
        /// Add and configure services
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //register engine
            services.AddSingleton<IEngine>(this);

            //find startup configurations provided by other assemblies
            var typeFinder = Singleton<ITypeFinder>.Instance;
            var startupConfigurations = typeFinder.FindClassesOfType<ICetasStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Select(startup => (ICetasStartup)Activator.CreateInstance(startup)!)
                .OrderBy(startup => startup.Order);

            //configure services
            foreach (var instance in instances)
                instance.ConfigureServices(services, configuration);

            services.AddSingleton(services);

            //register mapper configurations
            //AddAutoMapper();

            //run startup tasks
            RunStartupTasks();

            //resolve assemblies here. otherwise, plugins can throw an exception when rendering views
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve!;
        }

        /// <summary>
        /// Configure HTTP request pipeline
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public virtual void ConfigureRequestPipeline(IApplicationBuilder application)
        {
            ServiceProvider = application.ApplicationServices;

            //find startup configurations provided by other assemblies
            var typeFinder = Singleton<ITypeFinder>.Instance;
            var startupConfigurations = typeFinder.FindClassesOfType<ICetasStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Select(startup => (ICetasStartup)Activator.CreateInstance(startup)!)
                .OrderBy(startup => startup.Order);

            //configure request pipeline
            foreach (var instance in instances)
                instance.Configure(application);
        }


        /// <summary>
        /// Resolve dependency
        /// </summary>
        /// <param name="scope">Scope</param>
        /// <typeparam name="T">Type of resolved service</typeparam>
        /// <returns>Resolved service</returns>
        public virtual T Resolve<T>(IServiceScope scope = null) where T : class
        {
            return (T)Resolve(typeof(T), scope);
        }

        /// <summary>
        /// Resolve dependency
        /// </summary>
        /// <param name="type">Type of resolved service</param>
        /// <param name="scope">Scope</param>
        /// <returns>Resolved service</returns>
        public virtual object Resolve(Type type, IServiceScope? scope = null)
        {
            return GetServiceProvider(scope)?.GetService(type)!;
        }

        /// <summary>
        /// Resolve dependencies
        /// </summary>
        /// <typeparam name="T">Type of resolved services</typeparam>
        /// <returns>Collection of resolved services</returns>
        public virtual IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
        }

        /// <summary>
        /// Resolve unregistered service
        /// </summary>
        /// <param name="type">Type of service</param>
        /// <returns>Resolved service</returns>
        public virtual object ResolveUnregistered(Type type)
        {
            Exception innerException = null;
            foreach (var constructor in type.GetConstructors())
            {
                try
                {
                    //try to resolve constructor parameters
                    var parameters = constructor.GetParameters().Select(parameter =>
                    {
                        var service = Resolve(parameter.ParameterType);
                        if (service == null)
                            throw new Exception("Unknown dependency");
                        return service;
                    });

                    //all is ok, so create instance
                    return Activator.CreateInstance(type, parameters.ToArray());
                }
                catch (Exception ex)
                {
                    innerException = ex;
                }
            }

            throw new Exception("No constructor was found that had all the dependencies satisfied.", innerException);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Service provider
        /// </summary>
        public virtual IServiceProvider? ServiceProvider { get; protected set; }

        #endregion

        #region Nested class

        protected static class AssemblyResolver
        {
            private static readonly Dictionary<string, IDictionary<string, Assembly>> _assemblies = new(StringComparer.InvariantCultureIgnoreCase);

            public static Assembly GetAssemblyByFullName(string assemblyFullName)
            {
                Assembly getAssembly(string fullName)
                {
                    var name = new AssemblyName(fullName);

                    if (string.IsNullOrEmpty(name.Name))
                        return null;

                    if (!_assemblies.ContainsKey(name.Name))
                        return null;

                    var assemblies = _assemblies[name.Name];

                    if (!assemblies.Any())
                        return null;

                    assemblies.TryGetValue(assemblyFullName, out var assembly);

                    return assembly ?? assemblies.Values.First();
                }

                void addAssembly(Assembly assembly)
                {
                    var name = assembly.GetName();

                    if (string.IsNullOrEmpty(name.Name))
                        return;

                    if (!_assemblies.TryGetValue(name.Name, out var assemblies))
                    {
                        assemblies = new Dictionary<string, Assembly>();
                        _assemblies.Add(name.Name, assemblies);
                    }

                    assemblies.TryAdd(name.FullName, assembly);
                }

                if (_assemblies.Any())
                    return getAssembly(assemblyFullName);

                var fileProvider = CommonHelper.DefaultFileProvider;

                foreach (var dll in AppDomain.CurrentDomain.GetAssemblies())
                    addAssembly(dll);

                foreach (var dllPath in fileProvider.GetFiles(AppContext.BaseDirectory, "*.dll"))
                    try
                    {
                        addAssembly(Assembly.LoadFrom(dllPath));
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }

                return getAssembly(assemblyFullName);
            }
        }

        #endregion

    }
}