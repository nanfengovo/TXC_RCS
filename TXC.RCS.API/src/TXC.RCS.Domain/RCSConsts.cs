using Volo.Abp.Identity;

namespace TXC.RCS;

public static class RCSConsts
{
    public const string DbTablePrefix = "TXC_";
    public const string? DbSchema = "_RCS";
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = "1q2w3E*";
}
