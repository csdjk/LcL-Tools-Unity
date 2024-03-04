using UnityEditor;
using UnityEngine;
/// <summary>
/// 自定义灯光调节工具
/// </summary>
internal class LightDirDrawer : MaterialPropertyDrawer
{
    float m_Height = 16;
    bool m_IsEditor = false;
    bool m_StarEditor = true;
    GameObject m_SelectGameObj;
    public Quaternion rot = Quaternion.identity;
    MaterialProperty m_Prop;
    static bool IsPropertyTypeSuitable(MaterialProperty prop)
    {
        return prop.type == MaterialProperty.PropType.Vector;
    }
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        //如果不是Vector类型，则把unity的默认警告框的高度40
        if (!IsPropertyTypeSuitable(prop))
        {
            return 40f;
        }
        m_Height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, new GUIContent(label));
        return m_Height;
    }
    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        //如果不是Vector类型，则显示一个警告框
        if (!IsPropertyTypeSuitable(prop))
        {
            GUIContent c = EditorGUIUtility.TrTextContent("LightDir used on a non-Vector property: " + prop.name, EditorGUIUtility.IconContent("console.erroricon").image);
            EditorGUI.LabelField(position, c, EditorStyles.helpBox);
            return;
        }

        EditorGUI.BeginChangeCheck();

        float oldLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 0f;

        Color oldColor = GUI.color;
        if (m_IsEditor) GUI.color = Color.green;

        //绘制属性
        Rect VectorRect = new Rect(position)
        {
            width = position.width - 68f
        };
        Vector3 value = EditorGUI.Vector4Field(VectorRect, label, prop.vectorValue);
        //绘制开关
        Rect ToggleRect = new Rect(position)
        {
            x = position.xMax - 64f,
            y = position.y,
            width = 60f,
            height = 18
        };
        m_IsEditor = GUI.Toggle(ToggleRect, m_IsEditor, "Edit", "Button");
        if (m_IsEditor)
        {
            if (m_StarEditor)
            {
                m_Prop = prop;
                InitSceneGUI(value);
            }
        }
        else
        {
            if (!m_StarEditor)
            {
                ClearSceneGUI();
            }
        }

        GUI.color = oldColor;
        EditorGUIUtility.labelWidth = oldLabelWidth;
        if (EditorGUI.EndChangeCheck())
        {
            prop.vectorValue = new Vector4(value.x, value.y, value.z);
        }

    }
    void InitSceneGUI(Vector3 value)
    {
        Tools.current = Tool.None;
        m_SelectGameObj = Selection.activeGameObject;
        if (m_SelectGameObj == null)
        {
            return;
        }
        Vector3 worldDir = m_SelectGameObj.transform.rotation * value;
        rot = Quaternion.FromToRotation(Vector3.forward, worldDir);
        SceneView.duringSceneGui += OnSceneGUI;
        m_StarEditor = false;
    }
    void ClearSceneGUI()
    {

        SceneView.duringSceneGui -= OnSceneGUI;
        m_Prop = null;
        m_SelectGameObj = null;
        m_StarEditor = true;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeGameObject != m_SelectGameObj)
        {
            ClearSceneGUI();
            m_IsEditor = false;
            return;
        }

        Vector3 pos = m_SelectGameObj.transform.position;

        rot = Handles.RotationHandle(rot, pos);
        Vector3 newLocalDir = Quaternion.Inverse(m_SelectGameObj.transform.rotation) * rot * Vector3.forward;

        m_Prop.vectorValue = new Vector4(newLocalDir.x, newLocalDir.y, newLocalDir.z);

        Handles.color = Color.green;
        Handles.ConeHandleCap(0, pos, rot, HandleUtility.GetHandleSize(pos), EventType.Repaint);
    }
}
