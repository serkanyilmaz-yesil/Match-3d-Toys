#if TTP_CORE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Tabtale.TTPlugins
{
	public class TTPOnDemandsResources
	{
        private static OnDemandResourcesDelegate _odrDelegate;
        private static OnDemandResourcesDelegate Delegate
        {
            get
            {
                if (_odrDelegate == null)
                {
                    GameObject ttpGo = GameObject.Find("TTPluginsGameObject");
                    if (ttpGo != null)
                    {
                        _odrDelegate = ttpGo.GetComponent<OnDemandResourcesDelegate>();
                    }
                }
                return _odrDelegate;
            }
        }

        public class DownloadResult
        {
            public bool success;
            public string bundleName;
            public int errorCode;
            public AssetBundle assetBundle;
            
        }

        private class DownloadItem
        {
            public DownloadItem(string bundleName, System.Action<DownloadResult> action, bool shouldLoad)
            {
                this.bundleName = bundleName;
                this.action = action;
                this.shouldLoad = shouldLoad;
            }


            public string bundleName;
            public System.Action<DownloadResult> action;
            public bool shouldLoad;
        }

        private static List<DownloadItem> _actionsList = new List<DownloadItem>();
        

        private static ITTPOnDemandResources _impl;
        private static ITTPOnDemandResources Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.DevMode)
                    {
                        _impl = new EditorImpl();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
                        UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
                    {
#if UNITY_ANDROID
                        _impl = new AndroidImpl();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
						_impl = new IosImpl();
#endif
                    }
                    else
                    {
                        _impl = new EditorImpl();
                    }
                }
                if (_impl == null)
                {
                    Debug.LogError("TTPOnDemandsResources::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

		public static void DownloadBundle(string bundleName, System.Action<DownloadResult> downloadFinished, bool shouldLoad = false)
		{
            if(downloadFinished != null)
            {
                _actionsList.Add(new DownloadItem(bundleName, downloadFinished, shouldLoad));
            }
            if (Impl != null)
            {
                Impl.DownloadBundle(bundleName);
            }
        }

        public static bool IsBundleInstalled(string bundleName)
        {
            if(Impl != null)
            {
                return Impl.IsBundleInstalled(bundleName);
            }
            return false;
        }

        public static AssetBundle GetAssetBundle(string bundleName)
        {
            if(Impl != null)
            {
                return Impl.GetAssetBundle(bundleName);
            }
            return null;
        }

        private interface ITTPOnDemandResources
		{
			void DownloadBundle(string bundleName);
			bool IsBundleInstalled(string bundleName);
            AssetBundle GetAssetBundle(string bundleName);
		}
#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : ITTPOnDemandResources
        {

            public IEnumerator LoadAsset( string resourceName, string odrTag )
            {
                Debug.Log("TTPOnDemandsResources::IosImpl:LoadAsset:resourceName=" + resourceName);
                // Create the request
                OnDemandResourcesRequest request = OnDemandResources.PreloadAsync( new string[] { odrTag } );

                while (!request.isDone)
                {
                    Debug.Log("TTPOnDemandsResources::IosImpl:LoadAsset:Request:" + resourceName + " - " + (request.progress * 100) + "%");
                    yield return new WaitForFixedUpdate();
                }

                // Wait until request is completed
                //yield return request;

                // Check for errors
                if(!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogError("TTPOnDemandsResources::IosImpl:LoadAsset: " + request.error);
                    if (Delegate != null)
                    {
                        Delegate.OnODRFailed("{\"bundleName\":\"" + resourceName + "\", \"errorCode\":" + request.error + "}");
                    }
                    yield break;
                }

                var path = request.GetResourcePath(resourceName);
                Debug.Log("TTPOnDemandsResources::IosImpl:LoadAsset:path=" + path);
                bool isBundleDownloaded = true;
                AssetBundle bundle = AssetBundle.LoadFromFile("res://" + resourceName);
                if (bundle == null)
                {
                    Debug.LogError("TTPOnDemandsResources::IosImpl:LoadAsset: Bundle is null in res after request " + resourceName);
                    bundle = AssetBundle.LoadFromFile("odr://" + resourceName);
                    if (bundle == null)
                    {
                        Debug.LogError("TTPOnDemandsResources::IosImpl:LoadAsset: Bundle is null in odr after request " + resourceName);
                        isBundleDownloaded = false;
                        if (Delegate != null)
                        {
                            Delegate.OnODRFailed("{\"bundleName\":\"" + resourceName + "\", \"errorCode\":Bundle is null after request}");
                        }
                    }
                }
                if(isBundleDownloaded)
                {
                    if (Delegate != null)
                    {
                        Delegate.OnODRInstalled("{\"bundleName\":\"" + resourceName + "\"}");
                    }
                }

                request.Dispose();
            }

            public bool IsBundleInstalled(string bundleName)
            {
                Debug.Log("TTPOnDemandsResources::IosImpl:IsBundleInstalled:bundleName=" + bundleName);
                AssetBundle bundle = AssetBundle.LoadFromFile("res://" + bundleName);
                if (bundle == null)
                {
                    Debug.Log("TTPOnDemandsResources::IosImpl:IsBundleInstalled:bundleName=" + bundleName + " is null from res");
                    bundle = AssetBundle.LoadFromFile("odr://" + bundleName);
                    if (bundle == null)
                    {
                        Debug.Log("TTPOnDemandsResources::IosImpl:IsBundleInstalled:bundleName=" + bundleName + " is null from odr");
                    }
                }

                return bundle != null;
            }

            public void DownloadBundle(string bundleName)
            {
                Debug.Log("TTPOnDemandsResources::IosImpl:DownloadBundle:bundleName=" + bundleName);
                System.Reflection.MethodInfo method = typeof(TTPCore).GetMethod("GetTTPGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    GameObject gameObject = method.Invoke(null, null) as GameObject;
                    if (gameObject != null)
                    {
                        MonoBehaviour mono = gameObject.GetComponent<MonoBehaviour>();
                        mono.StartCoroutine(LoadAsset(bundleName, bundleName));
                    }
                }
            }

            public AssetBundle GetAssetBundle(string bundleName)
            {
                Debug.Log("TTPOnDemandsResources::IosImpl:GetAssetBundle:bundleName=" + bundleName);
                AssetBundle bundle = AssetBundle.LoadFromFile("res://" + bundleName);
                if (bundle == null)
                {
                    Debug.LogError("TTPOnDemandsResources::IosImpl:GetAssetBundle: Bundle is null from res: " + bundleName);
                    bundle = AssetBundle.LoadFromFile("odr://" + bundleName);
                    if (bundle == null)
                    {
                        Debug.LogError("TTPOnDemandsResources::IosImpl:GetAssetBundle: Bundle is null from odr: " + bundleName);
                    }
                }
                return bundle;
            }
        }
#endif
#if UNITY_ANDROID
        private class AndroidImpl : ITTPOnDemandResources
        {
            private const string SERVICE_GET_METHOD = "getOnDemandResources";

            private AndroidJavaObject _serivceJavaObject;

            private AndroidJavaObject ServiceJavaObject
            {
                get
                {
                    if (_serivceJavaObject == null)
                    {
                        _serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject(SERVICE_GET_METHOD);
                    }
                    if (_serivceJavaObject == null)
                        Debug.LogError("TTPBanners::AndroidImpl: failed to get native instance.");
                    return _serivceJavaObject;
                }
            }

            public bool IsBundleInstalled(string bundleName)
            {
                if(ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("isBundleInstalled", new object[] { bundleName?? "" });
                    
                }
                return false;
            }

            public void DownloadBundle(string bundleName)
            {
                if(ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("installBundle", new object[] { bundleName ?? "" });
                }
            }

            public AssetBundle GetAssetBundle(string bundleName)
            {
                if (ServiceJavaObject != null)
                {
                    AndroidJavaObject dataJavaObject = ServiceJavaObject.Call<AndroidJavaObject>("getBundleData", new object[] { bundleName ?? "" });
                    if(dataJavaObject != null)
                    {
                        byte[] result = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(dataJavaObject.GetRawObject());
                        AssetBundle bundle = AssetBundle.LoadFromMemory(result);
                        return bundle;
                    }
                }
                return null;
            }
        }
#endif
        private class EditorImpl : ITTPOnDemandResources
        {
            public void DownloadBundle(string bundleName)
            {
                Debug.Log("TTPOnDemandsResources::EditorImpl:DownloadBundle:bundleName=" + bundleName);
                Delegate.OnODRInstalled("{\"bundleName\":\"" + bundleName + "\"}");
            }

            public AssetBundle GetAssetBundle(string bundleName)
            {
                Debug.Log("TTPOnDemandsResources::EditorImpl:GetAssetBundle:bundleName=" + bundleName);
                AssetBundle bundle = AssetBundle.LoadFromFile("res://" + bundleName);
                if (bundle == null)
                {
                    Debug.LogError("TTPOnDemandsResources::EditorImpl:GetAssetBundle: Bundle is null from res: " + bundleName);
                    bundle = AssetBundle.LoadFromFile("odr://" + bundleName);
                    if (bundle == null)
                    {
                        Debug.LogError("TTPOnDemandsResources::EditorImpl:GetAssetBundle: Bundle is null from odr: " + bundleName);
                    }
                }
                return bundle;
            }

            public bool IsBundleInstalled(string bundleName)
            {
                return true;
            }
        }
        [Preserve]
        class OnDemandResourcesDelegate : MonoBehaviour
        {
            [System.Serializable]
            private class OnODRInstalledMessage
            {
                public string bundleName = null;
            }

            [System.Serializable]
            private class OnODRFailedMessage
            {
                public string bundleName = null;
                public int errorCode = 0;
            }


            public void OnODRInstalled(string message)
            {
                Debug.Log("OnDemandResourcesDelegate::OnODRInstalled: " + message);
                if (message != null)
                {
                    OnODRInstalledMessage parsedMessage = JsonUtility.FromJson<OnODRInstalledMessage>(message);
                    if(parsedMessage != null)
                    {
                        DownloadResult downloadResult = new DownloadResult();
                        downloadResult.bundleName = parsedMessage.bundleName;
                        downloadResult.success = true;
                        downloadResult.errorCode = 0;
                        callActions(downloadResult);
                    }
                }
            }

            public void OnODRFailed(string message)
            {
                Debug.Log("OnDemandResourcesDelegate::OnODRFailed: " + message);
                if (message != null)
                {
                    OnODRFailedMessage parsedMessage = JsonUtility.FromJson<OnODRFailedMessage>(message);
                    if (parsedMessage != null)
                    {
                        DownloadResult downloadResult = new DownloadResult();
                        downloadResult.bundleName = parsedMessage.bundleName;
                        downloadResult.success = false;
                        downloadResult.errorCode = parsedMessage.errorCode;
                        callActions(downloadResult);
                    }
                }
            }

            private void callActions(DownloadResult downloadResult)
            {
                List<DownloadItem> itemsToRemove = new List<DownloadItem>();
                foreach (DownloadItem downloadItem in _actionsList)
                {
                    if (downloadItem.bundleName == downloadResult.bundleName)
                    {
                        if (downloadItem.shouldLoad)
                        {
                            downloadResult.assetBundle = GetAssetBundle(downloadResult.bundleName);
                        }
                        downloadItem.action.Invoke(downloadResult);
                        itemsToRemove.Add(downloadItem);
                    }
                }
                foreach (DownloadItem downloadItem in itemsToRemove)
                {
                    _actionsList.Remove(downloadItem);
                }
            }
        }

    }
}

#endif
