#if UNITY_EDITOR
using System.IO;
using UnityEngine;


namespace THEBADDEST.BuildModuleSystem
{


	internal static class BuildModuleDefaultPaths
	{

		const string BuildsFolder = "Builds";

		/// <summary>
		/// Returns the default build output path for the given platform and game name.
		/// Uses project root and OS-appropriate path separators (normalized to forward slash with trailing slash).
		/// </summary>
		public static string GetDefaultBuildPath(BuildModule.Platform platform, string gameName)
		{
			string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			string platformName = platform == BuildModule.Platform.Android ? "Android" : "iOS";
			string name = string.IsNullOrEmpty(gameName) ? "Game" : gameName;
			string path = Path.Combine(projectRoot, BuildsFolder, platformName, name);
			return path.Replace("\\", "/").TrimEnd('/') + "/";
		}

	}


}

#endif
