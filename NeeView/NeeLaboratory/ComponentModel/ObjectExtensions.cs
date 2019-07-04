using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.ComponentModel
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// DevaultValue属性でプロパティを初期化する
        /// from: https://stackoverflow.com/questions/2329868/net-defaultvalue-attribute
        /// </summary>
        /// <param name="obj"></param>
        public static void InitializePropertyDefaultValues(this object obj)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var d = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (d != null)
                {
                    prop.SetValue(obj, d.Value);
                }
            }
        }
    }
}
