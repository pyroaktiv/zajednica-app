using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Zajednica.BuildingBlocks.Tests;

public class BaseWebIntegrationTest<TTestFactory> : IClassFixture<TTestFactory>
    where TTestFactory : WebApplicationFactory<Program>
{
    protected TTestFactory Factory { get; }

    public BaseWebIntegrationTest(TTestFactory factory) => Factory = factory;

    // Helper for unit-testing controllers directly: builds a ControllerContext with a
    // fake authenticated principal carrying the account id.
    protected static ControllerContext BuildContext(string accountId)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("accountId", accountId)
                }))
            }
        };
    }
}
