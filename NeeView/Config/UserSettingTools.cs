using NeeView.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public static class UserSettingTools
    {
        public static UserSettingV2 CreateUserSetting()
        {
            // 情報の確定
            WindowPlacement.Current.StorePlacement();

            return new UserSettingV2()
            {
                Format = new FormatVersion(Environment.SolutionName, Environment.AssemblyVersion.Major, Environment.AssemblyVersion.Minor, 0),
                Config = Config.Current,
                ContextMenu = ContextMenuManager.Current.CreateContextMenuNode(),
                SusiePlugins = SusiePluginManager.Current.CreateSusiePluginCollection(),
                DragActions = DragActionTable.Current.CreateDragActionCollection(),
                Commands = CommandTable.Current.CreateCommandCollectionMemento(),
            };
        }

        public static void Save(string path)
        {
            Save(path, CreateUserSetting());
        }

        public static void Save(string path, UserSettingV2 setting)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(setting, GetSerializerOptions());
            File.WriteAllBytes(path, json);
        }



        public static UserSettingV2 Load(string path)
        {
            var json = File.ReadAllBytes(path);
            return Load(new ReadOnlySpan<byte>(json));
        }

        public static UserSettingV2 Load(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return Load(new ReadOnlySpan<byte>(ms.ToArray()));
            }
        }

        public static UserSettingV2 Load(ReadOnlySpan<byte> json)
        {
            return JsonSerializer.Deserialize<UserSettingV2>(json, GetSerializerOptions());

            // TODO: v.38以後の互換性処理をここで？
        }

        private static JsonSerializerOptions GetSerializerOptions()
        {
            var options = new JsonSerializerOptions();

            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = true;
            options.IgnoreReadOnlyProperties = true;
            options.Converters.Add(new JsonEnumFuzzyConverter());
            options.Converters.Add(new JsonColorConverter());
            options.Converters.Add(new JsonSizeConverter());
            options.Converters.Add(new JsonTimeSpanConverter());
            options.Converters.Add(new JsonGridLengthConverter());
            return options;
        }

        public static void Restore(UserSettingV2 setting, ObjectMergeOption options = null)
        {
            if (setting == null) return;

            ObjectMerge.Merge(Config.Current, setting.Config, options);

            // コマンド設定反映
            CommandTable.Current.RestoreCommandCollection(setting.Commands);

            // ドラッグアクション反映
            DragActionTable.Current.RestoreDragActionCollection(setting.DragActions);

            // コンテキストメニュー設定反映
            ContextMenuManager.Current.Resotre(setting.ContextMenu);

            // SusiePlugins反映
            SusiePluginManager.Current.RestoreSusiePluginCollection(setting.SusiePlugins);
        }
    }


    /// <summary>
    /// Sizeを文字列に変換する
    /// </summary>
    public sealed class JsonSizeConverter : JsonConverter<Size>
    {
        public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (Size)new SizeConverter().ConvertFrom(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Colorを文字列に変換する
    /// </summary>
    public sealed class JsonColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (Color)ColorConverter.ConvertFromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// Enumを文字列に変換する。
    /// 文字列がEnumに変換できないときはdefault値とみなす
    /// </summary>
    public class JsonEnumFuzzyConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(EnumFuzzyConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { },
                culture: null);

            return converter;
        }


        public class EnumFuzzyConverter<T> : JsonConverter<T>
            where T : struct, Enum
        {
            public override bool CanConvert(Type type)
            {
                return type.IsEnum;
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return Enum.TryParse(reader.GetString(), out T value) ? value : default;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }


    /// <summary>
    /// GridLengthを文字列に変換する
    /// </summary>
    public sealed class JsonGridLengthConverter : JsonConverter<GridLength>
    {
        public override GridLength Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (GridLength)new GridLengthConverter().ConvertFromString(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, GridLength value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    /// <summary>
    /// TimeSpanを文字列に変換する
    /// </summary>
    public sealed class JsonTimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


    /// <summary>
    /// 設定V1を設定V2に変換
    /// </summary>
    public static class UserSettingV1Extensions
    {
        public static UserSettingV2 ConvertToV2(this UserSetting setting)
        {
            var settingV2 = new UserSettingV2();

            settingV2.Format = new FormatVersion(Environment.SolutionName, 36, 9, 0);

            // restore setting
            //void RestoreSetting(UserSetting setting)
            {
                settingV2.Config = new Config();
                setting.RestoreConfig(settingV2);
            }

            return settingV2;

            // 記帳中の設定更新MEMO（同期、インポート等）
            // - ウィドウ状態等の引き継がない情報をsettingV2で修正
            // - Current.Configにマージ
        }
    }

}