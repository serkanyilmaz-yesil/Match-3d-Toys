#if TTP_RATEUS
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Scripting;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace Tabtale.TTPlugins
{
    public class TTPRateUs
    {
#if UNITY_IOS && !TTP_DEV_MODE
        [DllImport ("__Internal")]
        private static extern void ttpRateUsDisplayRateUsModal();

        [DllImport("__Internal")]
        private static extern void ttpRateUsGoToStore();
#endif

        private const string PLAYER_PREFS_NEVER = "TTPRateUsNeverShow";
        private const string PLAYER_PREFS_LATER = "TTPRateUsLaterNextTime";

        private static string _iconFileExtenstion;
        private static TTPRateUsCanvas _rateUsCanvas;
        private static TTPRateUsNotConnectedCanvas _notConnectedCanvas;

        public static void Button()
        {
            if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.rateUs)
            {
                return;
            }
            Debug.Log("TTPRateUs::Button:");
            if (TTPCore.IsConnectedToTheInternet())
            {
                GoToRateFromButton();
            }
            else
            {
                LogPopUpEvent(TTPEvents.RATE_US_BUTTON, false, true, "NA");
                InformNoInternet();
            }

        }

        public static void Popup()
        {
            if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.rateUs)
            {
                return;
            }
            Debug.Log("TTPRateUs::Popup");
            if (ShouldSuggestRateUs())
            {
                SuggestRateUs();
            }
        }

        static void Later()
        {
            Debug.Log("TTPRateUs::Later:");
        }

        static void SuggestRateUs()
        {
            Debug.Log("TTPRateUs::SuggestRateUs:");
            LogPopUpEvent(TTPEvents.RATE_US_POPUP, true, false, "NA");
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS && !TTP_DEV_MODE
                ttpRateUsDisplayRateUsModal();        
#endif
            }
            else
            {
#if UNITY_ANDROID
                var serviceManager = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceManager();
                if (serviceManager != null)
                {
                    serviceManager.Call("ShowInAppReview");
                    Debug.Log("GoToRate:: show inapp review");
                }
                else
                {
                    Debug.Log("GoToRate:: could not find service manager");
                }
#endif
            }
        }
        
        [Preserve]
        static bool HandleAndroidBackPressed()
        {
            bool handled = false;
            if (_rateUsCanvas != null)
            {
                Later();
                _rateUsCanvas.OnClickClose();
                handled = true;
            }

            if (_notConnectedCanvas != null)
            {
                _notConnectedCanvas.OnClickClose();
                handled = true;
            }
            return handled;

        }

        static void InformNoInternet()
        {
            Debug.Log("TTPRateUs::InformNoInternet:");
            UnityEngine.Object noConnectionCanvasResource = Resources.Load("TTPRateUsNotConnectedCanvas");
            if (noConnectionCanvasResource != null)
            {
                GameObject go = (GameObject)UnityEngine.Object.Instantiate(noConnectionCanvasResource);
                _notConnectedCanvas = go.GetComponent<TTPRateUsNotConnectedCanvas>();
            }
        }

        private static void GoToRateFromPopup()
        {
            Debug.Log("TTPRateUs::GoToRateFromPopup:");
            GoToRate();
        }

        private static void GoToRateFromButton()
        {
            Debug.Log("TTPRateUs::GoToRateFromButton:");
            LogPopUpEvent(TTPEvents.RATE_US_BUTTON, true, false, "rate");
            GoToRate();
        }

        private static void GoToRate()
        {
            Debug.Log("TTPRateUs::GoToRate:");
            if (TTPCore.DevMode)
            {
                return;
            }
            if (Application.platform == RuntimePlatform.Android ||
                        Application.platform == RuntimePlatform.IPhonePlayer)
            {
#if UNITY_IOS && !TTP_DEV_MODE
                ttpRateUsGoToStore();
#elif UNITY_ANDROID
                AndroidJavaObject appLauncher = new AndroidJavaObject("com.tabtale.ttplugins.ttpcore.common.TTPAppLauncher");
                if (appLauncher != null)
                {
                    appLauncher.Call("OpenAppImpl", "google", null, null, ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetCurrentActivity());
                    Debug.Log("TTPRateUs:: call appLauncher");
                }
                else
                {
                    Debug.Log("TTPRateUs:: Could not initiate appLauncher - class not found");
                }
#endif
            }

#if UNITY_EDITOR
            Debug.Log("TTPRateUs::GoToRate");
#endif
        }
        
        private static bool ShouldSuggestRateUs()
        {
            Debug.Log("TTPRateUs::ShouldSuggestRateUs:");
            if (!TTPCore.IsConnectedToTheInternet())
            {
                LogPopUpEvent(TTPEvents.RATE_US_POPUP, false, true, "NA");
                Debug.Log("TTPRateUs::ShouldSuggestRateUs: not connected to the internet. will not show.");
                return false;
            }
#if TTP_POPUPMGR        
            if(!TTPPopupMgr.ShouldShow("RateUs"))
            {
                LogPopUpEvent(TTPEvents.RATE_US_POPUP, false, false, "NA");
                Debug.Log("TTPRateUs::ShouldSuggestRateUs: PopupMgr return false. will not show.");
                return false;
            }
#endif
            return true;
        }

        private static void Closed()
        {
#if TTP_POPUPMGR
            TTPPopupMgr.OnClosed("rateus");
#endif
            Debug.Log("TTPRateUs::Closed");
        }

        [Serializable]
        private class RateUsConfiguration
        {
            public string iconUrl = ".png";
        }

        private static string IconFileExtenstion
        {
            get
            {
                if(_iconFileExtenstion == null)
                {
                    string configurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("rateUs");
                    if (!string.IsNullOrEmpty(configurationJson))
                    {
                        RateUsConfiguration configuration = JsonUtilityWrapper.FromJson<RateUsConfiguration>(configurationJson);
                        if (configuration != null)
                        {
                            _iconFileExtenstion = configuration.iconUrl.Substring(configuration.iconUrl.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase)+1);
                        }
                    }
                }
                return _iconFileExtenstion;
            }
        }

        private static void LogPopUpEvent(string eventName, bool show, bool noInternet, string response)
        {
            Debug.Log("TTPRateUs::LogPopUpEvent:eventName=" + eventName + " response=" + response);
            IDictionary<string, object> logEventParams = new Dictionary<string, object>()
            {
                {"showrateus", show},
                {"noInternet", noInternet},
                {"response", response}
            };

            System.Type analyticsClsType = System.Type.GetType("Tabtale.TTPlugins.TTPAnalytics");
            if (analyticsClsType != null)
            {
                System.Reflection.MethodInfo method = analyticsClsType.GetMethod("LogEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    long ttAnalyticsConst = 1;
                    long ddnaAnalyticsConst = 1 << 1;
                    long firebaseAnalyticsConst = 1 << 2;
                    method.Invoke(null, new object[] { ttAnalyticsConst | ddnaAnalyticsConst | firebaseAnalyticsConst, eventName, logEventParams, false, true });
                }
                else
                {
                    Debug.LogWarning("CallAnalyticsByReflection:: could not find method - LogEvent");
                }
            }
            else
            {
                Debug.LogWarning("CallAnalyticsByReflection:: could not find TTPAnalytics class");
            }
        }
    }
}
#endif