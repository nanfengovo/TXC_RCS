using TXC.RCS.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TXC.RCS.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class RCSController : AbpControllerBase
{
    protected RCSController()
    {
        LocalizationResource = typeof(RCSResource);
    }
}
