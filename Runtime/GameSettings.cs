#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

using UnityEngine;


namespace THEBADDEST.Settings
{


	[CreateAssetMenu(menuName = "THEBADDEST/Settings")]
	public class GameSettings : ScriptableObject
	{

		public General general;
		public Build   build;
		public Content content;

		static GameSettings settings;
		public static GameSettings Settings
		{
			get
			{
				if (settings == null)
				{
					Initialize();
				}

				return settings;
			}
		}

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			if (settings == null)
			{
				settings = Resources.Load<GameSettings>("GameSettings");
			}
			#if UNITY_EDITOR
			if (settings == null)
			{
				string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(GameSettings)}");
				string   path  = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
				settings = UnityEditor.AssetDatabase.LoadAssetAtPath<GameSettings>(path);
			}

			if (settings == null)
			{
				settings = ScriptableObject.CreateInstance<GameSettings>();
				string path = "Assets/Resources/GameSettings.asset";
				AssetDatabase.CreateAsset(settings, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			#endif
		}


		public void Sync()
		{
			#if UNITY_EDITOR
			general.UpdateQuality();
			PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, general.BundleId);
			PlayerSettings.companyName               = general.ComapanyName;
			PlayerSettings.productName               = general.GameName;
			PlayerSettings.bundleVersion             = general.BuildVersion;
			PlayerSettings.Android.bundleVersionCode = general.BuildNumber;
			PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new Texture2D[] { general.GameIcon.texture }, IconKind.Any);
			Screen.orientation = ScreenOrientation.Portrait;
			// Screen.autorotateToPortrait           = true;
			// Screen.autorotateToLandscapeLeft      = false;
			// Screen.autorotateToLandscapeRight     = false;
			// Screen.autorotateToPortraitUpsideDown = true;
			#endif
		}
		#if UNITY_EDITOR
		[MenuItem("Tools/THEBADDEST/Game/Game Settings %&g")]
		public static void OpenMainGameSettings()
		{
			Selection.activeObject = GameSettings.Settings;
		}
		#endif

	}


}