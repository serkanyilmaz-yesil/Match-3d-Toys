#if TTP_OPENADS && !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{
    [InitializeOnLoad]
    public class OpenAdsConfigurationDownloader
    {
        private const string OPENADS_URL_ADDITION = "/app-open/";
        private const string OPENADS_JSON_FN = "openads";

        static OpenAdsConfigurationDownloader()
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
            string url = domain + OPENADS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            Debug.Log("OpenAdsConfigurationDownloader:url=" + url);
            bool result = TTPMenu.DownloadConfiguration(url, OPENADS_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("OpenAdsConfigurationDownloader:: DownloadConfiguration: failed to download configuration.");
            }

        }
    }
}
#endif
