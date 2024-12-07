using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


namespace THEBADDEST.Settings
{


	[CustomEditor(typeof(GameSettings))]
	public class GameSettingEditor : Editor
	{

		GameSettings settings;
		int          tab = 0;
		public bool IsSavedProductName
		{
			get => EditorPrefs.GetBool("IsSavedProductName", false);
			set => EditorPrefs.SetBool("IsSavedProductName", value);
		}

		public override void OnInspectorGUI()
		{
			if (settings == null)
			{
				settings = (GameSettings) target;
			}

			EditorGUILayout.Space();
			GUILayout.Label("GAME SETTINGS", new GUIStyle {fontSize = 30, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = {textColor = Color.white}});
			EditorGUILayout.Space();
			GUILayout.Label("Developed by Umair Saifullah", new GUIStyle() {alignment = TextAnchor.LowerRight, fontStyle = FontStyle.Italic});
			EditorGUILayout.Space();
			tab = GUILayout.Toolbar(tab, new string[] {"General", "Build", "Content"}, GUILayout.MaxHeight(20));
			switch (tab)
			{
				case 0:
					GeneralSettingsEditor();
					break;

				case 1:
					BuildSettingEditor();
					break;

				case 2:
					ContentSettingsEditor();
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}

		void ContentSettingsEditor()
		{
			CreateSubFields("content");
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Add Packages");
			if (GUILayout.Button("Refresh", GUILayout.MaxWidth(75)))
			{
				settings.content.UpdatePackages();
			}

			EditorGUILayout.EndHorizontal();
			AddCustomPackageGUI("Addressables", "com.unity.addressables");
			bool isVideoRecorderAdded = AddCustomPackageGUI("Video Recorder", "com.unity.recorder");
			if (isVideoRecorderAdded)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Name Format", GUILayout.MaxWidth(100));
				EditorGUILayout.LabelField(settings.content.VideoName);
				if (GUILayout.Button("Copy", GUILayout.MaxWidth(50)))
				{
					EditorGUIUtility.systemCopyBuffer = settings.content.VideoName;
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Path", GUILayout.MaxWidth(100));
				EditorGUILayout.LabelField(settings.content.VideoPath);
				if (GUILayout.Button("Copy", GUILayout.MaxWidth(50)))
				{
					EditorGUIUtility.systemCopyBuffer = settings.content.VideoPath;
				}

				EditorGUILayout.EndHorizontal();
			}
		}

		bool AddCustomPackageGUI(string packageName, string identifier)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(packageName);
			bool added = settings.content.packages[identifier];
			EditorGUI.BeginDisabledGroup(added);
			if (GUILayout.Button(!added ? "Add" : "Added", GUILayout.MaxWidth(50)))
			{
				Client.Add(identifier);
				settings.content.UpdatePackageValue(identifier, true);
			}

			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			return added;
		}


		void BuildSettingEditor()
		{
			EditorGUILayout.Space();
			var childEnum = serializedObject.FindProperty("build").GetEnumerator();
			while (childEnum.MoveNext())
			{
				var current = childEnum.Current as SerializedProperty;
				if (current.name == "buildPath")
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(current);
					if (GUILayout.Button("Browse", GUILayout.MaxWidth(60)))
					{
						string path = EditorUtility.OpenFolderPanel("Build Path", "", "");
						settings.build.buildPath = path;
						current.serializedObject.Update();
						current.serializedObject.ApplyModifiedProperties();
					}

					if (GUILayout.Button("Reset", GUILayout.MaxWidth(50)))
					{
						settings.build.ResetPath();
						current.serializedObject.Update();
						current.serializedObject.ApplyModifiedProperties();
					}

					EditorGUILayout.EndHorizontal();
				}
				else if (current.name.Contains("key"))
				{
					if (settings.build.signed)
					{
						if (current.name == "keystoreName")
						{
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.PropertyField(current);
							if (GUILayout.Button("Browse", GUILayout.MaxWidth(60)))
							{
								string path = EditorUtility.OpenFilePanel("Keystore", "", "keystore");
								settings.build.keystoreName = path;
								current.serializedObject.Update();
								current.serializedObject.ApplyModifiedProperties();
							}

							EditorGUILayout.EndHorizontal();
						}
						else
						{
							EditorGUILayout.PropertyField(current);
						}
					}
				}
				else
				{
					EditorGUILayout.PropertyField(current);
				}
			}

			BuidTypeGUI();
			EditorGUILayout.BeginHorizontal();
			if (!IsSavedProductName && string.IsNullOrEmpty(settings.build.customBuildAndroidProductName))
				EditorGUILayout.LabelField(settings.build.BuildAndroidProductName);
			if (IsSavedProductName)
				settings.build.customBuildAndroidProductName = EditorGUILayout.TextField(settings.build.customBuildAndroidProductName);
			if (GUILayout.Button("Edit", GUILayout.MaxWidth(50)))
			{
				settings.build.customBuildAndroidProductName = settings.build.BuildAndroidProductName;
				IsSavedProductName                           = true;
			}

			if (GUILayout.Button("Reset", GUILayout.MaxWidth(50)))
			{
				settings.build.customBuildAndroidProductName = string.Empty;
				IsSavedProductName                           = false;
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Build Android", GUILayout.Height(25)))
			{
				GameSettings.Settings.build.DoBuild();
			}

			if (GUILayout.Button("Locate", GUILayout.MaxWidth(75), GUILayout.Height(25)))
			{
				settings.build.CreateFolder();
				EditorUtility.RevealInFinder(settings.build.buildPath);
			}

			EditorGUILayout.EndHorizontal();
		}

		void BuidTypeGUI()
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Build Type");
			EditorGUI.BeginDisabledGroup(!settings.build.IsMono);
			if (GUILayout.Button("IL2CPP", GUILayout.MaxWidth(100)))
			{
				PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
				PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
				PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
			}

			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(settings.build.IsMono);
			if (GUILayout.Button("Mono", GUILayout.MaxWidth(100)))
			{
				PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
				PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);
				PlayerSettings.SetArchitecture(BuildTargetGroup.Android, 1);
			}

			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		void GeneralSettingsEditor()
		{
			CreateSubFields("general");
			EditorGUILayout.Space();
			if (GUILayout.Button("Open Quality Settings"))
			{
				Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("QualitySettings");
			}

			if (GUILayout.Button("Open Player Settings"))
			{
				Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
			}

			EditorGUILayout.Space();
			if (GUILayout.Button("Sync", GUILayout.MaxHeight(25)))
			{
				settings.Sync();
			}
		}

		void CreateSubFields(string propertyName)
		{
			EditorGUILayout.Space();
			var childEnum = serializedObject.FindProperty(propertyName).GetEnumerator();
			while (childEnum.MoveNext())
			{
				var current = childEnum.Current as SerializedProperty;
				EditorGUILayout.PropertyField(current);
			}
		}

	}


}