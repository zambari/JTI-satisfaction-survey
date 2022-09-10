using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ToucanApp.Data
{
    public class ContentTools
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Toucan App/Find empty ID's &1", false, 101)]
        private static void EmptyID()
        {
            var selected = UnityEditor.Selection.activeTransform;
            if (selected == null)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            var tempContent = Utilities.GetAllComponentsInChildren<IContent>(selected, true);

            foreach (var c in tempContent)
            {
                if (string.IsNullOrEmpty(c.ID))
                {
                    var mb = ((MonoBehaviour)c);
                    Debug.Log(mb.name, mb.gameObject);
                }
            }
        }

        [UnityEditor.MenuItem("Toucan App/Find duplicate ID's &2", false, 102)]
        private static void DuplicateID()
        {
            var selected = UnityEditor.Selection.activeTransform;
            var duplicates = GatherDuplicates(selected);

            if (duplicates == null)
                return;

            int idx = 0;
            foreach (var duplicate in duplicates)
            {
                if (duplicate.Value.Count > 1)
                {
                    Random.InitState((int)idx);

                    var color = ColorUtility.ToHtmlStringRGB(new Color(Random.value, Random.value, Random.value));

                    foreach (var c in duplicate.Value)
                    {
                        Debug.Log(string.Format("<color=#{0}>", color) + ((IContent)c).ID + "</color>, name:" + ((MonoBehaviour)c).name, ((MonoBehaviour)c).gameObject);
                    }

                    idx++;
                }
            }

            if (idx == 0)
            {
                Debug.Log("<color=green>Duplicates was not found.</color>");
            }
        }

        private static Dictionary<string, List<IContent>> GatherDuplicates(Transform selected)
        {
            if (selected == null)
            {
                Debug.LogError("Gameobject was not selected!");
                return null;
            }

            var tempContent = Utilities.GetAllComponentsInChildren<IContent>(selected, true);
            Dictionary<string, List<IContent>> duplicates = new Dictionary<string, List<IContent>>();

            List<IContent> temp;

            foreach (var c in tempContent)
            {
                if (!string.IsNullOrEmpty(c.ID))
                {
                    if (!duplicates.TryGetValue(c.ID, out temp))
                    {
                        temp = new List<IContent>();
                        temp.Add(c);
                        duplicates.Add(c.ID, temp);
                    }
                    else
                    {
                        temp.Add(c);
                    }
                }
            }

            return duplicates;
        }

        public static MonoBehaviour[] ConvertToMB(object[] array)
        {
            List<MonoBehaviour> temp = new List<MonoBehaviour>();
            foreach (var e in array)
            {
                temp.Add((MonoBehaviour)e);
            }

            return temp.ToArray();
        }

        [UnityEditor.MenuItem("Toucan App/Find objects with same ID &3", false, 103)]
        private static void SameID()
        {
            var selected = UnityEditor.Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            var root = selected.GetComponentInParent<RootContent>();
            if (root == null)
            {
                Debug.LogError("Root was not found in parent!");
                return;
            }

            var searchFor = selected.GetComponent<IContent>();
            if (searchFor == null)
                return;

            var tempContent = Utilities.GetAllComponentsInChildren<IContent>(root.transform, true);

            List<GameObject> temp = new List<GameObject>();

            foreach (var c in tempContent)
            {
                if (c.ID == searchFor.ID)
                {
                    temp.Add(((MonoBehaviour)c).gameObject);
                }
            }

            if (temp != null && temp.Count > 1)
            {
                UnityEditor.Selection.objects = temp.ToArray();
            }
            else
            {
                Debug.Log("<color=green>No objects with id '" + searchFor.ID + "' was found.</color>");
            }
        }

        [UnityEditor.MenuItem("Toucan App/Clear ID's &4", false, 104)]
        private static void ClearID()
        {
            var selected = UnityEditor.Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            foreach (var sel in selected)
            {
                var tempContent = Utilities.GetAllComponentsInChildren<IContent>(sel.transform, true);

                var mbs = ConvertToMB(tempContent);
                UnityEditor.Undo.RecordObjects(mbs, "Clear Id");

                foreach (var c in tempContent)
                {
                    c.ID = "";
                    var mb = ((MonoBehaviour)c);
                    Debug.Log("'" + mb.name + "' -> id cleared.", mb.gameObject);
                }

                Debug.Log("<color=red>Id's under '" + sel.name + "' cleared successfully!</color>");
            }
        }

        [UnityEditor.MenuItem("Toucan App/Generate ID's &5", false, 105)]
        private static void GenerateID()
        {
            var selected = UnityEditor.Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            foreach (var sel in selected)
            {
                GenerateID(sel.transform);
            }
        }

        [UnityEditor.MenuItem("Toucan App/Generate group ID's &6", false, 106)]
        private static void GenerateGroupID()
        {
            var selected = UnityEditor.Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            foreach (var sel in selected)
            {
                GenerateGroupID(sel.transform);
            }
        }


        [UnityEditor.MenuItem("Toucan App/Check structure &7", false, 107)]
        private static void CheckStructure()
        {
            var selected = UnityEditor.Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            var contentHandler = selected.GetComponentInParent<ContentHandler>();
            if (contentHandler == null)
                return;

            contentHandler.ApplyContent(contentHandler.RootContent);
            var builder = DataBuilder.FromTree(contentHandler.RootContent.Data);

            BaseData[] errorsList = builder.CheckStructure();
            if (errorsList.Length == 0)
            {
                Debug.Log("<color=green>Data structure is correct.</color>");
            }
            else
            {
                Debug.LogError("Data structure is incorrect!");
                ContentHandler.LogIncorrectStructure(builder, contentHandler.RootContent, errorsList);
            }
        }

        public static Object FindScriptReference(MonoBehaviour mono)
        {
            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(mono);
            UnityEditor.SerializedProperty scriptProperty = so.FindProperty("m_Script");
            so.Update();

            return scriptProperty.objectReferenceValue;
        }

        [UnityEditor.MenuItem("Toucan App/Find dependencies &8", false, 108)]
        private static void SelectContentDependencies()
        {
            var selected = UnityEditor.Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            // check for missing files
            var missingAssetsPaths = new List<string>();
            var monos = Utilities.GetAllComponentsInChildren<MonoBehaviour>(selected.transform, true);
            foreach (var mono in monos)
            {
                var script = FindScriptReference(mono);
                var assetPath = AssetDatabase.GetAssetPath(script.GetInstanceID());

                var scriptType = mono.GetType().ToString();

                if (!scriptType.Contains("ToucanApp") && !scriptType.Contains("UnityEngine") && !scriptType.Contains("UnityEditor"))
                {
                    if (!assetPath.Contains("ToucanApp") && !assetPath.Contains("Resources"))
                        missingAssetsPaths.Add(assetPath);
                }
            }

            var texts = Utilities.GetAllComponentsInChildren<UnityEngine.UI.Text>(selected.transform, true);
            foreach (var text in texts)
            {
                var assetPath = AssetDatabase.GetAssetPath(text.font.GetInstanceID());
                if (!assetPath.Contains("ToucanApp") && !string.IsNullOrEmpty(assetPath) && !assetPath.Contains("Resources") && !missingAssetsPaths.Contains(assetPath))
                    missingAssetsPaths.Add(assetPath);
            }

            var images = Utilities.GetAllComponentsInChildren<UnityEngine.UI.Image>(selected.transform, true);
            foreach (var img in images)
            {
                var assetPath = AssetDatabase.GetAssetPath(img.sprite.GetInstanceID());
                if (!assetPath.Contains("ToucanApp") && !string.IsNullOrEmpty(assetPath) && !assetPath.Contains("Resources") && !missingAssetsPaths.Contains(assetPath))
                    missingAssetsPaths.Add(assetPath);
            }

            var rawImages = Utilities.GetAllComponentsInChildren<UnityEngine.UI.RawImage>(selected.transform, true);
            foreach (var img in rawImages)
            {
                var assetPath = AssetDatabase.GetAssetPath(img.texture.GetInstanceID());
                if (!assetPath.Contains("ToucanApp") && !string.IsNullOrEmpty(assetPath) && !assetPath.Contains("Resources") && !missingAssetsPaths.Contains(assetPath))
                    missingAssetsPaths.Add(assetPath);
            }

            if (missingAssetsPaths.Count == 0)
            {
                Debug.Log("<color=green>Dependencies was not found.</color>");
            }
            else
            {
                var moduleAssets = new List<Object>();

                foreach (var assetPath in missingAssetsPaths)
                {
                    Object assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    moduleAssets.Add(assetObj);
                }

                string missingPaths = "";

                foreach (var missingPath in missingAssetsPaths)
                {
                    missingPaths += missingPath + ", ";
                }

                Debug.Log(("<color=green>Dependencies found -> </color>") + missingPaths);

                EditorUtility.FocusProjectWindow();
                Selection.objects = moduleAssets.ToArray();
            }
        }

        [UnityEditor.MenuItem("Toucan App/Toggle Export &9", false, 109)]
        private static void ToggleExport()
        {
            var selected = UnityEditor.Selection.activeGameObject;
            if (selected == null)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            var content = selected.GetComponentsInChildren<IContent>(true);
            UnityEditor.Undo.RecordObjects(ConvertToMB(content), "Toggle Export");

            if (content.Length > 0)
            {
                bool export = !content[0].ShouldExport;
                foreach (var c in content)
                {
                    c.ShouldExport = export;
                }
            }
        }

        [UnityEditor.MenuItem("Toucan App/Duplicate ID registries &0", false, 109)]
        private static void DuplicateRegistries()
        {
            var selected = UnityEditor.Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                Debug.LogError("Gameobject was not selected!");
                return;
            }

            var temp = new List<ContentRegisteredID>();
            foreach (var sel in selected)
            {
                temp.AddRange(sel.GetComponentsInChildren<ContentRegisteredID>(true));
            }

            var contentRegistries = temp.ToArray();
            var copiedRegistries = new Dictionary<IDRegistry, IDRegistry>();

            UnityEditor.Undo.RecordObjects(ConvertToMB(contentRegistries), "Toggle Export");

            if (contentRegistries.Length > 0)
            {
                foreach (var cr in contentRegistries)
                {
                    IDRegistry originalRegistry = cr.registry;
                    IDRegistry registryCopy;
                    if (!copiedRegistries.TryGetValue(cr.registry, out registryCopy))
                    {
                        var originalAssetPath = AssetDatabase.GetAssetPath(originalRegistry);
                        string copyPath = AssetDatabase.GenerateUniqueAssetPath(originalAssetPath);

                        if (AssetDatabase.CopyAsset(originalAssetPath, copyPath))
                        {
                            registryCopy = AssetDatabase.LoadAssetAtPath<IDRegistry>(copyPath);
                            cr.registry = registryCopy;
                            PrefabUtility.RecordPrefabInstancePropertyModifications(cr);
                            copiedRegistries.Add(originalRegistry, registryCopy);
                        }
                        else
                        {
                            UnityEditor.Undo.PerformUndo();
                            Debug.LogError("Failed to duplicate registries");
                            return;
                        }
                    }
                    else
                    {
                        cr.registry = registryCopy;
                        PrefabUtility.RecordPrefabInstancePropertyModifications(cr);
                    }
                }
            }
        }

        public static void GenerateID(Transform transform)
        {
            var tempContent = Utilities.GetAllComponentsInChildren<IContent>(transform, true);

            var mbs = ConvertToMB(tempContent);
            UnityEditor.Undo.RecordObjects(mbs, "Generate Id");

            foreach (var c in tempContent)
            {
                if (string.IsNullOrEmpty(c.ID) && c.ShouldExport)
                {
                    c.ID = BaseData.GetRandomID();
                    var mb = ((MonoBehaviour)c);
                    Debug.Log("'" + mb.name + "' id set to -> '" + c.ID + "'", mb.gameObject);
                }
            }

            Debug.Log("<color=green>Id's under '" + transform.name + "' generated successfully!</color>");
        }

        public static void GenerateGroupID(Transform transform)
        {
            var tempContent = Utilities.GetAllComponentsInChildren<IContent>(transform, true);

            var mbs = ConvertToMB(tempContent);
            var groupID = "_" + BaseData.GetRandomID().Substring(0, 3);
            UnityEditor.Undo.RecordObjects(mbs, "Generate group Id");

            foreach (var c in tempContent)
            {
                if (!string.IsNullOrEmpty(c.ID))
                {
                    c.ID += groupID;
                    var mb = ((MonoBehaviour)c);
                    Debug.Log("'" + mb.name + "' id set to -> '" + c.ID + "'", mb.gameObject);
                }
            }

            Debug.Log("<color=green>Id's under '" + transform.name + "' generated successfully!</color>");
        }

        public static void FindNotRegisteredDuplicates(Transform selected)
        {
            var duplicates = GatherDuplicates(selected);
            bool unregisteredDuplicatesFound = false;

            if (duplicates == null)
                return;

            foreach (var duplicate in duplicates)
            {
                if (duplicate.Value.Count > 1)
                {
                    foreach (var c in duplicate.Value)
                    {
                        var content = (IContent)c;
                        if (content != null)
                        {
                            if (!content.UsesRegistry)
                            {
                                var mono = (MonoBehaviour)content;
                                Debug.LogWarning("Duplicated id not registered -> " + mono.name, mono);
                                unregisteredDuplicatesFound = true;
                            }
                        }
                    }
                }
            }

            if (unregisteredDuplicatesFound)
                Debug.LogWarning("Use 'ContentRegisteredId' component if you want to use shared (duplicated) Id's.");
        }

        [UnityEditor.MenuItem("Toucan App/Unhide hidden components", false, 110)]
        private static void UnhideComponents()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.Log("Select a gameobejct first");
                return;
            }
            var allComponents = Selection.activeGameObject.GetComponents<Component>();
            foreach (var c in allComponents)
                c.hideFlags = HideFlags.None;
        }
#endif
    }
}
