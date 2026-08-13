using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace TXC.RCS.Locations
{
    public class AddressMap : Entity<Guid>
    {
        public string AddressCode { get; private set; } = null!; // 与 FromAddress/ToAddress 一致
        public int TmTarget { get; private set; }
        public string? TmStorage { get; private set; }
        public string? Remark { get; private set; }
        public bool IsEnabled { get; private set; }

        protected AddressMap() { }
        public AddressMap(Guid id, string addressCode, int tmTarget, string tmStorage, string? remark = null, bool isEnabled = true) : base(id)
        {
            AddressCode = Check.NotNullOrWhiteSpace(addressCode, nameof(addressCode), 64);
            TmTarget = tmTarget;
            TmStorage = tmStorage;
            Remark = remark;
            IsEnabled = isEnabled;
        }

        public void Update(string? remark = null, bool isEnabled = true)
        {
            Remark = remark;
            IsEnabled = isEnabled;
        }

        public void ChangeTarget(int tmTarget)
        {
            TmTarget = tmTarget;
        }
    }
}