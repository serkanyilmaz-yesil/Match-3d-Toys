#if TTP_ANALYTICS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    /// Semi-enumeration class for choosing an analytic target.
    /// </summary>

    public static class AnalyticsTargets
    {
        [Obsolete]
        public const long ANALYTICS_TARGET_FLURRY = 0;
        public const long ANALYTICS_TARGET_TT_ANALYTICS = 1;
        public const long ANALYTICS_TARGET_DELTA_DNA = 1 << 1;
        public const long ANALYTICS_TARGET_FIREBASE = 1 << 2;
        public const long ANALYTICS_PSDK_INTERNAL_TARGETS = ANALYTICS_TARGET_TT_ANALYTICS;
    }

    /// <summary>
    /// This class provides:
    /// + simple access to sending analytics event logs
    /// + support for A/B Testing (using DDNA engagement/Firebase Remote Config).
    /// + DDNA Image Enagements
    /// </summary>
    public class TTPAnalytics
    {
        /// <summary> Called when DDNA is initialized and ready</summary>
        public static event System.Action<bool> OnDeltaDnaReadyEvent;

        /// <summary> (Firebase) Called when remote value provider is ready. (Reote value provider is firebase)</summary>
        public static event System.Action<bool> OnRemoteValueProviderReadyEvent;

        /// <summary> (Firebase) Called when remote config fetch is compeleted. Will be called once in the beginning of the game(Firebase remote config).</summary>
        public static event System.Action<bool> OnRemoteFetchCompletedEvent;

        /// <summary> (DDNA) Image message Fetch complete = succeeded or failed.
        /// string decisionPoint, bool didFetchSucceed, string error  </summary>
        /// <returns>
        public static event System.Action<string, bool, string> OnDidFetchCompleteForImageMessage;

        /// <summary> (DDNA) buttonImageSource is the source from which the dismiss originated (configured in DDNA).
        /// string decisionPoint </summary>
        public static event System.Action<string> OnDismissImageMessage;

        public static event System.Action<string, IDictionary<string,object>> OnDDNALogEvent;
        
        /// <summary> Called when the image engagement calls a custom action (see action value) </summary>
        /// <typeparam name="decisionPoint"> decision point name identifier </typeparam>
        /// <typeparam name="actionValue"> actionValue is a string that is configured in DDNA dashboard, and can describe anything the game wants to do as a
        /// reaction to pressing a button (or the background).</typeparam>
        /// <typeparam name="additionalParams">additional params</typeparam>
        public static event System.Action<string, string, IDictionary<string, object>> OnActionImageMessage;
        
        public delegate int GetUserScoreDelegate();

        private static System.Action<string, Dictionary<string, object>> _onDecisionPointResponseAction;
        private static System.Action<string> _onRequestValueResponseAction;
        private static System.Action<IDictionary<string, object>> _onRequestValueDictionaryResponseAction;

        private static GetUserScoreDelegate _getUserScoreDelegate;

        private static Dictionary<string, object> _overrideConfig;

        private static Action<string> _getGeoCallback;

        /// <summary> (Firebase) Sync method, Gets a value as string from key, from the configuration (remote config - firebase).
        ///  Do not call this method before you received #OnRemoteFetchCompletedEvent </summary>
        public static string GetStringValue(string key)
        {
            if (_overrideConfig != null && _overrideConfig.ContainsKey(key))
            {
                return (string)_overrideConfig[key];
            }

            if (Impl != null)
            {
               return Impl.GetRemoteStringSync(key);
            }

            return "";
        }

        /// <summary>(Firebase) - Check if remote fetch is complete #OnRemoteFetchCompletedEvent </summary>
        public static bool IsRemoteFetchComplete()
        {
            if (Impl != null)
            {
                return Impl.IsRemoteFetchComplete();
            }

            return false;
        }

        private static void NotifyOnRemoteFetchCompletedEvent(bool fetchSucceeded)
        {
            Debug.Log("TTPAnalytics::NotifyOnRemoteFetchCompletedEvent:fetchSucceeded=" + fetchSucceeded);
            if (OnRemoteFetchCompletedEvent != null)
                OnRemoteFetchCompletedEvent.Invoke(fetchSucceeded);
        }
        
        /// <summary>(Firebase) ASYNC method, Gets a value as string from key, from the configuration
        /// (remote config - firebase). Note that this can be called before #OnRemoteFetchCompletedEvent is received.
        /// and if it is called before #OnRemoteFetchCompletedEvent, it will return once #OnRemoteFetchCompletedEvent shoots or according to the input timeout
        /// </summary>
        public static bool GetRemoteValue(string key, System.Action<string> onRequestValueResponseAction, double timeout = 4.0)
        {
            if (!TTPCore.IsRemoteConfigExistAndEnabled())
            {
                Debug.Log("GetRemoteValue:: remoteconfig doesn't exist");
                return false;
            }
            if (Impl != null)
            {
                _onRequestValueResponseAction = onRequestValueResponseAction;
                if (!Impl.GetRemoteValue(key, timeout))
                {
                    System.Reflection.MethodInfo method = typeof(TTPCore).GetMethod("GetTTPGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (method != null)
                    {
                        GameObject gameObject = method.Invoke(null, null) as GameObject;
                        if (gameObject != null)
                        {
                            MonoBehaviour mono = gameObject.GetComponent<MonoBehaviour>();
                            mono.StartCoroutine(CallGetValueResponseAction(key));
                        }
                    }
                    else
                    {
                        Debug.LogWarning("GetRemoteValue:: could not find method GetTTPGameObject");
                    }
                }
                else
                    return true;
            }
            return false;
        }

        private static IEnumerator CallGetValueResponseAction(string key)
        {
            yield return new WaitForEndOfFrame();
            Debug.LogWarning("TTPAnalytics::CallGetValueResponseAction: failed to reach FireBase for value, calling action for " + key);
            if (_onRequestValueResponseAction != null)
                _onRequestValueResponseAction.Invoke("");
        }

        /// <summary> (Firebase) ASYNC method, Gets a dictionary as strings from a keys list, from the configuration
        /// (remote config - firebase). Note that this can be called before #OnRemoteFetchCompletedEvent is received.
        /// and if it is called before #OnRemoteFetchCompletedEvent, it will return once #OnRemoteFetchCompletedEvent shoots or according to the input timeout
        /// </summary>
        [Preserve]
        public static bool GetRemoteValueDictionary(IList<string> keys, System.Action<IDictionary<string, object>> onRequestValueDictionaryResponseAction, double timeout = 4.0)
        {
            if (!TTPCore.IsRemoteConfigExistAndEnabled())
            {
                Debug.Log("GetRemoteValueDictionary:: remoteconfig doesn't exist");
                return false;
            }
            if (Impl != null)
            {
                _onRequestValueDictionaryResponseAction = onRequestValueDictionaryResponseAction;
                if (!Impl.GetRemoteDictionary(keys,timeout))
                {
                    System.Reflection.MethodInfo method = typeof(TTPCore).GetMethod("GetTTPGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (method != null)
                    {
                        GameObject gameObject = method.Invoke(null, null) as GameObject;
                        if (gameObject != null)
                        {
                            MonoBehaviour mono = gameObject.GetComponent<MonoBehaviour>();
                            mono.StartCoroutine(CallGetValueDicResponseAction(keys));
                        }
                    }
                    else
                    {
                        Debug.LogWarning("GetRemoteValueDictionary:: could not find method GetTTPGameObject");
                    }
                }
                else
                    return true;
            }
            return false;
        }

        private static IEnumerator CallGetValueDicResponseAction(IList<string> keys)
        {
            yield return new WaitForEndOfFrame();
            Debug.LogWarning("TTPAnalytics::CallGetValueResponseAction: failed to reach FireBase for value, calling action for " + keys.ToString());
            if (_onRequestValueDictionaryResponseAction != null)
                _onRequestValueDictionaryResponseAction.Invoke(null);
        }

        public static void SetGetUserScoreDelegate(GetUserScoreDelegate getUserScoreDelegate)
        {
            _getUserScoreDelegate = getUserScoreDelegate;
        }

        /// <summary> Logs an analytics event to one or more analytics provider. </summary>
        /// <param name="targets">which targets to send the event to. see AnalyticsTargets</param>
        /// <param name="eventName">the name of the analytic event</param>
        /// <param name="eventParams">dictionary of key-values that describe the event</param>
        /// <param name="timed">if this event should be timed, meaning it will only be sent when EndLogEvent() is sent, with an additional parameter that denotes how much time it took.</param>
        [Preserve]
        public static void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal = false)
        {
            if (Impl != null)
                Impl.LogEvent(targets, eventName, eventParams, timed, ttpInternal);
        }

        /// <summary> Use in conjunction with LogEvent(), timed=true to measure the time of a event </summary>
        /// <param name="eventName">the name of the analytic event, should match the name of a previous event</param>
        /// <param name="eventParams">additional params to the event</param>
        public static void EndLogEvent(string eventName, IDictionary<string, object> eventParams)
        {
            if (Impl != null)
                Impl.EndLogEvent(eventName, eventParams);
        }

        /// <summary> (DDNA) Call a decision point to start a DDNA engagement. </summary>
        /// <param name="decisionPoint">the name of the decisionPoint. (Should be configured in DDNA)</param>
        /// <param name="parameters">params sent with the decision point</param>
        /// <param name="onDecisionPointResponseAction">callback for when the decision point returns a response</param>
        /// <param name="timeout">timeout in seconds to wait for the decision point engagement response</param>
        public static bool CallDecisionPoint(string decisionPoint, Dictionary<string, object> parameters, System.Action<string, Dictionary<string,object>> onDecisionPointResponseAction, double timeout = 4.0)
        {
            Debug.Log("TTPAnalytics::CallDecisionPoint:decisionPoint=" + decisionPoint);
            _onDecisionPointResponseAction = onDecisionPointResponseAction;
            Debug.LogWarning("TTPAnalytics::CallDecisionPoint: could not find class TTPDeltaDnaAgent");
            System.Reflection.MethodInfo method = typeof(TTPCore).GetMethod("GetTTPGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                GameObject gameObject = method.Invoke(null, null) as GameObject;
                if (gameObject != null)
                {
                    MonoBehaviour mono = gameObject.GetComponent<MonoBehaviour>();
                    mono.StartCoroutine(CallDecisionPointResponseAction(decisionPoint));
                }
            }
            else
            {
                Debug.LogWarning("TTPAnalytics::CallDecisionPoint: could not find method GetTTPGameObject");
            }
            return false;
        }

        public static void GetGeoCodeWhenReady(Action<string> callback)
        {
            _getGeoCallback = callback;
            if (Impl != null)
            {
                Impl.GetGeo();
            }
        }

        public static Dictionary<string, object> GetAdditionalParams()
        {
            if (Impl != null)
            {
                var str = Impl.GetAdditionalParams();
                if (str != null)
                {
                    return TTPJson.Deserialize(str) as Dictionary<string, object>;
                }
            }
            return null;
        }
        
        /// <summary> (DDNA) Show an image message of a decision point.
        /// Note that CallDecisionPoint() should first be called with a decision point that was configured with image engagements,
        // and #OnDidFetchCompleteForImageMessage should have return with true</summary>
        /// <param name="decisionPoint">the name of the decisionPoint. (Should be configured in DDNA)</param>
        public static bool ShowImageMessage(string decisionPoint)
        {
            Debug.Log("TTPAnalytics::ShowImageMessage:decisionPoint=" + decisionPoint);
            return false;
        }

        private static IEnumerator CallDecisionPointResponseAction(string decisionPoint)
        {
            yield return new WaitForEndOfFrame();
            Debug.LogWarning("TTPAnalytics::CallDecisionPointResponseAction: failed to reach DDNA for decision point, calling action for " + decisionPoint);
            if (_onDecisionPointResponseAction != null)
                _onDecisionPointResponseAction.Invoke(decisionPoint, null);
        }
        
        [Preserve]
        public static void LogTransaction(string transactionName,
                                          IDictionary<string, object> productsReceived,
                                          IDictionary<string, object> productsSpent,
                                          IDictionary<string, object> otherEventParams)
        {
            Debug.Log("TTPAnalytics::LogTransaction: transactionName=" + transactionName);
            if (Impl != null){
                Dictionary<string, object> log = new Dictionary<string, object>();
                log["transactionName"] = transactionName ?? "transaction";
                log["transactionType"] = "PURCHASE";
                log["productsReceived"] = productsReceived ?? new Dictionary<string, object>();
                log["productsSpent"] = productsSpent ?? new Dictionary<string, object>();
                int userScore = -1;
                if (_getUserScoreDelegate != null)
                    userScore = _getUserScoreDelegate.Invoke();
                if (userScore > -1)
                {
                    log.Add("userScore", userScore);
                }
                if (otherEventParams != null)
                {
                    foreach (var item in otherEventParams)
                    {
                        log.Add(item.Key, item.Value);
                    }
                }

                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_DELTA_DNA, TTPEvents.TRANSACTION, log, false, true);
            }
        }

        [Preserve]
        static string GetInstanceIdFirebase()
        {
            return Impl.GetFirebaseInstanceId();
        }

        [Preserve]
        static Dictionary<string, object> OverrideCurrentConfig()
        {
            if (_overrideConfig == null)
            {
                var jsonStr = Impl.GetCurrentConfig();
                if (!string.IsNullOrEmpty(jsonStr))
                {
                    _overrideConfig = TTPJson.Deserialize(jsonStr) as Dictionary<string, object>;

                }
            }
            return _overrideConfig;
        }

        [Preserve]
        static void OverrideParameter(string key, object val)
        {
            _overrideConfig[key] = val;
        }
        
        [Preserve]
        static void LevelUp(string skinName, string levelUpName, int level, Dictionary<string, object> additionalParams)
        {
            if(Impl != null){
                IDictionary<string, object> extras = new Dictionary<string, object>();
                extras["userLevel"] = level;

                IDictionary<string, object> eventParams = new Dictionary<string, object>();
                eventParams["levelUpName"] = levelUpName;
                eventParams["skinName"] = skinName;

                Impl.AddExtras(extras);

                if (additionalParams != null)
                {
                    foreach (var item in additionalParams)
                    {
                        eventParams.Add(item.Key, item.Value);
                    }
                }

                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_DELTA_DNA | AnalyticsTargets.ANALYTICS_TARGET_FIREBASE,
                    TTPEvents.LEVEL_UP, eventParams, false, false);
            }

        }
        
        // new API for mission logs
        [Preserve]
        static void LevelUpFirebase(int level, Dictionary<string, object> additionalParams)
        {
            if (Impl != null)
            {
                IDictionary<string, object> extras = new Dictionary<string, object>();
                extras["userLevel"] = level;

                IDictionary<string, object> eventParams = new Dictionary<string, object>();

                Impl.AddExtras(extras);

                if (additionalParams != null)
                {
                    foreach (var item in additionalParams)
                    {
                        eventParams.Add(item.Key, item.Value);
                    }
                }

                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_FIREBASE, 
                    TTPEvents.LEVEL_UP, eventParams, false, false);
            }
        }
        
        [Preserve]
        static void MissionStarted(string id, string name, string type, string missionStartedType, Dictionary<string, object> additionalParams)
        {
            if(Impl != null){
                IDictionary<string, object> extras = new Dictionary<string, object>()
                {
                    {"missionID", id},
                    {"missionName",name},
                    {"missionType",type},
                    {"missionStartedType", missionStartedType}
                };

                Impl.AddExtras(extras);

                if (additionalParams != null)
                {
                    foreach (var item in additionalParams)
                    {
                        extras.Add(item.Key, item.Value);
                    }
                }
                extras.Add("isTutorial", false);
                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_DELTA_DNA | AnalyticsTargets.ANALYTICS_TARGET_FIREBASE,
                    TTPEvents.MISSION_STARTED, extras, false, false);
            }
        }

        // new API for mission logs
        [Preserve]
        static void MissionStartedFirebase(int id, Dictionary<string, object> additionalParams)
        {
            if (Impl != null)
            {
                List<string> keys = new List<string>
                {
                    "missionID",
                    "missionName",
                    "missionType",
                    "missionStartedType"
                };
                Impl.RemoveExtras(keys);

                IDictionary<string, object> extras = new Dictionary<string, object>()
                {
                    {"missionID", id}
                };
                Impl.AddExtras(extras);

                if (additionalParams != null)
                {
                    foreach (var item in additionalParams)
                    {
                        extras.Add(item.Key, item.Value);
                    }
                }
                extras.Add("isTutorial", false);
                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_FIREBASE,
                    TTPEvents.MISSION_STARTED, extras, false, false);
            }
        }
        
        [Preserve]
        static void MissionComplete(string eventName, Dictionary<string, object> additionalParams)
        {
            if(Impl != null){
                IDictionary<string, object> eventParams = new Dictionary<string, object>();
				if (additionalParams != null) {
					foreach (var item in additionalParams) {
						eventParams.Add (item.Key,item.Value);
					}
				}
                eventParams.Add("isTutorial", false);
                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_DELTA_DNA | AnalyticsTargets.ANALYTICS_TARGET_FIREBASE,
                    eventName, eventParams, false, false);
                List<string> keys = new List<string>
                {
                    "missionID",
                    "missionName",
                    "missionType",
		            "missionStartedType"
                };
                Impl.RemoveExtras(keys);
            }
        }
        
        [Preserve]
        static void MissionCompleteFirebase(string eventName, Dictionary<string, object> additionalParams)
        {
            if (Impl != null)
            {
                IDictionary<string, object> eventParams = new Dictionary<string, object>();
                if (additionalParams != null)
                {
                    foreach (var item in additionalParams)
                    {
                        eventParams.Add(item.Key, item.Value);
                    }
                }
                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_FIREBASE, eventName, eventParams, false, false);
                List<string> keys = new List<string>
                {
                    "missionID",
                    "missionName",
                    "missionType",
                    "missionStartedType"
                };
                Impl.RemoveExtras(keys);
            }
        }
        
        [Preserve]
        static void TutorialStep(bool isMandatory, int tutorialStepID, string tutorialName, string tutorialStepName, Dictionary<string, object> additionalParams)
        {

            if (Impl != null){
                IDictionary<string, object> itemDDNA = new Dictionary<string, object>
                {
                    { "stepType", isMandatory ? "mandatory" : "optional" },
                    { "tutorialStepID", tutorialStepID },
                    { "tutorialName", tutorialName },
                    { "tutorialStepName", tutorialStepName }
                };

                if (additionalParams != null)
                {
                    foreach (var item in additionalParams)
                    {
                        itemDDNA.Add(item.Key, item.Value);
                    }
                }

                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_DELTA_DNA | AnalyticsTargets.ANALYTICS_TARGET_FIREBASE,
                    TTPEvents.TUTORIAL_STEP, itemDDNA, false, false);
            }
        }
        
        [Preserve]
        static void ReachedMainScreen(Dictionary<string, object> additionalParams)
        {
            if(Impl != null){
                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_DELTA_DNA | AnalyticsTargets.ANALYTICS_TARGET_FIREBASE,
                    TTPEvents.MAIN_SCREEN, additionalParams, false, false);
            }
        }
        
        [Preserve]
        static void ExcludeFromABTest(string decisionPoint)
        {
            if(Impl != null){
                IDictionary<string, object> extras = new Dictionary<string, object>()
                {
                    {"decisionPoint", decisionPoint}
                };
                Impl.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_TT_ANALYTICS | AnalyticsTargets.ANALYTICS_TARGET_DELTA_DNA | AnalyticsTargets.ANALYTICS_TARGET_FIREBASE,
                    TTPEvents.EXCLUDE_FROM_AB_TEST, extras, false, true);
            }
        }
        
        [Preserve]
        private static void TriggerOnDeltaDnaReady(bool isReady, string userId)
        {
            Debug.Log("TTPAnalytics::TriggerOnDeltaDnaReady");
            Impl.DdnaIsReady(isReady, userId);
            if (OnDeltaDnaReadyEvent != null)
                OnDeltaDnaReadyEvent(isReady);
        }
        
        [Preserve]
        static void OnDecisionPointResponse(string decisionPoint, Dictionary<string, object> parameters)
        {
            Debug.Log("TTPAnalytics::OnDecisionPointResponse: decisionPoint=" + decisionPoint);
            if (_onDecisionPointResponseAction != null)
            {
                _onDecisionPointResponseAction.Invoke(decisionPoint, parameters);
                _onDecisionPointResponseAction = null;
            }
        }
        
        [Preserve]
        private static void onImageMessageFetchCompleted(string decisionPoint, bool success, string error)
        {
            Debug.Log("TTPAnalytics::onImageMessageFetchCompleted: decisionPoint=" + decisionPoint);
            OnDidFetchCompleteForImageMessage.Invoke(decisionPoint, success, error);
        }
        
        [Preserve]
        private static void onImageMessageDismissed(string decisionPoint)
        {
            Debug.Log("TTPAnalytics::onImageMessageDismissed: " + decisionPoint);
            if (OnDismissImageMessage != null && !string.IsNullOrEmpty(decisionPoint))
            {
                OnDismissImageMessage(decisionPoint);
            }
        }
        
        [Preserve]
        private static void onImageMessageAction(string decisionPoint, string action, Dictionary<string, object> parameters)
        {
            Debug.Log("TTPAnalytics::onImageMessageAction:decisionPoint=" + decisionPoint + " aciton=" + action);
            if (OnActionImageMessage != null && !string.IsNullOrEmpty(decisionPoint))
            {
                OnActionImageMessage.Invoke(decisionPoint, action, parameters);
            }
        }

        private static IAnalytics _impl;
        private static IAnalytics Impl
        {
            get
            {

                if (_impl == null)
                {
                    
                    if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.analytics)
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
                    Debug.LogError("TTPAnalytics::Impl: failed to create native impl");
                }
                return _impl;
            }
        }

        private interface IAnalytics
        {
            void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal = false);
            void EndLogEvent(string eventName, IDictionary<string, object> eventParams);
            void AddExtras(IDictionary<string, object> extras);
            void RemoveExtras(IList<string> keys);
            bool GetRemoteValue(string key, double timeoutInSecs);
            string GetRemoteStringSync(string key);
            bool GetRemoteDictionary(IList<string> keys, double timeoutInSecs);
            bool IsRemoteFetchComplete();
            string GetFirebaseInstanceId();
            string GetCurrentConfig();
            void DdnaIsReady(bool isReady, string userId);
            void GetGeo();
            string GetAdditionalParams();
        }

