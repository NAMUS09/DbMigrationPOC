using Cetas.DbMigration.Core.Domain;
using Cetas.DbMigration.Data.Extensions;
using FluentMigrator;

namespace Cetas.DbMigration.Data.Migrations.Installation
{
    [CetasMigration("2024/04/20 00:00:00", "Test base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {

            Create.TableFor<Log>();
        }
    }
}

