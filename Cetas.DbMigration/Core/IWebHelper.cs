namespace Cetas.DbMigration.Core;

/// <summary>
/// Represents a web helper
/// </summary>
public partial interface IWebHelper
{

    /// <summary>
    /// Restart application domain
    /// </summary>
    void RestartAppDomain();
}