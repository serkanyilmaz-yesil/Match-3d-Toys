#if TTP_CRASHTOOL
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tabtale.TTPlugins
{
    /// <summary>
    /// This class automatically collects crashes, using app center, adds breadcrumb logs to the crash logs
    /// </summary>
	public class TTPCrashTool
    {
        private static ICrashTool _impl;
        private static ICrashTool Impl
        {
            get
            {
                if (_impl == null)
                {
                    if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.crashTool)
                    {
                        _impl = new EditorImpl();
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
                    Debug.LogError("TTPCrashTool::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

        /// <summary>
        /// Add "bread crumb" for identifying crush reasons 
        /// </summary>
        /// <param name="crumb">Information useful for crash report</param>
        public static void AddBreadCrumb(string crumb)
        {
            if (Impl != null)
                Impl.AddBreadCrumb(crumb);
        }
        /// <summary>
        /// Remove all "bread crumbs" from logs 
        /// </summary>
        public static void ClearAllBreadCrumbs()
        {
            if (Impl != null)
                Impl.ClearAllBreadCrumbs();
        }

        /// <summary>
        /// Private interface for crash tool service
        /// </summary>
        private interface ICrashTool
        {
            void AddBreadCrumb(string crumb);
            void ClearAllBreadCrumbs();
        }



#if UNITY_IOS && !TTP_DEV_MODE
                
        private class IosImpl : ICrashTool
        {
            [DllImport("__Internal")]
            private static extern void ttpCrashToolAddBreadCrumb(string crumb);

            [DllImport("__Internal")]
            private static extern void ttpCrashToolClearAllBreadCrumbs();

            public void AddBreadCrumb(string crumb)
            {
                ttpCrashToolAddBreadCrumb(crumb);
            }

            public void ClearAllBreadCrumbs()
            {
                ttpCrashToolClearAllBreadCrumbs();
            }

        }

#endif



#if UNITY_ANDROID
        private class AndroidImpl : ICrashTool
        {
            private const string SERVICE_GET_METHOD = "getCrashTool";

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
                        Debug.LogError("TTPCrashTool::AndroidImpl: failed to get Crash Tool native instance.");
                    return _serivceJavaObject;
                }
            }


            // methods for the native 

            public void AddBreadCrumb(string crumb)
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("addBreadCrumb", new object[] { crumb });
                }
            }

            public void ClearAllBreadCrumbs()
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("clearAllBreadCrumbs");
                }
            }
        }

#endif


        //#if UNITY_EDITOR

        private class EditorImpl : ICrashTool
        {
            public void AddBreadCrumb(string crumb)
            {
                Debug.Log("UnityEditorCrashTool adding bread crumb: " + crumb);
            }

            public void ClearAllBreadCrumbs()
            {
                Debug.Log("UnityEditorCrashTool clearAllBreadCrumbs");
            }
        }



//#endif

    }



}
#endif