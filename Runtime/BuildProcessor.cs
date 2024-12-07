#if UNITY_EDITOR
using THEBADDEST.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;


namespace THEBADDEST.Build
{


	public class BuildProcessor : IPreprocessBuildWithReport
	{

		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			GameSettings.Settings.Sync();
		}

	}
	
}


#endif