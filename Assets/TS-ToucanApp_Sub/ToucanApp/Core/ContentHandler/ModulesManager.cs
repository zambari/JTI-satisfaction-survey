//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace ToucanApp.Data
//{
//    public class ModulesManager : MonoBehaviour 
//    {
//        public static ModulesManager Instance
//        {
//            get;
//            private set;
//        }

//        [SerializeField]
//        private ModulesScriptable modules;
//        public ModulesScriptable Modules
//        {
//            get { return modules ; }
//            set { modules = value ; }
//        }

//        private void Awake()
//        {
//            Instance = this;
//            this.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
//            Debug.Log("Modules manager created!");
//        }
//    }
//}
