using UnityEngine;


namespace THEBADDEST.BuildModuleSystem
{


	[CreateAssetMenu(menuName = "THEBADDEST/Build Module", fileName = "BuildModule", order = 0)]
	public class BuildModule : ScriptableObject
	{

		public enum Platform
		{
			iOS,
			Android
		}

		public Platform selectedPlatform;
		public iOSModuleData iosModule = new iOSModuleData();
		public AndroidModuleData androidModule = new AndroidModuleData();

	}


}
