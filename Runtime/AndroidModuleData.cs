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

		[Header("Android Build")]
		public string buildPath;

		[Tooltip("Optional override for output file name. Leave empty for default.")]
		public string customOutputFileName;

		[Header("Keystore")]
		public bool signed = false;
		public string keystoreName;
		public string keystorePass = "123456";
		public string keyaliasName;
		public string keyaliasPass = "123456";

	}


}
