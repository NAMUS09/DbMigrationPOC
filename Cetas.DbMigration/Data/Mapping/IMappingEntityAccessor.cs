using LinqToDB.Mapping;

namespace Cetas.DbMigration.Data.Mapping
{
    /// <summary>
    /// Represents interface to implement an accessor to mapped entities
    /// </summary>
    public interface IMappingEntityAccessor
    {
        /// <summary>
        /// Returns mapped entity descriptor
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <returns>Mapped entity descriptor</returns>
        CetasEntityDescriptor GetEntityDescriptor(Type entityType);

        /// <summary>
        /// Get or create mapping schema with specified configuration name (<see cref="ConfigurationName"/>) and base mapping schema
        /// </summary>
        MappingSchema GetMappingSchema();
    }
}