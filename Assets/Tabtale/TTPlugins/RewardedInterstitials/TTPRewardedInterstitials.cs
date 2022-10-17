#if TTP_REWARDED_INTERSTITIALS && TTP_CORE
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
    public class TTPRewardedInterstitials
    {
        /// <summary>
        /// Interstitials are ready for showing event
        /// </summary>
        public static event System.Action<bool> ReadyEvent;

        /// <summary>
        /// Get result if view has been already shown
        /// </summary>
        private static System.Action<bool, TTPILRDData> _onResultActionILRD;

        public static bool Show(string location, System.Action<bool, TTPILRDData> onResultAction, bool force)
        {
            _onResultActionILRD = onResultAction;
            if (Impl != null)
            {
                if (Impl.Show(location, force))
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

        public static bool IsReady(bool force)
        {
            if (Impl != null)
            {
                return Impl.IsReady(force);
            }
            return false;
        }

        public static void NotifyPopupShown()
        {
            if (Impl != null)
            {
                Impl.PopupShown();
            }
        }

        public static void NotifyPopupCancelled()
        {
            if (Impl != null)
            {
                Impl.PopupCancelled();
            }
        }

        private static ITTPRewardedInterstitials _impl;
        private static ITTPRewardedInterstitials Impl
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
                    Debug.LogError("TTPRewardedInterstitials::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

        private interface ITTPRewardedInterstitials
        {
            bool Show(string location, bool force);
            bool IsReady(bool force);
            void PopupShown();
            void PopupCancelled();
        }

#if UNITY_ANDROID
        private class AndroidImpl : ITTPRewardedInterstitials
        {
            private const string SERVICE_GET_METHOD = "getRewardedInterstitials";

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
                        Debug.LogError("TTPRewardedInterstitials::AndroidImpl: failed to get native instance.");
                    return _serivceJavaObject;
                }
            }

            public bool Show(string location, bool force)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("show", new object[] { location, force });
                }
                return false;
            }

            public bool IsReady(bool force)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("isReady", new object[] { force });
                }
                return false;
            }

            public void PopupShown()
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("popupShown");
                }
            }

            public void PopupCancelled()
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("popupCancelled");
                }
            }
        }
#endif
#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : ITTPRewardedInterstitials
        {
            [DllImport("__Internal")]
            private static extern bool ttpRewardedInterstitialsShow(string location, bool force);

            [DllImport("__Internal")]
            private static extern bool ttpRewardedInterstitialsIsReady(bool force);

            [DllImport("__Internal")]
            private static extern void ttpRewardedInterstitialsPopupShown();

            [DllImport("__Internal")]
            private static extern void ttpRewardedInterstitialsPopupCancelled();

            public bool Show(string location, bool force)
            {
                return ttpRewardedInterstitialsShow(location, force);
            }
            public bool IsReady(bool force)
            {
                return ttpRewardedInterstitialsIsReady(force);
            }

            public void PopupShown()
            {
                ttpRewardedInterstitialsPopupShown();
            }

            public void PopupCancelled()
            {
                ttpRewardedInterstitialsPopupCancelled();
            }
        }
#endif
        //#if UNITY_EDITOR
        private class EditorImpl : ITTPRewardedInterstitials
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

            public bool Show(string location, bool force)
            {
                if (interCanvas == null)
                {
                    interCanvas = Resources.Load<GameObject>("Prefabs/TTPRewardedInterstitialsCanvas");
                    interCanvas = GameObject.Instantiate(interCanvas);
                    interCanvas.name = "TTPRewardedInterstitialsCanvas";
                }
                interCanvas.SetActive(true);
                return true;
            }
            public bool IsReady(bool force)
            {
                return true;
            }

            public void PopupShown()
            {

            }

            public void PopupCancelled()
            {

            }
        }
        //#endif

        private class EmptyImpl : ITTPRewardedInterstitials
        {
            public bool Show(string location, bool force)
            {
                return false;
            }

            public bool IsReady(bool force)
            {
                return false;
            }

            public void PopupShown()
            {

            }

            public void PopupCancelled()
            {

            }
        }
        [Preserve]
        public class RewardedInterstitialsDelegate : MonoBehaviour
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

            public void OnRewardedInterstitialsReady(string message)
            {
                if (message != null)
                {
                    Debug.Log("RewardedInterstitialsDelegate::OnRewardedInterstitialsReady: " + message);
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

            public void OnRewardedInterstitialsShown(string message)
            {
                Debug.Log("RewardedInterstitialsDelegate::OnRewardedInterstitialsShown");
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(true, TTPCore.TTPSoundMgr.Caller.REWARDED_INTER);
            }

            public void OnRewardedInterstitialsClosed(string message)
            {
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(false, TTPCore.TTPSoundMgr.Caller.REWARDED_INTER);
                if(message != null)
                {
                    Debug.Log("RewardedInterstitialsDelegate::OnRewardedInterstitialsClosed: " + message);
                    var onClosedMessage = JsonUtilityWrapper.FromJson<OnClosedMessage>(message);
                    var ilrdData = JsonUtilityWrapper.FromJson<TTPILRDData>(message);
                    if (onClosedMessage != null)
                    {
                        if (_onResultActionILRD != null)
                        {
                            _onResultActionILRD.Invoke(onClosedMessage.shouldReward, ilrdData);
                        }

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
