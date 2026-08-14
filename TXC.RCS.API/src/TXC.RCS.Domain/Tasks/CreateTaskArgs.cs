using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks
{
    public sealed class CreateTaskArgs
    {
        public required string FromAddress { get; init; }
        public string? FromPort {get; init;}
        public string? MiddleAddress { get; init; }
        public string? MiddlePort { get; init; }
        public string? ToAddress { get; init; }
        public string? ToPort {get; init;}
        public string? ContainerId { get; init; }
        public int? FetchCount { get; init; }
        public int? PutCount { get; init; }
        public string? FetchMaterialCode { get; init; }
        public string? PutMaterialCode { get; init; }
        public string? FetchEquipmentCode { get; init; }
        public string? PutEquipmentCode { get; init; }

        /// <summary>人工/MES 传入的 TaskCode 字段，key 对齐当前 Schema。</summary>
        public IReadOnlyDictionary<string, int>? OptionFields { get; init; }
    }
}