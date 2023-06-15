using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;

namespace LcLTools
{

    public class TextFieldList : BaseField<List<string>>
    {
        public static StyleSheet styleSheet => LcLEditorUtilities.GetStyleSheet("UIElementsExtend");
        private const string ussContainer = "text-field-list-container";
        private const string ussTitle = "text-field-list-title";
        private const string ussItem = "text-field-list-item";
        private const string ussButtonContainer = "text-field-list-button-container";
        private const string ussButton = "text-field-list-button";

        string title;
        List<string> contentList;
        private TextField selectTextField;

        // ---------------------------------------------------------
        public TextFieldList(string title, List<string> contentList) : base(null, null)
        {
            styleSheets.Add(styleSheet);
            this.AddToClassList(ussContainer);
            this.contentList = contentList;
            this.title = title;
            this.Init();
        }

        public TextFieldList(string title, string[] contentList) : this(title, contentList.ToList())
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

            var folderBox = new VisualElement();
            folderBox.style.flexDirection = FlexDirection.Row;
            folderBox.style.alignItems = Align.Center;
            for (int i = 0; i < contentList.Count; i++)
            {
                contentList[i] = contentList[i].Trim();
                folderBox.Add(CreateExcludeFolderTextField(contentList[i], i));
            }
            scrollView.Add(folderBox);


            var buttonBox = new VisualElement();
            buttonBox.AddToClassList(ussButtonContainer);
            this.Add(buttonBox);

            // 创建增加排除文件夹的按钮
            var addButton = new Button();
            buttonBox.AddToClassList(ussButton);
            addButton.text = "+";
            addButton.RegisterCallback<ClickEvent>(evt =>
            {
                contentList.Add("");
                folderBox.Add(CreateExcludeFolderTextField("", contentList.Count - 1));
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
                    var index = folderBox.IndexOf(selectTextField);
                    if (index >= 0)
                    {
                        folderBox.RemoveAt(index);
                        contentList.RemoveAt(index);
                        // 赋值当前最后一个输入框
                        if (folderBox.childCount > 0)
                        {
                            selectTextField = folderBox.ElementAt(folderBox.childCount - 1) as TextField;
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

        private TextField CreateExcludeFolderTextField(string excludeFolder, int i)
        {
            var label = new TextField();
            label.AddToClassList(ussItem);
            label.value = excludeFolder;
            // 注册值改变事件
            var index = i;
            label.RegisterValueChangedCallback(evt =>
            {
                var value = evt.newValue.Trim();
                contentList[index] = value;
                (evt.currentTarget as TextField).value = value;
                SendEvent();
            });
            // 注册焦点事件
            label.RegisterCallback<FocusInEvent>(evt =>
            {
                selectTextField = (evt.currentTarget as TextField);
            });
            selectTextField = label;
            return label;


        }

        public void SendEvent()
        {
            using (var changeEvent = ChangeEvent<List<string>>.GetPooled(contentList, contentList))
            {
                changeEvent.target = this;
                this.SendEvent(changeEvent);
            }
            this.SetValueWithoutNotify(contentList);
        }

        public override void SetValueWithoutNotify(List<string> list)
        {
            base.SetValueWithoutNotify(list);
        }
    }
}
