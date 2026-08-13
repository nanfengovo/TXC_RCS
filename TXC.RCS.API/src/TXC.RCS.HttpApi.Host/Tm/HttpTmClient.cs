using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TXC.RCS.Options;
using TXC.RCS.Tasks.TM;
using Volo.Abp;

namespace TXC.RCS.Tm;

/// <summary>Real：HTTP 调用调度 task_add。</summary>
public class HttpTmClient : ITmClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TmOptions _options;

    public HttpTmClient(IHttpClientFactory httpClientFactory, IOptions<TmOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task TaskAddAsync(TmTaskAddRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Tm");
        var baseUrl = _options.BaseUrl.TrimEnd('/') + "/";
        client.BaseAddress ??= new Uri(baseUrl);

        using var response = await client.PostAsJsonAsync(_options.TaskAddPath.TrimStart('/'), request, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TmHttpResult>(cancellationToken: ct);
        if (body is not { Result: true })
        {
            throw new BusinessException("RCS:TmTaskAddFailed")
                .WithData("ErrMsg", body?.ErrMsg ?? string.Empty);
        }
    }
}

public sealed class TmHttpResult
{
    public bool Result { get; set; }
    public string? ErrMsg { get; set; }
}
