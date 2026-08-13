using TXC.RCS.Samples;
using Xunit;

namespace TXC.RCS.EntityFrameworkCore.Domains;

[Collection(RCSTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<RCSEntityFrameworkCoreTestModule>
{

}
