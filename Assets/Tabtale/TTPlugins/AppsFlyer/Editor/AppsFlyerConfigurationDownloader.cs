#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{

    [InitializeOnLoad]
    public class AppsFlyerConfigurationDownloader
    {
        private const string APPSFLYER_URL_ADDITION = "/appsflyer/";
        private const string APPSFLYER_JSON_FN = "appsFlyer";

        static AppsFlyerConfigurationDownloader()
        {
            TTPMenu.OnDownloadConfigurationCommand += DownloadConfiguration;
        }

        private static void DownloadConfiguration(string domain)
        {
            string store = "google";
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                store = "apple";
            }
            string url = domain + APPSFLYER_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            bool result = TTPMenu.DownloadConfiguration(url, APPSFLYER_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("AppsFlyerConfigurationDownloader:: DownloadConfiguration: failed to download configuration for privacy settings.");
            }
            
        }
    }
}
#endif