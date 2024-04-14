using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;

namespace LcLToolsUnity
{

    public class LcLEditorWindow : Editor
    {
        [MenuItem("Assets/LcL Image Tools/垂直翻转图片", false, 1)]
        public static void FlipImageVertically()
        {
            Texture2D image = Selection.activeObject as Texture2D;
            if (image == null)
            {
                Debug.LogError("No image selected!");
                return;
            }

            Texture2D flippedImage = new Texture2D(image.width, image.height);

            for (int i = 0; i < image.width; i++)
            {
                for (int j = 0; j < image.height; j++)
                {
                    flippedImage.SetPixel(i, image.height - j - 1, image.GetPixel(i, j));
                }
            }

            flippedImage.Apply();

            // Save the flipped image to a file
            byte[] bytes = flippedImage.EncodeToPNG();
            File.WriteAllBytes(AssetDatabase.GetAssetPath(image), bytes);
            AssetDatabase.Refresh();
        }

    }
}