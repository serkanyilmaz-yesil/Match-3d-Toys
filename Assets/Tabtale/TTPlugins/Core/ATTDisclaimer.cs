#if UNITY_IOS && !TTP_DEV_MODE
using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Tabtale.TTPlugins
{

    public class ATTDisclaimer
    {
        [DllImport("__Internal")]
        private static extern void ttpAttDisclaimer();

        private static ATTDisclaimer _instance;
        private Action _onFinishedAction;

        private ATTDisclaimer()
        {}

        public static ATTDisclaimer GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ATTDisclaimer();
            }
            return _instance;
        }

        public void StartDisclaimer(Action onFinished)
        {
            Debug.Log("ATTDisclaimer::StartDisclaimer:");
            _onFinishedAction = onFinished;
            ttpAttDisclaimer();
        }

        public class DisclaimerDelegate : MonoBehaviour
        {
            public void OnAskedForATT(string _)
            {
                Debug.Log("ATTDisclaimer::DisclaimerDelegate::NotifyAskForAtt:");
                if (GetInstance()._onFinishedAction != null)
                {
                    GetInstance()._onFinishedAction.Invoke();
                }
            }
        }
    }
}
#endif