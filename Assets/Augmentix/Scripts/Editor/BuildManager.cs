using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.CodeDom;
using System.Threading;
using Photon.Realtime;
using UnityEditor.PackageManager;
using UnityEngine.UI;
using UnityEngine.VR;

namespace Augmentix.Scripts.Editor
{
    public class BuildManager : EditorWindow
    {
        [SerializeField] private List<SceneAsset> _scenes;
        [SerializeField] private List<string> _scenePaths;
        [SerializeField] private string _buildPath = "";
        [SerializeField] private List<string> _packages;
        [SerializeField] private bool _usesVR = false;
        [SerializeField] private bool _run = false;
        [SerializeField] private bool _open = false;
        [SerializeField] private bool _scriptDebugging = false;

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            _buildPath = EditorGUILayout.TextField("Build Path", _buildPath);
            if (GUILayout.Button("..."))
            {
                _buildPath = EditorUtility.SaveFolderPanel("Choose Path", "", "");
            }

            EditorGUILayout.EndHorizontal();

            ScriptableObject scriptableObj = this;
            SerializedObject serialObj = new SerializedObject(scriptableObj);

            SerializedProperty serialProp = serialObj.FindProperty("_scenes");
            EditorGUILayout.PropertyField(serialProp, true);
            serialObj.ApplyModifiedProperties();

            serialProp = serialObj.FindProperty("_packages");
            EditorGUILayout.PropertyField(serialProp, true);
            serialObj.ApplyModifiedProperties();
            
            _usesVR = EditorGUILayout.Toggle("VR", _usesVR);
            
            _scriptDebugging = EditorGUILayout.Toggle("Script Debugging", _scriptDebugging);

            _open = EditorGUILayout.Toggle("Open Folder after Build", _open);

            _run = EditorGUILayout.Toggle("Do Run after Build", _run);
        }

        protected void OnEnable()
        {
            // Here we retrieve the data if it exists or we save the default field initialisers we set above
            var data = EditorPrefs.GetString(GetType().Name, JsonUtility.ToJson(this, false));
            // Then we apply them to this window
            JsonUtility.FromJsonOverwrite(data, this);
            _scenes = _scenePaths.Select(s => (SceneAsset) AssetDatabase.LoadAssetAtPath(s, typeof(SceneAsset)))
                .ToList();
        }

        protected void OnDisable()
        {
            // We get the Json data
            _scenePaths = _scenes.Select(asset => AssetDatabase.GetAssetPath(asset)).ToList();

            var data = JsonUtility.ToJson(this, false);
            // And we save it
            EditorPrefs.SetString(GetType().Name, data);
        }


        public static void DoBuild(Type type, BuildTarget target)
        {
            if (target == BuildTarget.WSAPlayer)
            {
                Debug.LogError("Please use MRTK Build manager");
                return;
            }
            
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                Debug.LogError("Please Switch to Target before build!");
                return;
            }
            
            BuildManager manager = GetManager();
            BuildManager newManager = GetManager(type.Name);

            string[] levels = newManager._scenePaths.ToArray();

            BuildOptions options = BuildOptions.None;

            if (newManager._scriptDebugging)
                options |= BuildOptions.Development | BuildOptions.AllowDebugging;

            if (newManager._run)
                options |= BuildOptions.AutoRunPlayer;

            if (newManager._open)
                options |= BuildOptions.ShowBuiltPlayer;

            string filename = "";

            if (type == typeof(VRBuildManager))
            {
                filename = Application.productName + ".apk";
            }
            else if (type == typeof(ARBuildManager))
            {
                filename = Application.productName + ".exe";
            }

