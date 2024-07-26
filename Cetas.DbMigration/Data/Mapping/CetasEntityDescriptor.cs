namespace Cetas.DbMigration.Data.Mapping
{
    public partial class CetasEntityDescriptor
    {
        public CetasEntityDescriptor()
        {
            Fields = new List<CetasEntityFieldDescriptor>();
        }

        public string EntityName { get; set; }
        public string SchemaName { get; set; }
        public ICollection<CetasEntityFieldDescriptor> Fields { get; set; }
    }
}