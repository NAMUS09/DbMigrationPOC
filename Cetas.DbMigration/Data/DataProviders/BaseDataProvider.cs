using Cetas.DbMigration.Core.Infrastructure;
using Cetas.DbMigration.Data.Extensions;
using Cetas.DbMigration.Data.Mapping;
using Cetas.DbMigration.Data.Migrations;
using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using System.Collections.Concurrent;
using System.Reflection;

namespace Cetas.DbMigration.Data.DataProviders
{
    public abstract partial class BaseDataProvider
    {
        #region Fields

        protected static ConcurrentDictionary<Type, CetasEntityDescriptor> EntityDescriptors { get; } = new ConcurrentDictionary<Type, CetasEntityDescriptor>();

        #endregion


        /// <summary>
        /// Initialize database
        /// </summary>
        public virtual void InitializeDatabase()
        {
            var migrationManager = EngineContext.Current.Resolve<IMigrationManager>();

            var targetAssembly = typeof(CetasDbStartup).Assembly;
            migrationManager.ApplyUpMigrations(targetAssembly);

            var typeFinder = Singleton<ITypeFinder>.Instance;
            var mAssemblies = typeFinder.FindClassesOfType<MigrationBase>()
                .Select(t => t.Assembly)
                .Where(assembly => !assembly.FullName?.Contains("FluentMigrator.Runner") ?? false)
                .Distinct()
                .ToArray();

            //mark update migrations as applied
            foreach (var assembly in mAssemblies)
                migrationManager.ApplyUpMigrations(assembly, MigrationProcessType.Update, true);

        }

        /// <summary>
        /// Returns mapped entity descriptor
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <returns>Mapped entity descriptor</returns>
        public virtual CetasEntityDescriptor GetEntityDescriptor(Type entityType)
        {
            return EntityDescriptors.GetOrAdd(entityType, t =>
            {
                var tableName = NameCompatibilityManager.GetTableName(t);
                var expression = new CreateTableExpression { TableName = tableName };
                var builder = new CreateTableExpressionBuilder(expression, new NullMigrationContext());
                builder.RetrieveTableExpressions(t);

                return new CetasEntityDescriptor
                {
                    EntityName = tableName,
                    SchemaName = builder.Expression.SchemaName,
                    Fields = builder.Expression.Columns.Select(column => new CetasEntityFieldDescriptor
                    {
                        Name = column.Name,
                        IsPrimaryKey = column.IsPrimaryKey,
                        IsNullable = column.IsNullable,
                        Size = column.Size,
                        Precision = column.Precision,
                        IsIdentity = column.IsIdentity,
                        Type = getPropertyTypeByColumnName(t, column.Name)
                    }).ToList()
                };
            });

            static Type getPropertyTypeByColumnName(Type targetType, string name)
            {
                var (mappedType, _) = Array.Find(targetType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty), pi => name.Equals(NameCompatibilityManager.GetColumnName(targetType, pi.Name)))!.PropertyType.GetTypeToMap();

                return mappedType;
            }
        }
    }
}