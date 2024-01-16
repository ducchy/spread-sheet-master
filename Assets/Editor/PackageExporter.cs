using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class PackageExporter {
	#region Constants

	public const string RootDirectory = "Assets/SpreadSheetMaster";
	public const string FileName = "SpreadSheetMaster";

	#endregion

	#region Methods

	/// <summary>
	/// パッケージの書き出し(エディタ上でのテスト用)
	/// メニュー 「Tools > Export Unitypackage Test」をクリックで実行
	/// </summary>
	[MenuItem("Tools/Export Unitypackage Test")]
	public static void ExportTestOnEditor() {
		var exportPath = EditorUtility.SaveFilePanel
		(
			"保存先を選択",
			Application.dataPath,
			$"{FileName}.unitypackage",
			"unitypackage"
		);

		CreatePackage(RootDirectory, exportPath);
	}

	/// <summary>
	/// パッケージの書き出し
	/// GitHub Actionsから呼ばれる
	/// </summary>
	public static void Export() {
		CreatePackage(RootDirectory, $"build/{FileName}.unitypackage");
	}

	public static void CreatePackage(string rootDirectory, string exportPath) {
		SafeCreateDirectory(exportPath);
		var assetsPaths = GetAllAssetsAtPath(rootDirectory);
		AssetDatabase.ExportPackage(assetsPaths, exportPath, ExportPackageOptions.Default);
		Debug.Log($"Export complete: {Path.GetFullPath(exportPath)}\nExport below files:\n{string.Join("\n", assetsPaths)}");
	}

	public static DirectoryInfo SafeCreateDirectory(string path) {
		var diParent = Directory.GetParent(path);
		if (diParent == null || Directory.Exists(diParent.FullName)) {
			return null;
		}

		return Directory.CreateDirectory(diParent.FullName);
	}

	public static string[] GetAllAssetsAtPath(string root) {
		return Directory.GetFiles(root, "*", SearchOption.AllDirectories)
			.Where(x => !string.IsNullOrEmpty(x))
			.Where(x => !x.EndsWith(".meta"))
			.Where(x => x != ".DS_Store")
			.ToArray();
	}

	#endregion
}