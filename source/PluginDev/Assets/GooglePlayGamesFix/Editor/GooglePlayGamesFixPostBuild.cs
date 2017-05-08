using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEditor;
using System.Diagnostics;
using System.IO;

public class GooglePlayGamesFixPostBuild : MonoBehaviour {
	[PostProcessBuild(1000)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
#if UNITY_IOS
		const string prefix = "Google_";

		var buildToolsDir = Application.dataPath + @"/GooglePlayGamesFix/Editor/build-tools";

		var searchPattern = prefix + "*.py";  // This would be for you to construct your prefix

		var folder = new DirectoryInfo(buildToolsDir);

		var files = folder.GetFiles(searchPattern);

		foreach (var file in files)
		{
			Process proc = new Process();
			proc.StartInfo.FileName = "python2.6";
			proc.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\"", file.FullName, pathToBuiltProject, string.Empty);
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.Start();
			string err = proc.StandardError.ReadToEnd();
			proc.WaitForExit();

			if (proc.ExitCode != 0)
			{
				UnityEngine.Debug.Log("error: " + err + "   code: " + proc.ExitCode);
			}
		}
#endif
    }
}
