using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(JsonFormatVersionConverter))]
    public class FormatVersion : IComparable<FormatVersion>, IEquatable<FormatVersion>
    {
        public FormatVersion(string name)
        {
            Name = name;
            MajorVersion = Environment.AssemblyVersion.Major;
            MinorVersion = Environment.AssemblyVersion.Minor;
            BuildVersion = 0;
        }

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

        public static string CreateFormatName(params string[] names)
        {
            return Environment.ApplicationName + "." + string.Join(".", names);
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

        public int CompareTo(FormatVersion other)
        {
            if (this.Name != other.Name)
            {
                return this.Name.CompareTo(other.Name);
            }
            if (this.MajorVersion != other.MajorVersion)
            {
                return this.MajorVersion - other.MajorVersion;
            }
            if (this.MinorVersion != other.MinorVersion)
            {
                return this.MinorVersion - other.MinorVersion;
            }
            if (this.BuildVersion != other.BuildVersion)
            {
                return this.BuildVersion - other.BuildVersion;
            }
            return 0;
        }

        public bool Equals(FormatVersion other)
        {
            return other != null &&
                this.Name == other.Name &&
                this.MajorVersion == other.MajorVersion &&
                this.MinorVersion == other.MinorVersion &&
                this.BuildVersion == other.BuildVersion;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;

            var formatVersion = other as FormatVersion;
            if (formatVersion == null)
            {
                return false;
            }
            else
            {
                return Equals(formatVersion);
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ VersionNumber;
        }

        public static bool operator ==(FormatVersion v1, FormatVersion v2)
        {
            if (((object)v1) == null || ((object)v2) == null)
                return Equals(v1, v2);

            return v1.Equals(v2);
        }

        public static bool operator !=(FormatVersion v1, FormatVersion v2)
        {
            if (((object)v1) == null || ((object)v2) == null)
                return !Equals(v1, v2);

            return !(v1.Equals(v2));
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