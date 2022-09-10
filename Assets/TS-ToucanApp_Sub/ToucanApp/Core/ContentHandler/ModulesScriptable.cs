//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace ToucanApp.Data
//{
//	public class ModulesScriptable : ScriptableObject 
//	{
//		private static ModulesScriptable current;
//		public static ModulesScriptable Current 
//		{
//			get 
//			{
//                if (current == null)
//                {
//                    var modulesManager = ModulesManager.Instance;
//                    if (modulesManager == null)
//                    {
//                        modulesManager = FindObjectOfType<ModulesManager>();
//                        if (modulesManager == null)
//                        {
//#if UNITY_EDITOR
//                            var go = new GameObject("ModulesManager");
//                            modulesManager = go.AddComponent<ModulesManager>();
//                            modulesManager.Modules = CreateOrFindModuleData();
//#else
//                            Debug.Log("Can't find module manager!");
//#endif
//                        }
//                    }

//                    if (modulesManager != null)
//                        current = modulesManager.Modules;
//                }

//				return current;
//			}
//		}

//		[SerializeField]
//		private string modulesRoot;

//		[SerializeField]
//		private ModuleContent[] prefabs = new ModuleContent[0];
//		public ModuleContent[] Prefabs
//		{
//			get { return prefabs; }
//			set { prefabs = value; }
//		}

//		public string ModulesRoot
//		{
//			get { return modulesRoot; }
//		}

//		public string[] GetModulesNames(string prefix = null)
//		{
//			var temp = new List<string> ();
//			foreach (var p in prefabs) 
//			{
//				if (!string.IsNullOrEmpty(prefix))
//					p.name = prefix + "@" + p.name;

//				temp.Add (p.name);
//			}

//			return temp.ToArray ();
//		}

//#if UNITY_EDITOR

//		public void RefreshModules()
//		{  
//			var temp = new List<ModuleContent> ();
//			string[] files = Directory.GetFiles(modulesRoot, "*.*", SearchOption.AllDirectories);
//			foreach (string file in files) 
//			{
//				ModuleContent objAsset = AssetDatabase.LoadAssetAtPath(file, typeof(ModuleContent)) as ModuleContent;
//				if (objAsset != null)
//					temp.Add (objAsset);
//			}

//			prefabs = temp.ToArray ();
//		}

//		public string GetModulePrefabPathWithID(string id)
//		{
//			foreach (var prefab in prefabs) 
//			{
//				if (prefab != null && prefab.ID == id)
//					return AssetDatabase.GetAssetPath(prefab.GetInstanceID());
//			}

//			return null;
//		}

//		public void CreatePreviewFile(ModuleContent module)
//		{
//			if (module.gameObject.scene.rootCount > 0) 
//			{
//				Debug.LogError ("Preview creation failed !!!");
//				return;
//			}

//			var prefabPath = GetModulePrefabPathWithID(module.ID);

//			if (string.IsNullOrEmpty(prefabPath))
//			{
//				Debug.LogError ("Preview creation failed !!!");
//				return;
//			}

//			var modulePath = Path.GetDirectoryName(prefabPath);
//			Directory.CreateDirectory (modulePath);

//			string assetPath = AssetDatabase.GetAssetPath(module.preview.GetInstanceID());
//			if (string.IsNullOrEmpty(assetPath))
//				assetPath = modulePath + "/Cover_" +  module.ID + ".jpg";

//			File.WriteAllBytes(assetPath, module.preview.EncodeToJPG());
//			AssetDatabase.Refresh();
//			module.preview = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

//			TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
//			importer.isReadable = true;
//			importer.textureCompression = TextureImporterCompression.Uncompressed;

//			AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
//		}

//		public void CreateModule(ModuleContent module)
//		{
//			if (string.IsNullOrEmpty (modulesRoot)) 
//			{
//				Debug.LogError("No CMS Modules directory found!");
//				return;
//			}

//			try
//			{
//				// create module files
//				if (string.IsNullOrEmpty(module.ID))
//					ContentEditorTools.GenerateID(module.transform);

//				var prefabPath = GetModulePrefabPathWithID(module.ID);
//				if (string.IsNullOrEmpty(prefabPath))
//					prefabPath = modulesRoot + "/" + module.ID + "/Module_" + module.ID + ".prefab";

//				var modulePath = Path.GetDirectoryName(prefabPath);
//				Directory.CreateDirectory (modulePath);

//				PrefabUtility.CreatePrefab (prefabPath, module.gameObject);
//				//Object prefab = PrefabUtility.CreatePrefab (prefabPath, module.gameObject);
//				//PrefabUtility.ReplacePrefab (module.gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);

//				// check for missing files
//				var missingAssetsPaths = new List<string>();
//				var monos = Utilities.GetAllComponentsInChildren<MonoBehaviour>(module.transform, true);
//				foreach (var mono in monos)
//				{
//					var script = AppBase.FindScriptReference(mono);
//					var assetPath = AssetDatabase.GetAssetPath(script.GetInstanceID());

