namespace Dockyra.Domain.ValueObjects;

public readonly record struct HexColor
{
    public string Value { get; }

    public HexColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = "#3B82F6";
            return;
        }

        string trimmed = value.Trim();
        if (!trimmed.StartsWith('#'))
        {
            trimmed = "#" + trimmed;
        }

        Value = (trimmed.Length is 7 or 9) ? trimmed.ToUpperInvariant() : "#3B82F6";
    }

    public static HexColor DefaultBlue => new("#3B82F6");
    public override string ToString() => Value;
}

public readonly record struct IconReference(string GlyphOrPath)
{
    public static IconReference DefaultFolder => new("FolderGlyph");
    public override string ToString() => string.IsNullOrWhiteSpace(GlyphOrPath) ? "AppGlyph" : GlyphOrPath;
}

public sealed record AppIdentity(
    string ProcessName,
    string? ExecutablePath,
    string? WindowTitle = null);

