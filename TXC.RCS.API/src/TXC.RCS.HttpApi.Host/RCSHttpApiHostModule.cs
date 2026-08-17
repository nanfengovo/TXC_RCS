using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Server.AspNetCore;
using TXC.RCS.EntityFrameworkCore;
using TXC.RCS.MultiTenancy;
using TXC.RCS.HealthChecks;
using Microsoft.OpenApi;
using Volo.Abp;
using Volo.Abp.Studio;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Microsoft.AspNetCore.Hosting;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Studio.Client.AspNetCore;
using Volo.Abp.Security.Claims;
using Volo.Abp.AspNetCore.Mvc.Libs;
using Microsoft.Extensions.Options;
using TXC.RCS.Options;
using TXC.RCS.Tasks.TM;
using TXC.RCS.Tasks.Mes;
using TXC.RCS.Tm;
using TXC.RCS.Mes;
using TXC.RCS.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TXC.RCS;

[DependsOn(
    typeof(RCSHttpApiModule),
    typeof(AbpStudioClientAspNetCoreModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(RCSApplicationModule),
    typeof(RCSEntityFrameworkCoreModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule)
    )]
public class RCSHttpApiHostModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("RCS");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<TmOptions>(configuration.GetSection(TmOptions.SectionName));
        context.Services.AddHttpClient("Tm");
        // 两个实现：不要都靠 ITransientDependency 自动暴露成 ITmClient
        context.Services.AddTransient<SimulationTmClient>();
        context.Services.AddTransient<HttpTmClient>();
        context.Services.AddTransient<ITmClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<TmOptions>>().Value;
            return opt.Mode.Equals("Real", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<HttpTmClient>()
                : sp.GetRequiredService<SimulationTmClient>();
        });

        Configure<MesOptions>(configuration.GetSection(MesOptions.SectionName));
        context.Services.AddHttpClient("Mes");
        context.Services.AddTransient<SimulationMesJobResultReporter>();
        context.Services.AddTransient<HttpMesJobResultReporter>();
        context.Services.AddTransient<IMesJobResultReporter>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<MesOptions>>().Value;
            return opt.Mode.Equals("Real", StringComparison.OrdinalIgnoreCase)
                ? sp.GetRequiredService<HttpMesJobResultReporter>()
                : sp.GetRequiredService<SimulationMesJobResultReporter>();
        });


        var hostingEnvironment = context.Services.GetHostingEnvironment();

        // Swagger / 纯 API 调用没有 antiforgery cookie；未带有效 Bearer 时 ABP 会校验并返回空 400。
        // S1：关闭自动校验，后续 TM/MES 回调也不会被拦。
        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidate = false;
        });

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
            
            Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        if (hostingEnvironment.IsDevelopment())
        {
            context.Services.AddRazorPages()
                .AddRazorRuntimeCompilation();
        }

        ConfigureStudio(hostingEnvironment);
        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles(hostingEnvironment);
        ConfigureConventionalControllers();
        ConfigureHealthChecks(context);
        ConfigureSwagger(context, configuration);
        ConfigureVirtualFileSystem(context);
        ConfigureCors(context, configuration);

        // 禁用检查libs 纯API项目 
        Configure<AbpMvcLibsOptions>(options =>
        {
            options.CheckLibs = false;
        });
    }

    private void ConfigureStudio(IHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsProduction())
        {
            Configure<AbpStudioClientOptions>(options =>
            {
                options.IsLinkEnabled = false;
            });
        }
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });
    }

    private void ConfigureBundles(IHostEnvironment hostingEnvironment)
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                    if (hostingEnvironment.IsDevelopment())
                    {
                        bundle.AddFiles("/dev-login-helper.js");
                    }
                }
            );
        });
    }


    private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<RCSDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}TXC.RCS.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<RCSDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}TXC.RCS.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<RCSApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}TXC.RCS.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<RCSApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}TXC.RCS.Application"));
            });
        }
    }

    private void ConfigureConventionalControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(RCSApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAbpSwaggerGenWithOidc(
            configuration["AuthServer:Authority"]!,
            ["RCS"],
            [AbpSwaggerOidcFlows.AuthorizationCode],
            null,
            options =>
            {
                // —— 标签页 1：ABP 平台（账号 / 身份 / 权限…）——
                options.SwaggerDoc(RcsSwaggerDocs.Platform, new OpenApiInfo
                {
                    Title = "RCS API",
                    Version = "v1",
                    Description = "ABP 平台与系统接口（Identity / Account / Permission / Setting 等）。"
                });

                // —— 标签页 2：TXC RCS 业务（自研 API 都进这里）——
                options.SwaggerDoc(RcsSwaggerDocs.Biz, new OpenApiInfo
                {
                    Title = "TXC RCS 业务",
                    Version = "v1",
                    Description =
                        "晶技 RCS 业务接口：任务创建、工作流、TM 回调、Erack、MES 等。\n\n" +
                        "新增 AppService / Controller 时请加：\n" +
                        $"[ApiExplorerSettings(GroupName = \"{RcsSwaggerDocs.Biz}\")]"
                });

                // 按 GroupName 分流到不同文档；未标注的进平台页
                options.DocInclusionPredicate((docName, description) =>
                {
                    var group = description.GroupName;
                    if (string.Equals(docName, RcsSwaggerDocs.Biz, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Equals(group, RcsSwaggerDocs.Biz, StringComparison.OrdinalIgnoreCase);
                    }

                    // Platform(v1)：排除业务分组
                    return !string.Equals(group, RcsSwaggerDocs.Biz, StringComparison.OrdinalIgnoreCase);
                });

                options.CustomSchemaIds(type => type.FullName);
                IncludeXmlCommentsIfPresent(options);
            });
    }

    /// <summary>把各项目生成的 *.xml 注释灌进 Swagger（字段说明 / remarks）。</summary>
    private static void IncludeXmlCommentsIfPresent(SwaggerGenOptions options)
    {
        var baseDir = AppContext.BaseDirectory;
        foreach (var name in new[]
                 {
                     "TXC.RCS.Application.Contracts.xml",
                     "TXC.RCS.Application.xml",
                     "TXC.RCS.Domain.Shared.xml",
                     "TXC.RCS.HttpApi.Host.xml"
                 })
        {
            var path = Path.Combine(baseDir, name);
            if (File.Exists(path))
            {
                options.IncludeXmlComments(path, includeControllerXmlComments: true);
            }
        }
    }

    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddRCSHealthChecks();
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseRouting();
        app.MapAbpStaticAssets();
        app.UseAbpStudioLink();
        app.UseAbpSecurityHeaders();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            // 下拉框里的两个「标签页」
            options.SwaggerEndpoint($"/swagger/{RcsSwaggerDocs.Biz}/swagger.json", RcsSwaggerDocs.BizDisplayName);
            options.SwaggerEndpoint($"/swagger/{RcsSwaggerDocs.Platform}/swagger.json", RcsSwaggerDocs.PlatformDisplayName);

            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
