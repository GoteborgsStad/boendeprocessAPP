#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

public class AuthPostProcessor
{

#if UNITY_IOS
#pragma warning disable 0162
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string buildPath)
	{
		if (target != BuildTarget.iOS)
			return;

		string plistPath = Path.Combine(buildPath, "Info.plist");

		PlistDocument plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));

		PlistElementDict rootDict = plist.root;		
		PlistElementArray LSApplicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
		LSApplicationQueriesSchemes.AddString("bankid");
		
		File.WriteAllText(plistPath, plist.WriteToString());
	}
#pragma warning restore 0162
#endif
}