using System.Collections.Generic;
using Microsoft.Win32;

public class LcLEditorTools
{
    /// <summary>
    /// 获取应用程序路径
    /// </summary>
    /// <param name="appName"></param>
    /// <returns></returns>
    // public static string GetApplicationPath(string appName)
    // {
    //     List<RegistryKey> RegistryKeys = new List<RegistryKey>();
    //     RegistryKeys.Add(Registry.ClassesRoot);
    //     RegistryKeys.Add(Registry.CurrentConfig);
    //     RegistryKeys.Add(Registry.CurrentUser);
    //     RegistryKeys.Add(Registry.LocalMachine);
    //     RegistryKeys.Add(Registry.PerformanceData);
    //     RegistryKeys.Add(Registry.Users);
    //     string SubKeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + appName;

    //     foreach (RegistryKey registrykey in RegistryKeys)
    //     {
    //         using (RegistryKey subkey = registrykey.OpenSubKey(SubKeyName, false))
    //         {
    //             if (subkey == null)
    //                 return "";

    //             object path = subkey.GetValue("Path");

    //             if (path != null)
    //                 return (string)path;
    //         }
    //     }
    //     return "";
    // }

    /// <summary>
    /// 查找安装的软件
    /// %ProgramData%\Microsoft\Windows\Start Menu\Programs
    /// %AppData%\Microsoft\Windows\Start Menu\Programs
    /// </summary>
    /// <param name="appName"> 软件名称</param>
    /// <param name="appPath "> 安装路径</param>
    /// <returns> true or false </returns>
    public static bool GetApplicationPath(string appName, out string appPath)
    {
        appPath = null;
        List<RegistryKey> RegistryKeys = new List<RegistryKey>();
        RegistryKeys.Add(Registry.ClassesRoot);
        RegistryKeys.Add(Registry.CurrentConfig);
        RegistryKeys.Add(Registry.CurrentUser);
        RegistryKeys.Add(Registry.LocalMachine);
        RegistryKeys.Add(Registry.PerformanceData);
        RegistryKeys.Add(Registry.Users);
        Dictionary<string, string> softwares = new Dictionary<string, string>();
        string SubKeyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
        foreach (RegistryKey registrykey in RegistryKeys)
        {
            using (RegistryKey registryKey1 = registrykey.OpenSubKey(SubKeyName, false))
            {
                if (registryKey1 == null) // 判断对象不存在
                    continue;
                if (registryKey1.GetSubKeyNames() == null)
                    continue;
                string[] KeyNames = registryKey1.GetSubKeyNames();
                foreach (string KeyName in KeyNames) // 遍历子项名称的字符串数组
                {
                    using (RegistryKey RegistryKey2 = registryKey1.OpenSubKey(KeyName, false)) // 遍历子项节点
                    {
                        if (RegistryKey2 == null)
                            continue;
                        string name = RegistryKey2.GetValue("DisplayName", "").ToString(); // 获取软件名
                        string InstallLocation = RegistryKey2.GetValue("InstallLocation", "").ToString(); // 获取安装路径
                        if (!string.IsNullOrEmpty(InstallLocation) && !string.IsNullOrEmpty(name))
                        {
                            if (!softwares.ContainsKey(name))
                                softwares.Add(name, InstallLocation);
                        }
                    }
                }
            }
        }

        if (softwares.Count <= 0)
            return false;
        foreach (string name in softwares.Keys)
        {
            if (name.Contains(appName))
            {
                appPath = softwares[name];
                return true;
            }
        }

        return false;
    }
}
