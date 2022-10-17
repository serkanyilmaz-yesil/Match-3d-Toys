#if TTP_OPENADS && TTP_CORE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Tabtale.TTPlugins
{
    /// <summary>
    /// This class controls openAds behaviour. Mediation provider can be AdMob
    /// </summary>
    public class TTPOpenAds
    {
        public static event System.Action<TTPILRDData> OnShowingOpenAdsHasFinished;

        private static GameObject loadingScreenCanvas;
        private static bool showingLoadingScreen;

        private static string _customLoadingScreen;
        public static string CustomLoadingScreen
        {
            set { _customLoadingScreen = value; }
            get { return _customLoadingScreen; }
        }

        private static void ShowLoadingScreen()
        {
            Debug.Log("TTPOpenAds::ShowLoadingScreen:");
            if (loadingScreenCanvas == null)
            {
                if (string.IsNullOrEmpty(_customLoadingScreen))
                {
                    Debug.Log("TTPOpenAds::ShowLoadingScreen:show default loading screen");
                    loadingScreenCanvas = Resources.Load<GameObject>("Prefabs/TTPOpenAdsLoadingScreenCanvas");
                }
                else
                {
                    Debug.Log("TTPOpenAds::ShowLoadingScreen:show custom loading screen:" + _customLoadingScreen);
                    loadingScreenCanvas = Resources.Load<GameObject>(_customLoadingScreen);
                }
                loadingScreenCanvas = GameObject.Instantiate(loadingScreenCanvas);
            }
            GameObject.DontDestroyOnLoad(loadingScreenCanvas);
            loadingScreenCanvas.SetActive(true);
            showingLoadingScreen = true;
        }

        private static void CloseLoadingScreen()
        {
            Debug.Log("TTPOpenAds::CloseLoadingScreen: ");
            showingLoadingScreen = false;
            if (loadingScreenCanvas != null)
            {
                loadingScreenCanvas.SetActive(false);
            }
        }

        private static void NotifyOpenAdsHasFinished(TTPILRDData ilrdData)
        {
            Debug.Log("TTPOpenAds::NotifyOpenAdsHasFinished: ");
            if (OnShowingOpenAdsHasFinished != null)
            {
                OnShowingOpenAdsHasFinished(ilrdData);
            }
        }

        /// <summary>
        /// This class provides notifications about changes using events.
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        [Preserve]
        public class OpenAdsDelegate : MonoBehaviour
        {
            [System.Serializable]
            private class OnClosedMessage
            {
                public bool result;
            }

            public void ShowLoadingScreen()
            {
                Debug.Log("OpenAdsDelegate::ShowLoadingScreen: ");
                TTPOpenAds.ShowLoadingScreen();
            }

            public void CloseLoadingScreen(string message)
            {
                Debug.Log("OpenAdsDelegate::CloseLoadingScreen:" + message);
                TTPOpenAds.CloseLoadingScreen();
                if (message != null)
                {
                    Debug.Log("OpenAdsDelegate::CloseLoadingScreen: " + message);
                    var onClosedMessage = JsonUtilityWrapper.FromJson<OnClosedMessage>(message);
                    if (onClosedMessage != null)
                    {
                        if (onClosedMessage.result)
                        {
                            TTPOpenAds.NotifyOpenAdsHasFinished(null);
                        }
                    }
                }
            }

            public void OnOpenAdShown()
            {
                Debug.Log("OpenAdsDelegate::OnOpenAdShown: ");
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(true, TTPCore.TTPSoundMgr.Caller.OPEN_ADS);
                if (showingLoadingScreen)
                {
                    TTPOpenAds.CloseLoadingScreen();
                }
            }

            public void OnOpenAdClosed(string message)
            {
                Debug.Log("OpenAdsDelegate::OnOpenAdClosed:message=" + message);
                ((TTPCore.TTPSoundMgr)TTPCore.SoundMgr).PauseGameMusic(false, TTPCore.TTPSoundMgr.Caller.OPEN_ADS);
                var ilrdData = JsonUtilityWrapper.FromJson<TTPILRDData>(message);
                TTPOpenAds.NotifyOpenAdsHasFinished(ilrdData);
            }
        }
    }
}
#endif