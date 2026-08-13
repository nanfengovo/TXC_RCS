using TXC.RCS.Samples;
using Xunit;

namespace TXC.RCS.EntityFrameworkCore.Applications;

[Collection(RCSTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<RCSEntityFrameworkCoreTestModule>
{

}
