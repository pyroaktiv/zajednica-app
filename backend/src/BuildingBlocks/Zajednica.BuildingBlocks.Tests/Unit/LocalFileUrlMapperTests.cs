using Microsoft.Extensions.Configuration;
using Shouldly;
using Zajednica.BuildingBlocks.Infrastructure.Storage;

namespace Zajednica.BuildingBlocks.Tests.Unit;

public class LocalFileUrlMapperTests
{
    private static LocalFileUrlMapper Mapper(string publicBaseUrl) =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:PublicBaseUrl"] = publicBaseUrl
            })
            .Build());

    [Fact]
    public void ToUrl_prepends_base_url_and_public_path_to_a_key()
    {
        Mapper("http://192.168.1.8:5265").ToUrl("images/abc.jpg")
            .ShouldBe("http://192.168.1.8:5265/uploads/images/abc.jpg");
    }

    [Fact]
    public void ToKey_strips_base_url_and_public_path_from_a_url()
    {
        Mapper("http://192.168.1.8:5265").ToKey("http://192.168.1.8:5265/uploads/images/abc.jpg")
            .ShouldBe("images/abc.jpg");
    }

    [Fact]
    public void ToKey_then_ToUrl_round_trips_under_a_changed_host()
    {
        var key = Mapper("http://192.168.1.8:5265").ToKey("http://192.168.1.8:5265/uploads/images/abc.jpg");

        Mapper("http://10.0.0.5:5265").ToUrl(key)
            .ShouldBe("http://10.0.0.5:5265/uploads/images/abc.jpg");
    }

    [Fact]
    public void ToKey_is_idempotent_on_a_value_that_is_already_a_key()
    {
        Mapper("http://192.168.1.8:5265").ToKey("images/abc.jpg").ShouldBe("images/abc.jpg");
    }

    [Fact]
    public void ToUrl_passes_through_a_value_that_is_already_absolute()
    {
        Mapper("http://192.168.1.8:5265").ToUrl("https://cdn.example/x.jpg")
            .ShouldBe("https://cdn.example/x.jpg");
    }

    [Fact]
    public void Null_or_blank_maps_to_null_both_ways()
    {
        var mapper = Mapper("http://192.168.1.8:5265");
        mapper.ToUrl(null).ShouldBeNull();
        mapper.ToUrl("  ").ShouldBeNull();
        mapper.ToKey(null).ShouldBeNull();
        mapper.ToKey("").ShouldBeNull();
    }
}
