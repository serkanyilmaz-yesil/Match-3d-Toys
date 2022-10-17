#if TTP_PROMOTION
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

    public class TTPPromotion
    {
        public static event System.Action<bool> ReadyEvent;

        private static System.Action _onContinueAction;

        public static bool ShowStand(string location, System.Action onContinueAction)
        {
            _onContinueAction = onContinueAction;
            if (Impl != null)
            {
                if (Impl.Show(location))
                {
                    return true;
                }
                else
                {
                    _onContinueAction.Invoke();
                }
            }
            return false;
        }

        public static bool IsReady()
        {
            if (Impl != null)
            {
                return Impl.IsReady();
            }
            return false;
        }

        private interface ITTPPromotion
        {
            bool Show(string location);
            bool IsReady();
        }

        private static ITTPPromotion _impl;
        private static ITTPPromotion Impl
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
#if UNITY_EDITOR
                        _impl = new EditorImpl();
#endif
                    }
                }
                if (_impl == null)
                {
                    Debug.LogError("TTPPromotion::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : ITTPPromotion 
        {
            [DllImport("__Internal")]
            private static extern bool ttpPromotionShow(string location);

            [DllImport("__Internal")]
            private static extern bool ttpPromotionIsReady();

            public bool Show(string location)
            {
                return ttpPromotionShow(location);
            }
            public bool IsReady()
            {
                return ttpPromotionIsReady();
            }
        }
#endif

#if UNITY_ANDROID
        private class AndroidImpl : ITTPPromotion {

            private const string SERVICE_GET_METHOD = "getPromotion";

            private AndroidJavaObject _serivceJavaObject;

            private AndroidJavaObject ServiceJavaObject {
                get {
                    if (_serivceJavaObject == null) {
                        _serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject (SERVICE_GET_METHOD); 
                    }
                    if (_serivceJavaObject == null)
                        Debug.LogError ("TTPPromotion::AndroidImpl: failed to get promotion native instance.");
                    return _serivceJavaObject;
                }
            }

            public bool Show(string location)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("showStand", new object[] { location });
                }
                return false;
            }

            public bool IsReady()
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("isStandReady");
                }
                return false;
            }

        }
#endif

        //#if UNITY_EDITOR
        private class EditorImpl : ITTPPromotion
        {
            
            private GameObject _promotionCanvas;

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
                if (_promotionCanvas == null)
                {
                    _promotionCanvas = Resources.Load<GameObject>("Prefabs/TTPPromotionCanvas");
                    _promotionCanvas = GameObject.Instantiate(_promotionCanvas);
                    _promotionCanvas.name = "TTPPromotionCanvas";
                }
                _promotionCanvas.SetActive(true);
                return true;
            }
            public bool IsReady()
            {
                return true;
            }
        }
//#endif

        //Class methods


        //we need to add this class as a unity component for it to be compatible with SendUnityMessage.
        [Preserve]
        public class PromotionDelegate : MonoBehaviour
        {
            [System.Serializable]
            private class OnLoadedMessage
            {
                public bool loaded = false;
                public string error = null;
            }

            public void OnPromotionLoaded(string message)
            {
                Debug.Log("PromotionDelegate::OnPromotionLoaded: " + message);
                if (message != null)
                {
                    OnLoadedMessage onLoadedMessage = JsonUtilityWrapper.FromJson<OnLoadedMessage>(message);
                    if(onLoadedMessage != null)
                    {
                        if (ReadyEvent != null)
                        {
                            ReadyEvent.Invoke(onLoadedMessage.loaded);
                        }
                    }
                }
            }

            public void OnPromotionClosed(string message)
            {
                Debug.Log("PromotionDelegate::OnPromotionClosed");
                if (_onContinueAction != null)
                {
                    _onContinueAction.Invoke();
                }
                if (_impl.GetType() == typeof(EditorImpl) && ((EditorImpl)_impl)._onClosedAction != null)
                {
                    ((EditorImpl)_impl)._onClosedAction.Invoke();
                }
            }
        }
    }
}
#endif