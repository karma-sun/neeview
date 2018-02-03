// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// 
    /// </summary>
    public class PropertyDocument
    {
        // name
        public string Name { get; set; }

        // class source
        public object Source { get; set; }

        // properties
        public List<PropertyDrawElement> Elements { get; set; }

        // properties (member only)
        public List<PropertyMemberElement> PropertyMembers => Elements.OfType<PropertyMemberElement>().ToList();


        //
        public PropertyMemberElement GetPropertyMember(string path)
        {
            return Elements.OfType<PropertyMemberElement>().FirstOrDefault(e => e.Path == path);
        }


        /// <summary>
        /// 上書き
        /// </summary>
        /// <param name="source">元となるパラメータ</param>
        public void Set(object source)
        {
            Debug.Assert(Source.GetType() == source.GetType());
            foreach (var element in Elements)
            {
                var property = element as PropertyMemberElement;
                if (property != null)
                {
                    property.SetValue(property.GetValue(source));
                }
            }
        }

        /// <summary>
        /// 全ての設定値を初期化
        /// </summary>
        public void Reset()
        {
            foreach (var item in this.Elements.OfType<PropertyMemberElement>())
            {
                item.ResetValue();
            }
        }

        //
        public PropertyDocument()
        {
        }

        //
        public PropertyDocument(object source)
        {
            this.Source = source;
            this.Elements = CreateProperyContentList(source);
        }

        //
        public PropertyDocument(IEnumerable<object> sources)
        {
            ////this.Source = source;
            this.Elements = sources.Select(e => CreateProperyContentList(e)).SelectMany(e => e).ToList();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<PropertyDrawElement> CreateProperyContentList(object source)
        {
            var type = source.GetType();

            var list = new List<PropertyDrawElement>();

            foreach (PropertyInfo info in type.GetProperties())
            {
                var attribute = GetPropertyMemberAttribute(info);
                if (attribute != null)
                {
                    if (attribute.Title != null)
                    {
                        list.Add(new PropertyTitleElement(attribute.Title));
                    }

                    var element = attribute.CreateContent(source, info);
                    if (element.IsVisible)
                    {
                        list.Add(element);
                    }
                    else
                    {
                        ////Debug.WriteLine($"PropertyDocument: {element.Name} is Hide.");
                    }
                }
            }
            return list;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private PropertyMemberAttribute GetPropertyMemberAttribute(MemberInfo info)
        {
            return (PropertyMemberAttribute)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private DefaultValueAttribute GetDefaultValueAttribute(MemberInfo info)
        {
            return (DefaultValueAttribute)Attribute.GetCustomAttributes(info, typeof(DefaultValueAttribute)).FirstOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private ObsoleteAttribute GetObsoleteValueAttribute(MemberInfo info)
        {
            return (ObsoleteAttribute)Attribute.GetCustomAttributes(info, typeof(ObsoleteAttribute)).FirstOrDefault();
        }
    }
}
