using UnityEngine;
using ToucanApp.Data;

namespace ToucanApp.Helpers
{
    [RequireComponent(typeof(RuntimeConsole))]
    [RequireComponent(typeof(VersionController))]
    public class RuntimeTools : MonoBehaviour 
    {
        public enum Corner
        {
            None,
            LeftUp,
            RightUp,
            LeftDown,
            RightDown
        }

        [SerializeField]
        private float guiDisplayDuration = 5;

        private RuntimeConsole console;
        private VersionController version;

        private float guiTime;
        private float holdTime;

        public void SetRuntimeToolsActive(bool state)
        {
            enabled = state;

            console.enabled = state;
            version.enabled = state;
        }

        private void Awake()
        {
            console = GetComponent<RuntimeConsole>();
            version = GetComponent<VersionController>();
        }

        private void Start()
        {
            console.ConsoleSize = new Vector2(Screen.width - 20 , Screen.height / 2);
        }

        public Corner GetCorner(Vector2 point)
        {
            if (point.x < 100 && point.y < 100)
                return Corner.LeftDown;

            if (point.x > Screen.width - 100 && point.y < 100)
                return Corner.RightDown;

            if (point.x < 100 && point.y > Screen.height - 100)
                return Corner.LeftUp;

            if (point.x > Screen.width - 100 && point.y > Screen.height - 100)
                return Corner.RightUp;

            return Corner.None;
        }
            
    	void Update () 
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (Input.GetKeyDown(KeyCode.D))
                {
                    ToggleDebug();
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    ToggleVersion();
                }
            }

            if (Input.GetMouseButton(0))
            {
                holdTime += Time.unscaledDeltaTime;

                if (holdTime > 3)
                {
                    if (GetCorner(Input.mousePosition) == Corner.LeftUp)
                    {
                        ToggleDebug();
                    }

                    if (GetCorner(Input.mousePosition) == Corner.RightUp)
                    {
                        ToggleVersion();
                    }

                    holdTime = 0;
                }
            }
            else
            {
                holdTime = 0;
            }
    	}

        private void OnGUI()
        {
            if (guiTime > 0)
            {
                GUI.Box(new Rect(10, 10, 100, 25), "Input locked!");
                guiTime -= Time.deltaTime;
            }
        }

        private void ShowMessage(string message)
        {
            guiTime = guiDisplayDuration;
        }

        public void ToggleDebug()
        {
            console.Active = !console.Active;
            if (console.Active)
                Debug.Log("Console Ready");
        }

        public void ToggleVersion()
        {
            version.Active = !version.Active;
        }
    }
}
