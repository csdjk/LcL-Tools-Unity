using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace LcLTools
{

    public class OutlineTools : EditorWindow
    {
        [MenuItem("LcLTools/OutlineTools")]
        private static void ShowWindow()
        {
            var window = GetWindow<OutlineTools>();
            window.titleContent = new GUIContent("OutlineTools");
            window.Show();
        }
        List<Texture2D> textures = new List<Texture2D>();
        Texture2D prevImage;
        string path = "";
        bool overwrite;
        ComputeShader computeShader;
        int kernelHandle;
        RenderTexture outputRT;
        // RenderTexture msaaRT;
        int outlineWidth = 2;
        Color outlineColorTop = Color.red;
        Color outlineColorBottom = Color.green;
        Gradient gradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
                    {
                    new GradientColorKey(Color.red, 0),
                    new GradientColorKey(Color.green, 1),
                    },
            alphaKeys = new GradientAlphaKey[]
                    {
                    new GradientAlphaKey(1, 0),
                    new GradientAlphaKey(1, 1),
                    }
        };
        private void OnEnable()
        {
            computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Editor/SceneTools/OutlineTools/OutlineTools.compute");
            kernelHandle = computeShader.FindKernel("CSMain");

            // msaaRT = new RenderTexture(size.x, size.y, 24);
            // msaaRT.Create();

            outputRT = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true
            };
            outputRT.Create();


            Calculate();
        }
        private void OnDisable()
        {
            outputRT.Release();
            outputRT = null;
            // msaaRT.Release();
            // msaaRT = null;
        }
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var label = new Label("描边批处理")
            {
                style = {
                fontSize = 25,
                unityTextAlign = TextAnchor.MiddleCenter,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
            }
            };
            root.Add(label);

            // =================ListView=================
            Func<VisualElement> makeItem = () =>
             {
                 var box = new VisualElement()
                 {
                     style = {
                     flexDirection = FlexDirection.Row,
                     width = Length.Percent(100),
                     justifyContent = Justify.SpaceBetween,
                     }
                 };
                 var objectField = new ObjectField()
                 {
                     objectType = typeof(Texture2D),
                     style = {
                    flexGrow = 0.8f,
                     }
                 };
                 box.Add(objectField);

                 var button = new Button()
                 {
                     text = "预览",
                     style = {
                    flexGrow = 0.2f,
                     height = 20,
                     marginLeft = 5,
                     marginRight = 5,
                     }
                 };
                 box.Add(button);
                 return box;
             };
            Action<VisualElement, int> bindItem = (element, index) =>
            {
                element.parent.style.alignSelf = Align.Center;
                var box = element as VisualElement;
                var objectField = box[0] as ObjectField;
                objectField.value = textures[index];
                objectField.RegisterValueChangedCallback((evt) =>
                {
                    textures[index] = evt.newValue ? evt.newValue as Texture2D : null;
                });

                var button = box[1] as Button;
                button.clickable = new Clickable(() =>
                {
                    Process(textures[index]);
                });
            };

            var listView = new ListView(textures, 30, makeItem, bindItem)
            {
                selectionType = SelectionType.Multiple,
                showAddRemoveFooter = true,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
            };
            listView.showAddRemoveFooter = true;
            listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
            listView.showBoundCollectionSize = true;
            listView.selectionType = SelectionType.Single;

            bool dragObj = false;
            var dragTextures = new List<Texture2D>();
            listView.RegisterCallback<DragEnterEvent>((DragEnterEvent evt) =>
            {
                dragTextures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets).ToList();
                dragObj = dragTextures.Count > 0;
            });
            listView.RegisterCallback<DragUpdatedEvent>((DragUpdatedEvent evt) =>
            {
                DragAndDrop.visualMode = dragObj ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            });
            listView.RegisterCallback<DragPerformEvent>((DragPerformEvent evt) =>
            {
                if (dragObj)
                {
                    // 把拖拽的对象添加到列表中，并且如果
                    foreach (var texture in dragTextures)
                    {
                        if (!textures.Contains(texture))
                        {
                            textures.Add(texture);
                        }
                    }

                    listView.itemsSource = textures;
                    listView.Rebuild();
                }
            });

            root.Add(listView);

            // =================Image Preview=================
            var image = new Image
            {
                image = outputRT,
                style =
            {
                width = Length.Percent(95),
                height = 250,
                alignSelf = Align.Center,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
                borderBottomColor = Color.black,
                borderLeftColor = Color.black,
                borderRightColor = Color.black,
                borderTopColor = Color.black,
                borderLeftWidth = 2,
                borderRightWidth = 2,
                borderTopWidth = 2,
                borderBottomWidth = 2,
            }
            };
            root.Add(image);

            // ================= Outline Params =================
            var outlineWidth = new SliderInt("描边宽度", 1, 10)
            {
                value = 5,
                showInputField = true,
                style =
            {
                width = Length.Percent(95),
                alignSelf = Align.Center,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
            }
            };
            outlineWidth.RegisterValueChangedCallback((evt) =>
                {
                    this.outlineWidth = (int)evt.newValue;
                    Calculate();
                });
            root.Add(outlineWidth);

            // GradientField
            var gradientField = new GradientField("颜色")
            {
                value = gradient,
                style =
            {
                width = Length.Percent(95),
                alignSelf = Align.Center,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
            }
            };
            gradientField.RegisterValueChangedCallback((evt) =>
            {
                gradient = evt.newValue;
                outlineColorTop = gradient.colorKeys[0].color;
                outlineColorBottom = gradient.colorKeys[1].color;

                outlineColorTop.a = gradient.alphaKeys[0].alpha;
                outlineColorBottom.a = gradient.alphaKeys[1].alpha;

                Calculate();

            });
            root.Add(gradientField);


            var toggle = new Toggle("是否覆盖原图")
            {
                value = false,
                style =
            {
                width = Length.Percent(95),
                alignSelf = Align.Center,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
            }
            };
            toggle.RegisterValueChangedCallback((evt) =>
            {
                overwrite = evt.newValue;
            });
            root.Add(toggle);
            // =================Path=================
            var pathField = new FolderTextField("保存路径", Application.dataPath)
            {
                value = Application.dataPath,
                style =
            {
                width = Length.Percent(95),
                alignSelf = Align.Center,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
            }
            };
            path = Application.dataPath;
            pathField.RegisterValueChangedCallback((evt) =>
            {
                path = evt.newValue;
            });

            bool dragFolder = false;
            pathField.RegisterCallback<DragEnterEvent>((DragEnterEvent evt) =>
            {
                dragFolder = DragAndDrop.objectReferences[0] is DefaultAsset;
            });
            pathField.RegisterCallback<DragUpdatedEvent>((DragUpdatedEvent evt) =>
            {
                DragAndDrop.visualMode = dragFolder ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            });
            pathField.RegisterCallback<DragPerformEvent>((DragPerformEvent evt) =>
            {
                if (dragFolder)
                {
                    pathField.value = LcLUtility.AssetsRelativeToAbsolutePath(AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]));
                }
            });
            root.Add(pathField);


            // =================Button=================
            var button = new Button(() =>
            {
                ProcessAndSave(prevImage);
                AssetDatabase.Refresh();
            })
            {
                text = "保存",
                style =
            {
                width = 250,
                alignSelf = Align.Center,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
            }
            };
            root.Add(button);

            // =================Button=================
            var button2 = new Button(() =>
            {
                foreach (var texture in textures)
                {
                    ProcessAndSave(texture);
                }
                AssetDatabase.Refresh();
            })
            {
                text = "保存所有",
                style =
            {
                width = 250,
                alignSelf = Align.Center,
                marginTop = 10,
                marginBottom = 10,
                marginLeft = 10,
                marginRight = 10,
            }
            };
            root.Add(button2);

        }

        private void Calculate()
        {
            if (outputRT == null || computeShader == null || prevImage == null)
                return;

            ComputeBuffer buffer = new ComputeBuffer(gradient.colorKeys.Length, sizeof(float) * 5);
            buffer.SetData(gradient.colorKeys);
            ComputeBuffer buffer2 = new ComputeBuffer(gradient.alphaKeys.Length, sizeof(float) * 2);
            buffer2.SetData(gradient.alphaKeys);

            computeShader.SetBuffer(kernelHandle, "_ColorKeys", buffer);
            computeShader.SetInt("_ColorKeysLength", gradient.colorKeys.Length);

            computeShader.SetBuffer(kernelHandle, "_AlphaKeys", buffer2);
            computeShader.SetInt("_AlphaKeysLength", gradient.alphaKeys.Length);

            computeShader.SetInt("_OutlineWidth", outlineWidth);
            computeShader.SetTexture(kernelHandle, "Input", prevImage);
            computeShader.SetTexture(kernelHandle, "Result", outputRT);
            computeShader.Dispatch(kernelHandle, outputRT.width / 8, outputRT.height / 8, 1);

            buffer.Release();
            buffer2.Release();
        }


        private void Process(Texture2D texture)
        {
            if (texture == null) return;
            prevImage = texture;

            outputRT.Release();
            outputRT.width = prevImage.width;
            outputRT.height = prevImage.height;
            outputRT.format = RenderTextureFormat.ARGBFloat;
            Calculate();
        }


        private void ProcessAndSave(Texture2D texture)
        {
            if (texture == null) return;

            Process(texture);
            var texturePath = AssetDatabase.GetAssetPath(texture);

            string savePath = overwrite ? LcLEditorUtilities.AssetsRelativeToAbsolutePath(texturePath) : Path.Combine(path, texture.name + ".tga");

            LcLEditorUtilities.SaveRenderTextureToTexture(outputRT, savePath, TextureFormat.RGBA32);
            AssetDatabase.Refresh();


            TextureImporter sourceImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            TextureImporter targetImporter = AssetImporter.GetAtPath(LcLEditorUtilities.AssetsRelativePath(savePath)) as TextureImporter;
            // 复制设置
            if (sourceImporter != null && targetImporter != null)
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                sourceImporter.ReadTextureSettings(settings);
                targetImporter.SetTextureSettings(settings);
                // 应用更改
                AssetDatabase.ImportAsset(savePath);
            }

        }

    }

}

