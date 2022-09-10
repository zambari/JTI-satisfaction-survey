using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Helpers
{
    public class RuntimeConsole : MonoBehaviour
    {
        #region Nested Enums
        public enum Corner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }
        #endregion

        #region Nested Classes
        private class LogInfo
        {
            public string message = "";
            public LogType logType = LogType.Log;
            public Color color = Color.white;
        }
        #endregion

        #region Variables
        [SerializeField]
        private bool active = false;
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        // required for config setup
        public void SetActive(bool state) => Active = state;

        [Header("Log Settings")]
        [SerializeField, Range(1, 100)]
        private int maxLogsCount = 10;
        public int MaxLogsCount
        {
            get { return maxLogsCount; }
            set { maxLogsCount = Mathf.Clamp(value, 1, 100); }
        }

        [SerializeField, Range(1, 40)]
        private float showDuration = 16;
        public float ShowDuration
        {
            get { return showDuration; }
            set { showDuration = Mathf.Clamp(value, 1, 40); }
        }

        [SerializeField]
        private bool logNormal = true;
        public bool LogNormal
        {
            get
            {
                return logNormal;
            }
            set
            {
                logNormal = value;
                CreateMessage();
            }
        }

        [SerializeField]
        private bool logWarning = true;
        public bool LogWarning
        {
            get
            {
                return logWarning;
            }
            set
            {
                logWarning = value;
                CreateMessage();
            }
        }

        [SerializeField]
        private bool logError = true;
        public bool LogError
        {
            get
            {
                return logError;
            }
            set
            {
                logError = value;
                CreateMessage();
            }
        }

        [SerializeField]
        private bool logAssertion = true;
        public bool LogAssertion
        {
            get
            {
                return logAssertion;
            }
            set
            {
                logAssertion = value;
                CreateMessage();
            }
        }

        [SerializeField]
        private bool logException = true;
        public bool LogException
        {
            get
            {
                return logException;
            }
            set
            {
                logException = value;
                CreateMessage();
            }
        }

        [Header("Layout Settings")]
        [SerializeField]
        private Corner consoleCorner = Corner.TopLeft;
        public Corner ConsoleCorner
        {
            get
            {
                return consoleCorner;
            }
            set
            {
                consoleCorner = value;
                GetLayout();
            }
        }

        [SerializeField]
        private Vector2 consoleSize = new Vector2(800, 600);
        public Vector2 ConsoleSize
        {
            get
            {
                return consoleSize;
            }
            set
            {
                consoleSize = value;
                GetLayout();
            }
        }

        [SerializeField, Tooltip("Corrects size of console if it's too big to fit in current screen (based on current resolution")]
        private bool clipSize = true;
        public bool ClipSize
        {
            get { return clipSize; }
            set { clipSize = value; }
        }

        [SerializeField, Range(4, 40)]
        private int fontSize = 12;
        public int FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                fontSize = Mathf.Clamp(value, 4, 40);
                GetLayout();
            }
        }

        private List<LogInfo> logHistory = new List<LogInfo>();
        private bool linked = false;
        private bool draw = false;
        private bool lastDrawStatus = false;
        private string wholeMessage = "";
        private Rect outsideBox;
        private Rect insideScroll;
        private GUIStyle textStyle;
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 lastScrollPosition = Vector2.zero;
        private Coroutine timerCor = null;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (!linked)
            {
                Application.logMessageReceived += LogReceived;
                linked = true;
            }
        }

        private void OnEnable()
        {
            if (!linked)
            {
                Application.logMessageReceived += LogReceived;
                linked = true;
            }

            GetLayout();
        }

        private void Update()
        {
            if (!active)
                return;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        EnableAllTypes();
                    }

                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        ShowConsole();
                    }

                    if (Input.GetKeyDown(KeyCode.A))
                    {
                        ShowDuration += 2;
                        Debug.Log("Current console show duration: " + showDuration);
                    }

                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        ShowDuration -= 2;
                        Debug.Log("Current console show duration: " + showDuration);
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (linked)
            {
                Application.logMessageReceived -= LogReceived;
                linked = false;
            }

            StopTimerCor();
            draw = false;
            logHistory.Clear();
        }

        private void OnValidate()
        {
            GetLayout();
        }

        private void OnGUI()
        {
            if (!active)
                return;

            if (draw)
            {
                GUIContent content = new GUIContent(wholeMessage);
                float height = textStyle.CalcHeight(content, insideScroll.width);

                GUI.Box(outsideBox, GUIContent.none);

                if (!lastDrawStatus)
                    scrollPosition = new Vector2(0, Screen.currentResolution.height);

                using (GUI.ScrollViewScope scrollScope = new GUI.ScrollViewScope(insideScroll, scrollPosition, new Rect(0, 0, insideScroll.width - 20, height)))
                {
                    scrollPosition = scrollScope.scrollPosition;
                    GUI.Label(new Rect(0, 0, insideScroll.width, height), content, textStyle);
                }

                if (GUI.Button(new Rect(insideScroll.position + new Vector2(0, insideScroll.height + 20), new Vector2(80, 30)), "Close"))
                    HideConsole();

                LogNormal = GUI.Toggle(new Rect(insideScroll.position + new Vector2(100, insideScroll.height + 24), new Vector2(80, 30)), logNormal, " Normal");
                LogWarning = GUI.Toggle(new Rect(insideScroll.position + new Vector2(200, insideScroll.height + 24), new Vector2(80, 30)), logWarning, " Warning");
                LogError = GUI.Toggle(new Rect(insideScroll.position + new Vector2(300, insideScroll.height + 24), new Vector2(80, 30)), logError, " Error");
                LogAssertion = GUI.Toggle(new Rect(insideScroll.position + new Vector2(400, insideScroll.height + 24), new Vector2(80, 30)), logAssertion, " Assertion");
                LogException = GUI.Toggle(new Rect(insideScroll.position + new Vector2(500, insideScroll.height + 24), new Vector2(80, 30)), logException, " Exception");
            }

            if (scrollPosition != lastScrollPosition)
            {
                StopTimerCor();
                timerCor = StartCoroutine(TimerCor());
                draw = true;
            }

            lastScrollPosition = scrollPosition;
            lastDrawStatus = draw;
        }
        #endregion

        #region Functions
        private void LogReceived(string log, string stackTrace, LogType logType)
        {
            bool breakAfterAdd = false;
            string type = "Unknown";
            Color logColor = Color.white;
            string colorName = "white";

            switch (logType)
            {
                case LogType.Log:
                    if (!logNormal)
                        breakAfterAdd = true;
                    type = "Log";
                    logColor = Color.white;
                    colorName = "white";
                    break;
                case LogType.Warning:
                    if (!logWarning)
                        breakAfterAdd = true;
                    type = "Warning";
                    logColor = Color.yellow;
                    colorName = "yellow";
                    break;
                case LogType.Error:
                    if (!logError)
                        breakAfterAdd = true;
                    type = "Error";
                    logColor = Color.red;
                    colorName = "red";
                    break;
                case LogType.Assert:
                    if (!logAssertion)
                        breakAfterAdd = true;
                    type = "Assertion";
                    logColor = Color.red;
                    colorName = "red";
                    break;
                case LogType.Exception:
                    if (!logException)
                        breakAfterAdd = true;
                    type = "Exception";
                    logColor = Color.red;
                    colorName = "red";
                    break;
            }

            string logMessage = "\n<color=" + colorName + ">" + "Type: " + type + " | " + DateTime.Now.ToString()
                + "\nMessage: " + log
                + "\nStack: " + stackTrace
                + "\n--------------------------------\n</color>";

            logHistory.Add(new LogInfo()
            {
                message = logMessage,
                logType = logType,
                color = logColor
            });

            while (logHistory.Count > 1000)
                logHistory.RemoveAt(0);

            CreateMessage();

            if (breakAfterAdd)
                return;

            StopTimerCor();
            timerCor = StartCoroutine(TimerCor());
            draw = true;
        }

        private void EnableAllTypes()
        {
            LogNormal = true;
            LogWarning = true;
            LogError = true;
            LogAssertion = true;
            LogException = true;
        }

        private void CreateMessage()
        {
            List<LogInfo> currentLogs = logHistory.Where(x =>
            {
                if (x.logType == LogType.Log && logNormal)
                    return true;
                if (x.logType == LogType.Warning && logWarning)
                    return true;
                if (x.logType == LogType.Error && logError)
                    return true;
                if (x.logType == LogType.Assert && logAssertion)
                    return true;
                if (x.logType == LogType.Exception && logException)
                    return true;

                return false;
            }).ToList();

            while (currentLogs.Count > maxLogsCount)
                currentLogs.RemoveAt(0);

            wholeMessage = "";
            for (int i = 0; i < currentLogs.Count; i++)
                wholeMessage += currentLogs[i].message;
        }

        private void GetLayout()
        {
            outsideBox = GetRect(false);
            insideScroll = GetRect(true);
            textStyle = new GUIStyle();
            textStyle.fontSize = fontSize;
            textStyle.richText = true;
            textStyle.wordWrap = true;
        }

        private Rect GetRect(bool inside)
        {
            Rect finalRect = new Rect();
            Vector2 position = new Vector2();
            Vector2 size = consoleSize;

            int screenWidth = Screen.currentResolution.width;
            int screenHeight = Screen.currentResolution.height;

            if (clipSize)
            {
                if (size.x > screenWidth - 20)
                    size = new Vector2(screenWidth - 20, size.y);

                if (size.y > screenHeight - 20)
                    size = new Vector2(size.x, screenHeight - 20);
            }

            switch (consoleCorner)
            {
                case Corner.TopLeft:
                    position = new Vector2(10, 10);
                    break;
                case Corner.TopRight:
                    position = new Vector2(screenWidth - size.x - 10, 10);
                    break;
                case Corner.BottomLeft:
                    position = new Vector2(10, screenHeight - size.y - 10);
                    break;
                case Corner.BottomRight:
                    position = new Vector2(screenWidth - size.x - 10, screenHeight - size.y - 10);
                    break;
            }

            finalRect = new Rect(position, size);

            if (inside)
                finalRect = new Rect(finalRect.x + 5, finalRect.y + 5, finalRect.width - 10, finalRect.height - 10 - 50);

            return finalRect;
        }

        public void ShowConsole()
        {
            if (!active)
                return;

            StopTimerCor();
            timerCor = StartCoroutine(TimerCor());
            draw = true;
        }

        public void HideConsole()
        {
            StopTimerCor();
            draw = false;
        }

        private void StopTimerCor()
        {
            if (timerCor != null)
            {
                StopCoroutine(timerCor);
                timerCor = null;
            }
        }

        private IEnumerator TimerCor()
        {
            yield return new WaitForSecondsRealtime(showDuration);
            draw = false;
            timerCor = null;
        }
        #endregion
    }
}