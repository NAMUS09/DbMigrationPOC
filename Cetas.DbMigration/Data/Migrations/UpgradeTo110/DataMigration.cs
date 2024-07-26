using FluentMigrator;

namespace Cetas.DbMigration.Data.Migrations.UpgradeTo110
{
    [CetasMigration("2024-07-25 03:50:00", "1.10", UpdateMigrationType.Data, MigrationProcessType.Update)]
    public class DataMigration : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Log").Row(new { Name = "Test", Active = true });
        }

        public override void Down()
        {

        }
    }
}
