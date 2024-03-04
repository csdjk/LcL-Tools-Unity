
using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using UnityEditor.UIElements;
namespace LcLTools
{
    public class ObjectFieldList<T> : BaseField<List<T>> where T : UnityEngine.Object
    {
        public static StyleSheet styleSheet => LcLEditorUtilities.GetStyleSheet("UIElementsExtend");
        private const string ussContainer = "object-field-list-container";
        private const string ussTitle = "object-field-list-title";
        private const string ussItem = "object-field-list-item";
        private const string ussButtonContainer = "object-field-list-button-container";
        private const string ussButton = "object-field-list-button";

        string title;
        List<T> contentList;
        private ObjectField selectTextField;
        public Type objectType;

        // ---------------------------------------------------------
        // 把UnityEngine.Object类型改成泛型T 


        public ObjectFieldList(string title, List<T> contentList) : base(null, null)
        {
            styleSheets.Add(styleSheet);
            this.AddToClassList(ussContainer);
            this.contentList = contentList ?? new List<T>();
            this.title = title;
            this.Init();
        }
        public ObjectFieldList(List<T> contentList) : this("", contentList)
        {
        }

        public ObjectFieldList(T[] contentList) : this("", contentList.ToList())
        {
        }

        public ObjectFieldList(string title, T[] contentList) : this(title, contentList.ToList())
        {
        }

        void Init()
        {
            this.Clear();
            var titleLabel = new Label(title);
            titleLabel.AddToClassList(ussTitle);
            this.Add(titleLabel);

            var scrollView = new ScrollView();
            this.Add(scrollView);

            var listBox = new VisualElement();
            listBox.style.flexDirection = FlexDirection.Row;
            listBox.style.alignItems = Align.Center;
            for (int i = 0; i < contentList.Count; i++)
            {
                listBox.Add(CreateObjectField(contentList[i], i));
            }
            scrollView.Add(listBox);


            var buttonBox = new VisualElement();
            buttonBox.AddToClassList(ussButtonContainer);
            this.Add(buttonBox);

            var addButton = new Button();
            buttonBox.AddToClassList(ussButton);
            addButton.text = "+";
            addButton.RegisterCallback<ClickEvent>(evt =>
            {
                contentList.Add(null);
                listBox.Add(CreateObjectField(null, contentList.Count - 1));
                SendEvent();
            });
            buttonBox.Add(addButton);

            var deleteButton = new Button();
            deleteButton.AddToClassList(ussButton);
            deleteButton.text = "-";
            deleteButton.RegisterCallback<ClickEvent>(evt =>
            {
                if (selectTextField != null)
                {
                    var index = listBox.IndexOf(selectTextField);
                    if (index >= 0)
                    {
                        listBox.RemoveAt(index);
                        contentList.RemoveAt(index);
                        // 赋值当前最后一个输入框
                        if (listBox.childCount > 0)
                        {
                            selectTextField = listBox.ElementAt(listBox.childCount - 1) as ObjectField;
                            SendEvent();
                        }
                        else
                        {
                            selectTextField = null;
                        }
                    }
                }
            });
            buttonBox.Add(deleteButton);
        }

        private ObjectField CreateObjectField(T obj, int i)
        {
            var item = new ObjectField();
            item.AddToClassList(ussItem);
            item.value = obj;
            item.objectType = typeof(T);
            // 注册值改变事件
            var index = i;
            item.RegisterValueChangedCallback(evt =>
            {
                contentList[index] = evt.newValue as T;
                SendEvent();
            });
            // 注册焦点事件
            item.RegisterCallback<FocusInEvent>(evt =>
            {
                selectTextField = (evt.currentTarget as ObjectField);
            });
            selectTextField = item;
            return item;
        }

        public void SendEvent()
        {
            using (var changeEvent = ChangeEvent<List<T>>.GetPooled(contentList, contentList))
            {
                changeEvent.target = this;
                this.SendEvent(changeEvent);
            }
            this.SetValueWithoutNotify(contentList);
        }

        public override void SetValueWithoutNotify(List<T> list)
        {
            base.SetValueWithoutNotify(list);
        }
    }
}