#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{

    [InitializeOnLoad]
    public class AnalyticsConfigurationDownloader
    {
        private const string ANALYTICS_URL_ADDITION = "/analytics-build/";
        private const string ANALYTICS_JSON_FN = "analytics";

        static AnalyticsConfigurationDownloader()
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
            string url = domain + ANALYTICS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            bool result = TTPMenu.DownloadConfiguration(url, ANALYTICS_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("PrivacySettingsConfigurationDownloader:: DownloadConfiguration: failed to download configuration for privacy settings.");
            }
            
        }
    }
}
#endif