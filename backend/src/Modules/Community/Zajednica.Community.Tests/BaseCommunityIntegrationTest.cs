using Zajednica.BuildingBlocks.Tests;

namespace Zajednica.Community.Tests;

public class BaseCommunityIntegrationTest : BaseWebIntegrationTest<CommunityTestFactory>
{
    public BaseCommunityIntegrationTest(CommunityTestFactory factory) : base(factory) { }
}
