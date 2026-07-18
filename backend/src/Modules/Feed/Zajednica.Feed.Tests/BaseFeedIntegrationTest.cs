using Zajednica.BuildingBlocks.Tests;

namespace Zajednica.Feed.Tests;

public class BaseFeedIntegrationTest : BaseWebIntegrationTest<FeedTestFactory>
{
    public BaseFeedIntegrationTest(FeedTestFactory factory) : base(factory) { }
}
