#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace THEBADDEST.BuildModuleSystem
{


	public class BuildProcessor : IPreprocessBuildWithReport
	{

		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			BuildModule buildModule = FindBuildModule();
			if (buildModule == null)
				return;

			BuildTarget target = report.summary.platform;
			if (target == BuildTarget.Android)
				BuildModuleRunner.SyncAndroid(buildModule.androidModule);
			else if (target == BuildTarget.iOS)
				BuildModuleRunner.SyncIOS(buildModule.iosModule);
		}

		static BuildModule FindBuildModule()
		{
			string[] guids = AssetDatabase.FindAssets($"t:{nameof(BuildModule)}");
			if (guids.Length == 0)
				return null;
			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			return AssetDatabase.LoadAssetAtPath<BuildModule>(path);
		}

	}


}

#endif
