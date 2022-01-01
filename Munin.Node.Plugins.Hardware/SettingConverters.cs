namespace Munin.Node.Plugins.Hardware;

using System.Text.Json;
using System.Text.Json.Serialization;

using LibreHardwareMonitor.Hardware;

public sealed class ComplexHardwareConverter : JsonConverter<HardwareType[]>
{
    public override HardwareType[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new[] { Enum.Parse<HardwareType>(reader.GetString()!) };
        }
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var hardwareTypes = new List<HardwareType>();

            do
            {
                if (!reader.Read())
                {
                    throw new JsonException("Read error.");
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    hardwareTypes.Add(Enum.Parse<HardwareType>(reader.GetString()!));
                }
                else
                {
                    throw new JsonException("Unsupported type.");
                }
            }
            while (reader.TokenType != JsonTokenType.EndArray);

            return hardwareTypes.ToArray();
        }

        throw new JsonException("Unsupported type.");
    }

    public override void Write(Utf8JsonWriter writer, HardwareType[] value, JsonSerializerOptions options) =>
        throw new NotSupportedException();
}

public sealed class ComplexFilterConverter : JsonConverter<object[]>
{
    public override object[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new object[] { reader.GetString()! };
        }
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return new object[] { ParseEntry(ref reader) };
        }
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var objects = new List<object>();

            while (true)
            {
                if (!reader.Read())
                {
                    throw new JsonException("Read error.");
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    objects.Add(reader.GetString()!);
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    objects.Add(ParseEntry(ref reader));
                }
                else if (reader.TokenType == JsonTokenType.EndArray)
                {
                    break;
                }
                else
                {
                    throw new JsonException("Unsupported type.");
                }
            }

            return objects.ToArray();
        }

        throw new JsonException("Unsupported type.");
    }

    private static FilterEntry ParseEntry(ref Utf8JsonReader reader)
    {
        if (!reader.Read())
        {
            throw new JsonException("Read error.");
        }

        var entry = new FilterEntry();

        while (true)
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Invalid entry.");
            }

            var property = reader.GetString()!;

            if (!reader.Read())
            {
                throw new JsonException("Read error.");
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Invalid property value.");
            }

            var value = reader.GetString()!;

            switch (property)
            {
                case "Type":
                    entry.Type = Enum.Parse<HardwareType>(value);
                    break;
                case "Name":
                    entry.Name = value;
                    break;
                default:
                    throw new JsonException("Invalid property name.");
            }

            if (!reader.Read())
            {
                throw new JsonException("Read error.");
            }
        }

        return entry;
    }

    public override void Write(Utf8JsonWriter writer, object[] value, JsonSerializerOptions options) =>
        throw new NotSupportedException();
}
