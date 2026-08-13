using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.EventConst;

namespace TXC.RCS.Tasks.Workflow
{
    public static class WorkflowTemplateCatalog
    {
        public const string FetchPutCode = "fetch_put_standard";

        public const int FetchPutVersion =1;

        public static WorkflowTemplateDefinition CreateFetchPut()
        {
            return new WorkflowTemplateDefinition
            {
                Code = FetchPutCode,
                Version = FetchPutVersion,
                Steps = 
                [
                    new () {Id = "dispatch", Activity = WorkflowActivities.TmDispatch},
                    new ()
                    {
                        Id = "wait_fetch_started",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.TaskStarted,
                            Leg = TaskLegs.Fetch
                        }
                    },
                    new ()
                    {
                        Id = "wait_fetch_arrived",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.Arrived,
                            Leg = TaskLegs.Fetch
                        }
                    },
                    new ()
                    {
                        Id = "wait_fetch_permitted",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.PermitRequested,
                            Leg = TaskLegs.Fetch
                        },
                        Activity = WorkflowActivities.TmReplyPermit
                    },
                    new ()
                    {
                        Id = "wait_fetch_finished",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.Finished,
                            Leg = TaskLegs.Fetch
                        }
                    },
                    new ()
                    {
                        Id = "wait_put_started",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.TaskStarted,
                            Leg = TaskLegs.Put
                        }
                    },
                    new ()
                    {
                        Id = "wait_put_arrived",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.Arrived,
                            Leg = TaskLegs.Put
                        }
                    },
                    new ()
                    {
                        Id = "wait_put_permitted",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.PermitRequested,
                            Leg = TaskLegs.Put
                        },
                        Activity = WorkflowActivities.TmReplyPermit
                    },
                    new ()
                    {
                        Id = "wait_put_finished",
                        Wait = new WorkflowWaitDefinition
                        {
                            Event = TaskEvents.Finished,
                            Leg = TaskLegs.Put
                        }
                    },
                    new ()
                    {
                        Id = "complete",
                        Activity = WorkflowActivities.ExecutionComplete
                    }
                ]
            };
        }
    }
}