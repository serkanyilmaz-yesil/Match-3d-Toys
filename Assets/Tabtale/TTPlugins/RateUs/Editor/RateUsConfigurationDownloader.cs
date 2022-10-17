#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{
    [InitializeOnLoad]
    public class RateUsConfigurationDownloader
    {
        private const string RATEUS_URL_ADDITION = "/rate-us/";
        private const string RATEUS_JSON_FN = "rateUs";

        static RateUsConfigurationDownloader()
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
            string url = domain + RATEUS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            string output = null;
            if(!TTPMenu.DownloadConfiguration(url, RATEUS_JSON_FN, out output))
            {
                Debug.LogWarning("RateUsConfigurationDownloader:: DownloadConfiguration: failed to download configuration. Wont download app icon");
                return;
            }
#if UNITY_ANDROID
            if(output != null)
            {
                var rateUsJson = SimpleJSON.JSON.Parse(output);
                if(rateUsJson != null)
                {
                    string iconUrl = rateUsJson["iconUrl"];
                    if(iconUrl != null)
                    {
                        string extenstion = iconUrl.Substring(iconUrl.LastIndexOf(".", System.StringComparison.InvariantCultureIgnoreCase)+1);
                        TTPEditorUtils.DownloadFile(iconUrl, "Assets/StreamingAssets/ttp/rateus/game_icon." + extenstion);
                    }
                }

            }
#endif
        }
    }
}
#endif