#if UNITY_EDITOR
using System;
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

		const int ButtonHeight = 25;
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

			// Section 1 – Title
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.Space();
			GUILayout.Label("BUILD MODULE", new GUIStyle { fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
			EditorGUILayout.Space();
			GUILayout.Label("Developed by Umair Saifullah", new GUIStyle { alignment = TextAnchor.LowerRight, fontStyle = FontStyle.Italic });
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();

			// Section 2 – Platform (driven by current build target; no dropdown)
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
			bool isCompatible = activeTarget == BuildTarget.Android || activeTarget == BuildTarget.iOS;
			if (isCompatible)
			{
				module.selectedPlatform = activeTarget == BuildTarget.Android ? BuildModule.Platform.Android : BuildModule.Platform.iOS;
				BuildPrefs.LastSelectedPlatform = module.selectedPlatform;
				string platformLabel = module.selectedPlatform == BuildModule.Platform.Android ? "Android" : "iOS";
				GUILayout.Label(platformLabel, new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter });
			}
			else
				EditorGUILayout.HelpBox("Incompatible platform – switch to Android or iOS in File > Build Settings.", MessageType.Warning);
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();

			// Section 3 – Project / Build (platform-specific)
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			if (module.selectedPlatform == BuildModule.Platform.iOS)
				DrawIOSModuleContent(module.iosModule);
			else
				DrawAndroidModuleContent(module.androidModule);
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();

			// Section 4 – Keystore (Android only)
			if (module.selectedPlatform == BuildModule.Platform.Android)
			{
				EditorGUILayout.BeginVertical(EditorStyles.helpBox);
				DrawKeystoreSection();
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space();
			}

			// Section 5 – Actions
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			DrawActionButtons();
			EditorGUILayout.EndVertical();

			serializedObject.ApplyModifiedProperties();
		}

		void DrawIOSModuleContent(iOSModuleData data)
		{
			SerializedProperty iosProp = serializedObject.FindProperty("iosModule");
			DrawSharedFields(iosProp);
			DrawOrientation(iosProp.FindPropertyRelative("orientation"));
			EditorGUILayout.Space();
			ApplyDefaultPathIfEmpty(iosProp.FindPropertyRelative("buildPath"), module.selectedPlatform, data.GameName, v => BuildPrefs.LastBuildPathIOS = v);
			DrawBuildPath(iosProp.FindPropertyRelative("buildPath"), data.buildPath, () => BuildPrefs.LastBuildPathIOS, v => BuildPrefs.LastBuildPathIOS = v);
			EditorGUILayout.PropertyField(iosProp.FindPropertyRelative("customOutputFileName"), new GUIContent("Custom Output File Name"));
			DrawSuggestedBuildName(data.GameName, data.BuildVersion, data.BuildNumber);
		}

		void DrawAndroidModuleContent(AndroidModuleData data)
		{
			SerializedProperty androidProp = serializedObject.FindProperty("androidModule");
			DrawSharedFields(androidProp);
			DrawOrientation(androidProp.FindPropertyRelative("orientation"));
			EditorGUILayout.Space();
			ApplyDefaultPathIfEmpty(androidProp.FindPropertyRelative("buildPath"), module.selectedPlatform, data.GameName, v => BuildPrefs.LastBuildPathAndroid = v);
			DrawBuildPath(androidProp.FindPropertyRelative("buildPath"), data.buildPath, () => BuildPrefs.LastBuildPathAndroid, v => BuildPrefs.LastBuildPathAndroid = v);
			EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("customOutputFileName"), new GUIContent("Custom Output File Name"));
			DrawSuggestedBuildName(data.GameName, data.BuildVersion, data.BuildNumber);
		}

		void DrawOrientation(SerializedProperty orientationProp)
		{
			BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
			if (activeTarget != BuildTarget.Android && activeTarget != BuildTarget.iOS) return;
			EditorGUILayout.PropertyField(orientationProp, new GUIContent("Orientation"));
		}

		void DrawSuggestedBuildName(string gameName, string buildVersion, int buildNumber)
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField( BuildModuleRunner.GetSuggestedBuildName(gameName, buildVersion, buildNumber, true));
			EditorGUILayout.LabelField( BuildModuleRunner.GetSuggestedBuildName(gameName, buildVersion, buildNumber, false));
		}

		void ApplyDefaultPathIfEmpty(SerializedProperty buildPathProp, BuildModule.Platform platform, string gameName, System.Action<string> setLastPath)
		{
			if (!string.IsNullOrEmpty(buildPathProp.stringValue)) return;
			string defaultPath = BuildModuleDefaultPaths.GetDefaultBuildPath(platform, gameName);
			buildPathProp.stringValue = defaultPath;
			setLastPath(defaultPath);
		}

		void DrawKeystoreSection()
		{
			EditorGUILayout.LabelField("Keystore", EditorStyles.boldLabel);
			SerializedProperty androidProp = serializedObject.FindProperty("androidModule");
			EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("signed"));
			if (module.androidModule.signed)
			{
				DrawKeystorePath(androidProp.FindPropertyRelative("keystoreName"));
				EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("keystorePass"));
				EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("keyaliasName"));
				EditorGUILayout.PropertyField(androidProp.FindPropertyRelative("keyaliasPass"));
			}
		}

		void DrawSharedFields(SerializedProperty moduleProp)
		{
			moduleProp.FindPropertyRelative("GameName").stringValue = EditorGUILayout.TextField("Game Name", moduleProp.FindPropertyRelative("GameName").stringValue);
			moduleProp.FindPropertyRelative("ComapanyName").stringValue = EditorGUILayout.TextField("Company Name", moduleProp.FindPropertyRelative("ComapanyName").stringValue);
			moduleProp.FindPropertyRelative("BundleId").stringValue = EditorGUILayout.TextField("Bundle ID", moduleProp.FindPropertyRelative("BundleId").stringValue);
			moduleProp.FindPropertyRelative("BuildVersion").stringValue = EditorGUILayout.TextField("Build Version", moduleProp.FindPropertyRelative("BuildVersion").stringValue);
			moduleProp.FindPropertyRelative("BuildNumber").intValue = EditorGUILayout.IntField("Build Number", moduleProp.FindPropertyRelative("BuildNumber").intValue);
		}

		const int PathButtonWidth = 60;

		void DrawBuildPath(SerializedProperty buildPathProp, string currentPath, System.Func<string> getLastPath, System.Action<string> setLastPath)
		{
			EditorGUILayout.LabelField("Build Path", EditorStyles.boldLabel);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(buildPathProp, new GUIContent("Build Path"));
			if (GUILayout.Button("Browse", GUILayout.Width(PathButtonWidth)))
			{
				string path = EditorUtility.OpenFolderPanel("Build Path", currentPath ?? "", "");
				if (!string.IsNullOrEmpty(path))
				{
					buildPathProp.stringValue = path + "/";
					setLastPath(path + "/");
				}
			}
			if (GUILayout.Button("Locate", GUILayout.Width(PathButtonWidth)))
			{
				string path = buildPathProp.stringValue?.Trim();
				if (string.IsNullOrEmpty(path)) return;
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				EditorUtility.RevealInFinder(path);
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

		void DrawActionButtons()
		{
			bool isAndroid = module.selectedPlatform == BuildModule.Platform.Android;

			if (GUILayout.Button("Sync", GUILayout.Height(ButtonHeight)))
			{
				if (isAndroid)
					BuildModuleRunner.SyncAndroid(module.androidModule);
				else
					BuildModuleRunner.SyncIOS(module.iosModule);
			}

			// EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Debug Build", GUILayout.Height(ButtonHeight)))
			{
				if (isAndroid)
					BuildModuleRunner.BuildAndroid(module.androidModule, isDebug: true, buildAAB: false);
				else
					BuildModuleRunner.BuildIOS(module.iosModule, isDebug: true);
			}
			if (GUILayout.Button("Release Build", GUILayout.Height(ButtonHeight)))
			{
				if (isAndroid)
					BuildModuleRunner.BuildAndroidRelease(module.androidModule);
				else
					BuildModuleRunner.BuildIOS(module.iosModule, isDebug: false);
			}
			// EditorGUILayout.EndHorizontal();

			// EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Reset", GUILayout.Height(ButtonHeight)))
			{
				string defaultPath = BuildModuleDefaultPaths.GetDefaultBuildPath(module.selectedPlatform, isAndroid ? module.androidModule.GameName : module.iosModule.GameName);
				if (isAndroid)
				{
					serializedObject.FindProperty("androidModule").FindPropertyRelative("buildPath").stringValue = defaultPath;
					BuildPrefs.LastBuildPathAndroid = defaultPath;
				}
				else
				{
					serializedObject.FindProperty("iosModule").FindPropertyRelative("buildPath").stringValue = defaultPath;
					BuildPrefs.LastBuildPathIOS = defaultPath;
				}
			}
			// EditorGUILayout.EndHorizontal();
		}

	}


	public static class BuildModuleRunner
	{

		public static string GetSuggestedBuildName(string gameName, string buildVersion, int buildNumber, bool isDebug)
		{
			string now = DateTime.Now.ToString("yyyyMMdd_HHmm");
			return $"{gameName ?? "Game"}-v{buildVersion ?? "0.1"}[{buildNumber}]_{now}{(isDebug ? "(Debug)" : "(Release)")}";
		}

		public static void SyncIOS(iOSModuleData data)
		{
			PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, data.BundleId);
			PlayerSettings.companyName   = data.ComapanyName;
			PlayerSettings.productName   = data.GameName;
			PlayerSettings.bundleVersion = data.BuildVersion;
			PlayerSettings.iOS.buildNumber = data.BuildNumber.ToString();
			PlayerSettings.defaultInterfaceOrientation = data.orientation;
		}

		public static void SyncAndroid(AndroidModuleData data)
		{
			PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, data.BundleId);
			PlayerSettings.companyName                 = data.ComapanyName;
			PlayerSettings.productName                 = data.GameName;
			PlayerSettings.bundleVersion               = data.BuildVersion;
			PlayerSettings.Android.bundleVersionCode   = data.BuildNumber;
			PlayerSettings.defaultInterfaceOrientation = data.orientation;
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
			string ext = isAAB ? "aab" : "apk";
			return GetSuggestedBuildName(data.GameName, data.BuildVersion, data.BuildNumber, isDebug) + "." + ext;
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
			string name   = !string.IsNullOrEmpty(data.customOutputFileName) ? data.customOutputFileName : GetSuggestedBuildName(data.GameName, data.BuildVersion, data.BuildNumber, isDebug);
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
