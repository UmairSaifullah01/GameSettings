using UnityEditor;
using UnityEngine;


namespace THEBADDEST.BuildModuleSystem
{


	[System.Serializable]
	public class iOSModuleData
	{

		public string GameName = "GameName";
		public string ComapanyName = "Games";
		public string BundleId = "com.games.GameName";
		public string BuildVersion = "0.1";
		public int BuildNumber = 1;
		public UIOrientation orientation = UIOrientation.Portrait;

		[Header("iOS Build")]
		public string buildPath;

		[Tooltip("Optional override for output file/folder name. Leave empty for default.")]
		public string customOutputFileName;

	}


}
