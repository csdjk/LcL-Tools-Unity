
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace LcLTools
{
    public class CheckShaderProperties : EditorWindow
    {
        private TextField propertiesTextField;
        private TextField cbufferTextField;
        private TextField tipsTextField;
        private List<string> missingProperties = new List<string>();


        [MenuItem("LcLTools/检测Shader属性")]
        private static void ShowWindow()
        {
            var window = GetWindow<CheckShaderProperties>();
            window.titleContent = new GUIContent("CheckShaderProperties");
            window.minSize = new Vector2(400, 200);
            window.position = new Rect(500, 300, 400, 400);
            window.Show();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var title = new Label("检测Shader的Properties是否缺少CBUFFER里定义的属性\n(如果缺少的话会导致SRP Batch在移动端失效！！！)"){
                style = {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    flexDirection = FlexDirection.Column,
                    marginBottom = 10,
                    marginTop = 10,
                }
            };
            root.Add(title);
            // 创建左侧的 Properties 面板
            var scrollView = new ScrollView();
            propertiesTextField = new TextField("Properties")
            {
                style = {
                 unityTextAlign = TextAnchor.MiddleCenter,
                 flexDirection = FlexDirection.Column,
                 height = Length.Percent(100),
            }
            };
            propertiesTextField.multiline = true;
            scrollView.Add(propertiesTextField);

            // 创建右侧的 CBUFFER 面板
            cbufferTextField = new TextField("CBUFFER")
            {
                style = {
                 unityTextAlign = TextAnchor.MiddleCenter,
                 flexDirection = FlexDirection.Column,
                 height = Length.Percent(100),
             },
                multiline = true
            };

            // 创建 TwoPaneSplitView
            var twoPaneSplitView = new TwoPaneSplitView(0, 210f, TwoPaneSplitViewOrientation.Horizontal);
            twoPaneSplitView.Add(propertiesTextField);
            twoPaneSplitView.Add(cbufferTextField);

            tipsTextField = new TextField("Properties缺少的属性")
            {
                style = {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    flexDirection = FlexDirection.Column,
                }
            };

            // 创建 Check Properties 按钮
            var checkPropertiesButton = new Button(CheckProperties)
            {
                text = "Check Properties"
            };

            rootVisualElement.Add(twoPaneSplitView);
            rootVisualElement.Add(tipsTextField);
            rootVisualElement.Add(checkPropertiesButton);
        }

        private void CheckProperties()
        {

            missingProperties.Clear();
            // 定义正则表达式，用于匹配变量名和类型
            Regex regex = new Regex(@"(\w+)\s+(\w+);");

            // 使用正则表达式匹配变量名和类型，并输出结果
            MatchCollection matches = regex.Matches(cbufferTextField.value);
            foreach (Match match in matches)
            {
                string type = match.Groups[1].Value;
                string name = match.Groups[2].Value;
                if (!propertiesTextField.value.Contains(name))
                {
                    type = type.Replace("half", "float");
                    string propertyType = "";
                    string propertyValue = "";
                    if (type == "float")
                    {
                        propertyType = "float";
                        propertyValue = "1";
                    }
                    else if (type == "float2" || type == "float3" || type == "float4")
                    {
                        propertyType = "Vector";
                        propertyValue = "(0,0,0,0)";
                    }
                    else
                    {
                        propertyType = type;
                    }

                    if (name.Contains("Color"))
                    {
                        propertyType = "Color";
                        propertyValue = "(1,1,1,1)";
                    }
                    string propertyDeclaration = $"[HideInInspector]{name} (\"{name}\", {propertyType}) = {propertyValue}";
                    missingProperties.Add(propertyDeclaration);
                }
            }

            // 将缺失的属性显示在 CBUFFER 面板下方
            tipsTextField.value = string.Join("\n", missingProperties);
        }
    }
}