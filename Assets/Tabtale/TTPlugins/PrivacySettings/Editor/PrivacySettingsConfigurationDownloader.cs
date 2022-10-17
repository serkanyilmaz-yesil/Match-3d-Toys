#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{

    [InitializeOnLoad]
    public class PrivacySettingsConfigurationDownloader
    {
        private const string PRIVACY_SETTINGS_URL_ADDITION = "/privacy/build-config/v1/";
        private const string PRIVACY_SETTINGS_JSON_FN = "privacySettings";

        static PrivacySettingsConfigurationDownloader()
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
            string url = domain + PRIVACY_SETTINGS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;

            for(int tries = 0; tries < 3; tries++)
			{
				bool result = TTPMenu.DownloadConfiguration(url, PRIVACY_SETTINGS_JSON_FN);
				if (result)
				{
					DownloadLocalConsentForm downloadLocalConsentForm = new DownloadLocalConsentForm();
					downloadLocalConsentForm.DownloadLocalConsentForms(EditorUserBuildSettings.activeBuildTarget);
					return;
				}
				else
				{
					UnityEngine.Debug.LogWarning("Unity build returned with error: PrivacySettingsConfigurationDownloader:: DownloadConfiguration: failed to download configuration for privacy settings. retrying.");
				}
			}
			if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
			{
				UnityEngine.Debug.LogError("Unity build returned with error: PrivacySettingsConfigurationDownloader:: DownloadConfiguration: failed to download configuration for privacy settings. failing build");
				EditorApplication.Exit(-1);
			}
		}
    }
}
#endif