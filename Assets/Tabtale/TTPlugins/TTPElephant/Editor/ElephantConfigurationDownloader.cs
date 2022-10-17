#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{
	[InitializeOnLoad]
	public class ElephantConfigurationDownloader {

		private const string ELEPHANT_URL_ADDITION = "/elephant/build-config/v1/";
		private const string ELEPHANT_JSON_FN = "elephant";

		static ElephantConfigurationDownloader()
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
			string url = domain + ELEPHANT_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
			bool result = TTPMenu.DownloadConfiguration(url, ELEPHANT_JSON_FN);
			if (!result)
			{
				Debug.LogWarning("ElephantConfigurationDownloader:: DownloadConfiguration: failed to download configuration.");
			}

		}
	}
}
#endif