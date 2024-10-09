using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace vietlabs.fr2
{
    internal static class FR2_Parser
    {
        public static void ReadYaml(string filePath, Action<string, long> callback)
        {
            Read(filePath, ParseLine_Yaml, callback);
        }

        public static void ReadJson(string filePath, Action<string, long> callback)
        {
            // Debug.Log($"ReadJson {filePath}");
            Read(filePath, ParseLine_Json, callback);
        }
        
        public static void ReadUssUxml(string filePath, Action<string, long> callback)
        {
            // Debug.Log($"Read {filePath}");
            Read(filePath, ParseLine_Uxml_Uss, callback);
        }

        public static void ReadTss(string filePath, Action<string, long> callback)
        {
            Read(filePath, ParseLine_Tss, callback);
        }
        
        private static void Read(string filePath, Func<string, (string, long)> lineHandler, Action<string, long> add)
        {
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.IndexOf("guid", StringComparison.Ordinal) == -1) continue;
                        line = line.Replace(" ", string.Empty);
                        var (guid, fileId) = lineHandler(line);
                        if (guid == null)
                        {
                            if (filePath.Contains("ProjectSettings/EditorBuildSettings.asset")) continue;
                            #if FR2_DEV
                            Debug.LogWarning($"Missing Ref?\n{line}\n{filePath}\n\n");
                            #endif
                            continue;
                        }
                        add(guid, fileId);
                    }
                }
            }
            
            #pragma warning disable CS0168
            catch (Exception e)
            {
                // ignored
                #if FR2_DEV
                Debug.LogWarning(e);
                #endif
            }
            #pragma warning restore CS0168
        }
        private static (string guid, long fileId) ParseLine_Yaml(string line)
        {
            return FindRef(line, "guid:", "fileID:", ",");
        }
        private static (string guid, long fileId) ParseLine_Json(string line)
        {
            string guid = Find(line, "\\\"guid\\\":\\\"", "\\\",");
            if (string.IsNullOrEmpty(guid)) return (null, -1);
            
            string fileIdStr = Find(line, "\"fileID\\\":", ",");
            if (string.IsNullOrEmpty(fileIdStr)) return (null, -1);
            
            if (!long.TryParse(fileIdStr, out long fileId)) fileId = -1;
            // Debug.Log($"Found: {guid}/{fileId}\t\t {line}");
            return (guid, fileId);
        }
        
        private static (string guid, long fileId) ParseLine_Tss(string line)
        {
            string assetPath = Find(line, "@importurl(\"/", "\")");
            return string.IsNullOrEmpty(assetPath)
                ? (null, -1)
                : (AssetDatabase.AssetPathToGUID(assetPath), 0);
        }
        
        private static (string guid, long fileId) ParseLine_Uxml_Uss(string line)
        {
            return FindRef(line, "guid=", "fileID=", "&");
        }

        private static (string guid, long fileId) FindRef(string source, string guidPattern, string fileIdPattern, string separatorPattern)
        {
            string guid = Find(source, guidPattern, separatorPattern);
            if (string.IsNullOrEmpty(guid)) return (null, -1);
            
            string fileIdStr = Find(source, fileIdPattern, separatorPattern);
            if (string.IsNullOrEmpty(fileIdStr)) return (null, -1);
            
            if (!long.TryParse(fileIdStr, out long fileId)) fileId = -1;
            // Debug.Log($"Found: {guid}/{fileId}\t\t {source}");
            return (guid, fileId);
        }
        
        private static string Find(string source, string str_begin, string str_end)
        {
            int st = source.IndexOf(str_begin, StringComparison.Ordinal);
            if (st == -1) return null;
            st += str_begin.Length;
            int ed = source.IndexOf(str_end, st, StringComparison.Ordinal);
            return ed == -1 ? null : source.Substring(st, ed - st);
        }
    }
}
