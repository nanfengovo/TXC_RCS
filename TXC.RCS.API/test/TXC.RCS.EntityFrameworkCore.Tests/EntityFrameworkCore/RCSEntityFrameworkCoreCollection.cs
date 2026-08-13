using Xunit;

namespace TXC.RCS.EntityFrameworkCore;

[CollectionDefinition(RCSTestConsts.CollectionDefinitionName)]
public class RCSEntityFrameworkCoreCollection : ICollectionFixture<RCSEntityFrameworkCoreFixture>
{

}
