using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zajednica.BuildingBlocks.Core.UseCases;

[JsonConverter(typeof(PageCursorJsonConverter))]
[TypeConverter(typeof(PageCursorTypeConverter))]
public readonly record struct PageCursor(DateTime At, Guid Id)
{
    private const char Separator = '_';

    public override string ToString() => $"{At:O}{Separator}{Id}";

    public static PageCursor Parse(string value)
    {
        var separator = value.LastIndexOf(Separator);

        if (separator < 0
            || !DateTime.TryParse(value[..separator], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                out var at)
            || !Guid.TryParse(value[(separator + 1)..], out var id))
            throw new FormatException("A page cursor reads as '<timestamp>_<id>'.");

        return new PageCursor(at, id);
    }
}

public sealed class PageCursorJsonConverter : JsonConverter<PageCursor>
{
    public override PageCursor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        PageCursor.Parse(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, PageCursor value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}

public sealed class PageCursorTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text ? PageCursor.Parse(text) : base.ConvertFrom(context, culture, value);
}
