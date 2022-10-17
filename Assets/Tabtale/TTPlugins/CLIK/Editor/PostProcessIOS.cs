using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostProcessIOS : MonoBehaviour
{
    [PostProcessBuild(45)] //must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
    private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
    {
#if UNITY_2019_3_OR_NEWER
        var ext = File.ReadAllText("Assets/Tabtale/TTPlugins/CLIK/Editor/PodfileExtension.txt");
        if (target == BuildTarget.iOS)
        {

            using (StreamWriter sw = File.AppendText(buildPath + "/Podfile"))
            {
                if (ext != "")
                {
                    sw.WriteLine(ext);    
                }
                else
                {
                    sw.WriteLine("# File Assets/Editor/Podfile.ext is empty");
                }
            }
        }
#endif
    }
}