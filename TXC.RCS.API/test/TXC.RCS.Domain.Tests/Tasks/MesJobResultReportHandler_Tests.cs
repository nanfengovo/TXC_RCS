using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using TXC.RCS.Tasks.Enums;
using Xunit;

namespace TXC.RCS.Tasks.Mes;

public class MesJobResultReportHandler_Tests
{
    [Fact]
    public async Task Should_Ignore_Manual_Source()
    {
        var reporter = new FakeReporter();
        var handler = new MesJobResultReportHandler(reporter, NullLogger<MesJobResultReportHandler>.Instance);

        await handler.HandleEventAsync(new TaskLifecycleEndedEvent(
            "T1", TaskSource.Manual, TaskLifecycleStatus.Succeeded));

        Assert.Empty(reporter.Calls);
    }

    [Fact]
    public async Task Should_Report_Completed_For_Mes_Succeeded()
    {
        var reporter = new FakeReporter();
        var handler = new MesJobResultReportHandler(reporter, NullLogger<MesJobResultReportHandler>.Instance);

        await handler.HandleEventAsync(new TaskLifecycleEndedEvent(
            "20231106111111001", TaskSource.Mes, TaskLifecycleStatus.Succeeded));

        Assert.Single(reporter.Calls);
        Assert.Equal(MesJobResults.Completed, reporter.Calls[0].JobResult);
        Assert.Equal("20231106111111001", reporter.Calls[0].JobId);
    }

    [Fact]
    public async Task Should_Report_Deleted_For_Mes_Canceled()
    {
        var reporter = new FakeReporter();
        var handler = new MesJobResultReportHandler(reporter, NullLogger<MesJobResultReportHandler>.Instance);

        await handler.HandleEventAsync(new TaskLifecycleEndedEvent(
            "J2", TaskSource.Mes, TaskLifecycleStatus.Canceled, "operator cancel"));

        Assert.Single(reporter.Calls);
        Assert.Equal(MesJobResults.Deleted, reporter.Calls[0].JobResult);
        Assert.Equal("operator cancel", reporter.Calls[0].CancelMessage);
    }

    [Fact]
    public async Task Should_Ignore_Failed()
    {
        var reporter = new FakeReporter();
        var handler = new MesJobResultReportHandler(reporter, NullLogger<MesJobResultReportHandler>.Instance);

        await handler.HandleEventAsync(new TaskLifecycleEndedEvent(
            "J3", TaskSource.Mes, TaskLifecycleStatus.Failed));

        Assert.Empty(reporter.Calls);
    }

    [Fact]
    public async Task Should_Swallow_Reporter_Exceptions()
    {
        var handler = new MesJobResultReportHandler(
            new ThrowingReporter(),
            NullLogger<MesJobResultReportHandler>.Instance);

        await handler.HandleEventAsync(new TaskLifecycleEndedEvent(
            "J4", TaskSource.Mes, TaskLifecycleStatus.Succeeded));
    }

    private sealed class FakeReporter : IMesJobResultReporter
    {
        public List<MesJobReportRequest> Calls { get; } = [];

        public Task<MesJobReportOutcome> ReportAsync(MesJobReportRequest request, CancellationToken ct = default)
        {
            Calls.Add(request);
            return Task.FromResult(MesJobReportOutcome.Ok());
        }
    }

    private sealed class ThrowingReporter : IMesJobResultReporter
    {
        public Task<MesJobReportOutcome> ReportAsync(MesJobReportRequest request, CancellationToken ct = default)
            => throw new InvalidOperationException("boom");
    }
}
