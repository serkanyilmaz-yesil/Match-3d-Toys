#if TTP_INTERSTITIALS && !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{
    [InitializeOnLoad]
    public class InterstitialsConfigurationDownloader
    {
        private const string INTERSTITIALS_URL_ADDITION = "/interstitials/";
        private const string INTERSTITIALS_JSON_FN = "interstitials";

        static InterstitialsConfigurationDownloader()
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
            string url = domain + INTERSTITIALS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            bool result = TTPMenu.DownloadConfiguration(url, INTERSTITIALS_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("InterstitialsConfigurationDownloader:: DownloadConfiguration: failed to download configuration.");
            }

        }
    }
}
#endif