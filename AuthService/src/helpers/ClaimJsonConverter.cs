using System;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ClaimJsonConverter : JsonConverter<Claim>
{
    /// <summary>
    /// The function reads a JSON object using Utf8JsonReader and constructs a Claim object based on the
    /// properties found in the JSON.
    /// </summary>
    /// <param name="Utf8JsonReader">The `Utf8JsonReader` class in .NET is used for reading and parsing
    /// JSON data efficiently from UTF-8 encoded text. It is a high-performance, low-allocation API for
    /// working with JSON data.</param>
    /// <param name="Type">The `Type` parameter in the code snippet represents the type of the claim. It
    /// is extracted from the JSON object being read and used to create a new `Claim` object.</param>
    /// <param name="JsonSerializerOptions">`JsonSerializerOptions` is a class that provides options to
    /// control the behavior during JSON serialization and deserialization. It allows you to customize
    /// various aspects of the serialization process, such as naming policies, converters, and more. In
    /// the provided code snippet, the `JsonSerializerOptions` parameter is used to</param>
    /// <returns>
    /// A `Claim` object is being returned, created using the values parsed from the JSON object. The
    /// `Claim` object is constructed using the 5-argument constructor with the following parameters:
    /// - `type` (or an empty string if not present in the JSON)
    /// - `value` (or an empty string if not present in the JSON)
    /// - `valueType` (defaulted to `
    /// </returns>
    public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Parse the JSON object into a JsonDocument for robust property access
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        string? type = root.TryGetProperty("Type", out var pType) ? pType.GetString() : null;
        string? value = root.TryGetProperty("Value", out var pValue) ? pValue.GetString() : null;
        string? valueType = root.TryGetProperty("ValueType", out var pValueType) ? pValueType.GetString() : ClaimValueTypes.String;
        string? issuer = root.TryGetProperty("Issuer", out var pIssuer) ? pIssuer.GetString() : ClaimsIdentity.DefaultIssuer;
        string? originalIssuer = root.TryGetProperty("OriginalIssuer", out var pOrig) ? pOrig.GetString() : ClaimsIdentity.DefaultIssuer;

        // Use the 5-arg constructor to preserve as much info as possible
        return new Claim(type ?? string.Empty, value ?? string.Empty, valueType, issuer, originalIssuer);
    }

    /// <summary>
    /// The function writes a Claim object to a Utf8JsonWriter in C#, including optional fields only if
    /// they are not null or empty.
    /// </summary>
    /// <param name="Utf8JsonWriter">The `Utf8JsonWriter` class in .NET is used to write JSON data in
    /// UTF-8 encoding. It provides methods to write JSON tokens like objects, arrays, properties,
    /// strings, numbers, and boolean values to a stream or buffer. In the provided code snippet, the
    /// `Write`</param>
    /// <param name="Claim">The `Write` method you provided is used to serialize a `Claim` object into
    /// JSON format using `Utf8JsonWriter`. The `Claim` class seems to have properties like `Type`,
    /// `Value`, `ValueType`, `Issuer`, and `OriginalIssuer`.</param>
    /// <param name="JsonSerializerOptions">The `JsonSerializerOptions` parameter in the `Write` method
    /// is used to provide options for customizing the JSON serialization process. These options can
    /// include settings such as property naming policies, converters, and other serialization-related
    /// configurations. By passing `JsonSerializerOptions` as a parameter, you can control how</param>
    public override void Write(Utf8JsonWriter writer, Claim value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value.Type);
        writer.WriteString("Value", value.Value);
        // Only write optional fields if they exist (keeps JSON smaller)
        if (!string.IsNullOrEmpty(value.ValueType))
            writer.WriteString("ValueType", value.ValueType);
        if (!string.IsNullOrEmpty(value.Issuer))
            writer.WriteString("Issuer", value.Issuer);
        if (!string.IsNullOrEmpty(value.OriginalIssuer))
            writer.WriteString("OriginalIssuer", value.OriginalIssuer);
        writer.WriteEndObject();
    }
}
