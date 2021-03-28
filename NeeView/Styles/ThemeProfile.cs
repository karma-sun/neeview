using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.Json;
using System.Collections;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Linq;
using System.IO;

namespace NeeView
{
    // パネルカラー
    public enum PanelColor
    {
        Dark,
        Light,
    }

    public enum ThemeColorType
    {
        Color,
        Link,
    }


    [TypeConverter(typeof(ThemeColorTypeConverter))]
    [JsonConverter(typeof(ThemeColorJsonConverter))]
    public class ThemeColor
    {
        public ThemeColor(Color color)
        {
            ThemeColorType = ThemeColorType.Color;
            Color = color;
        }

        public ThemeColor(string link)
        {
            ThemeColorType = ThemeColorType.Link;
            Link = link;
        }

        public Color Color { get; private set; }
        public string Link { get; private set; }
        public ThemeColorType ThemeColorType { get; private set; }

        public bool IsColor => ThemeColorType == ThemeColorType.Color;
        public bool IsLink => ThemeColorType == ThemeColorType.Link;


        public override string ToString()
        {
            switch (ThemeColorType)
            {
                case ThemeColorType.Color:
                    return Color.ToString();
                case ThemeColorType.Link:
                    return Link;
            }

            throw new InvalidOperationException();
        }