#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : IAnalytics
        {
            [DllImport("__Internal")]
            private static extern void ttpLogEvent(long targets, string eventName, string eventParamsJson, bool timed, bool ttpInternal);

            [DllImport("__Internal")]
            private static extern void ttpEndLogEvent(string eventName, string eventParamsJson);

            [DllImport("__Internal")]
            private static extern bool ttpGetRemoteValue(string key, double timeoutInSecs);

            [DllImport("__Internal")]
            private static extern bool ttpGetRemoteDictionary(string keys, double timeoutInSecs);

            [DllImport("__Internal")]
            private static extern void ttpAddExtras(string extrasStrJson);

            [DllImport("__Internal")]
            private static extern void ttpRemoveExtras(string keysStr);

            [DllImport("__Internal")]
            private static extern string ttpStringForKey(string key);

            [DllImport("__Internal")]
            private static extern bool ttpDidFetchComplete();

            [DllImport("__Internal")]
            private static extern string ttpGetFirebaseInstanceId();

            [DllImport("__Internal")]
            private static extern string ttpGetCurrentConfig();

            [DllImport("__Internal")]
            private static extern void ttpDdnaIsReady(bool isReady, string userId);
            
            [DllImport("__Internal")]
            private static extern void ttpGetGeo();

            [DllImport("__Internal")]
            private static extern string ttpGetAdditionalParams();

            public void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal)
            {
                ttpLogEvent(targets, eventName,eventParams != null ? TTPJson.Serialize(eventParams) : "{}", timed, ttpInternal);
            }

            public void EndLogEvent(string eventName, IDictionary<string, object> eventParams)
            {
                ttpEndLogEvent(eventName, eventParams != null ? TTPJson.Serialize(eventParams) : "{}");
            }

            public bool GetRemoteValue(string key, double timeoutInSecs)
            {
                return ttpGetRemoteValue(key, timeoutInSecs);
            }

            public string GetRemoteStringSync(string key)
            {
                return ttpStringForKey(key);
            }

            public bool GetRemoteDictionary(IList<string> keys, double timeoutInSecs)
            {
                return ttpGetRemoteDictionary(keys != null ? TTPJson.Serialize(keys): "[]", timeoutInSecs);
            }

            public void AddExtras(IDictionary<string, object> extras)
            {
                ttpAddExtras(extras != null ? TTPJson.Serialize(extras) : "{}");
            }

            public void RemoveExtras(IList<string> keys)
            {
                string keysStr = keys[0];
                for (int i = 1; i < keys.Count; i++){
                    keysStr += (";" + keys[i]);
                }
                ttpRemoveExtras(keysStr);
            }

            public bool IsRemoteFetchComplete()
            {
                return ttpDidFetchComplete();
            }

            public string GetFirebaseInstanceId()
            {
                return ttpGetFirebaseInstanceId();
            }

            public string GetCurrentConfig()
            {
                return ttpGetCurrentConfig();
            }

            public void DdnaIsReady(bool isReady, string userId)
            {
                ttpDdnaIsReady(isReady, userId);
            }

            public void GetGeo()
            {
                ttpGetGeo();
            }

            public string GetAdditionalParams()
            {
                return ttpGetAdditionalParams();
            }
        }
