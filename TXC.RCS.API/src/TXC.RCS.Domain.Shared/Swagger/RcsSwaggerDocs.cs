namespace TXC.RCS.Swagger;

/// <summary>
/// Swagger 文档分组常量。
/// <para>
/// 在 Swagger UI 顶部下拉框会出现多个「标签页」：
/// <list type="bullet">
///   <item><see cref="Platform"/>：ABP 平台能力（账号 / 身份 / 权限 / 设置等）</item>
///   <item><see cref="Biz"/>：TXC RCS 业务接口（任务、TM、Erack、MES…）——后续自研接口都放这里</item>
/// </list>
/// </para>
/// <para>
/// 用法：在 AppService / Controller 上加
/// <c>[ApiExplorerSettings(GroupName = RcsSwaggerDocs.Biz)]</c>
/// </para>
/// </summary>
public static class RcsSwaggerDocs
{
    /// <summary>平台 / ABP 内置接口文档 Id（对应 /swagger/v1/swagger.json）</summary>
    public const string Platform = "v1";

    /// <summary>
    /// TXC RCS 业务接口文档 Id（对应 /swagger/biz/swagger.json）。
    /// 人工建单、TM 回调、MES 对接等自研 API 统一进此分组。
    /// </summary>
    public const string Biz = "biz";

    /// <summary>Swagger UI 下拉显示名：平台</summary>
    public const string PlatformDisplayName = "RCS API（平台）";

    /// <summary>Swagger UI 下拉显示名：业务</summary>
    public const string BizDisplayName = "TXC RCS 业务";
}
