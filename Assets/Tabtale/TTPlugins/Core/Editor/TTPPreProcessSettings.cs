using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPPreProcessSettings : IPreprocessBuild
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            Debug.Log("TTPPreProcessSettings::OnPreprocessBuild for target " + target + " at path " + path);
            CheckConfig(target);
        }

        private void CheckConfig(UnityEditor.BuildTarget platform)
        {
            Debug.Log("TTPPreProcessSettings::CheckConfig: ");
            string configurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("global");
            if (!string.IsNullOrEmpty(configurationJson))
            {
                Debug.Log("TTPPreProcessSettings::CheckConfig: configurationJson=" + configurationJson);
                Dictionary<string, object> configuration = TTPJson.Deserialize(configurationJson) as Dictionary<string, object>;
                object storeObj;
                if (configuration.TryGetValue("store", out storeObj))
                {
                    string store = (string)storeObj;
                    Debug.Log("TTPPreProcessSettings::CheckConfig: store=" + store);
                    if (platform == UnityEditor.BuildTarget.iOS && !store.Equals("apple"))
                    {
                        Debug.LogError("Store in global.json does not match current platform:store=" + store + " platform=iOS");
                    }
                    else if (platform == UnityEditor.BuildTarget.Android && !store.Equals("google"))
                    {
                        Debug.LogError("Store in global.json does not match current platform:store=" + store + " platform=Android");
                    }
                }
            }
        }
    }
}