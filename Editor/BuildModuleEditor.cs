#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace THEBADDEST.BuildModuleSystem
{


	[CustomEditor(typeof(BuildModule))]
	public class BuildModuleEditor : Editor
	{

		BuildModule module;

		[MenuItem("Tools/THEBADDEST/Build Module %&b")]
		public static void OpenBuildModule()
		{
			string[] guids = AssetDatabase.FindAssets($"t:{nameof(BuildModule)}");
			if (guids.Length > 0)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[0]);
				var buildModule = AssetDatabase.LoadAssetAtPath<BuildModule>(path);
				if (buildModule != null)
					Selection.activeObject = buildModule;
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			if (module == null)
				module = (BuildModule) target;

			EditorGUILayout.Space();
			GUILayout.Label("BUILD MODULE", new GUIStyle { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
			EditorGUILayout.Space();
			GUILayout.Label("Developed by Umair Saifullah", new GUIStyle { alignment = TextAnchor.LowerRight, fontStyle = FontStyle.Italic });
			EditorGUILayout.Space();

			EditorGUI.BeginChangeCheck();
			BuildModule.Platform platform = (BuildModule.Platform) EditorGUILayout.EnumPopup("Platform", module.selectedPlatform);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(module, "Change Platform");
				module.selectedPlatform = platform;
				BuildPrefs.LastSelectedPlatform = platform;
			}

			// Persist selection to prefs when changed via inspector
			if (module.selectedPlatform != BuildPrefs.LastSelectedPlatform)
				BuildPrefs.LastSelectedPlatform = module.selectedPlatform;

			EditorGUILayout.Space();

			if (module.selectedPlatform == BuildModule.Platform.iOS)
				DrawIOSModule(module.iosModule);
			else
				DrawAndroidModule(module.androidModule);

			serializedObject.ApplyModifiedProperties();
		}

		void DrawIOSModule(iOSModuleData data)
		{
			SerializedObject so = serializedObject;
			SerializedProperty iosProp = so.FindProperty("iosModule");
			DrawSharedFields(iosProp);
			EditorGUILayout.Space();
			DrawBuildPath(iosProp.FindPropertyRelative("buildPath"), data.buildPath, data.GameName, "iOS", () => BuildPrefs.LastBuildPathIOS, v => BuildPrefs.LastBuildPathIOS = v);
			EditorGUILayout.PropertyField(iosProp.FindPropertyRelative("customOutputFileName"), new GUIContent("Custom Output File Name"));
			EditorGUILayout.Space();
			DrawActionButtons(isAndroid: false);
		}

		void DrawAndroidModule(AndroidModuleData data)
		{
			SerializedObject so = serializedObject;
			SerializedProperty androidProp = so.FindProperty("androidModule");
			DrawSharedFields(androidProp);
			EditorGUILayout.Space();
			DrawBuildPath(androidProp.FindPropertyRelative("buildPath"), data.buildPath, data.GameName, "Android", () => BuildPrefs.LastBuildPathAndroid, v => BuildPrefs.LastBuildPathAndroid = v);
			EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("customOutputFileName"), new GUIContent("Custom Output File Name"));
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Keystore", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("signed"));
			if (data.signed)
			{
				DrawKeystorePath(androidProp.FindPropertyRelative("keystoreName"));
				EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("keystorePass"));
				EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("keyaliasName"));
				EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("keyaliasPass"));
			}
			EditorGUILayout.Space();
			DrawActionButtons(isAndroid: true);
		}

		void DrawSharedFields(SerializedProperty moduleProp)
		{
			moduleProp.FindPropertyRelative("GameName").stringValue = EditorGUILayout.TextField("Game Name", moduleProp.FindPropertyRelative("GameName").stringValue);
			moduleProp.FindPropertyRelative("ComapanyName").stringValue = EditorGUILayout.TextField("Company Name", moduleProp.FindPropertyRelative("ComapanyName").stringValue);
			moduleProp.FindPropertyRelative("BundleId").stringValue = EditorGUILayout.TextField("Bundle ID", moduleProp.FindPropertyRelative("BundleId").stringValue);
			moduleProp.FindPropertyRelative("BuildVersion").stringValue = EditorGUILayout.TextField("Build Version", moduleProp.FindPropertyRelative("BuildVersion").stringValue);
			moduleProp.FindPropertyRelative("BuildNumber").intValue = EditorGUILayout.IntField("Build Number", moduleProp.FindPropertyRelative("BuildNumber").intValue);
		}

		void DrawBuildPath(SerializedProperty buildPathProp, string currentPath, string gameName, string platformName, System.Func<string> getLastPath, System.Action<string> setLastPath)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(buildPathProp, new GUIContent("Build Path"));
			if (GUILayout.Button("Browse", GUILayout.MaxWidth(60)))
			{
				string path = EditorUtility.OpenFolderPanel("Build Path", currentPath ?? "", "");
				if (!string.IsNullOrEmpty(path))
				{
					buildPathProp.stringValue = path + "/";
					setLastPath(path + "/");
				}
			}
			if (GUILayout.Button("Reset", GUILayout.MaxWidth(50)))
			{
				string defaultPath = Path.Combine(Application.dataPath, "..", "Builds", platformName, gameName ?? "Game").Replace("\\", "/") + "/";
				buildPathProp.stringValue = defaultPath;
				setLastPath(defaultPath);
			}
			EditorGUILayout.EndHorizontal();
		}

		void DrawKeystorePath(SerializedProperty keystoreNameProp)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(keystoreNameProp, new GUIContent("Keystore"));
			if (GUILayout.Button("Browse", GUILayout.MaxWidth(60)))
			{
				string path = EditorUtility.OpenFilePanel("Keystore", "", "keystore");
				if (!string.IsNullOrEmpty(path))
				{
					keystoreNameProp.stringValue = path;
					serializedObject.ApplyModifiedProperties();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		void DrawActionButtons(bool isAndroid)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Sync", GUILayout.Height(25)))
			{
				if (isAndroid)
					BuildModuleRunner.SyncAndroid(module.androidModule);
				else
					BuildModuleRunner.SyncIOS(module.iosModule);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Debug Build", GUILayout.Height(25)))
			{
				if (isAndroid)
					BuildModuleRunner.BuildAndroid(module.androidModule, isDebug: true, buildAAB: false);
				else
					BuildModuleRunner.BuildIOS(module.iosModule, isDebug: true);
			}
			if (GUILayout.Button("Release Build", GUILayout.Height(25)))
			{
				if (isAndroid)
					BuildModuleRunner.BuildAndroidRelease(module.androidModule);
				else
					BuildModuleRunner.BuildIOS(module.iosModule, isDebug: false);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Locate", GUILayout.MaxWidth(75), GUILayout.Height(25)))
			{
				string path = isAndroid ? module.androidModule.buildPath : module.iosModule.buildPath;
				if (string.IsNullOrEmpty(path)) return;
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				EditorUtility.RevealInFinder(path);
			}
			EditorGUILayout.EndHorizontal();
		}

	}


	public static class BuildModuleRunner
	{

		public static void SyncIOS(iOSModuleData data)
		{
			PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, data.BundleId);
			PlayerSettings.companyName   = data.ComapanyName;
			PlayerSettings.productName   = data.GameName;
			PlayerSettings.bundleVersion = data.BuildVersion;
			PlayerSettings.iOS.buildNumber = data.BuildNumber.ToString();
		}

		public static void SyncAndroid(AndroidModuleData data)
		{
			PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, data.BundleId);
			PlayerSettings.companyName                 = data.ComapanyName;
			PlayerSettings.productName                 = data.GameName;
			PlayerSettings.bundleVersion               = data.BuildVersion;
			PlayerSettings.Android.bundleVersionCode   = data.BuildNumber;
		}

		static string GetAndroidFolderPath(AndroidModuleData data)
		{
			string path = data.buildPath ?? "";
			if (!path.EndsWith("/")) path += "/";
			return path + $"{data.GameName}{data.BuildVersion}V{data.BuildNumber}Android/";
		}

		static string GetAndroidOutputName(AndroidModuleData data, bool isDebug, bool isAAB)
		{
			string custom = !string.IsNullOrEmpty(data.customOutputFileName) ? data.customOutputFileName : BuildPrefs.CustomFileNameAndroid;
			if (!string.IsNullOrEmpty(custom))
				return custom;
			var backend   = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
			string arch   = PlayerSettings.Android.targetArchitectures.ToString().Replace(",", "");
			string suffix = isDebug ? "Dev" : "Prod";
			string ext    = isAAB ? "aab" : "apk";
			return $"{data.GameName}v{data.BuildVersion}V{data.BuildNumber}{backend}{arch}{suffix}.{ext}";
		}

		static string GetAndroidFullPath(AndroidModuleData data, bool isDebug, bool isAAB)
		{
			string folder = GetAndroidFolderPath(data);
			string name   = GetAndroidOutputName(data, isDebug, isAAB);
			return folder + name;
		}

		static void EnsureDirectory(string path)
		{
			string dir = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);
		}

		public static void BuildAndroid(AndroidModuleData data, bool isDebug, bool buildAAB)
		{
			SyncAndroid(data);
			EditorSceneManager.SaveOpenScenes();

			string fullPath = GetAndroidFullPath(data, isDebug, buildAAB);
			EnsureDirectory(fullPath);

			EditorUserBuildSettings.buildAppBundle               = buildAAB;
			EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
			EditorUserBuildSettings.installInBuildFolder         = true;
			EditorUserBuildSettings.SetBuildLocation(BuildTarget.Android, data.buildPath);

			BuildPlayerOptions opts = new BuildPlayerOptions
			{
				scenes            = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
				locationPathName  = fullPath,
				target            = BuildTarget.Android,
				options           = (isDebug ? BuildOptions.Development : BuildOptions.CompressWithLz4HC) | BuildOptions.ShowBuiltPlayer
			};

			PlayerSettings.Android.useCustomKeystore = data.signed;
			if (data.signed)
			{
				PlayerSettings.Android.keystoreName = data.keystoreName;
				PlayerSettings.Android.keystorePass = data.keystorePass;
				PlayerSettings.Android.keyaliasName = data.keyaliasName;
				PlayerSettings.Android.keyaliasPass = data.keyaliasPass;
			}

			BuildReport report  = BuildPipeline.BuildPlayer(opts);
			BuildSummary summary = report.summary;
			if (summary.result == BuildResult.Succeeded)
				Debug.Log($"Build succeeded: {summary.totalSize} bytes -> {fullPath}");
			else
				Debug.LogError("Build failed.");
		}

		public static void BuildAndroidRelease(AndroidModuleData data)
		{
			BuildAndroid(data, isDebug: false, buildAAB: false);
			BuildAndroid(data, isDebug: false, buildAAB: true);
		}

		static string GetIOSOutputPath(iOSModuleData data, bool isDebug)
		{
			string path = data.buildPath ?? "";
			if (!path.EndsWith("/")) path += "/";
			string folder = $"{data.GameName}{data.BuildVersion}V{data.BuildNumber}iOS/";
			string name   = !string.IsNullOrEmpty(data.customOutputFileName) ? data.customOutputFileName : (data.GameName + (isDebug ? "_Dev" : "_Release"));
			return path + folder + name;
		}

		public static void BuildIOS(iOSModuleData data, bool isDebug)
		{
			SyncIOS(data);
			EditorSceneManager.SaveOpenScenes();

			string fullPath = GetIOSOutputPath(data, isDebug);
			EnsureDirectory(fullPath);

			BuildPlayerOptions opts = new BuildPlayerOptions
			{
				scenes            = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
				locationPathName  = fullPath,
				target            = BuildTarget.iOS,
				options           = (isDebug ? BuildOptions.Development : BuildOptions.CompressWithLz4HC) | BuildOptions.ShowBuiltPlayer
			};

			BuildReport report  = BuildPipeline.BuildPlayer(opts);
			BuildSummary summary = report.summary;
			if (summary.result == BuildResult.Succeeded)
				Debug.Log($"Build succeeded: {summary.totalSize} bytes -> {fullPath}");
			else
				Debug.LogError("Build failed.");
		}

	}

}

#endif
