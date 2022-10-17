#if TTP_REWARDED_ADS && !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{
    [InitializeOnLoad]
    public class RewardedAdsConfigurationDownloader : MonoBehaviour
    {

        private const string REWARDED_ADS_URL_ADDITION = "/rewardedads/";
        private const string REWARDED_ADS_JSON_FN = "rewardedAds";

        static RewardedAdsConfigurationDownloader()
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
            string url = domain + REWARDED_ADS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            bool result = TTPMenu.DownloadConfiguration(url, REWARDED_ADS_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("RewardedAdsConfigurationDownloader:: DownloadConfiguration: failed to download configuration.");
            }

        }
    }
}
#endif