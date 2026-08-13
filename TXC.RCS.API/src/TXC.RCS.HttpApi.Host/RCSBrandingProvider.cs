using Microsoft.Extensions.Localization;
using TXC.RCS.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace TXC.RCS;

[Dependency(ReplaceServices = true)]
public class RCSBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<RCSResource> _localizer;

    public RCSBrandingProvider(IStringLocalizer<RCSResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
