using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Options
{
    public class TmOptions
    {
        public const string SectionName = "Tm";
        public string Mode { get; set; } = "Sim"; // Sim | Real
        public string BaseUrl { get; set; } = "http://127.0.0.1:9999";
        public string TaskAddPath { get; set; } = "api/v1/xinsong/task_add";
    }
}