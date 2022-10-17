#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{
    [InitializeOnLoad]
    public class PopUpsMgrConfigurationDownloader : MonoBehaviour
    {

        private const string POPUPMGR_URL_ADDITION = "/popup-manager/";
        private const string POPUPMGR_JSON_FN = "popupsMgr";

        static PopUpsMgrConfigurationDownloader()
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
            string url = domain + POPUPMGR_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            bool result = TTPMenu.DownloadConfiguration(url, POPUPMGR_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("PopUpsMgrConfigurationDownloader:: DownloadConfiguration: failed to download configuration.");
            }

        }
    }
}
#endif
