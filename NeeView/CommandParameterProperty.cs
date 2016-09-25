// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Setterメソッド装備
    /// </summary>
    public interface ISetter
    {
        object GetValue();
        void SetValue(object value);
    }

    // 基底クラス
    public class CommandParameterBase
    {
    }

    /// <summary>
    /// タイトル項目
    /// </summary>
    public class CommandParameterTitle : CommandParameterBase
    {
        public string Name { get; set; }

        public CommandParameterTitle(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// プロパティ項目
    /// </summary>
    public class CommandParameterProperty : CommandParameterBase, ISetter
    {
        public CommandParameter Source { get; set; }
        public PropertyInfo Info { get; set; }
        public string Path => Info?.Name;
        public string Name { get; set; }
        public string Tips { get; set; }

        public CommandParameterProperty(CommandParameter source, PropertyInfo info, string name, string tips)
        {
            Source = source;
            Info = info;
            Name = name;
            Tips = tips;

            TypeCode typeCode = Type.GetTypeCode(Info.PropertyType);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    this.TypeValue = new Value_Boolean(this);
                    break;
                case TypeCode.String:
                    this.TypeValue = new Value_String(this);
                    break;
                case TypeCode.Int32:
                    this.TypeValue = new Value_Integer(this);
                    break;
                default:
                    this.TypeValue = new Value_Object(this);
                    break;
            }
        }

        //
        public void SetValue(object value)
        {
            this.Info.SetValue(this.Source, value);
        }

        //
        public object GetValue()
        {
            return this.Info.GetValue(this.Source);
        }

        //
        public object GetValue(CommandParameter source)
        {
            return this.Info.GetValue(source);
        }

        //
        public ValueBase TypeValue { get; set; }
    }

    #region Value Class

    //
    public abstract class ValueBase
    {
    }

    //
    public class ValueGeneric<T, S> : ValueBase where S : ISetter
    {
        public S Setter { get; set; }

        public ValueGeneric(S setter)
        {
            Setter = setter;
        }

        public T Value
        {
            get { return (T)Setter.GetValue(); }
            set { Setter.SetValue(value); }
        }
    }

    //
    public class Value_Object : ValueGeneric<object, CommandParameterProperty>
    {
        public Value_Object(CommandParameterProperty setter) : base(setter)
        {
        }
    }

    //
    public class Value_Boolean : ValueGeneric<bool, CommandParameterProperty>
    {
        public Value_Boolean(CommandParameterProperty setter) : base(setter)
        {
        }
    }

    //
    public class Value_String : ValueGeneric<string, CommandParameterProperty>
    {
        public Value_String(CommandParameterProperty setter) : base(setter)
        {
        }
    }

    //
    public class Value_Integer : ValueGeneric<int, CommandParameterProperty>
    {
        public Value_Integer(CommandParameterProperty setter) : base(setter)
        {
        }
    }

    #endregion

}
