#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class DisableBitcodePostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
            if(report.summary.platform == BuildTarget.iOS)
            {
                var projectPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
 
                var pbxProject = new PBXProject();
                pbxProject.ReadFromFile(projectPath);
#if UNITY_2019_3_OR_NEWER
                var targetGuid = pbxProject.GetUnityMainTargetGuid();
#else
        var targetName = PBXProject.GetUnityTargetName();
        var targetGuid = pbxProject.TargetGuidByName(targetName);
#endif    
                pbxProject.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
                pbxProject.WriteToFile (projectPath);
        
                var projectInString = File.ReadAllText(projectPath);
 
                projectInString = projectInString.Replace("ENABLE_BITCODE = YES;",
                    $"ENABLE_BITCODE = NO;");
                File.WriteAllText(projectPath, projectInString);
            }
        }
    }
}

#endif