#if TTP_POPUPMGR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Tabtale.TTPlugins
{
    public class TTPPopupMgr
    {

        private interface ITTPPopupMgrInternal
        {
            bool ShouldShow(string source);
            void OnShown(string source);
            void OnClosed(string source);


        }

        private static ITTPPopupMgrInternal _impl;
        private static ITTPPopupMgrInternal Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.DevMode)
                    {
                        _impl = new UnityTTPPopupMgr();
                    }
                    else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
                        UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
                    {
#if UNITY_ANDROID
                        _impl = new AndroidTTPPopupMgr();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
                        _impl = new IosTTPPopupMgr();
#endif

                    }
                    else
                    {
#if UNITY_EDITOR
                        _impl = new UnityTTPPopupMgr();
#endif
                    }
                }
                if (_impl == null)
                {
                    Debug.LogError("");
                }
                return _impl;
            }
        }

        public static bool ShouldShow(string source)
        {
            return Impl.ShouldShow(source);
        }

        public static void OnShown(string source)
        {
            Impl.OnShown(source);
        }

        public static void OnClosed(string source)
        {
            Impl.OnClosed(source);
        }


        //public static bool OnClose(string source)
        //{
        //    return Impl.ShouldShow(source);
        //}

        //================================================================================================================================
        //Android Impl 
        //================================================================================================================================
#if UNITY_ANDROID
    private class AndroidTTPPopupMgr : ITTPPopupMgrInternal {

            private const string SERVICE_GET_METHOD = "getPopUpMgr";

            private AndroidJavaObject _serviceJavaObject;

            private AndroidJavaObject ServiceJavaObject
            {
                get
                {
                    if (_serviceJavaObject == null)
                    {
                        _serviceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject(SERVICE_GET_METHOD);
                    }
                    if (_serviceJavaObject == null)
                        Debug.LogError("TTPInterstitials::AndroidImpl: failed to get native instance.");
                    return _serviceJavaObject;
                }
            }
            
        public bool ShouldShow(string source)
        {
            if (ServiceJavaObject != null)
            {
                    return ServiceJavaObject.Call<bool>("shouldShow", new object[] { source });
            }
            else
            {
                Debug.LogError("");
            }
            return false;
        }

        public void OnShown(string source)
        {
            if (ServiceJavaObject != null)
            {
                ServiceJavaObject.Call("onShow", new object[] { source });
            }
            else
            {
                Debug.LogError("");
            }
        }

        public void OnClosed(string source)
        {
            if (ServiceJavaObject != null)
            {
                ServiceJavaObject.Call("onClose", new object[] { source });
            }
            else
            {
                Debug.LogError("");
            }
        }

    }
#endif
        //================================================================================================================================
        //END Android Impl 
        //================================================================================================================================


        //================================================================================================================================
        // IOS Impl 
        //================================================================================================================================
#if UNITY_IOS && !TTP_DEV_MODE
        public class IosTTPPopupMgr : ITTPPopupMgrInternal {

        [DllImport("__Internal")]
        private static extern bool ttpPopupMgrShouldShow(string source);

        //[DllImport("__Internal")]
        //private static extern void ttpPopupMgrOnShown(string source);

        public bool ShouldShow(string source)
        {
             return ttpPopupMgrShouldShow(source);
        }

        //Unused in IOS
        public void OnShown(string source)
        {
        }

        //Unused in IOS
        public void OnClosed(string source)
        {
        }

      }
#endif
        //================================================================================================================================
        // END IOS Impl 
        //================================================================================================================================


        //================================================================================================================================
        // Editor Impl 
        //================================================================================================================================
        //#if UNITY_EDITOR
        private class UnityTTPPopupMgr : ITTPPopupMgrInternal
        {

            public bool ShouldShow(string source)
            {
                return true;
            }

            public void OnShown(string source)
            {
                Debug.Log("TTPPopupMgr::OnShown: ");

            }
            public void OnClosed(string source)
            {
                Debug.Log("TTPPopupMgr::OnClosed: ");
            }

        }
//#endif
    }
}
#endif