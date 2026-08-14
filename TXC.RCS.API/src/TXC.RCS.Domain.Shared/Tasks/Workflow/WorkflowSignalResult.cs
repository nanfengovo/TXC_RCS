using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.Workflow
{
    public sealed class WorkflowSignalResult
    {
        public bool Accepted { get; init;}

        public IReadOnlyDictionary<string,string> Data { get; init;} = new Dictionary<string,string>();

        public string? ExpectedEvent { get; init;}

        public string? ExpectedLeg { get; init;}

        public string? ActualEvent { get; init;}

        public string? ActualLeg { get; init;}      

        public static WorkflowSignalResult Ok(IReadOnlyDictionary<string,string > data) => new WorkflowSignalResult(){Accepted = true, Data = data};

        public static WorkflowSignalResult Reject(string? expectedEvent, string? expectedLeg, string? actualEvent, string? actualLeg)
        => new()
        {
            Accepted = false,
            ExpectedEvent = expectedEvent,
            ExpectedLeg = expectedLeg,
            ActualEvent = actualEvent,
            ActualLeg = actualLeg
        };
    }
}