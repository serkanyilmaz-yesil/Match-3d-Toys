#if TTP_PROMOTION && !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{

    [InitializeOnLoad]
    public class PromotionConfigurationDownloader
    {
        private const string PROMOTION_URL_ADDITION = "/stand/build-config/v1/";
        private const string PROMOTION_JSON_FN = "promotion";

        static PromotionConfigurationDownloader()
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
            string url = domain + PROMOTION_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            bool result = TTPMenu.DownloadConfiguration(url, PROMOTION_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("PromotionConfigurationDownloader:: DownloadConfiguration: failed to download configuration for promotion.");
            }

        }
    }
}
#endif