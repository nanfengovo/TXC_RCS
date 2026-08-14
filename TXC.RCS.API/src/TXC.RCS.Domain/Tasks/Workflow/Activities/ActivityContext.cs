using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Tasks.Workflow
{
    public sealed class ActivityContext
    {
        public required TaskDo Task { get; init; }

        public TaskSignal? Signal { get; init; }

        public Dictionary<string, string> ResponseData { get; init; } = new();
    }

    /// <summary>
    /// 工作流 Activity 契约。
    /// <para>
    /// 注意：不要在本接口上挂 <see cref="ITransientDependency"/>。
    /// ABP 约定注册不会把「继承了依赖标记的接口」暴露给
    /// <c>IEnumerable&lt;IWorkflowActivity&gt;</c>，会导致 <c>RCS:ActivityNotFound</c>。
    /// 请在具体 Activity 类上实现 <see cref="ITransientDependency"/>。
    /// </para>
    /// </summary>
    public interface IWorkflowActivity
    {
        string Name { get; }

        Task ExecuteAsync(ActivityContext ctx, CancellationToken ct = default);
    }
}