#endif

#if UNITY_ANDROID
        private class AndroidImpl : IAnalytics
        {
            private const string SERVICE_GET_METHOD = "getAnalytics";

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
                        Debug.LogError("TTPAnalytics::AndroidImpl: failed to get analytics native instance.");
                    return _serivceJavaObject;
                }
            }

            public void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal)
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("logEvent", targets, eventName, eventParams != null ? TTPJson.Serialize(eventParams) : "{}", timed, ttpInternal);
                }
            }

            public void EndLogEvent(string eventName, IDictionary<string, object> eventParams)
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("endLogEvent", eventName, eventParams != null ? TTPJson.Serialize(eventParams) : "{}");
                }
            }

            public void AddExtras(IDictionary<string, object> extras)
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("addExtras", extras != null ? TTPJson.Serialize(extras) : "{}");
                }
            }

            public void RemoveExtras(IList<string> keys)
            {
                if (ServiceJavaObject != null)
                {
                    string keysStr = keys[0];
                    for (int i = 1; i < keys.Count; i++)
                    {
                        keysStr += (";" + keys[i]);
                    }
                    ServiceJavaObject.Call("removeExtras", keysStr);
                }
            }

            public bool GetRemoteValue(string key, double timeoutInSecs)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("getRemoteValue", key, 4.0);
                }
                return false;

            }

            public string GetRemoteStringSync(string key)
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<string>("getStringValue", key);
                }
                return "";
            }
            
            public bool GetRemoteDictionary(IList<string> keys, double timeoutInSecs)
            {
                if (ServiceJavaObject != null)
                {
                    string keysStr = keys[0];
                    for (int i = 1; i < keys.Count; i++)
                    {
                        keysStr += (";" + keys[i]);
                    }
                    return ServiceJavaObject.Call<bool>("getRemoteDictionaryForKeys", keysStr, 4.0);
                }
                return false;
            }

            public bool IsRemoteFetchComplete()
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<bool>("didFetchComplete");
                }
                return false;
            }

            public string GetFirebaseInstanceId()
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<string>("getFirebaseInstanceId");
                }
                return "";
            }

            public string GetCurrentConfig()
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<string>("getFirebaseCurrentConfig");
                }
                return "";
            }

            public void DdnaIsReady(bool isReady, string userId)
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("ddnaIsReady", isReady, userId);
                }
            }

            public void GetGeo()
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("getAndSendGeoCodeAsync");
                }
            }
            
            public string GetAdditionalParams()
            {
                if (ServiceJavaObject != null)
                {
                    return ServiceJavaObject.Call<string>("getAdditionalEventParamsJson");
                }
                return null;
            }
        }
