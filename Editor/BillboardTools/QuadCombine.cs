using System.Collections.Generic;
using System.IO;
using Autodesk.Fbx;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;

public class QuadCombine : EditorWindow
{
    [MenuItem("LcLTools/QuadCombine")]
    public static void OpenWindow()
    {
        GetWindow<QuadCombine>().Show();
    }
    private GameObject gameObject;
    private string savePath;

    private void OnEnable()
    {
        savePath = Path.Combine(Application.dataPath, "MyGame.fbx");
    }
    private void OnGUI()
    {

        gameObject = EditorGUILayout.ObjectField("合并父对象", gameObject, typeof(GameObject), true) as GameObject;
        savePath = EditorGUILayout.TextField("文件保存路径：", savePath);

        if (GUILayout.Button("合并Quad"))
        {
            CombineQuads();
        }
    }
    private void CombineQuads()
    {
        var meshfilters = gameObject.GetComponentsInChildren<MeshFilter>();
        if (meshfilters != null && meshfilters.Length > 0)
        {
            var centerOffset = new List<Vector4>(); //记录偏离向量的list

            var combineInstances = new CombineInstance[meshfilters.Length];
            for (int i = 0; i < meshfilters.Length; i++)
            {
                var mesh = meshfilters[i].sharedMesh;
                combineInstances[i] = new CombineInstance()
                {
                    mesh = mesh,
                    transform = meshfilters[i].transform.localToWorldMatrix
                };
                for (int j = 0; j < mesh.vertexCount; j++)
                {
                    //默认合并结构是，quad在一个父物体下，那么localPosition就是距离父物体中心（局部空间原点）的偏离向量。
                    // centerOffset.Add(meshfilters[i].transform.position);
                    centerOffset.Add(meshfilters[i].transform.localPosition);
                }
            }

            var newMesh = new Mesh();
            newMesh.CombineMeshes(combineInstances, true);

            //把偏移向量写入切线数据中
            newMesh.tangents = centerOffset.ToArray();

            // 保存文件
            var go = new GameObject(Path.GetFileNameWithoutExtension(savePath));
            go.AddComponent<MeshFilter>().sharedMesh = newMesh;
            ModelExporter.ExportObject(savePath, go);


            // 截取savePath为Unity项目中的相对路径
            savePath = savePath.Substring(savePath.IndexOf("Assets"));
            // 修改导入设置,使得切线不会被重新计算
            var importer = AssetImporter.GetAtPath(savePath) as ModelImporter;
            importer.importTangents = ModelImporterTangents.Import;
            importer.SaveAndReimport();

            AssetDatabase.Refresh();
            Debug.Log("保存文件到：" + savePath);

            DestroyImmediate(go);
        }

    }
}