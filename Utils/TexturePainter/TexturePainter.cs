//
//
// #if UNITY_EDITOR
// using UnityEngine;
// using UnityEditor;
// using System;
// using UnityEngine.Rendering;
// using UnityEngine.Experimental.Rendering;
// using System.Collections.Generic;
// using UnityEditor.SceneManagement;
// using UnityEngine.SceneManagement;
// using System.IO;
// namespace LcLTools
// {
//
//     public enum BlendModel
//     {
//         Add,
//         Sub,
//     }
//     public enum TexCoordChannel
//     {
//         UV1,
//         UV2,
//         UV3,
//     }
//     public enum DrawModel
//     {
//         BaseWorldPos,
//         BaseUV,
//     }
//
//     public enum PainterDrawChannel
//     {
//         R,
//         G,
//         B,
//         A,
//         RGB,
//         RGBA,
//     }
//
//
//     public enum PainterProfileId
//     {
//         DrawTexture,
//     }
//
//
//     public class TextureItem
//     {
//         public Texture texture;
//         // public string shaderPropName;
//         public string texturePath;
//         public Vector2Int textureSize;
//         public GraphicsFormat graphicsFormat;
//         // List<Material> referenceMaterials;
//         Dictionary<Material, string> m_ReferenceMaterials;
//         public TextureItem(Texture texture)
//         {
//             this.texture = texture;
//             // this.shaderPropName = shaderPropName;
//             texturePath = texture ? AssetDatabase.GetAssetPath(texture) : string.Empty;
//             textureSize = texture ? new Vector2Int(texture.width, texture.height) : Vector2Int.one * 1024;
//             graphicsFormat = texture ? texture.graphicsFormat : GraphicsFormat.R8G8B8A8_UNorm;
//             m_ReferenceMaterials = new Dictionary<Material, string>();
//         }
//
//         public void AddReferenceMaterials(Material mat, string propName)
//         {
//             m_ReferenceMaterials.TryAdd(mat, propName);
//         }
//
//         /// <summary>
//         ///  设置材质球的贴图
//         /// </summary>
//         /// <param name="texture"></param> <summary>
//         public void SetMaterialTexture(Texture texture)
//         {
//             if (texture == null) return;
//             foreach (var item in m_ReferenceMaterials)
//             {
//                 item.Key.SetTexture(item.Value, texture);
//             }
//         }
//         /// <summary>
//         ///  恢复材质球的贴图
//         /// </summary>
//         public void RestoreMaterialTexture()
//         {
//             foreach (var item in m_ReferenceMaterials)
//             {
//                 item.Key.SetTexture(item.Value, texture);
//             }
//         }
//     }
//
//     [ExecuteAlways, DisallowMultipleComponent]
//     public class TexturePainter : MonoBehaviour
//     {
//         public Texture2D targetTexture;
//         BlendModel m_BlendModel = BlendModel.Add;
//         public BlendModel BlendModel
//         {
//             get
//             {
//                 return m_BlendModel;
//             }
//             set
//             {
//                 m_BlendModel = value;
//             }
//         }
//
//         public DrawModel drawModel = DrawModel.BaseWorldPos;
//         public TexCoordChannel texcoordChannel = TexCoordChannel.UV1;
//
//         public PainterDrawChannel drawChannel = PainterDrawChannel.A;
//
//         public Color brushColor = Color.red;
//         [Range(0, 1)]
//         public float brushSize = 0.2f;
//         public float brushStrength = 1.0f;
//         public float brushHardness = 0.5f;
//
//         private Material m_DrawMaterial;
//         private Material DrawMaterial
//         {
//             get
//             {
//                 if (m_DrawMaterial == null)
//                     m_DrawMaterial = new Material(Shader.Find("Hidden/TexturePainter/PaintShader"));
//                 return m_DrawMaterial;
//             }
//             set
//             {
//                 m_DrawMaterial = value;
//             }
//         }
//         private Material m_LandMarkMaterial;
//         private Material LandMarkMaterial
//         {
//             get
//             {
//                 if (m_LandMarkMaterial == null)
//                     m_LandMarkMaterial = new Material(Shader.Find("Hidden/TexturePainter/MarkIlsands"));
//                 return m_LandMarkMaterial;
//             }
//             set
//             {
//                 m_LandMarkMaterial = value;
//             }
//         }
//
//         private RenderTexture m_CurrentTexture;
//         public RenderTexture CurrentTexture
//         {
//             get
//             {
//                 if (!m_CurrentTexture) m_CurrentTexture = CreateRT();
//                 return m_CurrentTexture;
//             }
//             set => m_CurrentTexture = value;
//         }
//
//         RenderTexture m_TempTexture;
//         public RenderTexture TempTexture
//         {
//             get
//             {
//                 if (!m_TempTexture) m_TempTexture = CreateRT();
//                 return m_TempTexture;
//             }
//             set => m_TempTexture = value;
//         }
//
//         RenderTexture m_CompositeTexture;
//         public RenderTexture CompositeTexture
//         {
//             get
//             {
//                 if (!m_CompositeTexture) m_CompositeTexture = CreateRT();
//                 return m_CompositeTexture;
//             }
//             set => m_CompositeTexture = value;
//         }
//         RenderTexture m_LandMarkRT;
//
//         public RenderTexture LandMarkRT
//         {
//             get
//             {
//                 if (m_LandMarkRT == null) m_LandMarkRT = CreateRT();
//                 return m_LandMarkRT;
//             }
//             set => m_LandMarkRT = value;
//         }
//
//
//         public Vector3 mousePos;
//         public MeshRenderer render;
//         Mesh m_Mesh;
//         Mesh Mesh
//         {
//             get
//             {
//                 if (m_Mesh == null)
//                 {
//
//                     MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
//                     if (meshFilters.Length == 1) return meshFilters[0].sharedMesh;
//
//                     CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];
//
//                     for (int i = 0; i < meshFilters.Length; i++)
//                     {
//                         combineInstances[i].mesh = meshFilters[i].sharedMesh;
//                         combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
//                         if (meshFilters[i].transform.parent != null)
//                         {
//                             combineInstances[i].transform *= meshFilters[i].transform.parent.localToWorldMatrix.inverse;
//                         }
//                     }
//                     m_Mesh = new Mesh();
//                     m_Mesh.CombineMeshes(combineInstances);
//                 }
//                 return m_Mesh;
//             }
//         }
//         Material m_Material;
//
//
//         public Texture[] textureList;
//         public string[] shaderPropNameList;
//         public List<TextureItem> textureItems;
//         public TextureItem target;
//         int m_TargetIndex = 0;
//         public int TargetIndex
//         {
//             get
//             {
//                 return m_TargetIndex;
//             }
//             set
//             {
//                 if (value < 0 || value >= textureItems.Count) return;
//                 ClearPaint();
//                 m_TargetIndex = value;
//                 target = textureItems[value];
//                 ChangeTarget();
//             }
//         }
//         public string SourceTexturePath => target?.texturePath;
//         public Vector2Int SourceTextureSize => target == null ? Vector2Int.one * 2 : target.textureSize;
//         public Texture SourceTexture => target?.texture;
//         public GraphicsFormat SourceGraphicsFormat => target.graphicsFormat;
//
//         private MeshCollider m_MeshCollider;
//         private bool m_HasMeshCollider = true;
//
//         void OnEnable()
//         {
//             m_MeshCollider = GetComponent<MeshCollider>();
//             if (m_MeshCollider == null)
//             {
//                 m_MeshCollider = gameObject.AddComponent<MeshCollider>();
//                 m_HasMeshCollider = false;
//             }
//
//
//             InitData();
//             ChangeTarget();
//             Selection.selectionChanged += SelectionChangedCallback;
//             RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
//             // EditorSceneManager.sceneSaving += OnSceneSaving;
//         }
//
//         private void OnSceneSaving(Scene scene, string path)
//         {
//             Save();
//         }
//
//         void OnDisable()
//         {
//             if (!m_HasMeshCollider && m_MeshCollider != null)
//             {
//                 DestroyImmediate(m_MeshCollider);
//             }
//
//             Selection.selectionChanged -= SelectionChangedCallback;
//             RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
//             // EditorSceneManager.sceneSaving -= OnSceneSaving;
//             target.RestoreMaterialTexture();
//
//             ClearResource();
//             ShowUnityTools();
//         }
//
//         private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
//         {
//             if (!m_IsDraw || camera.cameraType != CameraType.SceneView) return;
//
//             int pass = BlendModel == BlendModel.Add ? 0 : 1;
//             CommandBuffer cmd = CommandBufferPool.Get();
//             using (new ProfilingScope(cmd, ProfilingSampler.Get(PainterProfileId.DrawTexture)))
//             {
//                 cmd.ClearRenderTarget(false, true, Color.black);
//
//                 cmd.SetGlobalInteger("_DrawChannel", (int)drawChannel);
//                 cmd.SetGlobalInteger("_TexCoordChannel", (int)texcoordChannel);
//
//                 DrawMaterial.SetTexture("_MainTex", TempTexture);
//                 cmd.SetRenderTarget(CurrentTexture);
//                 cmd.DrawMesh(Mesh, Matrix4x4.identity, DrawMaterial, 0, pass);
//
//                 // 标记
//                 cmd.SetRenderTarget(LandMarkRT);
//                 cmd.DrawMesh(Mesh, Matrix4x4.identity, LandMarkMaterial);
//                 // 修复边缘裂痕
//                 DrawMaterial.SetTexture("_IlsandMap", LandMarkRT);
//                 DrawMaterial.SetTexture("_MainTex", CurrentTexture);
//                 cmd.Blit(CurrentTexture, TempTexture, DrawMaterial, 3);
//
//                 // cmd.Blit(currentTexture, tempTexture);
//
//                 // 原图和Mask混合
//                 DrawMaterial.SetTexture("_MainTex", TempTexture);
//                 DrawMaterial.SetTexture("_SourceTex", SourceTexture);
//                 cmd.Blit(TempTexture, CompositeTexture, DrawMaterial, 2);
//             }
//
//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }
//
//         public void InitData()
//         {
//             var renders = GetComponentsInChildren<MeshRenderer>();
//             if (renders == null)
//             {
//                 Debug.LogError("renders is null");
//                 return;
//             }
//             var list = new List<Texture>();
//             textureItems = new List<TextureItem>();
//             var dict = new Dictionary<string, TextureItem>();
//             foreach (var render in renders)
//             {
//                 m_Material = render.sharedMaterial;
//                 if (m_Material == null) continue;
//                 var shader = m_Material.shader;
//                 for (int i = 0; i < shader.GetPropertyCount(); i++)
//                 {
//                     var name = shader.GetPropertyName(i);
//                     var type = shader.GetPropertyType(i);
//                     if (type == ShaderPropertyType.Texture)
//                     {
//                         var tex = m_Material.GetTexture(name);
//                         if (tex)
//                         {
//                             var item = new TextureItem(tex);
//                             if (dict.TryAdd(item.texturePath, item))
//                             {
//                                 list.Add(tex);
//                                 textureItems.Add(item);
//                                 item.AddReferenceMaterials(m_Material, name);
//                             }
//                             else
//                             {
//                                 dict[item.texturePath].AddReferenceMaterials(m_Material, name);
//                             }
//                         }
//                     }
//                 }
//
//             }
//
//             textureList = list.ToArray();
//             TargetIndex = 0;
//         }
//
//         public void ChangeTarget()
//         {
//             if (drawChannel != PainterDrawChannel.RGB && drawChannel != PainterDrawChannel.RGBA)
//             {
//                 Graphics.Blit(SourceTexture, TempTexture);
//                 // Graphics.Blit(sourceTexture, currentTexture);
//             }
//             target.RestoreMaterialTexture();
//             target.SetMaterialTexture(CompositeTexture);
//             Graphics.Blit(SourceTexture, CompositeTexture);
//         }
//
//         private void SelectionChangedCallback()
//         {
//             if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<TexturePainter>())
//                 HideUnityTools();
//             else
//                 ShowUnityTools();
//         }
//
//
//         public void HideUnityTools()
//         {
//             Tools.hidden = true;
//         }
//         public void ShowUnityTools()
//         {
//             Tools.hidden = false;
//         }
//
//         public RenderTexture CreateRT()
//         {
//             RenderTexture rt = RenderTexture.GetTemporary(SourceTextureSize.x, SourceTextureSize.y, 0, RenderTextureFormat.ARGB32, 0);
//             rt.wrapMode = target == null ? TextureWrapMode.Repeat : target.texture.wrapMode;
//             rt.filterMode = target == null ? FilterMode.Bilinear : target.texture.filterMode;
//             return rt;
//         }
//
//         public void DrawAt()
//         {
//             if (DrawMaterial == null)
//             {
//                 Debug.LogError("drawMat is null");
//                 return;
//             }
//             DrawMaterial.SetInteger("_DrawModel", (int)drawModel);
//             DrawMaterial.SetColor("_BrushColor", brushColor);
//             DrawMaterial.SetFloat("_BrushSize", brushSize);
//             DrawMaterial.SetFloat("_BrushStrength", brushStrength);
//             DrawMaterial.SetFloat("_BrushHardness", brushHardness);
//             DrawMaterial.SetVector("_Mouse", mousePos);
//             DrawMaterial.SetMatrix("mesh_Object2World", transform.localToWorldMatrix);
//         }
//
//         bool m_IsDraw = false;
//         public void SetMouseState(bool v)
//         {
//             m_IsDraw = v;
//             DrawMaterial.SetFloat("_IsDraw", v ? 1 : 0);
//         }
//
//
//         public void SetMousePos(Vector3 pos)
//         {
//             mousePos = pos;
//         }
//         public void SetBrushInfo(float size, float strength, float hardness, Color color)
//         {
//             brushSize = size;
//             brushStrength = strength;
//             brushHardness = hardness;
//             brushColor = color;
//         }
//         // 清除临时创建的资源
//         public void ClearResource()
//         {
//             DestroyImmediate(DrawMaterial);
//             DestroyImmediate(LandMarkMaterial);
//             DrawMaterial = null;
//             LandMarkMaterial = null;
//             ReleaseTexture();
//         }
//
//         // 清除绘制的内容
//         public void ClearPaint()
//         {
//             ReleaseTexture();
//             target?.RestoreMaterialTexture();
//         }
//
//
//
//         public void ReleaseTexture()
//         {
//             CurrentTexture?.Release();
//             TempTexture?.Release();
//             LandMarkRT?.Release();
//
//             CurrentTexture = null;
//             TempTexture = null;
//             LandMarkRT = null;
//         }
//
//         public float CheckBrushParm(float value, float min, float max)
//         {
//             return Mathf.Min(max, Mathf.Max(value, min));
//         }
//
//
//         // 保存RenderTexture
//         public static void SaveRenderTextureToTexture(RenderTexture rt, string path, TextureFormat format = TextureFormat.RGBA32)
//         {
//             var type = Path.GetExtension(path).ToLower();
//
//             RenderTexture.active = rt;
//             Texture2D tex = new Texture2D(rt.width, rt.height, format, false);
//
//             tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
//             RenderTexture.active = null;
//
//             byte[] bytes;
//             if (type == ".jpg" || type == ".jpeg")
//             {
//                 bytes = tex.EncodeToJPG();
//             }
//             else if (type == ".tga")
//             {
//                 bytes = tex.EncodeToTGA();
//             }
//             else
//             {
//                 bytes = tex.EncodeToPNG();
//             }
//
//             Debug.Log("Saved to " + path);
//             File.WriteAllBytes(path, bytes);
//             AssetDatabase.ImportAsset(path);
//             AssetDatabase.Refresh();
//         }
//
//         /// <summary>
//         /// 相对路径转绝对路径
//         /// </summary>
//         /// <param name="absolutePath"></param>
//         /// <returns></returns>
//         public static string AssetsRelativeToAbsolutePath(string path)
//         {
//             return Application.dataPath + path.Substring(6);
//         }
//
//         // save texture
//         public void Save()
//         {
//             var path = SourceTexturePath;
//             if (path.Equals(string.Empty))
//             {
//                 path = Path.Combine(Application.dataPath, "PainterTexture.png");
//             }
//             path = AssetsRelativeToAbsolutePath(path);
//             SaveRenderTextureToTexture(CompositeTexture, path);
//         }
//
//         // 另存为(打开文件夹窗口)
//         public void SaveAs()
//         {
//             var path = SourceTexturePath;
//             if (path.Equals(string.Empty))
//             {
//                 path = Path.Combine(Application.dataPath, "PainterTexture.png");
//             }
//             path = AssetsRelativeToAbsolutePath(path);
//             var dir = Path.GetDirectoryName(path);
//             var fileName = Path.GetFileNameWithoutExtension(path);
//             var extension = Path.GetExtension(path);
//             path = EditorUtility.SaveFilePanel("Save As", dir, fileName, extension);
//             if (path.Equals(string.Empty))
//             {
//                 return;
//             }
//             SaveRenderTextureToTexture(CompositeTexture, path);
//         }
//     }
// }
//
// #endif
