using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using System.IO;
using UnityEditor.Build.Reporting;

namespace ius
{
	public class BuildVersionProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		public int callbackOrder { get { return 1000; } }
		
		public void OnPreprocessBuild(BuildReport report)
		{
			if (report.summary.platform == BuildTarget.Android)
			{
				BuildNumberHolder.IncreaseBuildNumber();
			}
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			if (report.summary.platform == BuildTarget.Android)
			{
				int nameStart = report.summary.outputPath.LastIndexOf('/') + 1;
				string fileName = report.summary.outputPath.Substring(nameStart).Replace(".apk", string.Empty);
				string newFileName = fileName.Split('_')[0];
				newFileName += "_" + Application.version + "_" + BuildNumberHolder.BuildNumber;
				string newPath = report.summary.outputPath.Remove(nameStart) + newFileName + ".apk";

				if (File.Exists(newPath))
					File.Delete(newPath);

				File.Copy(report.summary.outputPath, newPath);

				Debug.Log("Built: " + newPath);
			}
		}
	}
}