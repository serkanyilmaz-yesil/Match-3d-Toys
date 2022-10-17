#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Tabtale.TTPlugins
{

	[InitializeOnLoad]
	public class CrashToolConfigurationDownloader
	{
		private const string CRASH_TOOL_URL_ADDITION = "/hockeyapp/";
		private const string CRASH_TOOL_JSON_FN = "crashMonitoringTool";

		static CrashToolConfigurationDownloader()
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
			string url = domain + CRASH_TOOL_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
			bool result = TTPMenu.DownloadConfiguration(url, CRASH_TOOL_JSON_FN);
			if (!result)
			{
				Debug.LogWarning("CrashToolConfigurationDownloader:: DownloadConfiguration: failed to download configuration for CrashTool settings.");
			}

		}
	}
}
#endif