#if !CRAZY_LABS_CLIK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tabtale.TTPlugins.SimpleJSON;

namespace Tabtale.TTPlugins
{


    [InitializeOnLoad]
    public class BannersConfigurationDownloader
    {
        private const string BANNERS_HOUSE_ADS_FP = "Assets/StreamingAssets/ttp/houseads/houseAds.json";
        private const string BANNERS_HOUSE_ADS_TEMP_DOMAIN = "http://gateway.ttpsdk.info";
        private const string BANNERS_URL_ADDITION = "/banners/";
        private const string BANNERS_JSON_FN = "banners";
        private const string BANNERS_HOUSE_ADS_URL_ADDITION = "/houseads/";
#if UNITY_ANDROID && !UNITY_2021_1_OR_NEWER
         private const string BANNER_ZIP_PATH = "Assets/Plugins/Android/assets/ttp/houseads/houseads.zip";
#else
         private const string BANNER_ZIP_PATH = "Assets/StreamingAssets/ttp/houseads/houseads.zip";
#endif
        static BannersConfigurationDownloader()
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
            string url = domain + BANNERS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            Debug.Log("BannersConfigurationDownloader:: DownloadConfiguration: download config from url: " + url);

            bool result = TTPMenu.DownloadConfiguration(url, BANNERS_JSON_FN);
            if (!result)
            {
                Debug.LogWarning("BannersConfigurationDownloader:: DownloadConfiguration: failed to download configuration.");
            }

            string houseAdsConfigUrl = domain + BANNERS_HOUSE_ADS_URL_ADDITION + store + "/" + PlayerSettings.applicationIdentifier;
            Debug.Log("BannersConfigurationDownloader:: DownloadConfiguration: houseAdsConfigUrl: " + houseAdsConfigUrl);
            string houseAdsJson = null;
            if(TTPEditorUtils.DownloadStringToFile(houseAdsConfigUrl, BANNERS_HOUSE_ADS_FP, out houseAdsJson))
            {
                var houseAdsConfig = JSON.Parse(houseAdsJson);
                if(houseAdsConfig != null)
                {
                    var apps = houseAdsConfig["apps"];
                    if(apps != null)
                    {
						if (System.IO.File.Exists("Assets/StreamingAssets/ttp/houseads/houseads.zip"))
						{
							System.IO.File.Delete("Assets/StreamingAssets/ttp/houseads/houseads.zip");
						}

						foreach (var app in apps.Values)
                        {
                            string thumbnailUrl = app["thumbnail"];
                            if (thumbnailUrl != null)
                            {
                                Debug.Log("BannersConfigurationDownloader:: DownloadConfiguration: download thumbnail form url: " + thumbnailUrl);
                                string fn = thumbnailUrl.Substring(thumbnailUrl.LastIndexOf("/") + 1);
                                if(TTPEditorUtils.DownloadFile(thumbnailUrl, "Assets/StreamingAssets/ttp/houseads/" + fn))
                                {
                                    app["thumbnail"] = System.IO.Path.GetFileName(thumbnailUrl);
                                }
                            }
                        }
                        System.IO.File.WriteAllText(BANNERS_HOUSE_ADS_FP, houseAdsConfig.ToString());

                        string[] files = ZipUtil.ZipDirectoryContents(BANNER_ZIP_PATH, "Assets/StreamingAssets/ttp/houseads");
                        if (files != null)
                        {
                            foreach (string path in files)
                            {
                                Debug.Log("BannersConfigurationDownloader:: DownloadConfiguration: delete file from path: " + path);
                                System.IO.File.Delete(path);
                            }
                        } else {
                            Debug.LogWarning("BannersConfigurationDownloader:: DownloadConfiguration: NO files in houseads dir.");
                        }
                    } else {
                        Debug.LogWarning("BannersConfigurationDownloader:: DownloadConfiguration: there're NO apps in JSON.");
                    }
				} else {
                    Debug.LogWarning("BannersConfigurationDownloader:: DownloadConfiguration: JSON parse of houseAdsConfig failed.");
                }
            }
        }
    }
}
#endif