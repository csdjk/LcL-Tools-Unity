using System;
using System.Reflection;

public class ReflectionUtil
{
    private object target;
    private Type targetType;

    public ReflectionUtil(object target)
    {
        this.target = target;
        this.targetType = target.GetType();
    }

    /// <summary>
    /// 创建指定类型的实例
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static ReflectionUtil CreateInstance(string assemblyName, string typeName)
    {
        // 加载包含MyClass的程序集
        Assembly assembly = Assembly.Load(assemblyName);

        // 获取MyClass的Type对象
        Type type = assembly.GetType(typeName);

        // 创建MyClass的实例
        object instance = Activator.CreateInstance(type, true);

        // 创建并返回ReflectionUtil对象
        return new ReflectionUtil(instance);
    }

    public object GetProperty(string propertyName)
    {
        PropertyInfo property = targetType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(target);
    }

    public void SetProperty(string propertyName, object value)
    {
        PropertyInfo property = targetType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        property?.SetValue(target, value);
    }

    public object GetField(string fieldName)
    {
        FieldInfo field = targetType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(target);
    }

    public void SetField(string fieldName, object value)
    {
        FieldInfo field = targetType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    public object InvokeMethod(string methodName, params object[] parameters)
    {
        MethodInfo method = targetType.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return method?.Invoke(target, parameters);
    }


     public static object GetStaticField(Type type, string fieldName)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        return field?.GetValue(null);
    }

    public static void SetStaticField(Type type, string fieldName, object value)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        field?.SetValue(null, value);
    }

     public static object GetStaticProperty(Type type, string propertyName)
    {
        PropertyInfo property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        return property?.GetValue(null);
    }

    public static void SetStaticProperty(Type type, string propertyName, object value)
    {
        PropertyInfo property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        property?.SetValue(null, value);
    }

    public static object InvokeStaticMethod(Type type, string methodName, params object[] parameters)
    {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        return method?.Invoke(null, parameters);
    }
}
