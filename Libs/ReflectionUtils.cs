using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReflectionUtils
{
    static BindingFlags m_BindingFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    static BindingFlags m_BindingFlagsStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    static BindingFlags m_BindingFlagsInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    // 缓存
    private static Dictionary<string, Type> m_TypeCache = new Dictionary<string, Type>();
    private static Dictionary<string, MethodInfo> m_MethodCache = new Dictionary<string, MethodInfo>();
    private static Dictionary<string, FieldInfo> m_FieldCache = new Dictionary<string, FieldInfo>();
    private static Dictionary<string, PropertyInfo> m_PropertyCache = new Dictionary<string, PropertyInfo>();
    private static Dictionary<object, Type> m_ObjectToTypeCache = new Dictionary<object, Type>();
    private static Dictionary<Type, Dictionary<string, MethodInfo>> m_TypeToMethodCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();

    public static Type GetTypeCache(this object target)
    {
        if (m_ObjectToTypeCache.ContainsKey(target))
        {
            return m_ObjectToTypeCache[target];
        }
        var type = target.GetType();
        m_ObjectToTypeCache[target] = type;
        return type;
    }

    public static MethodInfo GetMethodCache(this Type type, string methodName,
    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
    {
        if (m_TypeToMethodCache.ContainsKey(type) && m_TypeToMethodCache[type].ContainsKey(methodName))
        {
            return m_TypeToMethodCache[type][methodName];
        }

        var method = type.GetMethod(methodName, bindingFlags);

        if (!m_TypeToMethodCache.ContainsKey(type))
        {
            m_TypeToMethodCache[type] = new Dictionary<string, MethodInfo>();
        }

        m_TypeToMethodCache[type][methodName] = method;

        return method;
    }


    /// <summary>Copy the fields from one object to another</summary>
    /// <param name="src">The source object to copy from</param>
    /// <param name="dst">The destination object to copy to</param>
    /// <param name="bindingAttr">The mask to filter the attributes.
    /// Only those fields that get caught in the filter will be copied</param>
    public static void CopyFields(
        System.Object src, System.Object dst,
        System.Reflection.BindingFlags bindingAttr
            = System.Reflection.BindingFlags.Public
              | System.Reflection.BindingFlags.NonPublic
              | System.Reflection.BindingFlags.Instance)
    {
        if (src != null && dst != null)
        {
            Type type = src.GetType();
            FieldInfo[] fields = type.GetFields(bindingAttr);
            for (int i = 0; i < fields.Length; ++i)
                if (!fields[i].IsStatic)
                    fields[i].SetValue(dst, fields[i].GetValue(src));
        }
    }

    /// <summary>Search the assembly for all types that match a predicate</summary>
    /// <param name="assembly">The assembly to search</param>
    /// <param name="predicate">The type to look for</param>
    /// <returns>A list of types found in the assembly that inherit from the predicate</returns>
    public static IEnumerable<Type> GetTypesInAssembly(
        Assembly assembly, Predicate<Type> predicate)
    {
        if (assembly == null)
            return null;

        Type[] types = new Type[0];
        try
        {
            types = assembly.GetTypes();
        }
        catch (Exception)
        {
            // Can't load the types in this assembly
        }

        types = (from t in types
                 where t != null && predicate(t)
                 select t).ToArray();
        return types;
    }

    /// <summary>
    /// Finds a type by full name
    /// </summary>
    /// <param name="name">The full type name with namespace</param>
    /// <returns>The found type</returns>
    public static Type FindTypeByName(string name)
    {
        if (m_TypeCache.ContainsKey(name))
        {
            return m_TypeCache[name];
        }

        var type = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(name))
            .FirstOrDefault(tt => tt != null);


        if (type == null)
        {
            throw new InvalidOperationException("Type not found");
        }
        m_TypeCache[name] = type;

        return type;
    }

    /// <summary>
    /// Calls a private method from a class
    /// </summary>
    /// <param name="methodName">The method name</param>
    /// <param name="args">The arguments to pass to the method</param>
    public static object InvokeStatic(this Type targetType, string methodName, params object[] args)
    {
        if (targetType == null)
        {
            throw new ArgumentNullException(nameof(targetType), "Invalid Type");
        }

        if (string.IsNullOrEmpty(methodName))
        {
            throw new ArgumentException("The methodName to set could not be null or empty", nameof(methodName));
        }
        string key = targetType.FullName + "." + methodName;
        if (!m_MethodCache.ContainsKey(key))
        {
            var mi = targetType.GetMethodCache(methodName, m_BindingFlagsStatic);
            if (mi == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' not found in type '{targetType.FullName}'.");
            }
            m_MethodCache[key] = mi;
        }

        return m_MethodCache[key].Invoke(null, args);
    }

    /// <summary>
    /// Calls a private method from a class
    /// </summary>
    /// <param name="methodName">The method name</param>
    /// <param name="args">The arguments to pass to the method</param>
    public static object Invoke(this object target, string methodName, params object[] args)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target), "The target could not be null");
        }
        var mi = target.GetTypeCache().GetMethodCache(methodName, m_BindingFlags);
        return mi.Invoke(target, args);
    }

    private static FieldInfo FindField(this Type type, string fieldName)
    {
        string key = type.FullName + "." + fieldName;
        if (!m_FieldCache.ContainsKey(key))
        {
            FieldInfo fi = null;

            while (type != null)
            {
                fi = type.GetField(fieldName, m_BindingFlags);

                if (fi != null) break;

                type = type.BaseType;
            }

            if (fi == null)
            {
                throw new InvalidOperationException($"Field '{fieldName}' not found in type '{type.FullName}'.");
            }

            m_FieldCache[key] = fi;
        }

        return m_FieldCache[key];
    }

    private static PropertyInfo FindProperty(this Type type, string propertyName)
    {
        string key = type.FullName + "." + propertyName;
        if (!m_PropertyCache.ContainsKey(key))
        {
            PropertyInfo pi = null;

            while (type != null)
            {
                pi = type.GetProperty(propertyName, m_BindingFlags);

                if (pi != null) break;

                type = type.BaseType;
            }

            if (pi == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' not found in type '{type.FullName}'.");
            }
            m_PropertyCache[key] = pi;
        }

        return m_PropertyCache[key];
    }


    /// <summary>
    /// Sets a private field from a class
    /// </summary>
    /// <param name="fieldName">The field to change</param>
    /// <param name="value">The new value</param>
    public static void SetField(this object target, string fieldName, object value)
    {
        target.GetTypeCache().FindField(fieldName).SetValue(target, value);
    }

    /// <summary>
    /// Gets the value of a private field from a class
    /// </summary>
    /// <param name="fieldName">The field to get</param>
    public static object GetField(this object target, string fieldName)
    {
        return target.GetTypeCache().FindField(fieldName).GetValue(target);
    }

    /// <summary>
    /// Gets all the fields from a class
    /// </summary>
    public static IEnumerable<FieldInfo> GetFields(this object target)
    {
        return target.GetTypeCache().GetFields(m_BindingFlagsInstance).OrderBy(t => t.MetadataToken);
    }

    public static void SetProperty(this object target, string propertyName, object value)
    {
        target.GetTypeCache().FindProperty(propertyName).SetValue(target, value);
    }

    public static object GetProperty(this object target, string propertyName)
    {
        return target.GetTypeCache().FindProperty(propertyName).GetValue(target);
    }

    /// <summary>
    /// Gets all the properties from a class
    /// </summary>
    public static IEnumerable<PropertyInfo> GetProperties(this object target)
    {
        return target.GetTypeCache().GetProperties(m_BindingFlagsInstance).OrderBy(t => t.MetadataToken);
    }
}
