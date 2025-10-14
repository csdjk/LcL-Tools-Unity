using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SDFTextureGenerator : EditorWindow
{
    private string inputDirectory = "";
    private string outputDirectory = "";
    public ComputeShader sdfGenerator;
    private int textureSize = 128;

    // 通道选项
    private enum SourceChannel
    {
        Red,
        Green,
        Blue,
        Alpha,
        Luminance
    }

    private SourceChannel sourceChannel = SourceChannel.Luminance;
    private bool invertSource = false;
    private float threshold = 0.5f;

    // 用于映射SDF值的参数
    private float scaleFactor = 0.004f; // 默认值，映射到C++算法

    // 插值帧数
    private int lerpFrames = 255;

    private enum InterpolationMethod
    {
        AccumulatedResult,
        SequenceFrames
    }

    private InterpolationMethod interpolationMethod = InterpolationMethod.AccumulatedResult;


    [MenuItem("LcLTools/SDF Texture Generator")]
    public static void ShowWindow()
    {
        GetWindow<SDFTextureGenerator>("SDF生成器");
    }

    private void OnGUI()
    {
        GUILayout.Label("SDF纹理生成工具", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        inputDirectory = EditorGUILayout.TextField("输入目录:", inputDirectory);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            inputDirectory = EditorUtility.OpenFolderPanel("选择输入图片目录", "", "");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        outputDirectory = EditorGUILayout.TextField("输出目录:", outputDirectory);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            outputDirectory = EditorUtility.OpenFolderPanel("选择输出目录", "", "");
        }

        EditorGUILayout.EndHorizontal();

        sdfGenerator = (ComputeShader)EditorGUILayout.ObjectField("SDF Compute Shader:", sdfGenerator, typeof(ComputeShader), false);
        textureSize = EditorGUILayout.IntSlider("纹理尺寸:", textureSize, 32, 1024);

        // 源通道选择
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("源数据设置", EditorStyles.boldLabel);
        sourceChannel = (SourceChannel)EditorGUILayout.EnumPopup("使用通道:", sourceChannel);
        invertSource = EditorGUILayout.Toggle("反转通道值:", invertSource);
        threshold = EditorGUILayout.Slider("二值化阈值:", threshold, 0f, 1f);

        // SDF参数
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("SDF参数", EditorStyles.boldLabel);
        scaleFactor = EditorGUILayout.Slider("缩放因子:", scaleFactor, 0.001f, 0.02f);
        EditorGUILayout.HelpBox("缩放因子控制SDF的对比度。较小的值会产生更平滑的过渡，较大的值会产生更锐利的边界。\n对应C++算法的值约为0.004。", MessageType.Info);

        // 插值参数
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("插值设置", EditorStyles.boldLabel);
        lerpFrames = EditorGUILayout.IntSlider("插值帧数:", lerpFrames, 10, 255);


        // 在OnGUI中添加选项
        interpolationMethod = (InterpolationMethod)EditorGUILayout.EnumPopup("插值方法:", interpolationMethod);

        EditorGUILayout.Space();
        if (GUILayout.Button("生成SDF纹理"))
        {
            if (string.IsNullOrEmpty(inputDirectory) || string.IsNullOrEmpty(outputDirectory) || sdfGenerator == null)
            {
                EditorUtility.DisplayDialog("错误", "请确保已选择输入目录、输出目录和Compute Shader", "确定");
                return;
            }

            GenerateSDFTextures();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("处理单张测试图像"))
        {
            string imagePath = EditorUtility.OpenFilePanel("选择测试图像", "", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(imagePath) && !string.IsNullOrEmpty(outputDirectory) && sdfGenerator != null)
            {
                ProcessSingleImage(imagePath);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "请确保已选择有效的图像文件、输出目录和Compute Shader", "确定");
            }
        }
    }

    private void ProcessSingleImage(string imagePath)
    {
        EditorUtility.DisplayProgressBar("处理测试图像", "生成SDF...", 0.5f);

        try
        {
            // 加载图像
            Texture2D sourceTexture = LoadTextureFromFile(imagePath);
            if (sourceTexture == null)
            {
                EditorUtility.DisplayDialog("错误", "无法加载图像文件", "确定");
                return;
            }

            // 确保输出目录存在
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            string fileName = Path.GetFileNameWithoutExtension(imagePath);

            // 调整大小
            Texture2D resizedTexture = ResizeTexture(sourceTexture, textureSize, textureSize);

            // 创建二值化纹理 - 从所选通道提取数据
            Texture2D binaryTexture = CreateBinaryTexture(resizedTexture);

            // 创建SDF
            RenderTexture sdfTexture = GenerateSingleSDF(binaryTexture);

            // 保存SDF
            SaveRenderTextureToFile(sdfTexture, Path.Combine(outputDirectory, fileName + "_SDF.png"));

            // 清理
            sdfTexture.Release();
            DestroyImmediate(sourceTexture);
            DestroyImmediate(resizedTexture);
            DestroyImmediate(binaryTexture);

            EditorUtility.DisplayDialog("成功", "已处理测试图像并保存到输出目录", "确定");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", "处理图像时发生错误: " + e.Message, "确定");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
    }

    // 从源纹理创建二值化纹理，使用所选通道作为源数据
    private Texture2D CreateBinaryTexture(Texture2D source)
    {
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                Color pixel = source.GetPixel(x, y);
                float value = 0;

                // 提取指定通道的值
                switch (sourceChannel)
                {
                    case SourceChannel.Red:
                        value = pixel.r;
                        break;
                    case SourceChannel.Green:
                        value = pixel.g;
                        break;
                    case SourceChannel.Blue:
                        value = pixel.b;
                        break;
                    case SourceChannel.Alpha:
                        value = pixel.a;
                        break;
                    case SourceChannel.Luminance:
                        // 计算亮度 (0.299R + 0.587G + 0.114B)
                        value = pixel.r * 0.299f + pixel.g * 0.587f + pixel.b * 0.114f;
                        break;
                }

                // 如果需要反转值
                if (invertSource)
                    value = 1 - value;

                // 二值化
                float binary = value >= threshold ? 1 : 0;

                // 存储二值化结果
                Color resultColor = new Color(binary, binary, binary, 1);
                result.SetPixel(x, y, resultColor);
            }
        }

        result.Apply();
        return result;
    }

    private RenderTexture GenerateSingleSDF(Texture2D sourceTexture)
    {
        // 创建输入和输出纹理
        RenderTexture inputTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        inputTexture.enableRandomWrite = true;
        inputTexture.Create();

        Graphics.Blit(sourceTexture, inputTexture);

        RenderTexture outputTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        outputTexture.enableRandomWrite = true;
        outputTexture.Create();

        // 设置计算着色器参数
        int generateKernel = sdfGenerator.FindKernel("GenerateSDF");
        sdfGenerator.SetTexture(generateKernel, "InputTexture", inputTexture);
        sdfGenerator.SetTexture(generateKernel, "OutputSDF", outputTexture);
        sdfGenerator.SetInt("TextureWidth", textureSize);
        sdfGenerator.SetInt("TextureHeight", textureSize);
        sdfGenerator.SetFloat("ScaleFactor", scaleFactor);

        // 调度计算着色器
        sdfGenerator.Dispatch(generateKernel, Mathf.CeilToInt(textureSize / 8f), Mathf.CeilToInt(textureSize / 8f), 1);

        inputTexture.Release();
        DestroyImmediate(inputTexture);

        return outputTexture;
    }

    private void GenerateSDFTextures()
    {
        // 获取所有图像文件
        string[] imageFiles = Directory.GetFiles(inputDirectory)
            .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg"))
            .OrderBy(file => Path.GetFileNameWithoutExtension(file))
            .ToArray();

        if (imageFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "没有找到有效的图像文件", "确定");
            return;
        }

        // 创建输出目录（如果不存在）
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // 处理每个图像
        List<Texture2D> sdfTextures = new List<Texture2D>();
        for (int i = 0; i < imageFiles.Length; i++)
        {
            EditorUtility.DisplayProgressBar("SDF生成进度", $"处理图片 {i + 1}/{imageFiles.Length}", (float)i / imageFiles.Length);

            try
            {
                // 加载图像
                Texture2D sourceTexture = LoadTextureFromFile(imageFiles[i]);
                if (sourceTexture == null) continue;

                string fileName = Path.GetFileNameWithoutExtension(imageFiles[i]);

                // 调整大小
                Texture2D resizedTexture = ResizeTexture(sourceTexture, textureSize, textureSize);

                // 创建二值化纹理
                Texture2D binaryTexture = CreateBinaryTexture(resizedTexture);

                // 生成SDF
                RenderTexture sdfRT = GenerateSingleSDF(binaryTexture);

                // 转换为Texture2D并保存
                Texture2D sdfTexture = ConvertRenderTextureToTexture2D(sdfRT);
                sdfTextures.Add(sdfTexture);

                // 保存SDF
                SaveTextureToFile(sdfTexture, Path.Combine(outputDirectory, fileName + "_SDF.png"));

                // 清理
                sdfRT.Release();
                DestroyImmediate(sourceTexture);
                DestroyImmediate(resizedTexture);
                DestroyImmediate(binaryTexture);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理图像 {imageFiles[i]} 时出错: {e.Message}");
            }
        }

        // 处理插值（如果需要）
        if (sdfTextures.Count > 1)
        {
            if (interpolationMethod == InterpolationMethod.SequenceFrames)
                ProcessInterpolation(sdfTextures);
            else
                ProcessInterpolation2(sdfTextures);
        }

        // 清理SDF纹理
        foreach (var texture in sdfTextures)
        {
            DestroyImmediate(texture);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", "SDF纹理生成完毕", "确定");
    }

    private void ProcessInterpolation(List<Texture2D> sdfTextures)
    {
        if (sdfTextures.Count < 2)
            return;

        try
        {
            // 创建SDF插值目录
            string sdfLerpFolder = Path.Combine(outputDirectory, "SDF_Lerp");
            if (!Directory.Exists(sdfLerpFolder))
            {
                Directory.CreateDirectory(sdfLerpFolder);
            }

            int levelStep = lerpFrames / (sdfTextures.Count - 1);
            int lerpStep = 0;
            int curTexIndex = 0;
            int nextTexIndex = 1;

            Texture2D lerpedTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

            // 参照C++代码中的插值方式
            for (int i = 0; i < lerpFrames; i++)
            {
                EditorUtility.DisplayProgressBar("SDF插值进度", $"生成插值帧 {i + 1}/{lerpFrames}", (float)i / lerpFrames);

                // 检查索引是否有效
                if (nextTexIndex >= sdfTextures.Count)
                    break;

                float weight = (float)lerpStep / levelStep;

                // 执行像素级插值
                for (int y = 0; y < textureSize; y++)
                {
                    for (int x = 0; x < textureSize; x++)
                    {
                        // 获取当前纹理和下一个```csharp
                        // 获取当前纹理和下一个纹理的像素值
                        Color curPixel = sdfTextures[curTexIndex].GetPixel(x, y);
                        Color nextPixel = sdfTextures[nextTexIndex].GetPixel(x, y);

                        // 转换为灰度值 (使用红色通道，因为SDF是灰度图)
                        int curValue = Mathf.RoundToInt(curPixel.r * 255);
                        int nextValue = Mathf.RoundToInt(nextPixel.r * 255);

                        // 执行线性插值
                        int lerpValue = Mathf.RoundToInt(nextValue * weight + curValue * (1 - weight));

                        // 二值化操作，与C++代码相同
                        int result = lerpValue < 127 ? 255 : 0;

                        // 设置结果像素
                        float normalizedResult = result / 255f;
                        lerpedTexture.SetPixel(x, y, new Color(normalizedResult, normalizedResult, normalizedResult));
                    }
                }

                lerpedTexture.Apply();

                // 保存插值后的图像
                string imageName = $"SDF_{i:D3}.png";
                SaveTextureToFile(lerpedTexture, Path.Combine(sdfLerpFolder, imageName));

                // 更新插值步骤
                lerpStep++;
                if (lerpStep >= levelStep)
                {
                    lerpStep = 0;
                    curTexIndex++;
                    nextTexIndex++;
                }
            }

            DestroyImmediate(lerpedTexture);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"插值处理过程中出错: {e.Message}");
        }
    }

    private void ProcessInterpolation2(List<Texture2D> sdfTextures)
    {
        if (sdfTextures.Count < 2)
            return;

        try
        {
            // 创建SDF插值目录
            string sdfLerpFolder = Path.Combine(outputDirectory, "SDF_Lerp");
            if (!Directory.Exists(sdfLerpFolder))
            {
                Directory.CreateDirectory(sdfLerpFolder);
            }

            int levelStep = 255 / (sdfTextures.Count - 1);

            // 创建一个单一的输出纹理
            Texture2D resultTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

            // 进度显示
            int totalPixels = textureSize * textureSize;
            int processedPixels = 0;

            // 处理每个像素位置
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    // 对于每个像素，处理整个插值序列
                    int lerpStep = 0;
                    int curTexIndex = 0;
                    int nextTexIndex = 1;
                    int accumulatedResult = 0;

                    // 在这个像素位置执行完整的插值序列
                    for (int i = 0; i < 255; i++)
                    {
                        // 检查索引是否有效
                        if (nextTexIndex >= sdfTextures.Count)
                            break;

                        float weight = (float)lerpStep / levelStep;

                        // 获取当前和下一个纹理的像素值
                        Color curPixel = sdfTextures[curTexIndex].GetPixel(x, y);
                        Color nextPixel = sdfTextures[nextTexIndex].GetPixel(x, y);

                        // 转换为整数值 (0-255)
                        int curValue = Mathf.RoundToInt(curPixel.r * 255);
                        int nextValue = Mathf.RoundToInt(nextPixel.r * 255);

                        // 注意：权重的使用方式与之前相反
                        int lerpPixel = Mathf.RoundToInt(curValue * weight + nextValue * (1 - weight));

                        // 二值化并累加结果 (新算法的关键区别)
                        accumulatedResult += (lerpPixel > 127) ? 0 : 1;

                        // 更新插值步骤
                        lerpStep++;
                        if (lerpStep >= levelStep)
                        {
                            lerpStep = 0;
                            curTexIndex++;
                            nextTexIndex++;
                        }
                    }

                    // 设置像素的最终结果
                    float normalizedResult = accumulatedResult / 255f;
                    resultTexture.SetPixel(x, y, new Color(normalizedResult, normalizedResult, normalizedResult));

                    // 更新进度
                    processedPixels++;
                    if (processedPixels % 1000 == 0 || processedPixels == totalPixels)
                    {
                        EditorUtility.DisplayProgressBar("SDF插值进度",
                            $"处理像素 {processedPixels}/{totalPixels}",
                            (float)processedPixels / totalPixels);
                    }
                }
            }

            // 应用更改并保存最终图像
            resultTexture.Apply();
            SaveTextureToFile(resultTexture, Path.Combine(sdfLerpFolder, "SDF.png"));

            DestroyImmediate(resultTexture);
            EditorUtility.ClearProgressBar();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"插值处理过程中出错: {e.Message}");
            EditorUtility.ClearProgressBar();
        }
    }

    // 实用方法
    private Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(fileData);
        return texture;
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    private Texture2D ConvertRenderTextureToTexture2D(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        return tex;
    }

    private void SaveRenderTextureToFile(RenderTexture rt, string filePath)
    {
        Texture2D tex = ConvertRenderTextureToTexture2D(rt);
        SaveTextureToFile(tex, filePath);
        DestroyImmediate(tex);
    }

    private void SaveTextureToFile(Texture2D texture, string filePath)
    {
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
    }
}
