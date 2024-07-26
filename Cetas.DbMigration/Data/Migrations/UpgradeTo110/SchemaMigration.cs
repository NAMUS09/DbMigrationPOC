using Cetas.DbMigration.Core.Domain;
using Cetas.DbMigration.Data.Extensions;
using FluentMigrator;

namespace Cetas.DbMigration.Data.Migrations.UpgradeTo110
{
    [CetasMigration("2024-07-25 16:40:00", "Schema Migration for 1.10", MigrationProcessType.Update)]
    public class SchemaMigration : Migration
    {
        public override void Up()
        {
            Create.TableFor<User>();
        }
        public override void Down() { }
    }
}

