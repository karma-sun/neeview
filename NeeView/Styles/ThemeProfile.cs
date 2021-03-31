using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Diagnostics;
using System.Linq;
using NeeLaboratory;

namespace NeeView
{
    public class ThemeProfile
    {
        public static ThemeProfile Default { get; } = new ThemeProfile()
        {
            ["Window.Background"] = new ThemeColor(System.Windows.Media.Colors.Black, 1.0),
            ["Window.Foreground"] = new ThemeColor(System.Windows.Media.Colors.White, 1.0),
        };

        public static readonly List<string> Keys = new List<string>()
        {
            "Window.Background",
            "Window.Foreground",
            "Window.Border",

            "Control.Background",
            "Control.Foreground",
            "Control.Border",
            "Control.GrayText",
            "Control.Accent",
            "Control.AccentText",

            "Item.MouseOver.Background",
            "Item.MouseOver.Border",
            "Item.Selected.Background",
            "Item.Selected.Border",
            "Item.Inactive.Background",
            "Item.Inactive.Border",

            "Button.Background",
            "Button.Foreground",
            "Button.Border",
            "Button.MouseOver.Background",
            "Button.MouseOver.Border",
            "Button.Checked.Background",
            "Button.Checked.Border",
            "Button.Pressed.Background",
            "Button.Pressed.Border",

            "DialogButton.Background",
            "DialogButton.Foreground",
            "DialogButton.Border",
            "DialogRecommentedButton.Background",
            "DialogRecommentedButton.Foreground",
            "DialogRecommentedButton.Border",

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

        public ThemeProfile()
        {
            Format = new FormatVersion(Environment.SolutionName + ".Theme", 1, 0, 0);
            Colors = new Dictionary<string, ThemeColor>();
        }

        public FormatVersion Format { get; set; }

        public Dictionary<string, ThemeColor> Colors { get; set; }

        public ThemeColor this[string key] { get => Colors[key]; set => Colors[key] = value; }


        [Conditional("DEBUG")]
        public void Verify()
        {
            var lack = Keys.Except(Colors.Keys);
            var surplus = Colors.Keys.Except(Keys);

            Debug.WriteLine("ThemProfile.Verify.Lack: " + string.Join(", ", lack)); // 不足
            Debug.WriteLine("ThemProfile.Verify.Surplus: " + string.Join(", ", surplus)); // 余剰
        }

        public Color GetColor(string key, double opacity, IEnumerable<string> nests = null)
        {
            if (Colors.TryGetValue(key, out var value))
            {
                switch (value.ThemeColorType)
                {
                    case ThemeColorType.Default:
                        return GetDefaultColor(key, opacity);

                    case ThemeColorType.Color:
                        return AddOpacityToColor(value.Color, value.Opacity * opacity);

                    case ThemeColorType.Link:
                        if (nests != null && nests.Contains(key)) throw new FormatException($"Circular reference: {key}");
                        nests = nests is null ? new List<string>() { key } : nests.Append(key);
                        return GetColor(value.Link, value.Opacity * opacity, nests);

                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                return GetDefaultColor(key, opacity);
            }
        }

        private Color AddOpacityToColor(Color color, double opacity)
        {
            if (opacity == 1.0) return color;
            return Color.FromArgb((byte)(MathUtility.Clamp(color.A * opacity, 0.0, 255.0)), color.R, color.G, color.B);
        }

        private Color GetDefaultColor(string key, double opacity)
        {
            var tokens = key.Split('.');
            if (tokens.Length < 2) throw new FormatException($"Wrong format: {key}");

            var name = string.Join(".", tokens.Take(tokens.Length - 1));
            var role = tokens.Last();

            switch (role)
            {
                case "Foreground":
                case "Background":
                    if (name == "Window") return Default.GetColor(key, opacity);
                    return GetColor("Window." + role, opacity);

                default:
                    return GetColor(name + ".Background", opacity);
            }
        }

    }
}
