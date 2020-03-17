using NeeLaboratory.ComponentModel;
using System.Collections;
using System.Runtime.Serialization;

namespace NeeView
{
    public class Config : BindableBase
    {
        public static Config Current { get; } = new Config();


        [PropertyMapIgnore]
        public int _Version { get; set; } = Environment.ProductVersionNumber;

        public SystemConfig System { get; set; } = new SystemConfig();

        public StartUpConfig StartUp { get; set; } = new StartUpConfig();

        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();
    }

}