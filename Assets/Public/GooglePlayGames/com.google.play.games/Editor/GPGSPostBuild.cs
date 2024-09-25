// <copyright file="GPGSPostBuild.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

#if UNITY_ANDROID
namespace GooglePlayGames.Editor
{
    using System.IO;
    using System.Xml;
    using System.Linq;
    using System.Collections.Generic;

    using UnityEditor;
    using UnityEditor.Android;
    using UnityEditor.Callbacks;

    public class GPGSPostBuild : IPostGenerateGradleAndroidProject
    {
        [PostProcessBuild(99999)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (!GPGSProjectSettings.Instance.GetBool(GPGSUtil.ANDROIDSETUPDONEKEY, false))
            {
                EditorUtility.DisplayDialog("Google Play Games not configured!",
                    "Warning!!  Google Play Games was not configured, Game Services will not work correctly.",
                    "OK");
            }

            return;
        }

        public int callbackOrder => 999;

        const string androidNamespaceURL = "http://schemas.android.com/apk/res/android";
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path,"src","main","AndroidManifest.xml");
            if(!File.Exists(manifestPath))
            {
                EditorUtility.DisplayDialog("Google Play Games Error","Cannot find AndroidManifest.xml to modified","OK");
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(manifestPath);

            var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("android",androidNamespaceURL);

            var appID = FindOrCreate(xmlDoc,nsmgr,androidNamespaceURL,"manifest/application/meta-data","android:name","com.google.android.gms.games.APP_ID");
            SetAttributeNS(xmlDoc,appID,androidNamespaceURL,"android:value","\\u003" + GPGSProjectSettings.Instance.Get(GPGSUtil.APPIDKEY));

            var webClientID = FindOrCreate(xmlDoc,nsmgr,androidNamespaceURL,"manifest/application/meta-data","android:name","com.google.android.gms.games.WEB_CLIENT_ID");
            SetAttributeNS(xmlDoc,webClientID,androidNamespaceURL,"android:value",GPGSProjectSettings.Instance.Get(GPGSUtil.WEBCLIENTIDKEY));

            var version = FindOrCreate(xmlDoc,nsmgr,androidNamespaceURL,"manifest/application/meta-data","android:name","com.google.android.gms.games.unityVersion");
            SetAttributeNS(xmlDoc,version,androidNamespaceURL,"android:value","\\u003" + PluginVersion.VersionString);

            string serviceID = GPGSProjectSettings.Instance.Get(GPGSUtil.SERVICEIDKEY);
            if (!string.IsNullOrEmpty(serviceID))
            {
                foreach(var permission in new[]{ "BLUETOOTH","BLUETOOTH_ADMIN","ACCESS_WIFI_STATE","CHANGE_WIFI_STATE","ACCESS_COARSE_LOCATION" })
                    FindOrCreate(xmlDoc,nsmgr,androidNamespaceURL,"manifest/uses-permission","android:name","android.permission." + permission);

                var service = FindOrCreate(xmlDoc,nsmgr,androidNamespaceURL,"manifest/application/meta-data","android:name","com.google.android.gms.nearby.connection.SERVICE_ID");
                SetAttributeNS(xmlDoc,service,androidNamespaceURL,"android:value",serviceID);
            }

            xmlDoc.Save(manifestPath);
        }

        static void SetAttributeNS(XmlDocument xmlDoc,XmlElement element,string namespaceURL,string attributeName,string attributeValue)
        {
            var attr = xmlDoc.CreateAttribute(attributeName,namespaceURL);
            attr.Value = attributeValue;
            element.SetAttributeNode(attr);
        }

        static XmlElement FindOrCreate(XmlDocument xmlDoc,XmlNamespaceManager nsmgr,string attributeNamespace,string path,string attributeName,string attributeValue)
        {
            var nodes = xmlDoc.SelectNodes($"{path}[@{attributeName}='{attributeValue}']",nsmgr);
            if(nodes.Count > 0)
            {
                int i = 0;
                while(i < nodes.Count)
                {
                    if(nodes[i] is XmlElement element)
                        break;

                    i++;
                }

                foreach(var node in nodes.OfType<XmlNode>().Where((node,n) => i != n))
                    node.ParentNode.RemoveChild(node);

                return nodes[i] as XmlElement;
            }
            else
            {
                var element = xmlDoc.DocumentElement;
                var stack = new Stack<string>();
                while(path.LastIndexOf('/') is int i && i > 0)
                {
                    stack.Push(path.Substring(i + 1));
                    path = path.Remove(i);
                    element = xmlDoc.SelectNodes(path,nsmgr)?.OfType<XmlElement>().FirstOrDefault();
                    if(element != null)
                        break;
                }

                while(stack.TryPop(out string name))
                {
                    element = element.AppendChild(xmlDoc.CreateElement(name)) as XmlElement;
                }

                SetAttributeNS(xmlDoc,element,attributeNamespace,attributeName,attributeValue);

                return element;
            }
        }
    }
}
#endif //UNITY_ANDROID