#if TTP_APPSFLYER
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
	/// <summary>
    /// This class provides logging to AppsFlyer service
    /// </summary>
	public class TTPAppsFlyer
	{
		private static IAppsFlyer _impl;
		private static IAppsFlyer Impl
		{
			get
			{
				if (_impl == null)
				{
					if (TTPCore.IncludedServices != null && !TTPCore.IncludedServices.appsFlyer)
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
					Debug.LogError("TTPAppsFlyer::Impl: failed to create native impl");
				}
				return _impl;
			}
		}

		/// <summary>
        /// Log events to AppsFlyer
        /// </summary>
        /// <param name="eventName">Name of event</param>
        /// <param name="eventParams">Parameters dictionary logging with event</param>
		public static void LogEvent(string eventName, IDictionary<string, object> eventParams)
		{
			if (Impl != null)
				Impl.LogEvent(eventName, eventParams);
		}

		/// <summary>
		/// Private interface for methods
		/// </summary>
		private interface IAppsFlyer
		{
			void LogEvent (string eventName, IDictionary<string, object> eventParams);
		}



#if UNITY_IOS && !TTP_DEV_MODE

		private class IosImpl : IAppsFlyer
		{
			[DllImport("__Internal")]
			private static extern void ttpAppsFlyerLogEvent(string eventName, string eventParamsJson);

			public void LogEvent(string eventName, IDictionary<string, object> eventParams)
			{
				ttpAppsFlyerLogEvent(eventName, eventParams != null ? TTPJson.Serialize(eventParams) : "{}");
			}
		}

#endif



#if UNITY_ANDROID
		private class AndroidImpl : IAppsFlyer
		{
			private const string SERVICE_GET_METHOD = "getAppsFlyer";

			private AndroidJavaObject _serivceJavaObject;

			private AndroidJavaObject ServiceJavaObject
			{
				get
				{
					if (_serivceJavaObject == null)
					{
						_serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject(SERVICE_GET_METHOD);
					}
					if (_serivceJavaObject == null) {
						Debug.LogError ("TTPAppsFlyer::AndroidImpl: failed to get AppsFlyer Tool native instance.");
					}
					return _serivceJavaObject;
				}
			}


			// methods for the native 

			public void LogEvent(string eventName, IDictionary<string, object> eventParams)
			{
				if (ServiceJavaObject != null)
				{
					ServiceJavaObject.Call("logEvent", eventName, eventParams != null ? TTPJson.Serialize(eventParams) : "{}");
				}
			}
		}

#endif


        //#if UNITY_EDITOR

        private class EditorImpl : IAppsFlyer
		{
			public void LogEvent (string eventName, IDictionary<string, object> eventParams)
			{
				string paramsStr = "";
				if(eventParams != null && eventParams.Count > 0)
				{
					paramsStr = TTPJson.Serialize(eventParams);
				}
				Debug.Log("TTPAppsFlyer::EditorImpl::LogEvent: eventName: " + eventName + "\neventParams: " + paramsStr);
			}
		}
        //#endif

	}
}
#endif