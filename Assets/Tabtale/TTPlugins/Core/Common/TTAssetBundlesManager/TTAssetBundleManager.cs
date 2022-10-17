using System;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Threading;


namespace TTPluginsAssetBundles
{
	public class TTAssetBundleManager : MonoBehaviour
	{

		private bool m_AssetBundleManagerInitialized = false;
		private bool m_AssetBundleManagerInitializationFinished = false;

		private const string m_TTAssetBundlePrefix = "TTAssetBundle_";
		static TTAssetBundleManager m_Instance = null;

		// Use this for initialization
		void Start () {
			
		}

		static void Initialize()
		{
			if (!m_Instance) {
				GameObject go = new GameObject ("TTAssetBundleManager");
				m_Instance = go.AddComponent<TTAssetBundleManager> ();
				DontDestroyOnLoad(go);
			}

			if (!m_Instance.m_AssetBundleManagerInitialized) {
				m_Instance.StartCoroutine (m_Instance.InitializeCoro ());
			}
		}

		protected IEnumerator InitializeCoro()
		{
			if (!m_Instance) {
				GameObject go = new GameObject ("TTAssetBundleManager");
				m_Instance = go.AddComponent<TTAssetBundleManager> ();
				DontDestroyOnLoad(go);
			}

			if (m_Instance.m_AssetBundleManagerInitialized) {
				while (!m_Instance.m_AssetBundleManagerInitializationFinished) {
					yield return new WaitForEndOfFrame ();
				}
				yield break;
			}

			m_Instance.m_AssetBundleManagerInitialized = true;

			// Don't destroy the game object as we base on it to run the loading script.

			m_Instance.InitializeSourceURL();

			// Initialize AssetBundleManifest which loads the AssetBundleManifest object.
			var request = AssetBundleManager.Initialize();

			if (request != null) {
				yield return m_Instance.StartCoroutine (request);
			}

			m_Instance.m_AssetBundleManagerInitializationFinished = true;
		}

		// Initialize the downloading URL.
		// eg. Development server / iOS ODR / web URL
		protected void InitializeSourceURL()
		{
			#if UNITY_EDITOR
			// With this code, when in-editor always use the AssetBundle local directory
			AssetBundleManager.SetSourceAssetBundleURL("file://" + Path.Combine(Path.Combine(Application.dataPath, ".."), "AssetBundles"));
			return;
			#endif

			// If ODR is available and enabled, then use it and let Xcode handle download requests.
			#if ENABLE_IOS_ON_DEMAND_RESOURCES
			if (UnityEngine.iOS.OnDemandResources.enabled) {
				AssetBundleManager.SetSourceAssetBundleURL("odr://");
			}
			#endif
		}

		public void UnloadAssetBundle(string assetBundleName) {
			AssetBundleManager.UnloadAssetBundle (assetBundleName);
		}

		public static void GetAssetBundleAsync(string assetBundleName, Action<AssetBundle, string> callback, int timeoutInSeconds = 15) {
			if (!m_Instance) {
				Initialize();
			}
			m_Instance.StartCoroutine (m_Instance.GetAssetBundle(assetBundleName, callback, timeoutInSeconds));
		}

		private IEnumerator GetAssetBundle(string assetBundleName, Action<AssetBundle, string> callback, int timeoutInSeconds) {
			while (!m_Instance.m_AssetBundleManagerInitializationFinished) {
				yield return new WaitForEndOfFrame ();
			}
			AssetBundleManager.LoadAssetBundle (assetBundleName);

			float realtimeSinceStartup = Time.realtimeSinceStartup;
			string error = null;
			LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle (assetBundleName, out error);
			while (loadedAssetBundle == null && error == null) {
				yield return new WaitForEndOfFrame ();
				loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle (assetBundleName, out error);
				if (Time.realtimeSinceStartup - realtimeSinceStartup > timeoutInSeconds) {
					error = "Timeout";
				}
			}

			AssetBundle assetBundle = null;
			if (loadedAssetBundle != null) {
				assetBundle = loadedAssetBundle.m_AssetBundle;
				PlayerPrefs.SetInt (m_TTAssetBundlePrefix + assetBundleName, 1);
			}

			callback (assetBundle, error);
		}

		public static void PeekAssetBundleAsync(string assetBundleName, Action<bool> callback) {
			if (!m_Instance) {
				Initialize();
			}
			m_Instance.StartCoroutine (m_Instance.PeekAssetBundle(assetBundleName, callback));
		}

		private IEnumerator PeekAssetBundle(string assetBundleName, Action<bool> callback) {
			while (!m_Instance.m_AssetBundleManagerInitializationFinished) {
				yield return new WaitForEndOfFrame ();
			}

			bool exists = AssetBundleManager.AssetBundleManifestObject.GetAllAssetBundles ().Contains (assetBundleName);
			callback (exists);
		}

		public static bool IsAssetBundleAlreadyExists(string assetBundleName) {
			if (PlayerPrefs.HasKey(m_TTAssetBundlePrefix + assetBundleName)) {
				return true;
			}

			return false;
		}
	}
}

