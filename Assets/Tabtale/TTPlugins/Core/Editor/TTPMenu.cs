#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEditor.Build;
using System.IO;


namespace Tabtale.TTPlugins
{
    public enum TTPEnvironment
    {
        PRODUCTION, APPTESTING, STAGING
    }

    public class TTPMenu : MonoBehaviour
    {
        private const string PRODUCTION_DOMAIN = "http://ttplugins.ttpsdk.info";
        private const string PRODUCTION_APPSDB_DOMAIN = "http://appsdb.ttpsdk.info";
        private const string STAGING_DOMAIN = "http://ttplugins.ttpsdk-staging.info";
        private const string STAGING_APPSDB_DOMAIN = "http://appsdb.ttpsdk-staging.info";
        private const string APPTESTING_DOMAIN = "http://apptesting-ttplugins.ttpsdk-staging.info";
        private const string APPTESTING_APPSDB_DOMAIN = "http://tt-apptesting-appsdb.us-west-2.elasticbeanstalk.com";

#if UNITY_ANDROID && !UNITY_2021_1_OR_NEWER
        private const string CONFIGURATIONS_PATH = "Assets/Plugins/Android/assets/ttp/configurations";
#else
        private const string CONFIGURATIONS_PATH = "Assets/StreamingAssets/ttp/configurations";
#endif
        private const string APP_CONFIG_PATH = "Assets/StreamingAssets/app_config.json";

        public static event Action<string> OnDownloadConfigurationCommand;

        public static void DownloadAppConfigFile(string baseUrl)
        {
            string url = baseUrl + "/alm/title/" + PlayerSettings.applicationIdentifier;
            TTPEditorUtils.DownloadStringToFile(url, APP_CONFIG_PATH, true);
        }

        public static void DownloadConfigurations(string url)
        {
            if (OnDownloadConfigurationCommand != null)
            {
                OnDownloadConfigurationCommand(url);
            }
            else
            {
                Debug.Log("DownloadConfigurations:: OnDownloadConfigurationCommand are null");
            }
        }

        public static bool DownloadConfiguration(string url, string fileName)
        {
            if(TTPEditorUtils.DownloadStringToFile(url, CONFIGURATIONS_PATH + "/" + fileName + ".json"))
            {
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed to Download Configuration", "Failed to download configuration from the following url - " + url, "OK");
            }
            return false;
        }

        public static bool DownloadConfiguration(string url, string fileName, out string configurationOutput)
        {
            if (TTPEditorUtils.DownloadStringToFile(url, CONFIGURATIONS_PATH + "/" + fileName + ".json", out configurationOutput))
            {
                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("Failed to Download Configuration", "Failed to download configuration from the following url - " + url, "OK");
            }
            return false;
        }
    }

    public class PSDKDownloadFromCustomServerWindow : EditorWindow
    {
        private static string url; //static to retain the domain for next use

        void OnGUI()
        {
            GUILayout.Label("Select a custom domain:", EditorStyles.boldLabel);
            url = EditorGUILayout.TextField("Domain", url);
            EditorGUILayout.Space();
            if (GUILayout.Button("Download"))
            {
                TTPMenu.DownloadConfigurations(url);
                this.Close();
            }
        }
    }

    public class PSDKPreProcess : IPreprocessBuild
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                Debug.Log("ARG " + i + ": " + args[i]);
                if (args[i] == "-configEnv")
                {
                    string input = args[i + 1];
                    if (!input.StartsWith("-", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Debug.Log("TTPMenu: detected batch mode configEnv. will download configurations. env - " + input);
                        TTPMenu.DownloadConfigurations(MakeUrl(input));
                    }
                    else
                    {
                        Debug.Log("TTPMenu: detected batch mode configEnv, but env is not mentioned. param after configEnv - " + input);
                    }
                }
            }
        }

        private string MakeUrl(string domain)
        {
            string url = "";
            if(domain != null)
            {
                if (domain.Contains("://"))
                {
                    url = domain.Substring(domain.IndexOf("://", StringComparison.InvariantCultureIgnoreCase) + 3);
                    if (url.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        url = url.Substring(0, url.Length - 2);
                    }
                }
                else
                {
                    url = domain;
                }
                url = "http://" + url;
            }
            return url;
        }
    }
}
#endif