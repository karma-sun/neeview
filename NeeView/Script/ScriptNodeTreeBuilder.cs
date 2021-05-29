using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NeeView
{
    public static class ScriptNodeTreeBuilder
    {
        public static ScriptNodeDirect Create(object source, string name)
        {
            var root = new ScriptNodeDirect(source, name);
            root.Children = CreateChildren(root);
            return root;
        }

        private static List<ScriptNode> CreateChildren(ScriptNode node)
        {
            var type = node.Type;

            var children = new List<ScriptNode>();

            if (!node.Type.IsClass)
            {
                return null;
            }

            if (node.Type == typeof(string))
            {
                return null;
            }

            // 配列は非対応
            if (node.Type.IsArray)
            {
                return null;
            }

            // Genericは非対応
            if (node.Type.IsGenericType)
            {
                return null;
            }

            // Obsoleteは非対応
            if (node.Obsolete != null)
            {
                return null;
            }

            if (node.Value is PropertyMap propertyMap)
            {
                foreach (var item in propertyMap)
                {
                    var child = new ScriptNodeRefrection(new ScriptNodeRefrectionSource(propertyMap, new ScriptMemberInfo(item.Key)));
                    child.Children = CreateChildren(child);
                    children.Add(child);
                }
            }

            if (node.Value is CommandAccessorMap commandMap)
            {
                foreach (var item in commandMap)
                {
                    var child = new ScriptNodeRefrection(new ScriptNodeRefrectionSource(commandMap, new ScriptMemberInfo(item.Key)));
                    child.Children = CreateChildren(child);
                    children.Add(child);
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(e => IsDocumentable(e)))
            {
                var child = new ScriptNodeRefrection(new ScriptNodeRefrectionSource(node.Value, new ScriptMemberInfo(property)));
                child.Children = CreateChildren(child);
                children.Add(child);
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(e => IsDocumentable(e)))
            {
                var child = new ScriptNodeRefrection(new ScriptNodeRefrectionSource(node.Value, new ScriptMemberInfo(method)));
                children.Add(child);
            }

            return children;


            bool IsDocumentable(MemberInfo info)
            {
                return info.GetCustomAttribute<DocumentableAttribute>() != null;
            }
        }
    }



    /// <summary>
    /// ScriptNode 情報表示用
    /// </summary>
    /// <remarks>
    /// FullNameが取得できるのがScriptNodeとの違い
    /// </remarks>
    public class ScriptNodeUnit
    {
        public ScriptNodeUnit(string prefix, ScriptNode node)
        {
            Prefix = prefix;
            Node = node;
        }

        public string Prefix { get; }
        public ScriptNode Node { get; }

        public string Name => Node.Name;
        public string FullName => CreateFullName();

        public string Alternative
        {
            get
            {
                if (Node.Alternative?.Alternative is null) return "x";
                var alt = Node.Alternative;
                var msg = alt.IsFullName ? alt.Alternative : Prefixed(alt.Alternative);
                return msg;
            }
        }

        private string CreateFullName()
        {
            var name = Prefixed(Name);
            if (Node.Category == ScriptMemberInfoType.Method)
            {
                name += "()";
            }
            return name;
        }

        private string Prefixed(string s)
        {
            return Prefix != null ? Prefix + "." + s : s;
        }
    }



    public abstract class ScriptNode
    {
        public abstract string Name { get; }
        public virtual ScriptMemberInfoType Category => ScriptMemberInfoType.None;
        public abstract Type Type { get; }
        public abstract object Value { get; }
        public virtual ObsoleteAttribute Obsolete => null;
        public virtual AlternativeAttribute Alternative => null;
        public List<ScriptNode> Children { get; set; }


        public IEnumerable<ScriptNodeUnit> GetUnitEnumerator(string prefix)
        {
            var parent = new ScriptNodeUnit(prefix, this);
            yield return parent;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var node in child.GetUnitEnumerator(parent.FullName))
                    {
                        yield return node;
                    }
                }
            }
        }
    }


    
    /// <summary>
    /// ScriptNode: sourceをそのまま参照するタイプ
    /// </summary>
    public class ScriptNodeDirect : ScriptNode
    {
        private object _source;

        public ScriptNodeDirect(object source, string name)
        {
            _source = source;
            Name = name;
        }

        public override string Name { get; }
        public override Type Type => _source.GetType();
        public override object Value => _source;
    }


    /// <summary>
    /// ScriptNode: sourceを基準にリフレクションでプロパティ等のメンバーを参照するタイプ
    /// </summary>
    public class ScriptNodeRefrection : ScriptNode
    {
        private ScriptNodeRefrectionSource _source;

        public ScriptNodeRefrection(ScriptNodeRefrectionSource source)
        {
            _source = source;
        }

        public override string Name => _source.GetName();
        public override ScriptMemberInfoType Category => _source.MemberInfo.Type;
        public override Type Type => _source.GetValueType();
        public override object Value => _source.GetValue();
        public override ObsoleteAttribute Obsolete => _source.GetValueObsolete();
        public override AlternativeAttribute Alternative => _source.GetValueAlternative();
    }



    /// <summary>
    /// リフレクション参照ソース
    /// </summary>
    /// <remarks>
    /// PropertyMapやCommandAccessorMapも対応
    /// </remarks>
    public class ScriptNodeRefrectionSource
    {
        public ScriptNodeRefrectionSource(object source, ScriptMemberInfo memberInfo)
        {
            Source = source;
            MemberInfo = memberInfo;
        }


        public object Source { get; set; }
        public ScriptMemberInfo MemberInfo { get; set; }


        public string GetName()
        {
            switch (MemberInfo.Type)
            {
                case ScriptMemberInfoType.Property:
                    return MemberInfo.PropertyInfo.Name;

                case ScriptMemberInfoType.Method:
                    return MemberInfo.MethodInfo.Name;

                case ScriptMemberInfoType.IndexKey:
                    return MemberInfo.IndexKey;

                default:
                    throw new NotSupportedException();
            }
        }

        public ObsoleteAttribute GetValueObsolete()
        {
            switch (MemberInfo.Type)
            {
                case ScriptMemberInfoType.Property:
                    return MemberInfo.PropertyInfo.GetCustomAttribute<ObsoleteAttribute>();

                case ScriptMemberInfoType.Method:
                    return MemberInfo.MethodInfo.GetCustomAttribute<ObsoleteAttribute>();

                case ScriptMemberInfoType.IndexKey:
                    return GetIndexerValueObsolete(Source, MemberInfo.IndexKey);

                default:
                    throw new NotSupportedException();
            }
        }

        public AlternativeAttribute GetValueAlternative()
        {
            switch (MemberInfo.Type)
            {
                case ScriptMemberInfoType.Property:
                    return MemberInfo.PropertyInfo.GetCustomAttribute<AlternativeAttribute>();

                case ScriptMemberInfoType.Method:
                    return MemberInfo.MethodInfo.GetCustomAttribute<AlternativeAttribute>();

                case ScriptMemberInfoType.IndexKey:
                    return GetIndexerValueAlternative(Source, MemberInfo.IndexKey);

                default:
                    throw new NotSupportedException();
            }
        }

        public Type GetValueType()
        {
            switch (MemberInfo.Type)
            {
                case ScriptMemberInfoType.Property:
                    return MemberInfo.PropertyInfo.PropertyType;

                case ScriptMemberInfoType.Method:
                    return MemberInfo.MethodInfo.ReturnType;

                case ScriptMemberInfoType.IndexKey:
                    return GetIndexerValueType(Source, MemberInfo.IndexKey);

                default:
                    throw new NotSupportedException();
            }
        }
        
        public object GetValue()
        {
            switch (MemberInfo.Type)
            {
                case ScriptMemberInfoType.Property:
                    return MemberInfo.PropertyInfo.GetValue(Source);

                case ScriptMemberInfoType.IndexKey:
                    return GetIndexerValue(Source, MemberInfo.IndexKey);

                default:
                    throw new NotSupportedException();
            }
        }

        private ObsoleteAttribute GetIndexerValueObsolete(object source, string key)
        {
            switch (source)
            {
                case PropertyMap propertyMap:
                    return GetPropertyMapValueObsolete(propertyMap, key);

                case CommandAccessorMap commandMap:
                    return GetCommandMapValueObsolete(commandMap, key);

                default:
                    throw new NotSupportedException();
            }
        }

        private AlternativeAttribute GetIndexerValueAlternative(object source, string key)
        {
            switch (source)
            {
                case PropertyMap propertyMap:
                    return GetPropertyMapValueAlternative(propertyMap, key);

                case CommandAccessorMap commandMap:
                    return GetCommandMapValueAlternative(commandMap, key);

                default:
                    throw new NotSupportedException();
            }
        }

        private Type GetIndexerValueType(object source, string key)
        {
            switch (source)
            {
                case PropertyMap propertyMap:
                    return GetPropertyMapValueType(propertyMap, key);

                case CommandAccessorMap commandMap:
                    return GetCommandMapValueType(commandMap, key);

                default:
                    throw new NotSupportedException();
            }
        }

        private object GetIndexerValue(object source, string key)
        {
            switch (source)
            {
                case PropertyMap propertyMap:
                    return GetPropertyMapValue(propertyMap, key);

                case CommandAccessorMap commandMap:
                    return GetCommandMapValue(commandMap, key);

                default:
                    throw new NotSupportedException();
            }
        }

        private ObsoleteAttribute GetPropertyMapValueObsolete(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            switch (node)
            {
                case PropertyMap _:
                case PropertyMapSource _:
                    return null;

                case PropertyMapObsolete propertyObsolete:
                    return propertyObsolete.Obsolete;

                default:
                    throw new NotSupportedException();
            }
        }

        private AlternativeAttribute GetPropertyMapValueAlternative(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            switch (node)
            {
                case PropertyMap _:
                case PropertyMapSource _:
                    return null;

                case PropertyMapObsolete propertyObsolete:
                    return propertyObsolete.Alternative;

                default:
                    throw new NotSupportedException();
            }
        }

        private Type GetPropertyMapValueType(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            switch (node)
            {
                case PropertyMap _:
                    return typeof(PropertyMap);

                case PropertyMapSource propertySource:
                    return propertySource.PropertyInfo.PropertyType;

                case PropertyMapObsolete propertyObsolete:
                    return propertyObsolete.PropertyType;

                default:
                    throw new NotSupportedException();
            }
        }

        private object GetPropertyMapValue(object source, string key)
        {
            var propertyMap = (PropertyMap)source;
            var node = propertyMap.GetNode(key);
            switch (node)
            {
                case PropertyMap propertyMap_:
                    return propertyMap_;

                case PropertyMapSource propertySource:
                    return propertySource.GetValue();

                case PropertyMapObsolete propertyObsolete:
                    return propertyObsolete.PropertyType.GetDefaultValue();

                default:
                    throw new NotSupportedException();
            }
        }


        private ObsoleteAttribute GetCommandMapValueObsolete(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            return commandMap.GetObsolete(key);
        }

        private AlternativeAttribute GetCommandMapValueAlternative(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            return commandMap.GetAlternative(key);
        }

        private Type GetCommandMapValueType(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            var command = commandMap.GetCommand(key);
            return command.GetType();
        }

        private object GetCommandMapValue(object source, string key)
        {
            var commandMap = (CommandAccessorMap)source;
            return commandMap[key];
        }
    }



    /// <summary>
    /// リフレクションのメンバー情報
    /// </summary>
    public class ScriptMemberInfo
    {
        public ScriptMemberInfo(PropertyInfo propertyInfo)
        {
            Type = ScriptMemberInfoType.Property;
            PropertyInfo = propertyInfo;
        }

        public ScriptMemberInfo(MethodInfo methodInfo)
        {
            Type = ScriptMemberInfoType.Method;
            MethodInfo = methodInfo;
        }

        public ScriptMemberInfo(string key)
        {
            Type = ScriptMemberInfoType.IndexKey;
            IndexKey = key;
        }


        public ScriptMemberInfoType Type { get; }
        public PropertyInfo PropertyInfo { get; }
        public MethodInfo MethodInfo { get; }
        public string IndexKey { get; }
    }



    /// <summary>
    /// リフレクションのメンバー情報の種類
    /// </summary>
    public enum ScriptMemberInfoType
    {
        None,
        Property,
        Method,
        IndexKey,
    }
}
