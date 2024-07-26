using Microsoft.Extensions.Hosting;


namespace Cetas.DbMigration.Core;

/// <summary>
/// Represents a web helper
/// </summary>
public partial class WebHelper : IWebHelper
{
    #region Fields  

    protected readonly IHostApplicationLifetime _hostApplicationLifetime;

    #endregion

    #region Ctor

    public WebHelper(IHostApplicationLifetime hostApplicationLifetime)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Restart application domain
    /// </summary>
    public virtual void RestartAppDomain()
    {
        _hostApplicationLifetime.StopApplication();
    }

    #endregion
}