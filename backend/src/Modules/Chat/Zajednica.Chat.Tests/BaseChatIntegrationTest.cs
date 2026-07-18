using Zajednica.BuildingBlocks.Tests;

namespace Zajednica.Chat.Tests;

public class BaseChatIntegrationTest : BaseWebIntegrationTest<ChatTestFactory>
{
    public BaseChatIntegrationTest(ChatTestFactory factory) : base(factory) { }
}
