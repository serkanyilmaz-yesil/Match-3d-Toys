#if TTP_BANNERS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tabtale.TTPlugins
{
    /// <summary>
    /// This class controls banners behaviour. Mediation provider can be AdMob or moPub
    /// </summary>
    public class TTPBanners
    {
        /// <summary>
        /// Banners status changed event
        /// </summary>
        public static event System.Action<int> OnStatusChangeEvent;
        
        /// <summary>
        /// Banners is ready for showing event
        /// </summary>
        public static event System.Action OnBannersReadyEvent;
        
        /// <summary>
        /// Banners have shown a modal (banners were clicked, and a modal window was consequently shown).
        /// </summary>
        public static event System.Action OnBannersModalViewShownEvent;
        
        /// <summary>
        /// Banners modal window closed event.
        /// </summary>
        public static event System.Action OnBannersModalViewClosedEvent;
        
        /// <summary>
        /// Get ILRD Data for banners
        /// </summary>
        public static System.Action<TTPILRDData> OnBannerILRD;

        /// <summary>
        /// Show banners view on the scene
        /// </summary>
        /// <returns>True - if plugin is implemented</returns>
        public static bool Show()
        {
            if (Impl != null)
            {
                return Impl.Show();
            }
            return false;
        }
        /// <summary>
        /// Hide banners view on the scene
        /// </summary>
        /// <returns>True - if plugin is implemented</returns>
        public static bool Hide()
        {
            if (Impl != null)
            {
                return Impl.Hide();
            }
            return false;
        }
        /// <summary>
        /// Returns current banner view height
        /// </summary>
        /// <returns>Current banner view height or 0 - if plugin is not implemented</returns>
        public static int GetAdHeight()
        {
            if (Impl != null)
            {
                return Impl.GetAdHeight();
            }
            return 0;
        }
        /// <summary>
        /// Private implemetation of plugin
        /// </summary>
        private static ITTPBanners _impl;
        private static ITTPBanners Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.banners)
                    {
                        _impl = new EmptyImpl();
                    }
                    else if (TTPCore.DevMode)
                    {
                        _impl = new EditorImpl();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
                        UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
                    {
#if UNITY_ANDROID
                        _impl = new AndroidImpl ();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
                        _impl = new IosImpl();
#endif

                    }
                    else
                    {
#if UNITY_EDITOR
                        _impl = new EditorImpl();
#endif
                    }
                }
                if (_impl == null)
                {
                    Debug.LogError("TTPBanners::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

#if UNITY_EDITOR
        [Preserve]
        private static void TriggerOnBannersReady()
        {
            Debug.Log("TTPBanners::TriggerOnBannersReady");
            if (OnBannersReadyEvent != null)
                OnBannersReadyEvent();
        }
#endif

        private interface ITTPBanners
        {
            bool Show();
            bool Hide();
            int GetAdHeight();
        }
#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : ITTPBanners
        {
            [DllImport("__Internal")]
            private static extern bool ttpBannersShow();

            [DllImport("__Internal")]
            private static extern bool ttpBannersHide();

            [DllImport("__Internal")]
            private static extern int ttpBannersGetAdHeight();

            public bool Show()
            {
                return ttpBannersShow();
            }

            public bool Hide()
            {
                return ttpBannersHide();
            }

            public int GetAdHeight()
            {
                return ttpBannersGetAdHeight();
            }
        }
#endif
#if UNITY_ANDROID
        private class AndroidImpl : ITTPBanners
        {
            private const string SERVICE_GET_METHOD = "getBanners";

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

            public int GetAdHeight()
            {
                if (ServiceJavaObject != null)
                    return ServiceJavaObject.Call<int>("getAdHeight");
                return 0;
            }

            public bool Hide()
            {
                if (ServiceJavaObject != null)
                    return ServiceJavaObject.Call<bool>("hide");
                return false;
            }

            public bool Show()
            {
                if (ServiceJavaObject != null)
                    return ServiceJavaObject.Call<bool>("show");
                return false;
            }
        }
#endif
        //#if UNITY_EDITOR
        private class EditorImpl : ITTPBanners
        {
            GameObject bannerCanvas;
            bool alignToTop;
            RectTransform bannerRectTransform;

            private class BannersConfiguration
            {
                public bool alignToTop = false;
            }

            public EditorImpl()
            {
                string path = Application.streamingAssetsPath + "/ttp/configurations/banners.json";
				if (System.IO.File.Exists(path))
				{
					string bannersJson = System.IO.File.ReadAllText(path);
					if (bannersJson != null)
					{
						BannersConfiguration configuration = new BannersConfiguration();
#if UNITY_EDITOR
                    UnityEditor.EditorJsonUtility.FromJsonOverwrite(bannersJson, configuration);
#endif
						if (configuration != null)
							alignToTop = configuration.alignToTop;
					}
				}
                
            }

            public int GetAdHeight()
            {
                return 0;
            }

            public bool Hide()
            {
                if(bannerCanvas != null)
                    bannerCanvas.SetActive(false);
                return true;
            }

            public bool Show()
            {
                if(bannerCanvas == null)
                {
                    bannerCanvas = Resources.Load<GameObject>("Prefabs/BannerCanvas");
                    bannerCanvas = GameObject.Instantiate(bannerCanvas);
                    GameObject.DontDestroyOnLoad(bannerCanvas);
                    bannerCanvas.name = "TTPBannersCanvas";
                }
                bannerCanvas.SetActive(true);

                foreach(RectTransform rectTransform in bannerCanvas.GetComponentsInChildren<RectTransform>())
                {
                    if (rectTransform.name == "BannerImage")
                        bannerRectTransform = rectTransform;
                }

                if (alignToTop)
                {
                    bannerRectTransform.anchorMin = new Vector2(0, 1);
                    bannerRectTransform.anchorMax = new Vector2(1, 1);
                    bannerRectTransform.offsetMax = new Vector2(0, 0);
                    bannerRectTransform.offsetMin = new Vector2(0, 0);
                    bannerRectTransform.anchoredPosition = new Vector3(bannerRectTransform.anchoredPosition.x, -100);
                }
                else
                {
                    bannerRectTransform.anchorMin = new Vector2(0, 0);
                    bannerRectTransform.anchorMax = new Vector2(1, 0);
                    bannerRectTransform.offsetMax = new Vector2(0, 0);
                    bannerRectTransform.offsetMin = new Vector2(0, 0);
                }

                bannerRectTransform.sizeDelta = new Vector2(bannerRectTransform.sizeDelta.x, 100);
                return true;
            }
        }
        
        private class EmptyImpl : ITTPBanners
        {
            public bool Show()
            {
                return false;
            }

            public bool Hide()
            {
                return false;
            }

            public int GetAdHeight()
            {
                return 0;
            }
        }
        
        //#endif
        /// <summary>
        /// This class provides notifications about changes using events
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        [Preserve]
        public class BannersDelegate : MonoBehaviour
        {
            [System.Serializable]
            private class OnStatusChangeMessage
            {
                public int adHeight = 0;
            }

            public void OnBannersStatusChange(string message)
            {
                Debug.Log("TTPBanners::BannersDelegate:OnBannersStatusChange:message=" + message);
                if (message != null)
                {
                    Debug.Log("TTPBanners::BannersDelegate:OnBannersStatusChange:message is not null");
                    OnStatusChangeMessage statusChangeMessage = JsonUtilityWrapper.FromJson<OnStatusChangeMessage>(message);
                    Debug.Log("TTPBanners::BannersDelegate:OnBannersStatusChange:OnStatusChangeMessage is created");
                    if (statusChangeMessage != null && OnStatusChangeEvent != null)
                    {
                        Debug.Log("TTPBanners::BannersDelegate:OnBannersStatusChange:OnStatusChangeEvent:before:adHeight=" + statusChangeMessage.adHeight);
                        OnStatusChangeEvent(statusChangeMessage.adHeight);
                        Debug.Log("TTPBanners::BannersDelegate:OnBannersStatusChange:OnStatusChangeEvent:after");
                    }
                }
            }

            public void OnBannersReady(string message)
            {
                if(OnBannersReadyEvent != null)
                    OnBannersReadyEvent();
            }

            public void BannersOnModalViewShown(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(true, TTPCore.TTPSoundMgr.Caller.BANNERS);
                if (OnBannersModalViewShownEvent != null)
                    OnBannersModalViewShownEvent.Invoke();

            }

            public void BannersOnModalViewClosed(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(false, TTPCore.TTPSoundMgr.Caller.BANNERS);
                if (OnBannersModalViewClosedEvent != null)
                    OnBannersModalViewClosedEvent.Invoke();
            }
            
            public void OnBannersILRD(string message)
            {
                if (message != null)
                {
                    var ilrdData = JsonUtilityWrapper.FromJson<TTPILRDData>(message);
                    if (OnBannerILRD != null)
                        OnBannerILRD.Invoke(ilrdData);
                }
            }

        }
    }
}
#endif