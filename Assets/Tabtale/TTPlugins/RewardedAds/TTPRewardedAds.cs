#if TTP_REWARDED_ADS
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
    /// This class controls rewarded ads behaviour. Mediation provider can be AdMob or moPub
    /// </summary>
    public class TTPRewardedAds
    {
        /// <summary>
        /// Interstitials are ready for showing event
        /// </summary>
        public static event System.Action<bool> ReadyEvent;

        /// <summary>
        /// Get result if view has been already shown 
        /// </summary>
        private static System.Action<bool, TTPILRDData> _onResultActionILRD;

        
        /// <summary>
        /// Get result if view has been already shown 
        /// </summary>
        private static System.Action<bool> _onResultAction;

        /// <summary>
        /// Show rewarded ads view in the scene
        /// </summary>
        /// <param name="location">Location for showing</param>
        /// <param name="onResultAction">Action delegate for result</param>
        /// <returns>True - if plugin is implemented</returns>
        public static bool ShowWithILRD(string location, System.Action<bool, TTPILRDData> onResultAction)
        {
            _onResultActionILRD = onResultAction;
            _onResultAction = null;
            if (Impl != null)
            {
                if (Impl.Show(location))
                {
                    return true;
                }
                else
                {
                    _onResultActionILRD.Invoke(false, null);
                }
            }
            return false;
        }
        
        /// <summary>
        /// Show rewarded ads view in the scene
        /// </summary>
        /// <param name="location">Location for showing</param>
        /// <param name="onResultAction">Action delegate for result</param>
        /// <returns>True - if plugin is implemented</returns>
        public static bool Show(string location, System.Action<bool> onResultAction)
        {
            _onResultAction = onResultAction;
            _onResultActionILRD = null;
            if (Impl != null)
            {
                if (Impl.Show(location))
                {
                    return true;
                }
                else
                {
                    _onResultAction.Invoke(false);
                }
            }
            return false;
        }


        /// <summary>
        /// Indicates that rewarded ads are ready to show
        /// </summary>
        /// <returns>True - rewarded ads are ready</returns>
        public static bool IsReady()
        {
            if (Impl != null)
            {
                return Impl.IsReady();
            }
            return false;
        }

        private interface ITTPRewardedAds
        {
            bool Show(string location);
            bool IsReady();
        }

        private static ITTPRewardedAds _impl;
        private static ITTPRewardedAds Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.rvs)
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
                    Debug.LogError("TTPRewardedAds::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

#if UNITY_ANDROID
        private class AndroidImpl : ITTPRewardedAds
        {
            private const string SERVICE_GET_METHOD = "getRewardedAds";

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
                        Debug.LogError("TTPRewardedAds::AndroidImpl: failed to get native instance.");
                    return _serivceJavaObject;
                }
            }


            public bool Show(string location)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("show", new object[] { location });
                }
                return false;
            }
            public bool IsReady()
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("isReady");
                }
                return false;
            }

        }
#endif
#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : ITTPRewardedAds
        {
            [DllImport("__Internal")]
            private static extern bool ttpRewardedAdsShow(string location);

            [DllImport("__Internal")]
            private static extern bool ttpRewardedAdsIsReady();

            public bool Show(string location)
            {
                return ttpRewardedAdsShow(location);
            }
            public bool IsReady()
            {
                return ttpRewardedAdsIsReady();
            }
            
        }
#endif
        //#if UNITY_EDITOR
        private class EditorImpl : ITTPRewardedAds
        {
            private GameObject interCanvas;

            public System.Action _onClosedAction = () => {
                if (ReadyEvent != null)
                {
                    ReadyEvent(true);
                }
            };

            public EditorImpl()
            {
                if (ReadyEvent != null)
                {
                    ReadyEvent(true);
                }
            }

            public bool Show(string location)
            {
                if (interCanvas == null)
                {
                    interCanvas = Resources.Load<GameObject>("Prefabs/TTPRewardedAdsCanvas");
                    interCanvas = GameObject.Instantiate(interCanvas);
                    interCanvas.name = "TTPRewardedAdsCanvas";
                }
                interCanvas.SetActive(true);
                return true;
            }
            public bool IsReady()
            {
                return true;
            }

        }
        //#endif

        private class EmptyImpl : ITTPRewardedAds
        {
            public bool Show(string location)
            {
                return false;
            }

            public bool IsReady()
            {
                return false;
            }
        }
        
        /// <summary>
        /// This class provides notifications about changes using events. 
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        [Preserve]
        public class RewardedAdsDelegate : MonoBehaviour
        {
            [System.Serializable]
            private class OnLoadedMessage
            {
                public bool loaded = false;
                public string error = null;
            }

            [System.Serializable]
            private class OnClosedMessage
            {
                public bool shouldReward;
            }

            public void OnRewardedAdsReady(string message)
            {
                if (message != null)
                {
                    Debug.Log("RewardedAdsDelegate::OnRewardedAdsReady: " + message);
                    OnLoadedMessage onLoadedMessage = JsonUtilityWrapper.FromJson<OnLoadedMessage>(message);
                    if (onLoadedMessage != null)
                    {
                        if (ReadyEvent != null)
                        {
                            ReadyEvent(onLoadedMessage.loaded);
                        }
                    }
                }
            }

            public void OnRewardedAdsShown(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(true, TTPCore.TTPSoundMgr.Caller.REWARDED_ADS);
            }

            public void OnRewardedAdsClosed(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(false, TTPCore.TTPSoundMgr.Caller.REWARDED_ADS);
                if(message != null)
                {
                    Debug.Log("RewardedAdsDelegate::OnRewardedAdsClosed: " + message);
                    var onClosedMessage = JsonUtilityWrapper.FromJson<OnClosedMessage>(message);
                    var ilrdData = JsonUtilityWrapper.FromJson<TTPILRDData>(message);
                    if (onClosedMessage != null)
                    {
                        if (_onResultActionILRD != null)
                        {
                            _onResultActionILRD.Invoke(onClosedMessage.shouldReward, ilrdData);
                        }
                        else if (_onResultAction != null)
                        {
                            _onResultAction.Invoke(onClosedMessage.shouldReward);

                        }
                        _onResultAction = null;
                        _onResultActionILRD = null;
                        
                        if (Impl != null && Impl.GetType() == typeof(EditorImpl) && ((EditorImpl)Impl)._onClosedAction != null)
                        {
                            ((EditorImpl)_impl)._onClosedAction.Invoke();
                        }
                    }
                }

            }
        }
    }
}
#endif