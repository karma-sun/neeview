using NeeLaboratory.ComponentModel;
using System;
using System.Collections;
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
                    // クラスは再帰的に処理する
                    Merge(property.GetValue(original), property.GetValue(target));
                }
                else
                {
                    // Setterが存在する場合のみ上書き
                    property.GetSetMethod()?.Invoke(original, new object[] { property.GetValue(target) });
                }
            }
        }

    }
}