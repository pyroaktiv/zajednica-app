using System.ComponentModel;
using System.Text.Json;
using Shouldly;
using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.BuildingBlocks.Tests.Unit;

public class PageCursorTests
{
    private static readonly PageCursor Cursor =
        new(new DateTime(2026, 7, 30, 18, 25, 4, DateTimeKind.Utc).AddTicks(3850881), Guid.NewGuid());

    [Fact]
    public void A_cursor_survives_the_trip_to_the_client_as_a_single_string()
    {
        var page = new CursorPage<string, PageCursor>(["a"], Cursor);

        var json = JsonSerializer.Serialize(page);
        var returned = JsonSerializer.Deserialize<CursorPage<string, PageCursor>>(json)!;

        json.ShouldContain($"\"{Cursor}\"");
        returned.NextCursor.ShouldBe(Cursor);
        returned.NextCursor!.Value.At.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void A_cursor_comes_back_from_a_query_string_down_to_the_tick()
    {
        var converter = TypeDescriptor.GetConverter(typeof(PageCursor?));

        converter.CanConvertFrom(typeof(string)).ShouldBeTrue();
        converter.ConvertFrom(Cursor.ToString()).ShouldBe(Cursor);
        Should.Throw<FormatException>(() => converter.ConvertFrom("juce"));
    }
}
