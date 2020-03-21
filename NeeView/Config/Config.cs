using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace NeeView
{
    public class Config : BindableBase
    {
        public static Config Current { get; } = new Config();


        public SystemConfig System { get; set; } = new SystemConfig();

        public StartUpConfig StartUp { get; set; } = new StartUpConfig();

        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();

        public ImageConfig Image { get; set; } = new ImageConfig();

        public ArchiveConfig Archive { get; set; } = new ArchiveConfig();

        public SusieConfig Susie { get; set; } = new SusieConfig();

        public HistoryConfig History { get; set; } = new HistoryConfig();

        public BookmarkConfig Bookmark { get; set; } = new BookmarkConfig();

        public PagemarkConfig Pagemark { get; set; } = new PagemarkConfig();

        public LayoutConfig Layout { get; set; } = new LayoutConfig();

        public SlideShowConfig SlideShow { get; set; } = new SlideShowConfig();

        public CommandConfig Command { get; set; } = new CommandConfig();

        public ScriptConfig Script { get; set; } = new ScriptConfig();



        /// <summary>
        /// Configプロパティを上書き
        /// </summary>
        public void Merge(Config config)
        {
            if (config == null) return;
            Merge(this, config);
        }

        /// <summary>
        /// インスタンスのプロパティを上書き
        /// </summary>
        private static void Merge(object original, object target)
        {
            var type = original.GetType();
            if (type != target.GetType()) throw new InvalidOperationException();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    if (property.PropertyType.GetCustomAttribute(typeof(PropertyMergeAttribute)) != null)
                    {
                        // 参照を変更する
                        property.GetSetMethod(false)?.Invoke(original, new object[] { property.GetValue(target) });
                    }
                    else
                    {
                        // クラスは再帰的に処理する
                        Merge(property.GetValue(original), property.GetValue(target));
                    }
                }
                else
                {
                    // Setterが存在する場合のみ上書き
                    property.GetSetMethod(false)?.Invoke(original, new object[] { property.GetValue(target) });
                }
            }
        }

    }

    // この属性がクラスに定義されている場合、リファレンスの変更のみとする
    [AttributeUsage(AttributeTargets.Class)]
    public class PropertyMergeAttribute : Attribute
    {
    }
}