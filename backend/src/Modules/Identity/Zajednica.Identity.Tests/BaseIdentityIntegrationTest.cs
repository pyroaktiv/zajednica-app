using Zajednica.BuildingBlocks.Tests;

namespace Zajednica.Identity.Tests;

public class BaseIdentityIntegrationTest : BaseWebIntegrationTest<IdentityTestFactory>
{
    public BaseIdentityIntegrationTest(IdentityTestFactory factory) : base(factory) { }
}
