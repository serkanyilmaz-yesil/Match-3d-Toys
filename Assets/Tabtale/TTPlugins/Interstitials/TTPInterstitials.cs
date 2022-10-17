#if TTP_INTERSTITIALS
using System;
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
    /// This class controls interstitials behaviour. Mediation provider can be AdMob or moPub
    /// </summary>
    public class TTPInterstitials
    {
        /// <summary>
        /// Interstitials are ready for showing event
        /// </summary>
        [Obsolete("Please call Show(location) directly. There is no need to be notified about readiness of interstitials.", false)]
        public static event System.Action<bool> ReadyEvent;

        /// <summary>
        /// Continue if promotion paused action
        /// </summary>
        private static System.Action<TTPILRDData> _onContinueActionILRD;

        /// Continue if promotion paused action
        /// </summary>
        private static System.Action _onContinueAction;

        
        public static bool ShowWithILRD(string location, System.Action<TTPILRDData> onContinueAction)
        {
            _onContinueActionILRD = onContinueAction;
            if(Impl != null)
            {
                if (Impl.Show(location))
                {
                    return true;
                }
                else if (_onContinueActionILRD != null)
                {
                    _onContinueActionILRD.Invoke(null);
                }
            }
            return false;
        }

        /// <summary>
        /// Show interstitails view in the scene
        /// </summary>
        /// <param name="location">Location for showing</param>
        /// <param name="onContinueAction">Action delegate for result</param>
        /// <returns>True - if plugin is implemented</returns>
        public static bool Show(Locations location, System.Action onContinueAction)
        {
            _onContinueAction = onContinueAction;
            if (Impl != null)
            {
                if (Impl.Show(location.ToString()))
                {
                    return true;
                }
                else if (_onContinueAction != null)
                {
                    _onContinueAction.Invoke();
                }
            }
            return false;
        }

        /// <summary>
        /// Show interstitails view in the scene
        /// </summary>
        /// <param name="location">Location for showing</param>
        /// <param name="onContinueAction">Action delegate for result</param>
        /// <returns>True - if plugin is implemented</returns>
        [System.Obsolete("Use Show with enum Locations")]
        public static bool Show(string location, System.Action onContinueAction)
        {
            _onContinueAction = onContinueAction;
            if (Impl != null)
            {
                if (Impl.Show(location))
                {
                    return true;
                }
                else if (_onContinueAction != null)
                {
                    _onContinueAction.Invoke();
                }
            }
            return false;
        }


        /// <summary>
        /// Indicates that interstitials are ready to show
        /// </summary>
        /// <returns>True - interstitials are ready</returns>
        [Obsolete("Please call Show(location) directly. There is no need to be check the readiness of interstitials.", false)]
        public static bool IsReady()
        {
            if (Impl != null)
            {
                return Impl.IsReady();
            }
            return false;
        }

        /// <summary>
        /// Private interface of interstitials
        /// </summary>
        private interface ITTPInterstitials
        {
            bool Show(string location);
            bool IsReady();
        }

        private static ITTPInterstitials _impl;
        private static ITTPInterstitials Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.interstitials)
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
                        _impl = new AndroidImpl();
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
                    Debug.LogError("TTPInterstitials::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

#if UNITY_ANDROID
        private class AndroidImpl : ITTPInterstitials
        {
            private const string SERVICE_GET_METHOD = "getInterstitials";

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
                        Debug.LogError("TTPInterstitials::AndroidImpl: failed to get native instance.");
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
        private class IosImpl : ITTPInterstitials
        {
            [DllImport("__Internal")]
            private static extern bool ttpInterstitialsShow(string location);

            [DllImport("__Internal")]
            private static extern bool ttpInterstitialsIsReady();

            public bool Show(string location)
            {
                return ttpInterstitialsShow(location);
            }
            public bool IsReady()
            {
                return ttpInterstitialsIsReady();
            }
        }
#endif
        //#if UNITY_EDITOR
        private class EditorImpl : ITTPInterstitials
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
                if(ReadyEvent != null)
                {
                    ReadyEvent(true);
                }
            }

            public bool Show(string location)
            {
                if(interCanvas == null)
                {
                    interCanvas = Resources.Load<GameObject>("Prefabs/TTPInterstitialsCanvas");
                    interCanvas = GameObject.Instantiate(interCanvas);
                    interCanvas.name = "TTPInterstitialsCanvas";
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

        private class EmptyImpl : ITTPInterstitials
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
        public class InterstitialsDelegate : MonoBehaviour
        {
            private class OnLoadedMessage
            {
                public bool loaded = false;
                public string error = null;
            }

            public void OnInterstitialLoaded(string message)
            {
                if (message != null)
                {
                    Debug.Log("InterstitialsDelegate::OnLoaded: " + message);
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

            public void OnInterstitialShown(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(true, TTPCore.TTPSoundMgr.Caller.INTERSTITIAL);
            }

            public void OnInterstitialClosed(string message)
            {
                Debug.Log("InterstitialsDelegate:: OnInterstitialClosed: " + message);
                TTPILRDData ilrdData = null;
                if (!string.IsNullOrEmpty(message))
                {
                    ilrdData = JsonUtility.FromJson<TTPILRDData>(message);
                }
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(false, TTPCore.TTPSoundMgr.Caller.INTERSTITIAL);
                
                if (_onContinueActionILRD != null)
                {
                    _onContinueActionILRD.Invoke(ilrdData);
                }
                else if (_onContinueAction != null)
                {
                    _onContinueAction.Invoke();
                }
                
                _onContinueAction = null;
                _onContinueActionILRD = null;

                if (Impl != null && Impl.GetType() == typeof(EditorImpl) && ((EditorImpl)Impl)._onClosedAction != null)
                {
                    ((EditorImpl)_impl)._onClosedAction.Invoke();
                }
            }
        }
    }


}
#endif