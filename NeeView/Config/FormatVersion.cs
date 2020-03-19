﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(JsonFormatVersionConverter))]
    public class FormatVersion
    {
        public FormatVersion(string name, int majorVersion, int minorVersion, int buildVersion)
        {
            Name = name;
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            BuildVersion = buildVersion;
        }

        public string Name { get; private set; }

        public int MajorVersion { get; private set; }
        public int MinorVersion { get; private set; }
        public int BuildVersion { get; private set; }


        public int VersionNumber => MajorVersion << 16 | MinorVersion << 8 | BuildVersion;


        public override string ToString()
        {
            return $"{Name}/{MajorVersion}.{MinorVersion}.{BuildVersion}";
        }

        public static FormatVersion Parse(string s)
        {
            var tokens = s.Trim().Split('/');
            if (tokens.Length < 2) throw new InvalidCastException();

            var name = tokens[0].Trim();

            var versions = tokens[1].Trim().Split('.');
            var major = (versions.Length > 0) ? int.Parse(versions[0]) : 0;
            var minor = (versions.Length > 1) ? int.Parse(versions[1]) : 0;
            var build = (versions.Length > 2) ? int.Parse(versions[2]) : 0;

            return new FormatVersion(name, major, minor, build);
        }
    }

    public sealed class JsonFormatVersionConverter : JsonConverter<FormatVersion>
    {
        public override FormatVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return FormatVersion.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, FormatVersion value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

}