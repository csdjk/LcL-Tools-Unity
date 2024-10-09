using System;
using System.Reflection;

public abstract class ReflectionBase
{
    private object m_Target;
    private Type m_TargetType;
    public static Type staticType;

    public ReflectionBase(Type type)
    {
        m_TargetType = type;
        m_Target = Activator.CreateInstance(m_TargetType, true);
        staticType = m_TargetType;
    }

    /// <summary>
    /// 创建指定类型的实例
    /// </summary>
    /// <param name="typeName">"Namespace.Type, Assembly"。如："UnityEditor.GameView,UnityEditor"</param>
    public ReflectionBase(string typeName)
    {
        m_TargetType = Type.GetType(typeName);
        if (m_TargetType != null)
        {
            staticType = m_TargetType;
            m_Target = Activator.CreateInstance(m_TargetType, true);
        }
    }

    public ReflectionBase(object obj)
    {
        m_Target = obj;
        m_TargetType = m_Target.GetType();
        staticType = m_TargetType;
    }


    public object GetProperty(string propertyName)
    {
        return ReflectionUtils.GetProperty(m_Target, propertyName);
    }

    public void SetProperty(string propertyName, object value)
    {
        ReflectionUtils.SetProperty(m_Target, propertyName, value);
    }

    public object GetField(string fieldName)
    {
        return ReflectionUtils.GetField(m_Target, fieldName);
    }

    public void SetField(string fieldName, object value)
    {
        ReflectionUtils.SetField(m_Target, fieldName, value);
    }

    public object InvokeMethod(string methodName, params object[] parameters)
    {
        return ReflectionUtils.Invoke(m_Target, methodName, parameters);
    }

}
