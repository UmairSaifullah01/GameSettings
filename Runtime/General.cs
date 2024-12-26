using UnityEngine;
using UnityEngine.Serialization;


namespace THEBADDEST.Settings
{


	[System.Serializable]
	public class General
	{

		public string GameName     = "GameName";
		public string ComapanyName = "Games";
		public string BundleId     = $"com.games.GameName";
		public string BuildVersion = "0.1";
		public int    BuildNumber  = 1;

		[Header("Debug")] public string GameSceneName = "GameScene";
		public                   bool   DebugMode     = false;


		[Header("Game Icon")] public Sprite GameIcon;


		[Header("QualitySettings")] public bool                 ForceUpdateQualitySettings = false;
		public                             int                  masterTextureLimit         = 0;
		public                             AnisotropicFiltering anisotropicFiltering       = AnisotropicFiltering.Disable;
		public                             int                  antiAliasing               = 0;
		public                             ShadowResolution     shadowResolution           = ShadowResolution.Medium;

		public void UpdateQuality()
		{
			if (ForceUpdateQualitySettings)
			{
				QualitySettings.globalTextureMipmapLimit = 0;
				QualitySettings.anisotropicFiltering     = AnisotropicFiltering.Disable;
				QualitySettings.antiAliasing             = 0;
				QualitySettings.shadowResolution         = ShadowResolution.Medium;
			}
		}

		public void OnBuildSuccess()
		{
		}

	}


}