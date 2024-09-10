using Cetas.DbMigration.Core.Domain;
using FluentMigrator.Builders.Create.Table;

namespace Cetas.DbMigration.Data.Mapping.Builders.Users
{

    /// <summary>
    /// Represents a log entity builder
    /// </summary>
    public partial class UserBuilder : CetasEntityBuilder<User>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(User.Name)).AsString(100)
                .WithColumn(nameof(User.Email)).AsString(500)
                .WithColumn(nameof(User.UserName)).AsString(500);
        }

        #endregion
    }
}
