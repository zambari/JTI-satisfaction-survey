using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace ToucanApp.Data
{
    [CustomEditor(typeof(AppHandler))]
    public class AppHandlerEditor : Editor
    {
        public string CoverFileName
        {
            get
            {
                var me = (AppHandler)target;
                return string.Format("Assets/ToucanAppSettings/COVER_{0}.jpg", me.ContentHandler.appName);
            }
        }

        public Texture2D CoverTexture
        {
            get
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(CoverFileName);
            }
        }

        public bool HasProperCover
        {
            get
            {
                var cover = CoverTexture;
                return cover != null && cover.width > 16 && cover.height > 16;
            }
        }

        public void CreateCover(Texture2D texture)
        {
            File.WriteAllBytes(CoverFileName, texture.EncodeToJPG());
            AssetDatabase.Refresh();

            var importer = TextureImporter.GetAtPath(CoverFileName) as TextureImporter;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            AssetDatabase.ImportAsset(CoverFileName);
            AssetDatabase.Refresh();
        }

        public void OnEnable()
        {
            var me = (AppHandler)target;

            me.UploadHandler.RequestApps(() =>  Repaint());
        }

        private void CreateDummyCover()
        {
            var temp = new Texture2D(2, 2);
            CreateCover(temp);
        }

        public override void OnInspectorGUI()
        {
            var me = (AppHandler)target;

            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
                me.UploadHandler.RequestApps(() => Repaint());

            if (me.UploadHandler.ServerAppInfo == null)
                return;

            var appExistsOnline = me.UploadHandler.ServerAppInfo.id > -1;

            GUILayout.Label("Connection Status", EditorStyles.boldLabel);
        
            var statusStyle = new GUIStyle(GUI.skin.label);
            var onlineText = me.UploadHandler.IsConnected ? "Online" : "Offline";
            statusStyle.normal.textColor = me.UploadHandler.IsConnected ? new Color(0, .5f, 0, 1) : Color.gray;
            GUILayout.Label(onlineText, statusStyle);

            GUILayout.Label("Deploy", EditorStyles.boldLabel);
            //GUILayout.Label("Export", EditorStyles.boldLabel);

            var exportText = !Application.isPlaying ? "Export content" : "Re-export content";
            if (GUILayout.Button(exportText))
            {
                if (me.ContentHandler.ExportData())
                {
                    // do something on export success
                    ContentTools.FindNotRegisteredDuplicates(me.transform);
                }
            }

#if CMS_2
            return;
#endif

            if (GUILayout.Button("Generate settings"))
            {
                GenerateSettings();
            }

            //GUILayout.Label("Package", EditorStyles.boldLabel);
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Create app cover"))
                {
                    me.StartCoroutine(Utilities.TakeScreenShot(false, false, (Texture2D tex) => {

                        CreateCover(tex);

                    }));
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = me.UploadHandler.ContentAwaiting && !me.UploadHandler.IsBussy;
                if (GUILayout.Button("Create content package"))
                {
                    me.AssignVersion();
                    me.UploadHandler.CreatePackage(false);
                }
                else if (GUILayout.Button("Create build package"))
                {
                    me.IncrVersion();
                    me.AssignVersion();
                    me.UploadHandler.CreatePackage(true);
                    return;
                }
                GUILayout.EndHorizontal();

                if (me.UploadHandler.IsConnected)
                {
                    //GUILayout.Label("Upload", EditorStyles.boldLabel);
                    GUI.enabled = me.UploadHandler.PackageAwaiting && !me.UploadHandler.IsBussy;
                    if (GUILayout.Button("Upload package"))
                    {
                        if (CoverTexture == null)
                            CreateDummyCover();

                        me.UploadHandler.RequestApps(() => {

                            //if (!HasProperCover)
                            //    Debug.LogError("Upload failed -> App cover not found!");
                            //else
                            me.UploadHandler.UploadPackage(CoverTexture);

                        });
                    }
                }
            }
        }

        private void GenerateSettings()
        {
            var me = (AppHandler)target;
            var asset = ScriptableObject.CreateInstance<ToucanAppSettings>();
            if (!AssetDatabase.IsValidFolder("Assets/ToucanAppSettings"))
                AssetDatabase.CreateFolder("Assets", "ToucanAppSettings");
            AssetDatabase.CreateAsset(asset, string.Format("Assets/ToucanAppSettings/{0}_Settings.asset", Path.GetFileNameWithoutExtension(Path.GetRandomFileName())));

            Undo.RecordObject(me, "Generated new setting");
            me.settings = asset;
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (HasProperCover)
                GUI.DrawTexture(r, CoverTexture, ScaleMode.ScaleToFit);
        }
    }
#endif
}
