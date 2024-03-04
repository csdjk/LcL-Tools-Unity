
using System.CodeDom.Compiler;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
namespace LcLTools
{
    public class FolderTextField : TextField
    {
        private readonly Button _button;
        // private string _folderPath;


        public FolderTextField(string label) : this()
        {
            this.label = label;
        }
        public FolderTextField(string label, string defaultPath) : this(label)
        {
            value = defaultPath;
        }

        public FolderTextField() : base()
        {
            _button = new Button();
            _button.text = "...";
            _button.clicked += OnButtonClicked;
            Add(_button);
            style.width = 250;
            style.alignSelf = Align.Center;
            style.marginTop = 10;
            style.marginBottom = 10;
            style.marginLeft = 10;
            style.marginRight = 10;
            style.justifyContent = Justify.SpaceBetween;
        }

        private void OnButtonClicked()
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                value = path;
            }
        }
    }
}