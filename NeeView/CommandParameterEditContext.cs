// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandParameterEditContext
    {
        // command name
        public string Name { get; set; }

        // command parameter source
        public CommandParameter Source { get; set; }

        // command parameter properties
        public List<CommandParameterBase> Items { get; set; }


        /// <summary>
        /// 上書き
        /// </summary>
        /// <param name="source">元となるパラメータ</param>
        public void Set(CommandParameter source)
        {
            Debug.Assert(Source.GetType() == source.GetType());
            foreach (var item in Items)
            {
                var property = item as CommandParameterProperty;
                if (property != null)
                {
                    property.SetValue(property.GetValue(source));
                }
            }
        }


        // instance factory
        #region factory

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CommandParameterEditContext Create(CommandParameter source, string name)
        {
            var package = new CommandParameterEditContext();
            package.Name = name;
            package.Source = source;
            package.Items = CreateProperties(source);
            return package;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static List<CommandParameterBase> CreateProperties(CommandParameter args)
        {
            var type = args.GetType();

            var list = new List<CommandParameterBase>();

            foreach (PropertyInfo info in type.GetProperties())
            {
                var attribute = GetDispNameAttribute(info); // ?? new DispNameAttribute(info.Name);
                if (attribute != null)
                {
                    if (attribute.Title != null)
                    {
                        list.Add(new CommandParameterTitle(attribute.Title));
                    }

                    list.Add(new CommandParameterProperty(args, info, attribute.Name, attribute.Tips));
                }
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private static DispNameAttribute GetDispNameAttribute(MemberInfo info)
        {
            return (DispNameAttribute)Attribute.GetCustomAttributes(info, typeof(DispNameAttribute)).FirstOrDefault();
        }

        #endregion
    }

}
