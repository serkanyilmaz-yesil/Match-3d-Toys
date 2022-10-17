using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;
using System;

namespace Tabtale.TTPlugins
{
    public class JsonUtilityWrapper
    {
        public static string ToJson(object obj)
        {
            string output = "";
            try
            {
                output = JsonUtility.ToJson(obj);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return output;
        }

        public static string ToJson(object obj, bool prettyPrint)
        {
            string output = "";
            try
            {
                output = JsonUtility.ToJson(obj, prettyPrint);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return output;
        }

        public static T FromJson<T>(string json)
        {
            T output = default(T);
            try
            {
                output = JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return output;
        }

        public static object FromJson(string json, Type type)
        {
            object output = null;
            try
            {
                output = JsonUtility.FromJson(json, type);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return output;
        }

        public static void FromJsonOverwrite(string json, object objectToOverwrite)
        {
            try
            {
                JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