            BuildPipeline.BuildPlayer(levels, Path.Combine(newManager._buildPath, filename), target, options);

        }

        public static void DoSwitch(Type type, BuildTarget target)
        {
            
            /*
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
 
            SerializedObject obj = new SerializedObject(inputManager);
 
            SerializedProperty axisArray = obj.FindProperty("m_Axes");
 
            if (axisArray.arraySize == 0)
                Debug.Log("No Axes");

            var output = "new string[] {";
            
            for( int i = 0; i < axisArray.arraySize; ++i )
            {
                var axis = axisArray.GetArrayElementAtIndex(i);
 
                var name = axis.FindPropertyRelative("m_Name").stringValue;

                output += "\""+name + "\",";
            }

            output += "}";
            
            Debug.Log(output);
            
            return;
            */
            
            
            if (EditorUserBuildSettings.activeBuildTarget == target)
                return;

            BuildManager manager = GetManager();
            BuildManager newManager = GetManager(type.Name);

            switch (target)
            {
                case BuildTarget.Android:
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, target);
                    break;
                }
                case BuildTarget.StandaloneWindows64:
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, target);
                    break;
                }
                case BuildTarget.WSAPlayer:
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, target);
                    break;
                }
            }

            var searchRequest = Client.List();
            
            while(!searchRequest.IsCompleted)
                Thread.Sleep(10);

            foreach (var package in manager._packages)
            {
                if (!newManager._packages.Contains(package) && searchRequest.Result.Select(info => info.name).Contains(package))
                {
                    Client.Remove(package);
                }
            }
            
            foreach (var package in newManager._packages)
            {
                if (!searchRequest.Result.Select(info => info.name).Contains(package))
                    Client.Add(package);
            }/*
            if (searchRequest.Result.Select(info => info.name).Contains("com.ptc.vuforia.engine"))
            {
                if (!newManager._usesVuforia)
                {
                    Client.Remove("com.ptc.vuforia.engine");
                }
            }
            else
            {
                if (newManager._usesVuforia)
                {
                    Client.Add("com.ptc.vuforia.engine");
                }
            }
            */

            if (newManager._usesVR)
            {
                XRSettings.LoadDeviceByName("newDevice");
                Thread.Sleep(10);
                XRSettings.enabled = true;
            }
            else
            {
                XRSettings.LoadDeviceByName("");
                Thread.Sleep(10);
                XRSettings.enabled = false;
            }

            /*
            if (newManager._usesVR)
            {
                if (type == typeof(ARBuildManager))
                {
                    if (searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.oculus"))
                        Client.Remove("com.unity.xr.oculus");
                    if (!searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.windowsmr"))
                        Client.Add("com.unity.xr.windowsmr");
                    if (!searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.windowsmr"))
                        Client.Add("com.unity.xr.windowsmr.metro");
                } else if (type == typeof(VRBuildManager))
                {
                    if (!searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.oculus"))
                        Client.Add("com.unity.xr.oculus");
                    if (searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.windowsmr"))
                        Client.Remove("com.unity.xr.windowsmr");
                    if (searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.windowsmr"))
                        Client.Remove("com.unity.xr.windowsmr.metro");
                }
            }
            else
            {
                if (searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.oculus"))
                    Client.Remove("com.unity.xr.oculus");
                if (searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.windowsmr"))
                    Client.Remove("com.unity.xr.windowsmr");
                if (searchRequest.Result.Select(info => info.name).Contains("com.unity.xr.windowsmr.metro"))
                    Client.Remove("com.unity.xr.windowsmr.metro");
            }
            */

            EditorBuildSettings.scenes =
                newManager._scenePaths.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();

            if (newManager._scenePaths.Count > 0)
                EditorSceneManager.OpenScene(newManager._scenePaths[0]);
            else
                Debug.Log("No Scenes defined. See BuildManager->Setup");
        }


        public static BuildManager GetManager()
        {
            string type = "";

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.WSAPlayer:
                {
                    type = "ARBuildManager";
                    break;
                }

                case BuildTarget.Android:
                {
                    type = "VRBuildManager";
                    break;
                }
                
                case BuildTarget.StandaloneWindows64:
                {
                    type = "LeapMotionManager";
                    break;
                }
            }

            return GetManager(type);
        }

        public static BuildManager GetManager(string type)
        {
            BuildManager manager = new BuildManager();
            // Here we retrieve the data if it exists or we save the default field initialisers we set above
            var data = EditorPrefs.GetString(type, JsonUtility.ToJson(manager, false));
            // Then we apply them to this window
            JsonUtility.FromJsonOverwrite(data, manager);
            return manager;
        }

        public class VRBuildManager : BuildManager
        {
            [MenuItem("BuildManager/Build/VR")]
            public static void Build()
            {
                DoBuild(typeof(VRBuildManager), BuildTarget.Android);
            }

            [MenuItem("BuildManager/Switch/VR")]
            public static void Switch()
            {
                DoSwitch(typeof(VRBuildManager), BuildTarget.Android);
            }

            [MenuItem("BuildManager/Setup/VR")]
            public static void Setup()
            {
                GetWindow(typeof(VRBuildManager)).Show();
            }
        }

        public class ARBuildManager : BuildManager
        {
            [MenuItem("BuildManager/Build/AR")]
            public static void Build()
            {
                DoBuild(typeof(ARBuildManager), BuildTarget.WSAPlayer);
            }

            [MenuItem("BuildManager/Switch/AR")]
            public static void Switch()
            {
                DoSwitch(typeof(ARBuildManager), BuildTarget.WSAPlayer);
            }

            [MenuItem("BuildManager/Setup/AR")]
            public static void Setup()
            {
                GetWindow(typeof(ARBuildManager)).Show();
            }
        }
    }
}
#endif