//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.Events;
//using UnityEngine;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace ToucanApp.Data
//{
//	public class ModuleContent : StateContent
//	{
//		[Multiline(3)]
//		public string description;
//		public Texture2D preview;

//		public void ChangeLanguage(Transform transform)
//		{
//			ChangeLanguage(transform.GetSiblingIndex ());
//		}

//		public void ChangeLanguage(int idx)
//		{
//			ContentHandler.ChangeLanguage (idx);
//		}
//	}

//#if UNITY_EDITOR

//	[CustomEditor(typeof(ModuleContent))]
//	public class ModuleContentEditor : Editor
//	{
//		public void CapturePreview()
//		{
//			ModuleContent me = (ModuleContent)target;
//			var someMono = FindObjectOfType<ContentHandlerBase> ();

//			someMono.StartCoroutine(Utilities.TakeScreenShot (!Application.isPlaying, false, (Texture2D tex) => {

//				var modulesData = ModulesScriptable.Current;
//				me.preview = tex;
//				modulesData.CreatePreviewFile (me);
//				AssetDatabase.SaveAssets();

//			}));
//		}

//		public void ExportModule()
//		{
//			ModuleContent me = (ModuleContent)target;
//			var modulesData = ModulesScriptable.Current;
//			modulesData.CreateModule (me);
//		}

//		public override void OnInspectorGUI ()
//		{
//			base.OnInspectorGUI ();

//			ModuleContent me = (ModuleContent)target;
//			if (me.gameObject.scene.rootCount == 0) 
//			{
//				if (GUILayout.Button ("Create Module Preview")) 
//				{
//					CapturePreview ();
//				}
//			} 
//			else 
//			{
//				if (GUILayout.Button ("Export Module Data")) 
//				{
//					ExportModule ();
//				}
//			}
//		}

//		public override bool HasPreviewGUI()
//		{
//			return true;
//		}

//		public override void OnPreviewGUI(Rect r, GUIStyle background)
//		{
//			var me = (ModuleContent)target;
//			if (me.preview != null)
//				GUI.DrawTexture (r, me.preview, ScaleMode.ScaleToFit);
//		}
//	}

//#endif
//}
