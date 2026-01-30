#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace THEBADDEST.BuildModuleSystem
{


	internal static class BuildPrefs
	{

		const string Prefix = "THEBADDEST.BuildModule.";

		const string KeyLastSelectedPlatform = Prefix + "LastSelectedPlatform";
		const string KeyLastBuildPathIOS     = Prefix + "LastBuildPathIOS";
		const string KeyLastBuildPathAndroid = Prefix + "LastBuildPathAndroid";
		const string KeyCustomFileNameIOS    = Prefix + "CustomFileNameIOS";
		const string KeyCustomFileNameAndroid = Prefix + "CustomFileNameAndroid";

		public static BuildModule.Platform LastSelectedPlatform
		{
			get => (BuildModule.Platform) EditorPrefs.GetInt(KeyLastSelectedPlatform, 0);
			set => EditorPrefs.SetInt(KeyLastSelectedPlatform, (int) value);
		}

		public static string LastBuildPathIOS
		{
			get => EditorPrefs.GetString(KeyLastBuildPathIOS, "");
			set => EditorPrefs.SetString(KeyLastBuildPathIOS, value);
		}

		public static string LastBuildPathAndroid
		{
			get => EditorPrefs.GetString(KeyLastBuildPathAndroid, "");
			set => EditorPrefs.SetString(KeyLastBuildPathAndroid, value);
		}

		public static string CustomFileNameIOS
		{
			get => EditorPrefs.GetString(KeyCustomFileNameIOS, "");
			set => EditorPrefs.SetString(KeyCustomFileNameIOS, value);
		}

		public static string CustomFileNameAndroid
		{
			get => EditorPrefs.GetString(KeyCustomFileNameAndroid, "");
			set => EditorPrefs.SetString(KeyCustomFileNameAndroid, value);
		}

	}


}

#endif
