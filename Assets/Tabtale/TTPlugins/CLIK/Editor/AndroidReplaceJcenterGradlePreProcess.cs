using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class AndroidReplaceJcenterGradlePreProcess : IPostGenerateGradleAndroidProject
    {

        public int callbackOrder => 1;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            Debug.Log("AndroidReplaceJcenterGradlePreProcess: Start change jcenter() to mavenCentral() in build.gradle files");
            var buildGradleFilePaths = Directory.GetFiles(path + "/..", "build.gradle",SearchOption.AllDirectories);
            foreach(var buildGradleFilePath in buildGradleFilePaths)
            {
                Debug.Log("AndroidReplaceJcenterGradlePreProcessl: Change jcenter() to mavenCentral() in: " + buildGradleFilePath);
                var buildGradleContent = File.ReadAllText(buildGradleFilePath);
                buildGradleContent = buildGradleContent.Replace("jcenter()", "mavenCentral()");
                File.WriteAllText(buildGradleFilePath, buildGradleContent);
            }
            Debug.Log("AndroidReplaceJcenterGradlePreProcess: End change jcenter() to mavenCentral() in build.gradle files");
        }
    
    }
}


