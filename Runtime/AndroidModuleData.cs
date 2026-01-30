using UnityEditor;
using UnityEngine;


namespace THEBADDEST.BuildModuleSystem
{


	[System.Serializable]
	public class AndroidModuleData
	{

		public string GameName = "GameName";
		public string ComapanyName = "Games";
		public string BundleId = "com.games.GameName";
		public string BuildVersion = "0.1";
		public int BuildNumber = 1;
		public UIOrientation orientation = UIOrientation.Portrait;

		
		public string buildPath;
		
		public string customOutputFileName;
		
		public bool signed = false;
		public string keystoreName;
		public string keystorePass = "123456";
		public string keyaliasName;
		public string keyaliasPass = "123456";

	}


}
