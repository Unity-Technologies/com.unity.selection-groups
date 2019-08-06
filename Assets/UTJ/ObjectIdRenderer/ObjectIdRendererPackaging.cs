#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Utj.Film.Compositor.Packaging {

public static class Packager {
	const string PackageName = "ObjectIdRenderer";
	static readonly string[] PackageDirs = new string[] {
		  "Assets/UTJ/ObjectIdRenderer"
	};

	[MenuItem("Assets/Make Package/" + PackageName + ".unitypackage")]
	static void MakePackage() {
		var time = System.DateTime.Now.ToString("yyyyMMdd-HHmm");
		var packageName = string.Format(
			  System.Globalization.CultureInfo.InvariantCulture
			, "{0}-{1}.unitypackage"
			, PackageName
			, time
		);
		AssetDatabase.ExportPackage(
			  PackageDirs
			, packageName
			, ExportPackageOptions.Recurse
		);
		Debug.LogFormat("{0} is created", packageName);
	}
}

} // namespace
#endif