//					var scriptType = mono.GetType().ToString();

//					if (!scriptType.Contains("ToucanApp") && !scriptType.Contains("UnityEngine")  && !scriptType.Contains("UnityEditor"))
//					{
//						if (!assetPath.Contains(modulesRoot) && !assetPath.Contains("Resources"))
//							missingAssetsPaths.Add(assetPath);
//					}
//				}

//				if (module.preview != null)
//				{
//					var coverPath = AssetDatabase.GetAssetPath(module.preview.GetInstanceID());
//					if (!coverPath.Contains(modulesRoot) && !string.IsNullOrEmpty(coverPath) && !missingAssetsPaths.Contains(coverPath))
//						missingAssetsPaths.Add(coverPath);
//				}

//				var texts = Utilities.GetAllComponentsInChildren<UnityEngine.UI.Text>(module.transform, true);
//				foreach (var text in texts)
//				{
//					var assetPath = AssetDatabase.GetAssetPath(text.font.GetInstanceID());
//					if (!assetPath.Contains(modulesRoot) && !string.IsNullOrEmpty(assetPath) && !assetPath.Contains("Resources") && !missingAssetsPaths.Contains(assetPath))
//						missingAssetsPaths.Add(assetPath);
//				}

//				var images = Utilities.GetAllComponentsInChildren<UnityEngine.UI.Image>(module.transform, true);
//				foreach (var img in images)
//				{
//					var assetPath = AssetDatabase.GetAssetPath(img.sprite.GetInstanceID());
//					if (!assetPath.Contains(modulesRoot) && !string.IsNullOrEmpty(assetPath) && !assetPath.Contains("Resources") && !missingAssetsPaths.Contains(assetPath))
//						missingAssetsPaths.Add(assetPath);
//				}

//				var rawImages = Utilities.GetAllComponentsInChildren<UnityEngine.UI.RawImage>(module.transform, true);
//				foreach (var img in rawImages)
//				{
//					var assetPath = AssetDatabase.GetAssetPath(img.texture.GetInstanceID());
//					if (!assetPath.Contains(modulesRoot) && !string.IsNullOrEmpty(assetPath) && !assetPath.Contains("Resources") && !missingAssetsPaths.Contains(assetPath))
//						missingAssetsPaths.Add(assetPath);
//				}

//				// finalize export

//				EditorUtility.FocusProjectWindow();
//				Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);

//				RefreshModules();
//				if (missingAssetsPaths.Count == 0)
//				{
//					Debug.Log("Module export success. -> " + prefabPath);
//				}
//				else
//				{
//					var moduleAssets = new List<Object>();

//					foreach (var assetPath in missingAssetsPaths)
//					{
//						Object assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
//						moduleAssets.Add(assetObj);
//					}

//					string missingPaths = "";

//					foreach (var missingPath in missingAssetsPaths)
//					{
//						missingPaths += missingPath + ", ";
//					}

//					Debug.LogWarning(("Module " + module.ID + " exported with warning: file not found in module directory -> ").ToUpper() + missingPaths);

//					EditorUtility.FocusProjectWindow();
//					Selection.objects = moduleAssets.ToArray();
//				}
//			} 
//			catch (System.Exception e) 
//			{
//				Debug.LogError ("Module export failed -> " + e);
//			}
//		}

//		private static ModulesScriptable CreateOrFindModuleData()
//		{
//			var assetName = "ToucanModulesData.asset";
//			string modulesPath;
//			var foldersFound = System.IO.Directory.GetDirectories (Application.dataPath, "*Modules_Storage*", System.IO.SearchOption.AllDirectories);
//			if (foldersFound.Length > 0) 
//			{
//				modulesPath = foldersFound [0].Replace ('\\', '/');
//				modulesPath = modulesPath.Substring(Application.dataPath.Length - 6);
//			} 
//			else 
//			{
//				Debug.LogError("No CMS Modules directory found!");
//				return null;
//			}

//			var assetPathsFound = AssetDatabase.FindAssets ("t:" + typeof(ModulesScriptable).ToString(), new string[] {modulesPath});
//			string assetPath = modulesPath + "/" + assetName;

//			ModulesScriptable assetFound;
//			if (assetPathsFound.Length > 0) 
//			{
//				assetFound = AssetDatabase.LoadAssetAtPath<ModulesScriptable> (assetPath);
//			} 
//			else 
//			{
//				assetFound = ScriptableObject.CreateInstance<ModulesScriptable> ();
//				AssetDatabase.CreateAsset(assetFound, assetPath);
//				AssetDatabase.SaveAssets();

//				Selection.activeObject = assetFound;
//				Debug.Log ("Modules created.");
//			}

//			assetFound.modulesRoot = modulesPath;

//			return assetFound;
//		}

//#endif
//	}
//}
