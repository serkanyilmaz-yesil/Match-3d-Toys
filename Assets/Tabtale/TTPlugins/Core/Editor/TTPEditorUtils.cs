using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;

namespace Tabtale.TTPlugins
{
    public class TTPEditorUtils : MonoBehaviour
    {
        public static void DefineSymbol(string symbol)
        {
            string currAndroid = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (!currAndroid.Contains(symbol))
            {
                currAndroid += ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, currAndroid);
            }
            string currIOS = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
            if (!currIOS.Contains(symbol))
            {
                currIOS += ";" + symbol;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, currIOS);
            }
        }

        private static void EnsureFilePath(string filePath)
        {
            if (File.Exists(filePath))
            {
                Debug.LogWarning("TTPEditorUtils::EnsureFilePath: file already exists, replacing - " + filePath);
                File.Delete(filePath);
            }

            if (filePath.Contains("/"))
            {
                string dirPath = filePath.Substring(0, filePath.LastIndexOf("/", System.StringComparison.CurrentCultureIgnoreCase));
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
        }

        public static bool DownloadFile(string url, string filePath)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            try
            {
                byte[] data = webClient.DownloadData(url);
                if (data != null)
                {
                    Debug.Log("TTPEditorUtils::DownloadFile: downloaded data");
                    EnsureFilePath(filePath);
                    File.WriteAllBytes(filePath, data);
                    Debug.Log("TTPEditorUtils::DownloadStringToFile: written string to path  - " + filePath);
                    return true;

                }
                else
                {
                    Debug.LogWarning("TPEditorUtils::DownloadStringToFile: failed to dowload string");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("TPEditorUtils::DownloadStringToFile: failed to dowload string. exception - " + e.Message);
            }
            return false;
        }


        public static bool DownloadStringToFile(string url, string filePath, bool appsDbConfiguration = false)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            if (appsDbConfiguration)
            {
#if !CRAZY_LABS_CLIK
                TTPAppsDBConfigurator.AddAppsDBAuthorizationHeader(webClient);
#endif
            }
            try
            {
                string str = webClient.DownloadString(url);
                if (str != null)
                {
                    Debug.Log("TTPEditorUtils::DownloadStringToFile: downloaded string - " + str);
                    EnsureFilePath(filePath);
                    File.WriteAllText(filePath, str);
                    Debug.Log("TTPEditorUtils::DownloadStringToFile: written string to path  - " + filePath);
                    return true;

                }
                else
                {
                    Debug.LogWarning("TPEditorUtils::DownloadStringToFile: failed to dowload string");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("TPEditorUtils::DownloadStringToFile: failed to dowload string. exception - " + e.Message);
            }
            return false;
        }

        public static bool DownloadStringToFile(string url, string filePath, out string resStr)
        {
            resStr = null;
            System.Net.WebClient webClient = new System.Net.WebClient();
            try
            {
                resStr = webClient.DownloadString(url);
                if (resStr != null)
                {
                    Debug.Log("TTPEditorUtils::DownloadStringToFile: downloaded string - " + resStr);
                    EnsureFilePath(filePath);
                    File.WriteAllText(filePath, resStr);
                    Debug.Log("TTPEditorUtils::DownloadStringToFile: written string to path  - " + filePath);
                    return true;

                }
                else
                {
                    Debug.LogWarning("TPEditorUtils::DownloadStringToFile: failed to dowload string");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("TPEditorUtils::DownloadStringToFile: failed to dowload string. exception - " + e.Message);
            }
            return false;
        }

        public static bool DownloadStringToJsonKeyInFile(string url, string filePath, string jsonKey, out string resStr)
        {
            System.Net.WebClient webClient = new System.Net.WebClient();
            try
            {
                string str = webClient.DownloadString(url);
                if (str != null)
                {
                    string modified = "{\"" + jsonKey + "\":" + str + "}";
                    Debug.Log("TTPEditorUtils::DownloadStringToJsonKeyInFile: downloaded and modified string - " + str);
                    EnsureFilePath(filePath);
                    resStr = modified;
                    File.WriteAllText(filePath, modified);
                    Debug.Log("TTPEditorUtils::DownloadStringToJsonKeyInFile: written string to path  - " + filePath);
                    return true;

                }
                else
                {
                    Debug.LogWarning("TPEditorUtils::DownloadStringToJsonKeyInFile: failed to dowload string");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("TPEditorUtils::DownloadStringToJsonKeyInFile: failed to dowload string. exception - " + e.Message);
            }
            resStr = null;
            return false;
        }
    }
}


