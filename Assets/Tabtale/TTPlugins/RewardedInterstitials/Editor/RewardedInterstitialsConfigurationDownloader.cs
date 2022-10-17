#if TTP_REWARDED_INTERSTITIALS && !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{
    [InitializeOnLoad]
    public class RewardedInterstitialsConfigurationDownloader
    {
        private const string URL_ADDITION = "/rewarded-inter/";
        private const string JSON_FN = "rewardedInterstitials";

        static RewardedInterstitialsConfigurationDownloader()
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
            string url = domain + URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            Debug.Log("RewardedInterstitialsConfigurationDownloader:url=" + url);
            bool result = TTPMenu.DownloadConfiguration(url, JSON_FN);
            if (!result)
            {
                Debug.LogWarning("RewardedInterstitialsConfigurationDownloader:: DownloadConfiguration: failed to download configuration.");
            }

        }
    }
}
#endif