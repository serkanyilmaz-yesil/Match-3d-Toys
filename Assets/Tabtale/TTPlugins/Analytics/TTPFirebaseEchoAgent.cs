using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
namespace Tabtale.TTPlugins
{
    class TTPFirebaseEchoAgent : MonoBehaviour
    {
#if UNITY_IOS && !TTP_DEV_MODE
        [DllImport("__Internal")]
        private static extern void ttpSetFirebaseInstanceId(string firebaseInstanceId);
#endif
        
        public static event Action<string, Dictionary<string, object>> FirebaseLogEvent;

        public void CallFirebaseLogEvent(string message)
        {
            Debug.Log("CallFirebaseLogEvent:: " + message);

            if (FirebaseLogEvent == null)
            {
                Debug.LogWarning("CallFirebaseLogEvent:: called but not implemented");
                return;
            }
            if (message == null)
            {
                Debug.LogWarning("CallFirebaseLogEvent:: message null");
                return;
            }
            var dic = TTPJson.Deserialize(message) as Dictionary<string, object>;
            if (dic == null)
            {
                Debug.LogWarning("CallFirebaseLogEvent:: dictionary null");
                return;
            }
            var hasKeysAndCorrectTypes = dic.ContainsKey("name") &&
                                         dic["name"] is string &&
                                         dic.ContainsKey("params") &&
                                         dic["params"] is Dictionary<string, object>;
            if (!hasKeysAndCorrectTypes)
            {
                Debug.LogWarning("CallFirebaseLogEvent:: keys missing");
                return;
            }
            var eventName = dic["name"] as string;
            var eventParams = dic["params"] as Dictionary<string, object>;
            var hasValues = eventName != null && eventParams != null;
            if (!hasValues)
            {
                Debug.LogWarning("CallFirebaseLogEvent:: incorrect values");
                return;
            }
            List<string> nullKeys = new List<string>();
            
            foreach (KeyValuePair<string,object> kvp in eventParams)
            {
                if (kvp.Value == null)
                {
                    nullKeys.Add(kvp.Key);
                }
            }
            foreach (string key in nullKeys)
            {
                eventParams[key] = "NULL";
            }
            FirebaseLogEvent.Invoke(eventName,eventParams);
        }
        
        [Preserve]
        public static void SetInstanceIdFirebase(string firebaseInstanceId)
        {
            Debug.Log("TTPFirebaseEchoAgent::SetInstanceIdFirebase: " + firebaseInstanceId);
#if UNITY_IOS && !TTP_DEV_MODE
            ttpSetFirebaseInstanceId(firebaseInstanceId);
#elif UNITY_ANDROID
            AndroidJavaObject serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject("getAnalytics");
            if (serivceJavaObject == null)
            {
                Debug.LogError("TTPAnalytics::SetInstanceIdFirebase: failed to get analytics native instance.");
                return;
            }
            serivceJavaObject.Call("setFirebaseInstanceId", firebaseInstanceId);
#endif
        }
    }
}


