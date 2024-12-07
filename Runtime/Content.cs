using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif


namespace THEBADDEST.Settings
{


	[System.Serializable]
	public class Content
	{

		public string VideoPath => $"{Paths.VideoPathPrefix}{GameSettings.Settings.general.GameName}{Paths.VideoPathSuffix}";
		public string VideoName => $"{GameSettings.Settings.general.GameName}{Paths.VideoNamePrefix}";

		#if UNITY_EDITOR


		public Dictionary<string, bool> packages = new Dictionary<string, bool>() { { Paths.AddressablesPackageName, false }, { Paths.RecorderPackageName, false } };

		public void UpdatePackages()
		{
			if (File.Exists(Paths.PackageManifestPath))
			{
				string jsonText = File.ReadAllText(Paths.PackageManifestPath);
				var    listData = packages.ToList();
				foreach (var package in listData)
				{
					if (jsonText.Contains(package.Key))
					{
						UpdatePackageValue(package.Key, true);
					}
				}
			}
		}

		public void UpdatePackageValue(string key, bool value)
		{
			packages[key] = value;
		}


		#endif

	}


}