#endif
        private class EditorImpl : IAnalytics
        {
            public void LogEvent(long targets, string eventName, IDictionary<string, object> eventParams, bool timed, bool ttpInternal)
            {
                string paramsStr = "";
                if(eventParams != null && eventParams.Count > 0)
                {
                    paramsStr = TTPJson.Serialize(eventParams);
                }
                Debug.Log("TTPAnalytics::EditorImpl::LogEvent: eventName: " + eventName + "\neventParams: " + paramsStr + "\ntimed: " + timed);
            }

            public void EndLogEvent(string eventName, IDictionary<string, object> eventParams)
            {
                string paramsStr = "";
                if (eventParams != null && eventParams.Count > 0)
                {
                    paramsStr = TTPJson.Serialize(eventParams);
                }
                Debug.Log("TTPAnalytics::EditorImpl::EndLogEvent: eventName: " + eventName + "\neventParams: " + paramsStr);
            }

            public bool CallDecisionPoint(string decisionPoint, Dictionary<string, object> parameters, double timeoutInSecs)
            {
                string paramsStr = "";
                if (parameters != null && parameters.Count > 0)
                {
                    paramsStr = TTPJson.Serialize(parameters);
                }
                Debug.Log("TTPAnalytics::EditorImpl::CallDecisionPoint: decisionPoint: " + decisionPoint + "\nparameters: " + paramsStr);
                return false;
            }

            public void AddExtras(IDictionary<string, object> extras)
            {
            }

            public void RemoveExtras(IList<string> keys)
            {

            }

            public bool GetRemoteValue(string key, double timeoutInSecs)
            {
                return false;
            }

            public string GetRemoteStringSync(string key)
            {
                return "";
            }

            public bool GetRemoteDictionary(IList<string> keys, double timeoutInSecs)
            {
                return false;
            }

            public bool ShowImageMessage(string decisionPoint)
            {
                return false;
            }

            public bool IsRemoteFetchComplete()
            {
                return false;
            }

            public string GetFirebaseInstanceId()
            {
                return "";
            }

            public string GetCurrentConfig()
            {
                string path = TTPUtils.CombinePaths(
                    new List<string> { "Assets", "StreamingAssets", "ttpGameConfig", "keyvalue", "deafults.json" });
                if (File.Exists(path))
                {
                    return System.IO.File.ReadAllText(path);
                }
                return null;

            }

            public void DdnaIsReady(bool isReady, string userId)
            {
                Debug.Log("TTPAnalytics::EditorImpl::DdnaIsReady: isReady=" + isReady + " userId=" + userId);
            }

            public void GetGeo()
            {
                if (_getGeoCallback != null)
                {
                    _getGeoCallback.Invoke("US");
                }
            }
            
            public string GetAdditionalParams()
            {
                return "{\"fakeAdditionalParams\":\"howdy\"}";
            }
        }

        /// <summary>
        /// This class provides notifications about changes using events.
        /// Add this class as a unity component for compatibility with SendUnityMessage.
        /// </summary>
        [Preserve]
        class AnalyticsDelegate : MonoBehaviour
        {
            private class OnRemoteFetchCompleteMessage
            {
                public bool fetchSucceeded = false;
            }

            private class OnRemoteValueProviderReadyMessage
            {
                public bool ready = false;
            }
            
            private class OnGotGetCodeMessage
            {
                public string geo = "NA";
            }

            public void onRemoteFetchCompleted(string messageStr)
            {
                Debug.Log("TTPAnalytics::AnalyticsDelegate::onRemoteFetchCompleted: " + messageStr);
                if (messageStr != null)
                {
                    OnRemoteFetchCompleteMessage onRemoteFetchCompleteMessage = JsonUtility.FromJson<OnRemoteFetchCompleteMessage>(messageStr);
                    if (onRemoteFetchCompleteMessage != null)
                    {
                        NotifyOnRemoteFetchCompletedEvent(onRemoteFetchCompleteMessage.fetchSucceeded);
                    }
                }
            }
            
            public void onRemoteValueProviderReady(string messageStr)
            {
                Debug.Log("TTPAnalytics::AnalyticsDelegate::onEngagementProviderReady: " + messageStr);
                if (messageStr != null)
                {
                    OnRemoteValueProviderReadyMessage onEngagementProviderReadyMessage = JsonUtility.FromJson<OnRemoteValueProviderReadyMessage>(messageStr);
                    if (onEngagementProviderReadyMessage != null)
                    {
                        if (OnRemoteValueProviderReadyEvent != null)
                            OnRemoteValueProviderReadyEvent.Invoke(onEngagementProviderReadyMessage.ready);
                    }
                }
            }


            public void onRequestValueComplete(string messageStr)
            {
                Debug.Log("TTPAnalytics::AnalyticsDelegate::OnRequestValueResponse: " + messageStr);
                if (messageStr != null)
                {

                    if (_onRequestValueResponseAction != null)
                    {
                        _onRequestValueResponseAction.Invoke(messageStr);
                        _onRequestValueResponseAction = null;
                    }
                }
            }

            public void onRequestValueDictionaryComplete(string messageDictionaryStr)
            {
                Debug.Log("TTPAnalytics::AnalyticsDelegate::OnRequestValueResponse: " + messageDictionaryStr);
                if (messageDictionaryStr != null)
                {
                    Dictionary<string, object> dictionary = TTPJson.Deserialize(messageDictionaryStr) as Dictionary<string, object>;

                    if (_onRequestValueDictionaryResponseAction != null && dictionary != null)
                    {
                        _onRequestValueDictionaryResponseAction.Invoke(dictionary);
                        _onRequestValueDictionaryResponseAction = null;
                    }
                }
            }

            public void OnDDNALogEventCalled(string message)
            {
                Debug.Log("TTPAnalytics::AnalyticsDelegate::OnDDNALogEventCalled: " + message);
                if (message != null)
                {
                    var dict = TTPJson.Deserialize(message) as Dictionary<string,object>;
                    if (dict != null)
                    {
                        string eventName = null;
                        IDictionary<string, object> eventParams = null;
                        if (dict.ContainsKey("eventName"))
                        {
                            eventName = dict["eventName"] as string;
                        }

                        if (dict.ContainsKey("eventParams"))
                        {
                            eventParams = dict["eventParams"] as Dictionary<string, object>;
                        }

                        if (eventName != null && OnDDNALogEvent != null)
                        {
                            OnDDNALogEvent.Invoke(eventName, eventParams);
                        }
                    }
                }
            }
            
            public void OnGotGeoCode(string message)
            {
                if (message != null)
                {
                    var gotGetCodeMessage = JsonUtility.FromJson<OnGotGetCodeMessage>(message);
                    if (gotGetCodeMessage != null && _getGeoCallback != null)
                    {
                        _getGeoCallback.Invoke(gotGetCodeMessage.geo);
                    }
                }
            }
        }
    }
}
#endif
