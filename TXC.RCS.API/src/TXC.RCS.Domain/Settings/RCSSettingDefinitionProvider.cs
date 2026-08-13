using Volo.Abp.Settings;

namespace TXC.RCS.Settings;

public class RCSSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(RCSSettings.MySetting1));
    }
}
