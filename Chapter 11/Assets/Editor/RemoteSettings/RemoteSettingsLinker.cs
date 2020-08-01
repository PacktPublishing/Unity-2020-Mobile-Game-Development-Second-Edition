#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
    using UnityEditor.Build.Reporting;
#endif
using Object = UnityEngine.Object;


namespace UnityEngine.Analytics
{
    // During a build, collect classes referenced by RemoteSettings components
    // in each scene and in prefabs, and write to a link.xml file to prevent
    // those classes from being stripped.
    public class RemoteSettingsLinker : IPreprocessBuildWithReport, IProcessSceneWithReport
    {
        const string k_LinkPath = "Assets/Editor/RemoteSettings/link.xml";

        string lastActiveScene;
        Dictionary<string, HashSet<string>> types;

        // We need to know if OnProcessScene is being called from a player build, or
        // from entering play mode in the Editor.
        private bool buildingPlayer = false;

        private static bool isIL2CPPBuild()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var backend = PlayerSettings.GetScriptingBackend(buildTargetGroup);

            return backend == ScriptingImplementation.IL2CPP;
        }

        public int callbackOrder { get { return 0; } }
#if UNITY_2018_1_OR_NEWER
        public void OnPreprocessBuild(BuildReport report)
#else
        public void OnPreprocessBuild(BuildTarget target, string path)
#endif
        {
            // OnPreprocessBuild is only called when building a player.
            buildingPlayer = true;

            if (File.Exists(k_LinkPath))
            {
                Console.WriteLine("Deleting Remote Settings link file at " + k_LinkPath);
                try
                {
                    File.Delete(k_LinkPath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Failed to clean up Remote Settings link.xml file due to: " + e);
                }
            }

            if (isIL2CPPBuild())
            {
                var scenes = EditorBuildSettings.scenes;
                foreach (var scene in scenes)
                {
                    if (scene.enabled)
                    {
                        lastActiveScene = scene.path;
                    }
                }

                types = new Dictionary<string, HashSet<string>>();

                // Collected referenced types for all prefabs
                var prefabs = AssetDatabase.FindAssets("t:GameObject");
                foreach (var guid in prefabs)
                {
                    GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(GameObject));
                    if (go)
                    {
                        foreach (var rs in go.GetComponentsInChildren<RemoteSettings>())
                        {
                            DriveableProperty dp = rs.driveableProperty;
                            AddTypesToHash(dp.fields, types);
                        }
                    }
                }
            }
        }

#if UNITY_2018_1_OR_NEWER
        public void OnProcessScene(SceneManagement.Scene scene, BuildReport report)
#else
        public void OnProcessScene(SceneManagement.Scene scene)
#endif
        {
            // In addition to the build process, OnProcessScene is also called on the current scene
            // when entering playmode in the Editor, but without OnPreprocessBuild.
            // Since we only want to generate a link.xml when building with the IL2CPP scripting
            // backend, we need to check for both conditions.
            if (buildingPlayer && isIL2CPPBuild())
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var rs in root.GetComponentsInChildren<RemoteSettings>())
                    {
                        DriveableProperty dp = rs.driveableProperty;
                        AddTypesToHash(dp.fields, types);
                    }
                }

                // OnPostprocessBuild is called after assemblies are already stripped
                // so we need to write the link.xml file immediately after the last scene
                // is processed.
                if (scene.path == lastActiveScene)
                {
                    WriteLinkXml(types);
                    types = null;
                    // I can't tell the lifecycle of this class, so set this to false just in case.
                    buildingPlayer = false;
                }
            }
        }

        static private void AddTypesToHash(List<DriveableProperty.FieldWithRemoteSettingsKey> fields, Dictionary<string, HashSet<string>> types)
        {
            foreach (var remoteSetting in fields)
            {
                object target = remoteSetting.target;
                string fieldPath = remoteSetting.fieldPath;

                if (target == null || String.IsNullOrEmpty(fieldPath))
                {
                    continue;
                }

                object val = target;
                var fieldNames = fieldPath.Split('.');
                for (var i = 0; i < fieldNames.Length; i++)
                {
                    string s = fieldNames[i];
                    var type = val.GetType();
                    var assemblyName = (new AssemblyName(type.Assembly.FullName)).Name;
                    var prop = type.GetProperty(s);
                    var field = type.GetField(s);
                    if (prop != null)
                    {
                        // If property is a List, parse out the index and advance to the next field name
                        object[] parameters = null;
                        var indexParams = prop.GetIndexParameters();
                        if (indexParams.GetLength(0) == 1 && indexParams[0].ParameterType == typeof(int))
                        {
                            int index;
                            if (fieldNames[i + 1] != null && Int32.TryParse(fieldNames[i + 1], out index))
                            {
                                parameters = new object[] { index };
                                i++;
                            }
                        }
                        val = prop.GetValue(val, parameters);
                    }
                    else if (field != null)
                    {
                        val = field.GetValue(val);
                    }

                    if (!types.ContainsKey(assemblyName))
                    {
                        types[assemblyName] = new HashSet<string>();
                    }
                    var typeset = types[assemblyName];
                    typeset.Add(type.FullName);
                }
            }
        }

        static private void WriteLinkXml(Dictionary<string, HashSet<string>> types)
        {
            XmlDocument doc = new XmlDocument();
            var linker = doc.CreateElement("linker");
            doc.AppendChild(linker);
            var comment = doc.CreateComment("\n" +
                    "    This file is autogenerated by the Remote Settings component to prevent \n" +
                    "    the IL2CPP compiler from stripping symbols which may be only referenced\n" +
                    "    through reflection.\n");
            doc.AppendChild(comment);
            foreach (var pair in types)
            {
                var assembly = pair.Key;
                var typeset = pair.Value;

                var assemblyElem = doc.CreateElement("assembly");
                assemblyElem.SetAttribute("fullname", assembly);
                linker.AppendChild(assemblyElem);

                foreach (var type in typeset)
                {
                    // TODO: to optimize build size, we could preserve only the fields by and/or
                    // propery get/set methods needed to set the remote settings.
                    // For fields, set preserve="all"/"fields"/"nothing".
                    // For properties, add <method> elements.
                    var typeElem = doc.CreateElement("type");
                    typeElem.SetAttribute("fullname", type);
                    typeElem.SetAttribute("preserve", "all");
                    assemblyElem.AppendChild(typeElem);
                }
            }

            XmlTextWriter writer = new XmlTextWriter(k_LinkPath, null);
            writer.Formatting = Formatting.Indented;
            doc.Save(writer);
            writer.Close();

            AssetDatabase.SaveAssets();

            Console.WriteLine("Wrote " + k_LinkPath);
        }
    }
}
#endif // UNITY_5_6_OR_NEWER
