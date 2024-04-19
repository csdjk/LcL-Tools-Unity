using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;

using UnityEngine;
namespace LcLTools
{

    [InitializeOnLoad]
    public class CheckFBXSupport
    {
        static CheckFBXSupport()
        {
            CheckAndAddFBXSupport();
            // LcL_RenderingPipelineDefines.AddDefine("FBX_EXPORTER");
        }


        private static void CheckAndAddFBXSupport()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            string manifestText = File.ReadAllText(manifestPath);

            if (!manifestText.Contains("com.unity.formats.fbx"))
            {
                manifestText = manifestText.Replace("\"dependencies\": {", "\"dependencies\": {\n    \"com.unity.formats.fbx\": \"4.1.3\",");
                File.WriteAllText(manifestPath, manifestText);
            }
        }
    }

    public class JaveLin_RDC_CSV2FBX : EditorWindow
    {
        [MenuItem("LcLTools/CSV To FBX")]
        private static void _Show()
        {
            var win = EditorWindow.GetWindow<JaveLin_RDC_CSV2FBX>();
            win.titleContent = new GUIContent("JaveLin_RDC_CSV2FBX");
            win.Show();
        }

        public class VertexIDInfo
        {
            public int IDX;
            public VertexInfo vertexInfo;
        }

        public enum SemanticType
        {
            Unknown,

            VTX,

            IDX,

            POSITION_X,
            POSITION_Y,
            POSITION_Z,
            POSITION_W,

            NORMAL_X,
            NORMAL_Y,
            NORMAL_Z,
            NORMAL_W,

            TANGENT_X,
            TANGENT_Y,
            TANGENT_Z,
            TANGENT_W,

            TEXCOORD0_X,
            TEXCOORD0_Y,
            TEXCOORD0_Z,
            TEXCOORD0_W,

            TEXCOORD1_X,
            TEXCOORD1_Y,
            TEXCOORD1_Z,
            TEXCOORD1_W,

            TEXCOORD2_X,
            TEXCOORD2_Y,
            TEXCOORD2_Z,
            TEXCOORD2_W,

            TEXCOORD3_X,
            TEXCOORD3_Y,
            TEXCOORD3_Z,
            TEXCOORD3_W,

            TEXCOORD4_X,
            TEXCOORD4_Y,
            TEXCOORD4_Z,
            TEXCOORD4_W,

            TEXCOORD5_X,
            TEXCOORD5_Y,
            TEXCOORD5_Z,
            TEXCOORD5_W,

            TEXCOORD6_X,
            TEXCOORD6_Y,
            TEXCOORD6_Z,
            TEXCOORD6_W,

            TEXCOORD7_X,
            TEXCOORD7_Y,
            TEXCOORD7_Z,
            TEXCOORD7_W,

            COLOR0_X,
            COLOR0_Y,
            COLOR0_Z,
            COLOR0_W,
        }

        // jave.lin : Semantic 映射类型
        public enum SemanticMappingType
        {
            Default,            // jave.lin : 使用默认的
            ManuallyMapping,    // jave.lin : 使用手动设置映射的
        }

        // jave.lin : 材质设置的方式
        public enum MaterialSetType
        {
            CreateNew,
            UsingExsitMaterialAsset,
        }

        // jave.lin : application to vertex shader 的通用类型（辅助转换用）
        public class VertexInfo
        {
            public int VTX;
            public int IDX;

            public float POSITION_X;
            public float POSITION_Y;
            public float POSITION_Z;
            public float POSITION_W;

            public float NORMAL_X;
            public float NORMAL_Y;
            public float NORMAL_Z;
            public float NORMAL_W;

            public float TANGENT_X;
            public float TANGENT_Y;
            public float TANGENT_Z;
            public float TANGENT_W;

            public float TEXCOORD0_X;
            public float TEXCOORD0_Y;
            public float TEXCOORD0_Z;
            public float TEXCOORD0_W;

            public float TEXCOORD1_X;
            public float TEXCOORD1_Y;
            public float TEXCOORD1_Z;
            public float TEXCOORD1_W;

            public float TEXCOORD2_X;
            public float TEXCOORD2_Y;
            public float TEXCOORD2_Z;
            public float TEXCOORD2_W;

            public float TEXCOORD3_X;
            public float TEXCOORD3_Y;
            public float TEXCOORD3_Z;
            public float TEXCOORD3_W;

            public float TEXCOORD4_X;
            public float TEXCOORD4_Y;
            public float TEXCOORD4_Z;
            public float TEXCOORD4_W;

            public float TEXCOORD5_X;
            public float TEXCOORD5_Y;
            public float TEXCOORD5_Z;
            public float TEXCOORD5_W;

            public float TEXCOORD6_X;
            public float TEXCOORD6_Y;
            public float TEXCOORD6_Z;
            public float TEXCOORD6_W;

            public float TEXCOORD7_X;
            public float TEXCOORD7_Y;
            public float TEXCOORD7_Z;
            public float TEXCOORD7_W;

            public float COLOR0_X;
            public float COLOR0_Y;
            public float COLOR0_Z;
            public float COLOR0_W;

            public Vector3 POSITION
            {
                get
                {
                    return new Vector3(
                    POSITION_X,
                    POSITION_Y,
                    POSITION_Z);
                }
            }

            // jave.lin : 齐次坐标
            public Vector4 POSITION_H
            {
                get
                {
                    return new Vector4(
                    POSITION_X,
                    POSITION_Y,
                    POSITION_Z,
                    1);
                }
            }

            public Vector4 NORMAL
            {
                get
                {
                    return new Vector4(
                    NORMAL_X,
                    NORMAL_Y,
                    NORMAL_Z,
                    NORMAL_W);
                }
            }
            public Vector4 TANGENT
            {
                get
                {
                    return new Vector4(
                    TANGENT_X,
                    TANGENT_Y,
                    TANGENT_Z,
                    TANGENT_W);
                }
            }

            public Vector4 TEXCOORD0
            {
                get
                {
                    return new Vector4(
                    TEXCOORD0_X,
                    TEXCOORD0_Y,
                    TEXCOORD0_Z,
                    TEXCOORD0_W);
                }
            }

            public Vector4 TEXCOORD1
            {
                get
                {
                    return new Vector4(
                    TEXCOORD1_X,
                    TEXCOORD1_Y,
                    TEXCOORD1_Z,
                    TEXCOORD1_W);
                }
            }

            public Vector4 TEXCOORD2
            {
                get
                {
                    return new Vector4(
                    TEXCOORD2_X,
                    TEXCOORD2_Y,
                    TEXCOORD2_Z,
                    TEXCOORD2_W);
                }
            }

            public Vector4 TEXCOORD3
            {
                get
                {
                    return new Vector4(
                    TEXCOORD3_X,
                    TEXCOORD3_Y,
                    TEXCOORD3_Z,
                    TEXCOORD3_W);
                }
            }

            public Vector4 TEXCOORD4
            {
                get
                {
                    return new Vector4(
                    TEXCOORD4_X,
                    TEXCOORD4_Y,
                    TEXCOORD4_Z,
                    TEXCOORD4_W);
                }
            }

            public Vector4 TEXCOORD5
            {
                get
                {
                    return new Vector4(
                    TEXCOORD5_X,
                    TEXCOORD5_Y,
                    TEXCOORD5_Z,
                    TEXCOORD5_W);
                }
            }

            public Vector4 TEXCOORD6
            {
                get
                {
                    return new Vector4(
                    TEXCOORD6_X,
                    TEXCOORD6_Y,
                    TEXCOORD6_Z,
                    TEXCOORD6_W);
                }
            }

            public Vector4 TEXCOORD7
            {
                get
                {
                    return new Vector4(
                    TEXCOORD7_X,
                    TEXCOORD7_Y,
                    TEXCOORD7_Z,
                    TEXCOORD7_W);
                }
            }

            public Color COLOR0
            {
                get
                {
                    return new Color(
                    COLOR0_X,
                    COLOR0_Y,
                    COLOR0_Z,
                    COLOR0_W);
                }
            }
        }

        private const string GO_Parent_Name = "Models_From_CSV";

        // jave.lin : on_gui 上显示的对象
        private TextAsset RDC_Text_Asset;
        private string fbxName;
        private string outputDir;
        private string outputFullName;

        // jave.lin : on_gui - options
        private Vector2 optionsScrollPos;
        private static bool options_show = true;
        private static bool is_from_DX_CSV = false;
        private static Vector3 vertexOffset = Vector3.zero;
        private static Vector3 vertexRotation = Vector3.zero;
        private static Vector3 vertexScale = Vector3.one;
        private static bool is_reverse_vertex_order = false; // jave.lin : for reverse normal
        private static bool is_recalculate_bound = true;
        private static SemanticMappingType semanticMappingType = SemanticMappingType.ManuallyMapping;
        private static bool has_uv0 = true;
        private static bool has_uv1 = false;
        private static bool has_uv2 = false;
        private static bool has_uv3 = false;
        private static bool has_uv4 = false;
        private static bool has_uv5 = false;
        private static bool has_uv6 = false;
        private static bool has_uv7 = false;
        private static bool has_color0 = true;
        private static bool useAutoMapping = true;
        private static bool useAllComponent = true;
        private ModelImporterNormals normalImportType = ModelImporterNormals.Import;
        private ModelImporterTangents tangentImportType = ModelImporterTangents.Import;
        private bool show_mat_toggle = true;
        private MaterialSetType materialSetType = MaterialSetType.CreateNew;
        private Shader shader;
        private Texture texture;
        private Material material;

        private Dictionary<string, SemanticType> semanticTypeDict_key_name_helper;
        private Dictionary<string, SemanticType> semanticManullyMappingTypeDict_key_name_helper;
        private static Dictionary<string, SemanticType> semanticManullyMappingTypeDict_Cache = new Dictionary<string, SemanticType>();


        private SemanticType[] semanticsIDX_helper;
        private int[] semantics_check_duplicated_helper;
        private List<string> stringListHelper;

        private int[] GetSemantics_check_duplicated_helper()
        {
            if (semantics_check_duplicated_helper == null)
            {
                var vals = Enum.GetValues(typeof(SemanticType));
                semantics_check_duplicated_helper = new int[vals.Length];
                for (int i = 0; i < vals.Length; i++)
                {
                    semantics_check_duplicated_helper[i] = 0;
                }
            }
            return semantics_check_duplicated_helper;
        }

        private void ClearSemantics_check_duplicated_helper(int[] arr)
        {
            if (arr != null)
            {
                Array.Clear(arr, 0, arr.Length);
            }
        }

        private List<string> GetStringListHelper()
        {
            if (stringListHelper == null)
            {
                stringListHelper = new List<string>();
            }
            return stringListHelper;
        }

        // jave.lin : 删除指定目录+目录下的所有文件
        private void DelectDir(string dir)
        {
            try
            {
                if (!Directory.Exists(outputDir))
                    return;

                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                // 返回目录中所有文件和子目录
                FileSystemInfo[] fileInfos = dirInfo.GetFileSystemInfos();
                foreach (FileSystemInfo fileInfo in fileInfos)
                {
                    if (fileInfo is DirectoryInfo)
                    {
                        // 判断是否文件夹
                        DirectoryInfo subDir = new DirectoryInfo(fileInfo.FullName);
                        subDir.Delete(true);            // 删除子目录和文件
                    }
                    else
                    {
                        File.Delete(fileInfo.FullName);      // 删除指定文件
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        // jave.lin : 根据全路径名 转换为 assets 目录下的名字
        private string GetAssetPathByFullName(string fullName)
        {
            fullName = fullName.Replace("\\", "/");
            var dataPath_prefix = Application.dataPath.Replace("Assets", "");
            dataPath_prefix = dataPath_prefix.Replace(dataPath_prefix + "/", "");
            var mi_path = fullName.Replace(dataPath_prefix, "");
            return mi_path;
        }

        private void OnGUI()
        {
            Output_RDC_CSV_Handle();
        }


        bool IsEquals(string semantic, string target)
        {
            semantic = semantic.ToLower();
            string type, component;
            if (semantic.Contains("_x"))
            {
                type = semantic.Replace("_x", "");
                component = ".x";
            }
            else if (semantic.Contains("_y"))
            {
                type = semantic.Replace("_y", "");
                component = ".y";
            }
            else if (semantic.Contains("_z"))
            {
                type = semantic.Replace("_z", "");
                component = ".z";
            }
            else if (semantic.Contains("_w"))
            {
                type = semantic.Replace("_w", "");
                component = ".w";
            }
            else
            {
                type = semantic;
                component = semantic;
            }

            return target.Contains(type) && target.Contains(component);
        }

        SemanticType TryGetSemanticType(string str)
        {
            var lowStr = str.ToLower();
            foreach (SemanticType st in Enum.GetValues(typeof(SemanticType)))
            {
                if (IsEquals(st.ToString(), lowStr))
                {
                    return st;
                }
            }
            return SemanticType.Unknown;
        }

        //快速从字符串中提取Vector3
        public Vector3 ExtractVector3FromData(string data, Vector3 offset)
        {
            if (data == string.Empty)
                return offset;

            string[] splitData = data.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            List<float> list = new List<float>();
            foreach (var item in splitData)
            {
                if (float.TryParse(item, out float value))
                {
                    list.Add(value);
                }
            }
            if (list.Count < 3)
            {
                return offset;
            }
            return new Vector3(list[0], list[1], list[2]);
        }

        private bool refresh_data = false;
        private bool csv_asset_changed = false;
        private void Output_RDC_CSV_Handle()
        {
            var new_textAsset = EditorGUILayout.ObjectField("RDC_CSV", RDC_Text_Asset, typeof(TextAsset), false) as TextAsset;

            // RDC_Text_Asset = EditorGUILayout.ObjectField("RDC_CSV", RDC_Text_Asset, typeof(TextAsset), false) as TextAsset;

            csv_asset_changed = false;
            if (RDC_Text_Asset != new_textAsset)
            {
                csv_asset_changed = true;
                RDC_Text_Asset = new_textAsset;
            }

            if (RDC_Text_Asset == null)
            {
                var srcCol = GUI.contentColor;
                GUI.contentColor = Color.red;
                EditorGUILayout.LabelField("Have no setting the RDC_CSV yet!");
                GUI.contentColor = srcCol;
                return;
            }

            if (refresh_data || csv_asset_changed)
            {
                material = null;
                semanticManullyMappingTypeDict_key_name_helper = null;
                if (refresh_data)
                {
                    semanticManullyMappingTypeDict_Cache.Clear();
                }
                ClearSemantics_check_duplicated_helper(semantics_check_duplicated_helper);
            }

            fbxName = EditorGUILayout.TextField("FBX Name", fbxName);
            if (RDC_Text_Asset != null && (refresh_data || csv_asset_changed || string.IsNullOrEmpty(fbxName)))
            {
                fbxName = GenerateGOName(RDC_Text_Asset);
            }

            // jave.lin : output path
            EditorGUILayout.BeginHorizontal();
            outputDir = EditorGUILayout.TextField("Output Path(Dir)", outputDir);
            if (refresh_data || csv_asset_changed || string.IsNullOrEmpty(outputDir))
            {
                var csvPath = LcLEditorUtilities.GetAssetAbsolutePath(RDC_Text_Asset);
                outputDir = Path.Combine(Path.GetDirectoryName(csvPath), fbxName);
                outputDir = outputDir.Replace("\\", "/");
            }
            if (GUILayout.Button("Browser...", GUILayout.Width(100)))
            {
                outputDir = EditorUtility.OpenFolderPanel("Select an output path", outputDir, "");
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = false;
            outputFullName = Path.Combine(outputDir, fbxName + ".fbx");
            outputFullName = outputFullName.Replace("\\", "/");
            EditorGUILayout.TextField("Output Full Name", outputFullName);
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            {
                refresh_data = false;
                if (GUILayout.Button("Reset Settings"))
                {
                    refresh_data = true;
                }
                if (GUILayout.Button("Export FBX"))
                {
                    ExportHandle();
                }
            }
            GUILayout.EndHorizontal();

            optionsScrollPos = EditorGUILayout.BeginScrollView(optionsScrollPos);

            EditorGUILayout.Space(10);
            options_show = EditorGUILayout.BeginFoldoutHeaderGroup(options_show, "Model Options");
            if (options_show)
            {
                EditorGUI.indentLevel++;
                is_from_DX_CSV = EditorGUILayout.Toggle("Is From DirectX CSV", is_from_DX_CSV);
                is_reverse_vertex_order = EditorGUILayout.Toggle("Is Reverse Normal", is_reverse_vertex_order);
                is_recalculate_bound = EditorGUILayout.Toggle("Is Recalculate AABB", is_recalculate_bound);


                var offset = EditorGUILayout.TextField("从String中提取Offset", string.Empty);
                vertexOffset = ExtractVector3FromData(offset, vertexOffset);
                vertexOffset = EditorGUILayout.Vector3Field("Vertex Offset", vertexOffset);

                vertexRotation = EditorGUILayout.Vector3Field("Vertex Rotation", vertexRotation);
                vertexScale = EditorGUILayout.Vector3Field("Vertex Scale", vertexScale);
                // jave.lin : has_uv0,1,2,3,4,5,6,7
                has_uv0 = EditorGUILayout.Toggle("Has UV0", has_uv0);
                has_uv1 = EditorGUILayout.Toggle("Has UV1", has_uv1);
                has_uv2 = EditorGUILayout.Toggle("Has UV2", has_uv2);
                has_uv3 = EditorGUILayout.Toggle("Has UV3", has_uv3);
                has_uv4 = EditorGUILayout.Toggle("Has UV4", has_uv4);
                has_uv5 = EditorGUILayout.Toggle("Has UV5", has_uv5);
                has_uv6 = EditorGUILayout.Toggle("Has UV6", has_uv6);
                has_uv7 = EditorGUILayout.Toggle("Has UV7", has_uv7);
                // jave.lin : has_color0
                has_color0 = EditorGUILayout.Toggle("Has Color0", has_color0);
                normalImportType = (ModelImporterNormals)EditorGUILayout.EnumPopup("Normal Import Type", normalImportType);
                tangentImportType = (ModelImporterTangents)EditorGUILayout.EnumPopup("Tangent Import Type", tangentImportType);
                semanticMappingType = (SemanticMappingType)EditorGUILayout.EnumPopup("Semantic Mapping Type", semanticMappingType);
                if (semanticMappingType == SemanticMappingType.ManuallyMapping)
                {
                    var refreshCSVSemanticTitle = false;
                    if (GUILayout.Button("Refresh Analysis CSV Semantic Title"))
                    {
                        refreshCSVSemanticTitle = true;
                    }

                    if (semanticManullyMappingTypeDict_key_name_helper == null)
                    {
                        refreshCSVSemanticTitle = true;
                    }

                    if (refreshCSVSemanticTitle)
                    {
                        Analysis_CSV_SemanticTitle();
                    }

                    var keys = semanticManullyMappingTypeDict_key_name_helper.Keys;
                    var stringList = GetStringListHelper();
                    stringList.Clear();
                    stringList.AddRange(keys);

                    stringList.Sort();

                    var check_duplicated_helper = GetSemantics_check_duplicated_helper();
                    for (int i = 0; i < stringList.Count; i++)
                    {
                        if (semanticManullyMappingTypeDict_key_name_helper.TryGetValue(stringList[i], out SemanticType mappedST))
                        {
                            var idx = (int)mappedST;
                            check_duplicated_helper[idx]++;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        var src_col = GUI.contentColor;
                        GUI.contentColor = Color.yellow;
                        EditorGUILayout.LabelField("CSV Seman Name");
                        useAllComponent = EditorGUILayout.Toggle("Apply All Component", useAllComponent);
                        useAutoMapping = EditorGUILayout.Toggle("Auto Mapping", useAutoMapping);
                        GUI.contentColor = src_col;
                    }
                    EditorGUILayout.EndHorizontal();

                    for (int i = 0; i < stringList.Count; i++)
                    {
                        var semantic_name = stringList[i];
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(semantic_name);


                        if (!semanticManullyMappingTypeDict_key_name_helper.TryGetValue(semantic_name, out SemanticType mappedST))
                        {
                            Debug.LogError($"un mapped semantic name : {semantic_name}");
                            continue;
                        }

                        if (useAutoMapping)
                        {
                            mappedST = TryGetSemanticType(semantic_name);
                        }
                        mappedST = (SemanticType)EditorGUILayout.EnumPopup(mappedST);

                        if (useAllComponent)
                        {
                            if (mappedST != semanticManullyMappingTypeDict_key_name_helper[semantic_name])
                            {
                                SetAttrName(semantic_name, mappedST.ToString());
                            }
                            mappedST = TryGetSemanticType2(semantic_name, mappedST);
                        }


                        semanticManullyMappingTypeDict_key_name_helper[semantic_name] = mappedST;
                        StoreKeyDict();
                        if (check_duplicated_helper[(int)mappedST] > 1)
                        {
                            var src_col = GUI.contentColor;
                            GUI.contentColor = Color.red;
                            EditorGUILayout.LabelField("Duplicated Options");
                            GUI.contentColor = src_col;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    ClearSemantics_check_duplicated_helper(check_duplicated_helper);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);
            show_mat_toggle = EditorGUILayout.BeginFoldoutHeaderGroup(show_mat_toggle, "Material Options");
            if (show_mat_toggle)
            {
                EditorGUI.indentLevel++;
                var newMaterialSetType = (MaterialSetType)EditorGUILayout.EnumPopup("Material Set Type", materialSetType);
                if (material == null || materialSetType != newMaterialSetType)
                {
                    materialSetType = newMaterialSetType;
                    if (materialSetType == MaterialSetType.CreateNew)
                    {
                        if (shader == null)
                        {
                            shader = Shader.Find("Universal Render Pipeline/Lit");
                        }
                        material = new Material(shader);
                    }
                    else
                    {
                        var mat_path = Path.Combine(outputDir, fbxName + ".mat").Replace("\\", "/");
                        mat_path = GetAssetPathByFullName(mat_path);
                        var mat_asset = AssetDatabase.LoadAssetAtPath<Material>(mat_path);
                        if (mat_asset != null) material = mat_asset;
                    }
                }

                if (materialSetType == MaterialSetType.CreateNew)
                {
                    shader = EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false) as Shader;
                    texture = EditorGUILayout.ObjectField("Main Texture", texture, typeof(Texture), false) as Texture;
                }
                else // MaterialSetType.UseExsitMaterialAsset
                {
                    material = EditorGUILayout.ObjectField("Material Asset", material, typeof(Material), false) as Material;
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }


        private void StoreKeyDict()
        {
            semanticManullyMappingTypeDict_Cache.Clear();
            foreach (var item in semanticManullyMappingTypeDict_key_name_helper)
            {
                semanticManullyMappingTypeDict_Cache[item.Key] = item.Value;
            }
        }

        private Dictionary<string, string> attrNameDict;

        private void SetAttrName(string csvAttrName, string semanticName)
        {
            if (attrNameDict == null)
            {
                attrNameDict = new Dictionary<string, string>();
            }
            csvAttrName = csvAttrName.Split('.')[0];
            semanticName = semanticName.Split('_')[0];

            if (!attrNameDict.TryAdd(csvAttrName, semanticName))
            {
                attrNameDict[csvAttrName] = semanticName;
            }
        }

        SemanticType TryGetSemanticType2(string csvAttrName, SemanticType mappedST)
        {
            if (attrNameDict == null)
            {
                return mappedST;
            }

            var csvAttrNames = csvAttrName.Split('.');
            if (csvAttrNames.Length < 2) return mappedST;

            var componentName = csvAttrNames[1].ToUpper();
            if (attrNameDict.TryGetValue(csvAttrNames[0], out string semantic))
            {

                foreach (SemanticType type in Enum.GetValues(typeof(SemanticType)))
                {
                    var typeStr = type.ToString().ToUpper();
                    if (typeStr.Contains(semantic) && typeStr.Contains(componentName))
                    {
                        return type;
                    }
                }
            }
            return mappedST;
        }


        private void Analysis_CSV_SemanticTitle()
        {
            if (semanticManullyMappingTypeDict_key_name_helper != null)
            {
                semanticManullyMappingTypeDict_key_name_helper.Clear();
            }
            else
            {
                semanticManullyMappingTypeDict_key_name_helper = new Dictionary<string, SemanticType>();
            }
            var text = RDC_Text_Asset.text;
            var firstLine = text.Substring(0, text.IndexOf("\n")).Trim();
            var line_element_splitor = new string[] { "," };
            var semanticTitles = firstLine.Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);

            MappingSemanticsTypeByNames(ref semanticTypeDict_key_name_helper);

            for (int i = 0; i < semanticTitles.Length; i++)
            {
                var title = semanticTitles[i];
                var semantics = title.Trim();
                if (semanticTypeDict_key_name_helper.TryGetValue(semantics, out SemanticType semanticType))
                {
                    semanticManullyMappingTypeDict_key_name_helper[semantics] = semanticType;
                }
                else
                {
                    // 鐠囪褰囩紓鎾崇摠
                    if (semanticManullyMappingTypeDict_Cache.TryGetValue(semantics, out semanticType))
                    {
                        semanticManullyMappingTypeDict_key_name_helper[semantics] = semanticType;
                    }
                    else
                    {
                        semanticManullyMappingTypeDict_key_name_helper[semantics] = SemanticType.Unknown;
                    }
                }
            }
        }

        private void ExportHandle()
        {
            if (RDC_Text_Asset != null)
            {
                // try
                {
                    MappingSemanticsTypeByNames(ref semanticTypeDict_key_name_helper);
                    var parent = GetParentTrans();
                    var outputGO = GameObject.Find($"{GO_Parent_Name}/{fbxName}");
                    if (outputGO != null)
                    {
                        GameObject.DestroyImmediate(outputGO);
                    }
                    outputGO = GenerateGOWithMeshRendererFromCSV(RDC_Text_Asset.text, is_from_DX_CSV);
                    outputGO.transform.SetParent(parent);
                    outputGO.name = fbxName;

                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    if (materialSetType == MaterialSetType.CreateNew)
                    {
                        var create_mat = outputGO.GetComponent<MeshRenderer>().sharedMaterial;
                        create_mat.mainTexture = texture;

                        var mat_created_path = Path.Combine(outputDir, fbxName + ".mat").Replace("\\", "/");
                        mat_created_path = GetAssetPathByFullName(mat_created_path);
                        Debug.Log($"mat_created_path : {mat_created_path}");
                        var src_mat = AssetDatabase.LoadAssetAtPath<Material>(mat_created_path);
                        if (src_mat == create_mat)
                        {
                            // nop
                        }
                        else
                        {
                            AssetDatabase.DeleteAsset(mat_created_path);
                            AssetDatabase.CreateAsset(create_mat, mat_created_path);
                        }
                    }

                    ModelExporter.ExportObject(outputFullName, outputGO);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    string mi_path = GetAssetPathByFullName(outputFullName);
                    ModelImporter mi = ModelImporter.GetAtPath(mi_path) as ModelImporter;
                    mi.importNormals = normalImportType;
                    mi.importTangents = tangentImportType;
                    mi.importAnimation = false;
                    mi.importAnimatedCustomProperties = false;
                    mi.importBlendShapeNormals = ModelImporterNormals.None;
                    mi.importBlendShapes = false;
                    mi.importCameras = false;
                    mi.importConstraints = false;
                    mi.importLights = false;
                    mi.importVisibility = false;
                    mi.animationType = ModelImporterAnimationType.None;
                    mi.materialImportMode = ModelImporterMaterialImportMode.None;
                    mi.SaveAndReimport();

                    // jave.lin : replace outputGO from model prefab
                    var src_parent = outputGO.transform.parent;
                    var src_local_pos = outputGO.transform.localPosition;
                    var src_local_rot = outputGO.transform.localRotation;
                    var src_local_scl = outputGO.transform.localScale;
                    DestroyImmediate(outputGO);
                    // jave.lin : new model prefab
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(mi_path);
                    outputGO = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    outputGO.transform.SetParent(src_parent);
                    outputGO.transform.localPosition = src_local_pos;
                    outputGO.transform.localRotation = src_local_rot;
                    outputGO.transform.localScale = src_local_scl;
                    outputGO.name = fbxName;
                    // jave.lin : set material
                    var mat_path = Path.Combine(outputDir, fbxName + ".mat").Replace("\\", "/");
                    mat_path = GetAssetPathByFullName(mat_path);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(mat_path);
                    outputGO.GetComponent<MeshRenderer>().sharedMaterial = mat;
                    // jave.lin : new real prefab
                    var prefab_created_path = Path.Combine(outputDir, fbxName + ".prefab").Replace("\\", "/");
                    prefab_created_path = GetAssetPathByFullName(prefab_created_path);
                    Debug.Log($"prefab_created_path : {prefab_created_path}");
                    PrefabUtility.SaveAsPrefabAssetAndConnect(outputGO, prefab_created_path, InteractionMode.AutomatedAction);

                    // jave.lin : 打印打出成功的信息
                    Debug.Log($"Export FBX Successfully! outputPath : {outputFullName}");
                }
                // catch (Exception er)
                // {
                //     Debug.LogError($"Export FBX Failed! er: {er}");
                // }
            }
        }

        private void MappingSemanticsTypeByNames(ref Dictionary<string, SemanticType> container)
        {
            if (container == null)
            {
                container = new Dictionary<string, SemanticType>();
            }
            else
            {
                container.Clear();
            }
            container["VTX"] = SemanticType.VTX;
            container["IDX"] = SemanticType.IDX;
            container["SV_POSITION.x"] = SemanticType.POSITION_X;
            container["SV_POSITION.y"] = SemanticType.POSITION_Y;
            container["SV_POSITION.z"] = SemanticType.POSITION_Z;
            container["SV_POSITION.w"] = SemanticType.POSITION_W;
            container["SV_Position.x"] = SemanticType.POSITION_X;
            container["SV_Position.y"] = SemanticType.POSITION_Y;
            container["SV_Position.z"] = SemanticType.POSITION_Z;
            container["SV_Position.w"] = SemanticType.POSITION_W;
            container["POSITION.x"] = SemanticType.POSITION_X;
            container["POSITION.y"] = SemanticType.POSITION_Y;
            container["POSITION.z"] = SemanticType.POSITION_Z;
            container["POSITION.w"] = SemanticType.POSITION_W;
            container["NORMAL.x"] = SemanticType.NORMAL_X;
            container["NORMAL.y"] = SemanticType.NORMAL_Y;
            container["NORMAL.z"] = SemanticType.NORMAL_Z;
            container["NORMAL.w"] = SemanticType.NORMAL_W;
            container["TANGENT.x"] = SemanticType.TANGENT_X;
            container["TANGENT.y"] = SemanticType.TANGENT_Y;
            container["TANGENT.z"] = SemanticType.TANGENT_Z;
            container["TANGENT.w"] = SemanticType.TANGENT_W;
            container["TEXCOORD0.x"] = SemanticType.TEXCOORD0_X;
            container["TEXCOORD0.y"] = SemanticType.TEXCOORD0_Y;
            container["TEXCOORD0.z"] = SemanticType.TEXCOORD0_Z;
            container["TEXCOORD0.w"] = SemanticType.TEXCOORD0_W;
            container["TEXCOORD1.x"] = SemanticType.TEXCOORD1_X;
            container["TEXCOORD1.y"] = SemanticType.TEXCOORD1_Y;
            container["TEXCOORD1.z"] = SemanticType.TEXCOORD1_Z;
            container["TEXCOORD1.w"] = SemanticType.TEXCOORD1_W;
            container["TEXCOORD2.x"] = SemanticType.TEXCOORD2_X;
            container["TEXCOORD2.y"] = SemanticType.TEXCOORD2_Y;
            container["TEXCOORD2.z"] = SemanticType.TEXCOORD2_Z;
            container["TEXCOORD2.w"] = SemanticType.TEXCOORD2_W;
            container["TEXCOORD3.x"] = SemanticType.TEXCOORD3_X;
            container["TEXCOORD3.y"] = SemanticType.TEXCOORD3_Y;
            container["TEXCOORD3.z"] = SemanticType.TEXCOORD3_Z;
            container["TEXCOORD3.w"] = SemanticType.TEXCOORD3_W;
            container["TEXCOORD4.x"] = SemanticType.TEXCOORD4_X;
            container["TEXCOORD4.y"] = SemanticType.TEXCOORD4_Y;
            container["TEXCOORD4.z"] = SemanticType.TEXCOORD4_Z;
            container["TEXCOORD4.w"] = SemanticType.TEXCOORD4_W;
            container["TEXCOORD5.x"] = SemanticType.TEXCOORD5_X;
            container["TEXCOORD5.y"] = SemanticType.TEXCOORD5_Y;
            container["TEXCOORD5.z"] = SemanticType.TEXCOORD5_Z;
            container["TEXCOORD5.w"] = SemanticType.TEXCOORD5_W;
            container["TEXCOORD6.x"] = SemanticType.TEXCOORD6_X;
            container["TEXCOORD6.y"] = SemanticType.TEXCOORD6_Y;
            container["TEXCOORD6.z"] = SemanticType.TEXCOORD6_Z;
            container["TEXCOORD6.w"] = SemanticType.TEXCOORD6_W;
            container["TEXCOORD7.x"] = SemanticType.TEXCOORD7_X;
            container["TEXCOORD7.y"] = SemanticType.TEXCOORD7_Y;
            container["TEXCOORD7.z"] = SemanticType.TEXCOORD7_Z;
            container["TEXCOORD7.w"] = SemanticType.TEXCOORD7_W;
            container["COLOR0.x"] = SemanticType.COLOR0_X;
            container["COLOR0.y"] = SemanticType.COLOR0_Y;
            container["COLOR0.z"] = SemanticType.COLOR0_Z;
            container["COLOR0.w"] = SemanticType.COLOR0_W;
            container["COLOR.x"] = SemanticType.COLOR0_X;
            container["COLOR.y"] = SemanticType.COLOR0_Y;
            container["COLOR.z"] = SemanticType.COLOR0_Z;
            container["COLOR.w"] = SemanticType.COLOR0_W;
        }

        // jave.lin : 获取 parent transform 对象
        private Transform GetParentTrans()
        {
            var parentGO = GameObject.Find(GO_Parent_Name);
            if (parentGO == null)
            {
                parentGO = new GameObject(GO_Parent_Name);
                parentGO.transform.position = Vector3.zero;
                parentGO.transform.localRotation = Quaternion.identity;
                parentGO.transform.localScale = Vector3.one;
            }
            return parentGO.transform;
        }

        // jave.lin : 根据名字生成 GO Name
        private string GenerateGOName(TextAsset ta)
        {
            //return $"From_CSV_{ta.text.GetHashCode()}";
            //return $"From_CSV_{ta.name}";
            return ta.name;
        }

        // jave.lin : 根据 CSV 内容生成 MeshRenderer 对应的 GO
        private GameObject GenerateGOWithMeshRendererFromCSV(string csv, bool is_from_DX_CSV)
        {
            var ret = new GameObject();

            var mesh = new Mesh();

            // jave.lin : 根据 csv 来填充 mesh 信息
            FillMeshFromCSV(mesh, csv, is_from_DX_CSV);

            var meshFilter = ret.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = ret.AddComponent<MeshRenderer>();

            // jave.lin : 默认使用 URP 的 PBR Shader
            meshRenderer.sharedMaterial = material;

            ret.transform.position = Vector3.zero;
            ret.transform.localRotation = Quaternion.identity;
            ret.transform.localScale = Vector3.one;

            return ret;
        }

        // jave.lin : 根据 semantic type 和 data 来填充到 数据字段
        private void FillVertexFieldInfo(VertexInfo info, SemanticType semanticType, string data, bool is_from_DX_CSV)
        {
            switch (semanticType)
            {
                // jave.lin : VTX
                case SemanticType.VTX:
                    info.VTX = int.Parse(data);
                    break;

                // jave.lin : IDX
                case SemanticType.IDX:
                    info.IDX = int.Parse(data);
                    break;

                // jave.lin : position
                case SemanticType.POSITION_X:
                    info.POSITION_X = float.Parse(data);
                    break;
                case SemanticType.POSITION_Y:
                    info.POSITION_Y = float.Parse(data);
                    break;
                case SemanticType.POSITION_Z:
                    info.POSITION_Z = float.Parse(data);
                    break;
                case SemanticType.POSITION_W:
                    info.POSITION_W = float.Parse(data);
                    Debug.LogWarning("WARNING: unity mesh cannot transfer position.w to shader program.");
                    break;

                // jave.lin : normal
                case SemanticType.NORMAL_X:
                    info.NORMAL_X = float.Parse(data);
                    break;
                case SemanticType.NORMAL_Y:
                    info.NORMAL_Y = float.Parse(data);
                    break;
                case SemanticType.NORMAL_Z:
                    info.NORMAL_Z = float.Parse(data);
                    break;
                case SemanticType.NORMAL_W:
                    info.NORMAL_W = float.Parse(data);
                    break;

                // jave.lin : tangent
                case SemanticType.TANGENT_X:
                    info.TANGENT_X = float.Parse(data);
                    break;
                case SemanticType.TANGENT_Y:
                    info.TANGENT_Y = float.Parse(data);
                    break;
                case SemanticType.TANGENT_Z:
                    info.TANGENT_Z = float.Parse(data);
                    break;
                case SemanticType.TANGENT_W:
                    info.TANGENT_W = float.Parse(data);
                    break;

                // jave.lin : texcoord0
                case SemanticType.TEXCOORD0_X:
                    has_uv0 = true;
                    info.TEXCOORD0_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD0_Y:
                    info.TEXCOORD0_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD0_Y = 1 - info.TEXCOORD0_Y;
                    break;
                case SemanticType.TEXCOORD0_Z:
                    info.TEXCOORD0_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD0_W:
                    info.TEXCOORD0_W = float.Parse(data);
                    break;

                // jave.lin : texcoord1
                case SemanticType.TEXCOORD1_X:
                    has_uv1 = true;
                    info.TEXCOORD1_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD1_Y:
                    info.TEXCOORD1_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD1_Y = 1 - info.TEXCOORD1_Y;
                    break;
                case SemanticType.TEXCOORD1_Z:
                    info.TEXCOORD1_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD1_W:
                    info.TEXCOORD1_W = float.Parse(data);
                    break;

                // jave.lin : texcoord2
                case SemanticType.TEXCOORD2_X:
                    has_uv2 = true;
                    info.TEXCOORD2_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD2_Y:
                    info.TEXCOORD2_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD2_Y = 1 - info.TEXCOORD2_Y;
                    break;
                case SemanticType.TEXCOORD2_Z:
                    info.TEXCOORD2_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD2_W:
                    info.TEXCOORD2_W = float.Parse(data);
                    break;

                // jave.lin : texcoord3
                case SemanticType.TEXCOORD3_X:
                    has_uv3 = true;
                    info.TEXCOORD3_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD3_Y:
                    info.TEXCOORD3_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD3_Y = 1 - info.TEXCOORD3_Y;
                    break;
                case SemanticType.TEXCOORD3_Z:
                    info.TEXCOORD3_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD3_W:
                    info.TEXCOORD3_W = float.Parse(data);
                    break;

                // jave.lin : texcoord4
                case SemanticType.TEXCOORD4_X:
                    has_uv4 = true;
                    info.TEXCOORD4_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD4_Y:
                    info.TEXCOORD4_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD4_Y = 1 - info.TEXCOORD4_Y;
                    break;
                case SemanticType.TEXCOORD4_Z:
                    info.TEXCOORD4_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD4_W:
                    info.TEXCOORD4_W = float.Parse(data);
                    break;

                // jave.lin : texcoord5
                case SemanticType.TEXCOORD5_X:
                    has_uv5 = true;
                    info.TEXCOORD5_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD5_Y:
                    info.TEXCOORD5_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD5_Y = 1 - info.TEXCOORD5_Y;
                    break;
                case SemanticType.TEXCOORD5_Z:
                    info.TEXCOORD5_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD5_W:
                    info.TEXCOORD5_W = float.Parse(data);
                    break;

                // jave.lin : texcoord6
                case SemanticType.TEXCOORD6_X:
                    has_uv6 = true;
                    info.TEXCOORD6_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD6_Y:
                    info.TEXCOORD6_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD6_Y = 1 - info.TEXCOORD6_Y;
                    break;
                case SemanticType.TEXCOORD6_Z:
                    info.TEXCOORD6_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD6_W:
                    info.TEXCOORD6_W = float.Parse(data);
                    break;

                // jave.lin : texcoord7
                case SemanticType.TEXCOORD7_X:
                    has_uv7 = true;
                    info.TEXCOORD7_X = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD7_Y:
                    info.TEXCOORD7_Y = float.Parse(data);
                    if (is_from_DX_CSV) info.TEXCOORD7_Y = 1 - info.TEXCOORD7_Y;
                    break;
                case SemanticType.TEXCOORD7_Z:
                    info.TEXCOORD7_Z = float.Parse(data);
                    break;
                case SemanticType.TEXCOORD7_W:
                    info.TEXCOORD7_W = float.Parse(data);
                    break;

                // jave.lin : color0
                case SemanticType.COLOR0_X:
                    has_color0 = true;
                    info.COLOR0_X = float.Parse(data);
                    break;
                case SemanticType.COLOR0_Y:
                    info.COLOR0_Y = float.Parse(data);
                    break;
                case SemanticType.COLOR0_Z:
                    info.COLOR0_Z = float.Parse(data);
                    break;
                case SemanticType.COLOR0_W:
                    info.COLOR0_W = float.Parse(data);
                    break;
                case SemanticType.Unknown:
                    // jave.lin : nop
                    break;
                // jave.lin : un-implements
                default:
                    Debug.LogError($"Fill_A2V_Common_Type_Data un-implements SemanticType : {semanticType}");
                    break;
            }
        }
        void InitVertexInfoWriteSwitch()
        {
            has_uv0 = false;
            has_uv1 = false;
            has_uv2 = false;
            has_uv3 = false;
            has_uv4 = false;
            has_uv5 = false;
            has_uv6 = false;
            has_uv7 = false;
            has_color0 = false;
        }

        // jave.lin : 根据 csv 来填充 mesh 信息
        private void FillMeshFromCSV(Mesh mesh, string csv, bool is_from_DX_CSV)
        {
            InitVertexInfoWriteSwitch();
            var line_splitor = new string[] { "\n" };
            var line_element_splitor = new string[] { "," };

            var lines = csv.Split(line_splitor, StringSplitOptions.RemoveEmptyEntries);

            // jave.lin : lines[0] == "VTX, IDX, POSITION.x, POSITION.y, POSITION.z, NORMAL.x, NORMAL.y, NORMAL.z, NORMAL.w, TANGENT.x, TANGENT.y, TANGENT.z, TANGENT.w, TEXCOORD0.x, TEXCOORD0.y"

            // jave.lin : 构建 vertex buffer format 的 semantics 和 idx 的对应关系
            var semanticTitles = lines[0].Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, SemanticType> semantic_type_map_key_name;
            if (semanticMappingType == SemanticMappingType.Default)
            {
                semantic_type_map_key_name = semanticTypeDict_key_name_helper;
            }
            else
            {
                semantic_type_map_key_name = semanticManullyMappingTypeDict_key_name_helper;
            }

            semanticsIDX_helper = new SemanticType[semanticTitles.Length];
            Debug.Log($"semanticTitles : {lines[0]}");
            for (int i = 0; i < semanticTitles.Length; i++)
            {
                var title = semanticTitles[i];
                var semantics = title.Trim();
                if (semantic_type_map_key_name.TryGetValue(semantics, out SemanticType semanticType))
                {
                    semanticsIDX_helper[i] = semanticType;
                    //Debug.Log($"semantics : {semantics}, type : {semanticType}");
                }
                else
                {
                    Debug.LogWarning($"un-implements semantic : {semantics}");
                }
            }

            // jave.lin : 先根据 IDX 来排序还原 vertex buffer 的内容
            // lines[1~count-1] : 比如： 0, 0,  0.0402, -1.57095E-17,  0.12606, -0.97949,  0.00, -0.20056,  0.00,  0.1098,  0.83691, -0.53613,  1.00, -0.06058,  0.81738
            Dictionary<int, VertexInfo> vertex_dict_key_idx = new Dictionary<int, VertexInfo>();

            var idxList = new List<int>();
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var linesElements = line.Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);

                var idx = int.Parse(linesElements[1]);

                // jave.lin : 如果该 vertex 没有处理过，那么才去处理
                if (!vertex_dict_key_idx.TryGetValue(idx, out VertexInfo info))
                {
                    info = new VertexInfo();
                    vertex_dict_key_idx[idx] = info;
                    idxList.Add(idx);
                    // jave.lin : loop to fill the a2v field
                    for (int j = 0; j < linesElements.Length; j++)
                    {
                        var semanticType = semanticsIDX_helper[j];
                        FillVertexFieldInfo(info, semanticType, linesElements[j], is_from_DX_CSV);
                    }
                }
            }
            idxList.Sort();

            // jave.lin : 缩放、旋转、平移
            var rotation = Quaternion.Euler(vertexRotation);
            var TRS_mat = Matrix4x4.TRS(vertexOffset, rotation, vertexScale);
            // jave.lin : 法线变换矩阵需要特殊处理，针对 vertex scale 为非 uniform scale 的情况
            // ref : LearnGL - 11.5 - 矩阵04 - 法线从对象空间变换到世界空间
            // https://blog.csdn.net/linjf520/article/details/107501215
            var M_IT_mat = Matrix4x4.TRS(Vector3.zero, rotation, vertexScale).inverse.transpose;


            var vertices = new Vector3[vertex_dict_key_idx.Count];
            var normals = new Vector3[vertex_dict_key_idx.Count];
            var tangents = new Vector4[vertex_dict_key_idx.Count];
            var uv = new Vector2[vertex_dict_key_idx.Count];
            var uv2 = new Vector2[vertex_dict_key_idx.Count];
            var uv3 = new Vector2[vertex_dict_key_idx.Count];
            var uv4 = new Vector2[vertex_dict_key_idx.Count];
            var uv5 = new Vector2[vertex_dict_key_idx.Count];
            var uv6 = new Vector2[vertex_dict_key_idx.Count];
            var uv7 = new Vector2[vertex_dict_key_idx.Count];
            var uv8 = new Vector2[vertex_dict_key_idx.Count];
            var color0 = new Color[vertex_dict_key_idx.Count];

            Dictionary<int, int> vertexIdxIndex = new Dictionary<int, int>();
            for (int i = 0; i < idxList.Count; i++)
            {
                var idx = idxList[i];
                vertexIdxIndex[idx] = i;

                var info = vertex_dict_key_idx[idx];
                vertices[i] = TRS_mat * info.POSITION_H;
                normals[i] = M_IT_mat * info.NORMAL;
                tangents[i] = info.TANGENT;
                uv[i] = info.TEXCOORD0;
                uv2[i] = info.TEXCOORD1;
                uv3[i] = info.TEXCOORD2;
                uv4[i] = info.TEXCOORD3;
                uv5[i] = info.TEXCOORD4;
                uv6[i] = info.TEXCOORD5;
                uv7[i] = info.TEXCOORD6;
                uv8[i] = info.TEXCOORD7;
                color0[i] = info.COLOR0;
            }

            var indices = new List<int>();
            //todo:lcl
            // 修复中间顶点序号不连续的问题
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var linesElements = line.Split(line_element_splitor, StringSplitOptions.RemoveEmptyEntries);
                var idx = int.Parse(linesElements[1]);
                indices.Add(vertexIdxIndex[idx]);
            }

            // jave.lin : 设置 mesh 信息
            mesh.vertices = vertices;

            // jave.lin : 是否 reverse idx
            if (is_reverse_vertex_order) indices.Reverse();
            mesh.triangles = indices.ToArray();

            mesh.uv = has_uv0 ? uv : null;
            mesh.uv2 = has_uv1 ? uv2 : null;
            mesh.uv3 = has_uv2 ? uv3 : null;
            mesh.uv4 = has_uv3 ? uv4 : null;
            mesh.uv5 = has_uv4 ? uv5 : null;
            mesh.uv6 = has_uv5 ? uv6 : null;
            mesh.uv7 = has_uv6 ? uv7 : null;
            mesh.uv8 = has_uv7 ? uv8 : null;

            mesh.colors = has_color0 ? color0 : null;

            // jave.lin : AABB
            if (is_recalculate_bound)
            {
                mesh.RecalculateBounds();
            }

            // jave.lin : NORMAL
            switch (normalImportType)
            {
                case ModelImporterNormals.None:
                    // nop
                    break;
                case ModelImporterNormals.Import:
                    mesh.normals = normals.ToArray();
                    break;
                case ModelImporterNormals.Calculate:
                    mesh.RecalculateNormals();
                    break;
                default:
                    break;
            }

            // jave.lin : TANGENT
            switch (tangentImportType)
            {
                case ModelImporterTangents.None:
                    // nop
                    break;
                case ModelImporterTangents.Import:
                    mesh.tangents = tangents.ToArray();
                    break;
                case ModelImporterTangents.CalculateLegacy:
                case ModelImporterTangents.CalculateLegacyWithSplitTangents:
                case ModelImporterTangents.CalculateMikk:
                    mesh.RecalculateTangents();
                    break;
                default:
                    break;
            }
        }


    }
}
