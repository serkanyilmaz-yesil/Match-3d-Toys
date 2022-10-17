#if TTP_PRIVACY_SETTINGS
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
    /// This class helps to get user consent for tracking user information
    /// Depends on age and location user should consent according to privacy regulation
    /// </summary>
    public class TTPPrivacySettings
    {
        /// <summary>
        /// Consent type
        /// <list type="table">
        /// <item>
        /// <term>UA</term>
        /// <term>Under aged</term>
        /// </item>
        /// <item>
        /// <term>NPA</term>
        /// <term>Non-personalized ads</term>
        /// </item>
        /// <item>
        /// <term>PA</term>
        /// <term>Personalized ads</term>
        /// </item>
        /// <item>
        /// <term>NE</term>
        /// <term>Not European region</term>
        /// </item>
        /// <item>
        /// <term>UNKNOWN</term>
        /// <term>Not defined type (at first launch of application)</term>
        /// </item>
        /// </list>
        /// </summary>
        public enum ConsentType
        {
            UA, NPA, PA, NE, UNKNOWN
        }

        /// <summary>
        /// Audience mode for age restriction of application
        /// <list type="table">
        /// <item>
        /// <term>NON_CHILDREN</term>
        /// <description>Not for children app</description>
        /// </item>
        /// <item>
        /// <term>CHILDREN</term>
        /// <description>Exclusively for children app</description>
        /// </item>
        /// <item>
        /// <term>MIXED_NON_CHILDREN</term>
        /// <description>Mixed for not children and for children (user age is adult)</description>
        /// </item>
        /// <item>
        /// <term>MIXED_CHILDREN</term>
        /// <description>Mixed for not children and for children (user age is child)</description>
        /// </item>
        /// <item>
        /// <term>MIXED_UNKNOWN</term>
        /// <description>Mixed app and user age is still unknown</description>
        /// </item>
        /// </list>
        /// </summary>
        public enum AudienceMode
        {
            NON_CHILDREN, CHILDREN, MIXED_NON_CHILDREN, MIXED_CHILDREN, MIXED_UNKNOWN
        }

        /// <summary>
        /// Consent form type
        /// enumerator to indicate to the app whether it should raise a consent form or not. (and which kind).
        /// </summary>
        public enum ConsentFormType
        {
            NONE, ///< Instructs the app to not show any consent form
            ANY, ///< Instructs the app to show any consent form (it must).
            NO_PURCHASE ///< Instructs the app to show a consent form that should not included a purchase option.
        }

        public static event System.Action<ConsentFormType> OnRemoteConsentModeReadyEvent;
        public static event System.Action OnForgetMeEvent;
        
        private static System.Action _onPrivacySettingsClosedAction;
        private static event System.Action _onConsentProcessDoneEvent;
        
        public static event System.Action _onConsentUpdateEvent;

        private static IPrivacySettings _impl;
        private static IPrivacySettings Impl
        {
            get
            {
                if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.privacySettings)
                {
                    _impl = new EmptyImpl();
                }
                else if (_impl == null)
                {
                    if (TTPCore.DevMode)
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
                    Debug.LogError("TTPPrivacySettings::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

        /// <summary>
        /// Show web form view with privacy settings (For GDPR/CCPA etc).
        /// </summary>
        /// <param name="action">Callback for closed view action</param>
        public static void ShowPrivacySettings(System.Action action)
        {
            _onPrivacySettingsClosedAction = action;
            if (Impl != null)
                Impl.ShowPrivacySettings();
        }

        /// <summary>
        /// For GDPR legislation. instructs ttp to forget the user according to GDPR rules. 
        /// this will forward this request all relevant providers and crazy labs servers. 
        /// </summary>
        public static void ForgetUser()
        {
            if (Impl != null)
                Impl.ForgetUser();
        }

        /// <summary>
        /// Set user age
        /// </summary>
        /// <param name="age">Integer user age</param>
        public static void SetAge(int age)
        {
            if (Impl != null)
                Impl.SetAge(age);
        }

        /// <summary>
        /// Sets consent that was received from the user.
        /// NOTE THIS SHOULD ONLY BE CALLED if you received the consent status from the user.
        /// </summary>
        /// <param name="consentType">Custom consent type</param>
        /// <see cref="ConsentType"/>
        public static void CustomConsentSetConsent(ConsentType consentType)
        {
            if (Impl != null)
                Impl.SetConsent(consentType);
        }

        /// <summary>
        /// Returns current custom consent type
        /// </summary>
        /// <returns>Current consent type</returns>
        /// <see cref="ConsentType"/>see
        public static ConsentType CustomConsentGetConsent()
        {
            if (Impl != null)
                return Impl.GetConsent();
            return ConsentType.UNKNOWN;
        }

        /// <summary>
        /// Get boolean that indicated if the app should show age gate
        /// </summary>
        /// <returns>True - if the app should show age gate</returns>
        public static bool ShouldShowAgeGate()
        {
            if (Impl != null)
                return Impl.ShouldShowAgeGate();
            return false;
        }

        /// <summary>
        /// Get if we are in CCPA jurisdiction (California, USA)
        /// </summary>
        /// <returns>True - if it's CCPA jurisdiction</returns>
        public static bool IsCcpaJurisdiction()
        {
            if (Impl != null)
                return Impl.IsCcpaJurisdiction();
            return false;
        }

        private static void Register(System.Action onConsentProcessDoneAction)
        {
            Debug.Log("TTPPrivacySettings::Register");
            _onConsentProcessDoneEvent += onConsentProcessDoneAction;
        }
        
        [Preserve]
        private static void TriggerOnConsentModeReady()
		{
			Debug.Log("TriggerOnConsentModeReady");
			if (OnRemoteConsentModeReadyEvent != null)
				OnRemoteConsentModeReadyEvent(ConsentFormType.NONE);
		}

        private interface IPrivacySettings
        {
            void SetConsent(ConsentType consentType);
            ConsentType GetConsent();
            void ForgetUser();
            void ShowPrivacySettings();
            void SetAge(int age);
            bool ShouldShowAgeGate();
            bool IsCcpaJurisdiction();
        }

#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : IPrivacySettings {

            [DllImport ("__Internal")]
            private static extern string ttpPrivacySettingsGetConsent();

            [DllImport ("__Internal")]
            private static extern void ttpPrivacySettingsSetConsent(string consentType);

            [DllImport ("__Internal")]
            private static extern void ttpPrivacySettingsForgetUser();

            [DllImport ("__Internal")]
            private static extern void ttpPrivacySettingsShowPrivacySettings();

            [DllImport ("__Internal")]
            private static extern void ttpPrivacySettingsSetAge(int age);

            [DllImport ("__Internal")]
            private static extern bool ttpPrivacyShouldShowAgeGate();

            [DllImport("__Internal")]
            private static extern bool ttpIsCcpaJurisdiction();

            public void SetConsent (ConsentType consentType)
            {
                ttpPrivacySettingsSetConsent (consentType.ToString ());
            }
            public ConsentType GetConsent (){
                string consentStr = ttpPrivacySettingsGetConsent ();
                return StringToConsentType(consentStr);
            }
            public void ForgetUser() {
                ttpPrivacySettingsForgetUser ();
            }
            public void ShowPrivacySettings (){
                ttpPrivacySettingsShowPrivacySettings ();
            }
            public void SetAge(int age)
            {
                ttpPrivacySettingsSetAge(age);
            }
            public bool ShouldShowAgeGate()
            {
                return ttpPrivacyShouldShowAgeGate();
            }

            public bool IsCcpaJurisdiction()
            {
                return ttpIsCcpaJurisdiction();
            }

        }
#endif

#if UNITY_ANDROID
        private class AndroidImpl : IPrivacySettings {

            private const string SERVICE_GET_METHOD = "getPrivacySettings";

            private AndroidJavaObject _serivceJavaObject;

            private AndroidJavaObject ServiceJavaObject {
                get {
                    if (_serivceJavaObject == null) {
                        _serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject (SERVICE_GET_METHOD); 
                    }
                    if (_serivceJavaObject == null)
                        Debug.LogError ("TTPPrivacySettings::AndroidImpl: failed to get privacy settings native instance.");
                    return _serivceJavaObject;
                }
            }

            public void SetConsent (ConsentType consentType){
                if (ServiceJavaObject != null) {
                    ServiceJavaObject.Call ("setConsent", new object[] {consentType.ToString()});
                }
            }
            public ConsentType GetConsent (){
                if (ServiceJavaObject != null) {
                    string consentTypeStr = ServiceJavaObject.Call<string> ("getConsentStr");
                    return StringToConsentType (consentTypeStr);
                }
                return ConsentType.NE;
            }
            public void ForgetUser() {
                if (ServiceJavaObject != null) {
                    ServiceJavaObject.Call ("forgetUser");
                }
                
            }
            public void ShowPrivacySettings (){
                if (ServiceJavaObject != null) {
                    ServiceJavaObject.Call ("showPrivacySettings");
                }
            }

            public void SetAge(int age) {
                if (ServiceJavaObject != null) {
                    ServiceJavaObject.Call ("setAge", new object[] {age});
                }
            }
            public AudienceMode GetAudienceMode(){
                if (ServiceJavaObject != null) {
                    string audienceModeStr = ServiceJavaObject.Call<string> ("getAudienceModeStr");
                    return StringToAudienceMode (audienceModeStr);
                }
                return AudienceMode.NON_CHILDREN; 
            }

            public bool ShouldShowAgeGate()
            {
                if (ServiceJavaObject != null) {
                    return ServiceJavaObject.Call<bool>("shouldShowAgeGate");
                }
                return true;
            }

            public bool IsCcpaJurisdiction()
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("isCcpaJurisdiction");
                }
                return true;
            }

        }
#endif

        //#if UNITY_EDITOR
        private class EditorImpl : IPrivacySettings
        {
            private const string PLAYER_PREFS_CONSENT_MODE = "ttpPrivacySettingsConsent";
            private const string PLAYER_PREFS_AGE = "ttpPrivacySettingsAge";

            private ConsentType _consent = ConsentType.UNKNOWN;
            private int _age;
            private AudienceMode _audienceMode = AudienceMode.MIXED_UNKNOWN;

            private GameObject _consentCanvas;
            private GameObject _privacySettingsCanvas;

            [System.Serializable]
            private class PrivacySettingsData
            {
                public string consentFormURL = null;
                public string privacySettingsURL = null;
                public string consentFormVersion = null;
                public string audienceMode = null;
                public bool usePSDKGDPRPopups = true;
            }

            public EditorImpl()
            {
                _consent = StringToConsentType(PlayerPrefs.GetString(PLAYER_PREFS_CONSENT_MODE, "UNKNOWN"));
                _age = PlayerPrefs.GetInt(PLAYER_PREFS_AGE, -1);
#if UNITY_ANDROID
                //string path = System.IO.Path.Combine("jar:file://" + Application.dataPath + "!", "assets");
                //path = System.IO.Path.Combine(path, "ttp");
                string path = System.IO.Path.Combine("ttp", "configurations");
                path = System.IO.Path.Combine(path, "privacySettings.json");
                Debug.Log("shmulik - " + path);
                string privacySettingsJson = TTPUtils.ReadStreamingAssetsFile(path);
#else
                string path = Application.streamingAssetsPath + "/ttp/configurations/privacySettings.json";
                string privacySettingsJson = System.IO.File.ReadAllText(path);
#endif
                //string privacySettingsJson = System.IO.File.ReadAllText(path);

                if (privacySettingsJson != null)
                {
                    PrivacySettingsData privacySettingsData = JsonUtilityWrapper.FromJson<PrivacySettingsData>(privacySettingsJson);
                    if (privacySettingsData != null)
                    {
                        _audienceMode = StringToAudienceMode(privacySettingsData.audienceMode);
                        if (privacySettingsData.usePSDKGDPRPopups && _consent == ConsentType.UNKNOWN && _audienceMode != AudienceMode.MIXED_UNKNOWN)
                        {
                            ShowConsent();
                        }
                    }
                }
            }

            private void ShowConsent()
            {
                if (_consentCanvas == null)
                {
                    GameObject go = Resources.Load<GameObject>("Prefabs/PrivacySettingsConsentPopUp");
                    _consentCanvas = GameObject.Instantiate(go);
                    _consentCanvas.name = "TTPPrivacySettingsPopUp";
                }
                _consentCanvas.SetActive(true);
            }

            public void SetConsent(ConsentType consentType)
            {
                _consent = consentType;
                PlayerPrefs.SetString(PLAYER_PREFS_CONSENT_MODE, consentType.ToString());
            }
            public ConsentType GetConsent()
            {
                return _consent;
            }
            public void ForgetUser()
            {
                _consent = ConsentType.NPA;

            }
            public void ShowPrivacySettings()
            {
                if(_privacySettingsCanvas == null)
                {
                    GameObject go = Resources.Load<GameObject>("Prefabs/PrivacySettingsPopUp");
                    _privacySettingsCanvas = GameObject.Instantiate(go);
                    _privacySettingsCanvas.name = "TTPPrivacySettingsPopUp";
                }
                _privacySettingsCanvas.SetActive(true);

            }
            public bool IsPrivacyWebViewDisplayed()
            {
                return false;
            }

            public void SetAge(int age)
            {
                bool shouldShowConsent = _audienceMode == AudienceMode.MIXED_UNKNOWN;
                _age = age;
                if (_age < 16)
                {
                    _audienceMode = AudienceMode.CHILDREN;
                }
                else
                {
                    _audienceMode = AudienceMode.NON_CHILDREN;
                }
                PlayerPrefs.SetInt(PLAYER_PREFS_AGE, _age);
                if (shouldShowConsent)
                {
                    ShowConsent();
                }

            }

            public bool ShouldShowAgeGate()
            {
                return _audienceMode == AudienceMode.MIXED_UNKNOWN;
            }

            public bool IsCcpaJurisdiction()
            {
                return false;
            }
        }
//#endif

        //Class methods

        private static ConsentType StringToConsentType(string consentTypeStr)
        {
            ConsentType result = ConsentType.UNKNOWN;
            if (consentTypeStr != null)
            {
                if (consentTypeStr.Equals("pa", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = ConsentType.PA;
                }
                else if (consentTypeStr.Equals("npa", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = ConsentType.NPA;
                }
                else if (consentTypeStr.Equals("ua", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = ConsentType.UA;
                }
                else if (consentTypeStr.Equals("ne", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = ConsentType.NE;
                }
            }
            return result;
        }

        private static AudienceMode StringToAudienceMode(string audeinceModeStr)
        {
            AudienceMode result = AudienceMode.NON_CHILDREN;
            if (audeinceModeStr != null)
            {
                if (audeinceModeStr.Equals("children", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AudienceMode.CHILDREN;
                }
                else if (audeinceModeStr.Equals("non-children", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AudienceMode.NON_CHILDREN;
                }
                else if (audeinceModeStr.Equals("mixed", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AudienceMode.MIXED_UNKNOWN;
                }
                else if (audeinceModeStr.Equals("mixed-children", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AudienceMode.MIXED_CHILDREN;
                }
                else if (audeinceModeStr.Equals("mixed-non-children", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    result = AudienceMode.MIXED_NON_CHILDREN;
                }
            }
            return result;
        }
        
        private class EmptyImpl : IPrivacySettings
        {
            public void SetConsent(ConsentType consentType)
            {
            }

            public ConsentType GetConsent()
            {
                return ConsentType.UNKNOWN;
            }

            public void ForgetUser()
            {
            }

            public void ShowPrivacySettings()
            {
            }

            public void SetAge(int age)
            {
            }

            public bool ShouldShowAgeGate()
            {
                return false;
            }

            public bool IsCcpaJurisdiction()
            {
                return false;
            }
        }

        /// <summary>
        /// This class provides notifications about changes using events. 
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        [Preserve]
        public class PrivacySettingsDelegate : MonoBehaviour
        {
            [System.Serializable]
            private class OnRemoteConsentModeReadyEventMessage
            {
                public string consentFormType = null;
            }

            public void OnRemoteConsentModeReady(string messageStr)
            {
                ConsentFormType consentFormType = ConsentFormType.NONE;
                Debug.Log("TTPPrivacySettings::OnRemoteConsentModeReady jsonMessage: " + messageStr);
                OnRemoteConsentModeReadyEventMessage message = JsonUtilityWrapper.FromJson<OnRemoteConsentModeReadyEventMessage>(messageStr);
                if (message != null && message.consentFormType != null)
                {
                    if (message.consentFormType.Equals("NONE", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        consentFormType = ConsentFormType.NONE;
                    }
                    else if (message.consentFormType.Equals("ANY", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        consentFormType = ConsentFormType.ANY;
                    }
                    else if (message.consentFormType.Equals("NO_PURCHASE", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        consentFormType = ConsentFormType.NO_PURCHASE;
                    }
                }
                else
                {
                    Debug.LogError("TTPPrivacySettings::OnRemoteConsentModeReady failed to serialize message.");
                }
                if (OnRemoteConsentModeReadyEvent != null)
                {
                    OnRemoteConsentModeReadyEvent(consentFormType);
                }
                else
                {
                    Debug.Log("TTPPrivacySettings::OnRemoteConsentModeReadyEvent fired but no one is registered to it.");
                }
            }

			public void OnPrivacySettingsPopUpClosed()
			{
                Debug.Log("OnPrivacySettingsPopUpClosed " + (_onPrivacySettingsClosedAction == null ? "null" : "NOT null"));
				if (_onPrivacySettingsClosedAction != null)
				{
					_onPrivacySettingsClosedAction.Invoke();
				}
            }

            public void OnConsentProcessDone(string messageStr)
            {
                Debug.Log("PrivacySettingsDelegate::OnConsentProcessDone");
                if(_onConsentProcessDoneEvent != null)
                {
                    _onConsentProcessDoneEvent.Invoke();
                }
            }
            
            public void OnConsentUpdate(string messageStr)
            {
                Debug.Log("PrivacySettingsDelegate::OnConsentUpdate");
                if(_onConsentUpdateEvent != null)
                {
                    _onConsentUpdateEvent.Invoke();
                }
            }

            public void OnForgetMe()
            {
                Debug.Log("PrivacySettingsDelegate::OnForgetMe");
                if (OnForgetMeEvent != null)
                {
                    OnForgetMeEvent();
                }
                else
                {
                    Debug.Log("TTPPrivacySettings::OnForgetMe fired but no one is registered to it.");
                }
            }
        }
    }
}
#endif