        public static ThemeColor Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }
            else if (string.Compare(s, "default", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return null;
            }
            else if (s.IndexOf('.') >= 0)
            {
                return new ThemeColor(s);
            }
            else
            {
                return new ThemeColor((Color)ColorConverter.ConvertFromString(s));
            }
        }
    }

    public class ThemeColorTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return ThemeColor.Parse(value as string);
        }
    }

    public sealed class ThemeColorJsonConverter : JsonConverter<ThemeColor>
    {
        public override ThemeColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ThemeColor.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ThemeColor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


    public class ThemeColorMap
    {

        public static ThemeColorMap Default { get; } = new ThemeColorMap()
        {
            ["Window.Background"] = new ThemeColor(Colors.Black),
            ["Window.Foreground"] = new ThemeColor(Colors.White),
            ["Window.Border"] = new ThemeColor(Colors.Gray),
        };


        public static readonly List<string> Keys = new List<string>()
        {
            "Window.Background",
            "Window.Foreground",
            "Window.Border",

            "Control.Background",
            "Control.Foreground",
            "Control.Border",
            "Control.Disable",
            "Control.Accent",

            "Item.MouseOver.Background",
            "Item.MouseOver.Border",
            "Item.Selected.Background",
            "Item.Selected.Border",
            "Item.Inactive.Background",
            "Item.Inactive.Border",

            "Button.Background",
            "Button.Foreground",
            "Button.Border",
            "Button.MouseOver",
            "Button.Checked",
            "Button.Pressed",

            "DialogButton.Background",
            "DialogButton.Foreground",
            "DialogRecommentedButton.Background",
            "DialogRecommentedButton.Foreground",

            "Slider.Background",
            "Slider.Foreground",
            "Slider.Border",
            "Slider.Thumb",
            "Slider.Track",

            "ScrollBar.Background",
            "ScrollBar.Foreground",
            "ScrollBar.Border",
            "ScrollBar.MouseOver",
            "ScrollBar.Pressed",

            "TextBox.Background",
            "TextBox.Foreground",
            "TextBox.Border",

            "Menu.Background",
            "Menu.Foreground",
            "Menu.Border",
            "Menu.Sepalator",


            "SideBar.Background",
            "SideBar.Foreground",
            "SideBar.Border",

            "SidePanel.Background",
            "SidePanel.Foreground",
            "SidePanel.Border",
            "SidePanel.Header",
            "SidePanel.Sepalator",
            "SidePanel.Splitter",

            "MenuBar.Background",
            "MenuBar.Foreground",
            "MenuBar.Border",

            "PageSelectionBar.Background",
            "PageSelectionBar.Foreground",
            "PageSelectionBar.Border",
            "PageSelectionBar.Slider.Background",
            "PageSelectionBar.Slider.Foreground",
            "PageSelectionBar.Slider.Border",
            "PageSelectionBar.Slider.Thumb",
            "PageSelectionBar.Slider.Track",

            "Thumbnail.Background",
            "Thumbnail.Foreground",

            "SelectedMark.Foreground",
            "CheckIcon.Foreground",
            "BookmarkIcon.Foreground",
            "PagemarkIcon.Foreground",
        };

        public ThemeColorMap()
        {
            Format = new FormatVersion(Environment.SolutionName + ".ThemeColorMap");
            Items = new Dictionary<string, ThemeColor>();
        }

        public FormatVersion Format { get; set; }

        public Dictionary<string, ThemeColor> Items { get; set; }

        public ThemeColor this[string key] { get => Items[key]; set => Items[key] = value; }


        public Color GetColor(string key, IEnumerable<string> nests = null)
        {
            if (Items.TryGetValue(key, out var value))
            {
                if (value is null)
                {
                    return GetDefaultColor(key);
                }
                else if (value.IsColor)
                {
                    return value.Color;
                }
                else
                {
                    if (nests != null && nests.Contains(key)) throw new FormatException($"Circular reference: {key}");
                    nests = nests is null ? new List<string>() { key } : nests.Append(key);
                    return GetColor(value.Link, nests);
                }
            }
            else
            {
                return GetDefaultColor(key);
            }
        }

        private Color GetDefaultColor(string key)
        {
            var tokens = key.Split('.');
            if (tokens.Length < 2) throw new FormatException($"Wrong format: {key}");

            var name = string.Join(".", tokens.Take(tokens.Length - 1));
            var role = tokens.Last();

            switch (role)
            {
                case "Foreground":
                case "Background":
                    if (name == "Window") throw new FormatException($"Not defined: {key}");
                    return GetColor("Window." + role);

                default:
                    return GetColor(name + ".Background");
            }
        }

    }


    public class ThemeProfile : BindableBase
    {
        static ThemeProfile() => Current = new ThemeProfile();
        public static ThemeProfile Current { get; }


        private ThemeProfile()
        {
            InitializeThemeColorMap(); // ##

            RefreshThemeColor();

            Config.Current.Theme.AddPropertyChanged(nameof(ThemeConfig.PanelColor), (s, e) =>
            {
                RefreshThemeColor();
            });

        }


        public event EventHandler ThemeColorChanged;

        public ThemeColorMap ThemeColorMap { get; set; }


        private void InitializeThemeColorMap()
        {
            //LoadColorMap();
        }

        // [Develop]
        public void Save(string path)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(ThemeColorMap, UserSettingTools.GetSerializerOptions());
            System.IO.File.WriteAllBytes(path, json);
        }

        // [Develop]
        public static ThemeColorMap Load(string path)
        {
            var json = System.IO.File.ReadAllBytes(path);
            return JsonSerializer.Deserialize<ThemeColorMap>(json, UserSettingTools.GetSerializerOptions());
        }

        public static ThemeColorMap Load(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return JsonSerializer.Deserialize<ThemeColorMap>(ms.ToArray(), UserSettingTools.GetSerializerOptions());
            }
        }


        // [Develop]
        public void LoadColorMap(string contentFilePath)
        {
            try
            {
                Uri uri = new Uri(contentFilePath, UriKind.Relative);
                Debug.WriteLine($"#### LoadColorMap: {uri}");

                var info = Application.GetContentStream(uri);
                ThemeColorMap = Load(info.Stream);
            }
            catch (Exception ex)
            {
                ThemeColorMap = ThemeColorMap.Default;
                ToastService.Current.Show(new Toast(ex.Message, "Error", ToastIcon.Error));
            }
        }

        // [Develop]
        public void LoadColorMap()
        {
            if (Config.Current.Theme.PanelColor == PanelColor.Dark)
            {
                LoadColorMap("Resources/DarkThemeColorMap.json");
            }
            else
            {
                LoadColorMap("Resources/LightThemeColorMap.json");
            }
        }


        public void RefreshThemeColor()
        {
            if (App.Current == null) return;

            LoadColorMap();


            var sw = Stopwatch.StartNew();
            
            foreach (var key in ThemeColorMap.Keys)
            {
                var color = ThemeColorMap.GetColor(key);
                //Debug.WriteLine($"{key}: {color}");
                App.Current.Resources[key] = new SolidColorBrush(color);
            }
         
            sw.Stop();
            Debug.WriteLine($"#### RefreshThemeColor: {sw.ElapsedMilliseconds}ms");

            ThemeColorChanged?.Invoke(this, null);
        }



        /// <summary>
        /// TextBox の EditContextMenu にスタイルを適用する (未使用)
        /// </summary>
        /// <remarks>
        /// from https://stackoverflow.com/questions/30940939/wpf-default-textbox-contextmenu-styling
        /// </remarks>
        /// <param name="element">設定するリソースのエレメント</param>
        public static void InitializeEditorContextMenuStyle(FrameworkElement element)
        {
            var presentationFrameworkAssembly = typeof(Application).Assembly;
            var contextMenuStyle = element.FindResource(typeof(ContextMenu)) as Style;
            var editorContextMenuType = Type.GetType("System.Windows.Documents.TextEditorContextMenu+EditorContextMenu, " + presentationFrameworkAssembly);

            if (editorContextMenuType != null)
            {
                var editorContextMenuStyle = new Style(editorContextMenuType, contextMenuStyle);
                element.Resources.Add(editorContextMenuType, editorContextMenuStyle);
            }

            var menuItemStyle = element.FindResource(typeof(MenuItem)) as Style;
            var editorMenuItemType = Type.GetType("System.Windows.Documents.TextEditorContextMenu+EditorMenuItem, " + presentationFrameworkAssembly);

            if (editorMenuItemType != null)
            {
                var editorContextMenuStyle = new Style(editorMenuItemType, menuItemStyle);
                element.Resources.Add(editorMenuItemType, editorContextMenuStyle);
            }
        }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(PanelColor.Dark)]
            public PanelColor PanelColor { get; set; }

            [DataMember, DefaultValue(PanelColor.Light)]
            public PanelColor MenuColor { get; set; }

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            public void RestoreConfig(Config config)
            {
                config.Theme.PanelColor = PanelColor;
            }
        }

        #endregion

    }
}
