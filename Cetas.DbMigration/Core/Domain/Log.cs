namespace Cetas.DbMigration.Core.Domain
{
    public partial class Log : BaseEntity
    {
        public string? Name { get; set; }

        public bool Active { get; set; }
    }
}
