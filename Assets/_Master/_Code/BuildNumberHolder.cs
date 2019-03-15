using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ius
{
	[CreateAssetMenu]
	public class BuildNumberHolder : ScriptableObject 
	{
		[SerializeField] private int mBuildNumber;
		
		public static string BuildNumber
		{
			get
			{
				return Instance.mBuildNumber.ToString("0000");
			}
		}

		private static BuildNumberHolder Instance
		{
			get
			{
				return Resources.Load<BuildNumberHolder>("BuildNumberHolder");
			}
		}
		
		public static void IncreaseBuildNumber()
		{
#if UNITY_EDITOR
			BuildNumberHolder holder = Instance;
			holder.mBuildNumber++;

			string version = UnityEditor.PlayerSettings.bundleVersion;

			int lastDot = version.LastIndexOf('.');
			version = version.Remove(lastDot);
			version += "." + holder.mBuildNumber.ToString();
			UnityEditor.PlayerSettings.bundleVersion = version;

			UnityEditor.PlayerSettings.Android.bundleVersionCode = holder.mBuildNumber;
			UnityEditor.PlayerSettings.iOS.buildNumber = holder.mBuildNumber.ToString();

			// Force an asset save
			UnityEditor.EditorUtility.SetDirty(holder);
			UnityEditor.AssetDatabase.SaveAssets();
#endif
		}
	}
}