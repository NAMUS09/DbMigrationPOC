namespace Cetas.DbMigration.Core.Domain
{
    public partial class User : BaseEntity
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
    }
}
