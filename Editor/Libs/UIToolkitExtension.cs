using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace LcLTools
{
    public static class UIToolkitExtension
    {
        public static T CreateInstanceInResource<T>(this object obj, string fileName = "", string path = "") where T : ScriptableObject
        {
            //Create an instance of the scriptable object
            var scriptableObject = ScriptableObject.CreateInstance<T>();

            // Get source path
            var resourcesFolder = "Assets/Resources" + (string.IsNullOrEmpty(path) ? "" : $"/{path}");
            var fullResFolder = Application.dataPath + resourcesFolder.Replace("Assets", "");
            if (!Directory.Exists(fullResFolder))
                Directory.CreateDirectory(fullResFolder);

            var sourcePath = $"{resourcesFolder}/{(string.IsNullOrEmpty(fileName) ? typeof(T).Name : fileName)}.asset";

            //Create the asset 
            AssetDatabase.CreateAsset(scriptableObject, sourcePath);
            return scriptableObject;
        }

        public static void SetActive(this VisualElement target, bool value)
        {
            var visibility = target.style.visibility;
            visibility.value = value ? Visibility.Visible : Visibility.Hidden;
            target.style.visibility = visibility;
        }

        public static VisualElement CreateUIElementInspector(this UnityEngine.Object obj, params string[] propertiesToExclude)
        {
            var serializedObject = new SerializedObject(obj);

            return serializedObject.CreateUIElementInspector(propertiesToExclude);
        }

        public static VisualElement CreateUIElementInspector(this SerializedObject serializedObject, params string[] propertiesToExclude)
        {
            var container = new VisualElement();

            var fields = GetVisibleSerializedFields(serializedObject.targetObject.GetType());


            foreach (var field in fields)
            {
                // Do not draw HideInInspector fields or excluded properties.
                if (propertiesToExclude != null && propertiesToExclude.Contains(field.Name))
                {
                    continue;
                }

                var serializedProperty = serializedObject.FindProperty(field.Name);

                var box = new Box();
                box.style.marginTop = 5;
                box.style.marginBottom = 5;
                box.style.marginLeft = 5;
                box.style.marginRight = 5;

                var header = field.GetCustomAttribute<HeaderAttribute>();
                if (header != null)
                {
                    var label = new Label(header.header);
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    box.Add(label);
                }

                var propertyField = new PropertyField(serializedProperty);
                if (serializedProperty.isArray)
                {
                    // PropertyField是异步创建的，所以需要等待创建完成后再处理
                    propertyField.RegisterCallback<GeometryChangedEvent>(evt =>
                    {
                        var label = propertyField.Q<Foldout>()?.Q<Label>();
                        if (label != null)
                        {
                            var rename = field.GetCustomAttribute<RenameAttribute>();
                            label.text = rename?.name ?? label.text;
                        }
                    });
                }
                box.Add(propertyField);


                var tooltip = field.GetCustomAttribute<TooltipAttribute>();
                if (tooltip != null)
                {
                    propertyField.tooltip = tooltip.tooltip;
                }

                container.Add(box);
            }

            container.Bind(serializedObject);

            return container;
        }

#if UNITY_2022_1_OR_NEWER
    public static VisualElement CreateUIElementInspector(this SerializedProperty serializedProperty, params string[] propertiesToExclude)
    {
        var container = new VisualElement();

        var fields = GetVisibleSerializedFields(serializedProperty.boxedValue.GetType());

        foreach (var field in fields)
        {
            // Do not draw HideInInspector fields or excluded properties.
            if (propertiesToExclude != null && propertiesToExclude.Contains(field.Name))
            {
                continue;
            }

            //Debug.Log(field.Name);
            var propertyRelative = serializedProperty.FindPropertyRelative(field.Name);

            var propertyField = new PropertyField(propertyRelative);

            container.Add(propertyField);
        }

        return container;
    }
#endif

        /// <summary>
        /// Returns -1 if the property is not inside an array, otherwise returns its index inside the array
        /// </summary>
        public static int GetIndexInArray(this SerializedProperty property)
        {
            if (!property.IsArrayElement())
                return -1;
            int startIndex = property.propertyPath.LastIndexOf('[') + 1;
            int length = property.propertyPath.LastIndexOf(']') - startIndex;
            return int.Parse(property.propertyPath.Substring(startIndex, length));
        }

        /// <summary>Returns TRUE if this property is inside an array</summary>
        public static bool IsArrayElement(this SerializedProperty property) => property.propertyPath.Contains("Array");

        private static IEnumerable<FieldInfo> GetVisibleSerializedFields(Type T)
        {
            var publicFields = T.GetFields(BindingFlags.Instance | BindingFlags.Public);

            var infoFields = publicFields.Where(t => t.GetCustomAttribute<HideInInspector>() == null).ToList();

            var privateFields = T.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            infoFields.AddRange(privateFields.Where(t => t.GetCustomAttribute<SerializeField>() != null));

            return infoFields;
        }

        
        /// ================================ ListView Extension ================================
        public static Button GetAddButton(this ListView listView)
        {
            return listView.Q<Button>(name: "unity-list-view__add-button");
        }
        /// <summary>
        /// 注册添加按钮的onClick事件
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="onClick"></param>
        public static void RegisterAddButtonOnClick(this ListView listView, Action onClick)
        {
            var addButton = listView.GetAddButton();
            if (addButton != null)
            {
                addButton.clickable = new Clickable(onClick);
            }
        }

        public static Button GetRemoveButton(this ListView listView)
        {
            return listView.Q<Button>(name: "unity-list-view__remove-button");
        }
        /// <summary>
        /// 注册删除按钮的onClick事件
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="onClick"></param>
        public static void RegisterRemoveButtonOnClick(this ListView listView, Action onClick)
        {
            var removeButton = listView.GetRemoveButton();
            if (removeButton != null)
            {
                removeButton.clickable = new Clickable(onClick);
            }
        }
        /// <summary>
        /// 注册删除按钮的onClick事件
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="onClick"></param>
        public static void RegisterRemoveButtonOnClick(this ListView listView, Action<int> onClick)
        {
            var removeButton = listView.GetRemoveButton();
            if (removeButton != null)
            {
                removeButton.clickable = new Clickable(() =>
                {
                    var index = listView.selectedIndex;
                    if (index >= 0)
                    {
                        onClick(index);
                    }
                });
            }
        }



        /// ================================ Animation ================================
        /// 例如USS：
        // .warn {
        //     border-width: 1px;
        //     border-color: transparent;
        //     transition-property: border-color;
        //     transition-timing-function: ease-out;
        //     transition-duration: 0.5s;
        //     transition-delay: 0.2s;
        // }
        // .warn.animate {
        //     border-color: red;
        // }
        public static void DelayAddToClassList(this VisualElement ui, string classToAdd = "animate", int delay = 100)
        {
            ui.schedule.Execute(() => ui.AddToClassList(classToAdd)).StartingIn(delay);
        }
    }

}