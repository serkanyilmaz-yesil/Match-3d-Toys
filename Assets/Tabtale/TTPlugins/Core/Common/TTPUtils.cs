using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPUtils
    {

        public const string AssetBundlesOutputPath = "AssetBundles";

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }
        
        public static string ReadStreamingAssetsFile(string fileRelativePath)
        {
            byte[] data = ReadDataFromStreamingAssets(fileRelativePath);
            string str = null;
            if(data != null)
            {
                str = System.Text.Encoding.UTF8.GetString(data);
            }
            return str;
        }

        public static string CombinePaths(List<string> paths)
        {
            string result = "";
            foreach(string p in paths) 
            {
                result = Path.Combine(result, p);
            }
            return result;
        }

        public static void CopyDir(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            foreach (FileInfo fi in source.GetFiles())
            {
                Debug.LogWarning("Copying: '" + target.FullName + "' to: '" + fi.Name + "'");
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static byte[] ReadDataFromStreamingAssets(string fileRelativePath)
        {
            string assetsFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileRelativePath);
            try
            {
                if (assetsFilePath.Contains("jar:file") || assetsFilePath.Contains("://"))
                {
                    // Android
                    int timeout = 7; // seconds
                    System.DateTime startTime = System.DateTime.Now;

                    WWW www = new WWW(assetsFilePath);
                    Debug.Log("trying to read file: " + assetsFilePath);
                    while (!www.isDone)
                    {
                        System.TimeSpan interval = System.DateTime.Now - startTime;
                        if (interval.Seconds > timeout)
                            return null;
                    }
                    if (!System.String.IsNullOrEmpty(www.error))
                    {
                        return null;
                    }

                    return www.bytes;
                }
                else
                {
                    if (!System.IO.File.Exists(assetsFilePath))
                        return null;

                    return System.IO.File.ReadAllBytes(assetsFilePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        static string _bundleIdentifier = null;

        public static string BundleIdentifier
        {
            get
            {
                if (_bundleIdentifier != null)
                    return _bundleIdentifier;


#if UNITY_EDITOR
#if UNITY_5_6_OR_NEWER
#if UNITY_ANDROID
                return UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.Android);
#else
                             return UnityEditor.PlayerSettings.GetApplicationIdentifier( UnityEditor.BuildTargetGroup.iOS);
#endif
#else
                        return UnityEditor.PlayerSettings.bundleIdentifier;
#endif
#endif


#if UNITY_IOS
                // TODO: what to return?
                _bundleIdentifier = "what";
#elif UNITY_ANDROID

                using (AndroidJavaObject Context = CurrentActivity.Call<AndroidJavaObject>("getApplicationContext"))
                {
                    if (Context != null)
                    {
                        System.String packageName = Context.Call<string>("getPackageName");
                        using (AndroidJavaObject packageManager = Context.Call<AndroidJavaObject>("getPackageManager"))
                        {
                            if (packageManager != null)
                            {
                                try
                                {
                                    using (AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0))
                                    {
                                        if (packageManager != null)
                                        {
                                            _bundleIdentifier = packageInfo.Get<string>("packageName");
                                        }
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogException(e);
                                }
                            }
                        }
                    }
                }
#endif

                if (_bundleIdentifier != null)
                {
                    Debug.Log("Unity native bundle identifier: " + _bundleIdentifier);
                }
                else
                {
                    Debug.Log("Unity native bundle identifier NULL");
                }
                return _bundleIdentifier;


            }
        }

#if UNITY_ANDROID
        static AndroidJavaClass _unityPlayerJavaClass = null;
        public static AndroidJavaClass UnityPlayerJavaClass
        {
            get
            {
#if !UNITY_EDITOR
                if (_unityPlayerJavaClass == null)
                    _unityPlayerJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
#endif
                return _unityPlayerJavaClass;
            }
        }

        public static AndroidJavaObject CurrentActivity
        {
            get
            {

                if (UnityPlayerJavaClass == null) return null;

                return UnityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }
#endif

    }
}


