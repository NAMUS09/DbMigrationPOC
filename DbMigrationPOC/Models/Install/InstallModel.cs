using Cetas.DbMigration.Data;
using System.ComponentModel.DataAnnotations;

namespace DbMigrationPOC.Models.Install
{
    public partial record InstallModel : IConnectionStringInfo
    {


        public string DatabaseName { get; set; }
        public string ServerName { get; set; }

        public bool IntegratedSecurity { get; set; }

        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string ConnectionString { get; set; }


        public DataProviderType DataProvider { get; set; }

    }
}