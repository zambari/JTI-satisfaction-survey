using UnityEngine;
using UnityEditor;
using ToucanApp.Data;
using ToucanApp.States;

namespace ToucanApp
{
    [InitializeOnLoad]
    public static class MobileAppUtilities
    {
        private const string REG_CTM_VAR = "toucanAppConvertToMobile";

        static MobileAppUtilities()
        {
            if (PlayerPrefs.GetInt(REG_CTM_VAR) > 0)
                EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                PrepareForMobileStart();
            }
        }

        [MenuItem("Toucan App/Mobile/Convert to Mobile App", false)]
        public static void PrepareForMobile()
        {
            var appHandler = Selection.activeGameObject.GetComponentInParent<AppHandler>();

            appHandler.ResourceHanlder.compress = true;
            appHandler.ResourceHanlder.autoLoadResource = false;

            var items = appHandler.GetComponentsInChildren<CanvasSubState>(true);
            foreach (var item in items)
            {
                if (!item.GetComponent<ManageResourceExtension>())
                    item.gameObject.AddComponent<ManageResourceExtension>();
            }

            PlayerPrefs.SetInt(REG_CTM_VAR, 1);
            EditorApplication.isPlaying = true;
        }

        private static void PrepareForMobileStart()
        {
            PlayerPrefs.DeleteKey(REG_CTM_VAR);
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;

            var appHandler = Selection.activeGameObject.GetComponentInParent<AppHandler>();

            appHandler.ResourceHanlder.compress = true;
            appHandler.ResourceHanlder.autoLoadResource = true;
            appHandler.ResourceHanlder.downloadAndSaveOnly = true;

            appHandler.ResourceHanlder.onWorkEnd.AddListener(() =>
            {
                EditorApplication.isPlaying = false;

                string[] files = System.IO.Directory.GetFiles(appHandler.ContentHandler.LocalAddress);
                foreach (var file in files)
                {
                    if (Data.Utilities.IsVideo(System.IO.Path.GetExtension(file)))
                    {
                        System.IO.File.Delete(file);
                    }
                }
            });

            appHandler.ContentHandler.MarkForContentReimport();
        }
    }
}
