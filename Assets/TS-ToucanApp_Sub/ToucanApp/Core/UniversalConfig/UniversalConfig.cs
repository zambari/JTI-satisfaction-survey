using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ToucanApp.Config
{
    public class UniversalConfig : MonoBehaviour
    {
        #region Variables
        public enum PathRelativeTo
        {
            Data,
            StreammingAssets
        }

        [Header("Settings")]
        public PathRelativeTo pathRelativeTo = PathRelativeTo.Data;

        [Tooltip(@"It can be empty. Use it if you want to e.g. create subfolder. (example: aaa\bbb\ccc)")]
        public string additionalPath = "";

        public string fileName = "Config.txt";

        public bool loadOnAwake = true;

        public bool saveOnQuit = false;

        [Header("Main Event")]
        [Tooltip("Force event to be invoked even if no data was read or there's no config file.")]
        public bool alwaysInvokeEvent = false;

        public StandardEvent OnConfigLoaded;

        [Header("Config Elements")]
        public List<BoolConfigElement> boolElements = new List<BoolConfigElement>();

        public List<StringConfigElement> stringElements = new List<StringConfigElement>();

        public List<IntConfigElement> intElements = new List<IntConfigElement>();

        public List<FloatConfigElement> floatElements = new List<FloatConfigElement>();

        public List<Vector2ConfigElement> vector2Elements = new List<Vector2ConfigElement>();

        public List<Vector3ConfigElement> vector3Elements = new List<Vector3ConfigElement>();

        public List<ColorConfigElement> colorElements = new List<ColorConfigElement>();

        private string path = "";
        private string fullPath = "";
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (loadOnAwake)
                LoadConfig();
        }

        private void OnApplicationQuit()
        {
            if (saveOnQuit)
                SaveConfig();
        }
        #endregion

        #region Functions
        private void CreateFullPath()
        {
            try
            {
                if (!Application.isMobilePlatform || Application.isEditor)
                {
                    if (pathRelativeTo == PathRelativeTo.Data)
                        path = Path.Combine(Directory.GetCurrentDirectory(), additionalPath);
                    else if (pathRelativeTo == PathRelativeTo.StreammingAssets)
                        path = Path.Combine(Application.streamingAssetsPath, additionalPath);
                }
                else
                {
                    path = Path.Combine(Application.persistentDataPath, additionalPath);
                }

                fullPath = Path.Combine(path, fileName);
            }
            catch
            {
                Debug.LogError("Cannot create path from specified elements.");
                path = "";
                fullPath = "";
            }
        }

        [ContextMenu("Load Config")]
        public void LoadConfig()
        {
            CreateFullPath();

            if (string.IsNullOrEmpty(fullPath))
            {
                if (alwaysInvokeEvent)
                    OnConfigLoaded.Invoke();

                return;
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("Cannot load config file: specified file doesn't exist.");

                if (alwaysInvokeEvent)
                    OnConfigLoaded.Invoke();

                return;
            }

            string[] readLines = File.ReadAllLines(fullPath, Encoding.UTF8);
            string tempDescription = "";

            try
            {
                for (int i = 0; i < readLines.Length; i++)
                {
                    string currentString = readLines[i].Trim();

                    if (string.IsNullOrEmpty(currentString))
                    {
                        continue;
                    }
                    else if (currentString.StartsWith(@"//"))
                    {
                        tempDescription = currentString.Remove(0, 2);
                    }
                    else if (currentString.StartsWith("(bool)"))
                    {
                        string[] substrings = currentString.Split('=');
                        string variableName = substrings[0].Remove(0, 7).Trim();
                        bool variableFound = false;

                        foreach (BoolConfigElement entry in boolElements)
                        {
                            if (variableName == entry.name)
                            {
                                try
                                {
                                    bool variableValue = Convert.ToBoolean(substrings[1].Trim());
                                    entry.value = variableValue;

                                    if (!string.IsNullOrEmpty(tempDescription))
                                        entry.optionalDescription = tempDescription;
                                }
                                catch
                                {
                                    Debug.LogWarning("Error reading (" + variableName + ") value. Using default value.");
                                }

                                entry.loaded = true;
                                entry.OnElementLoaded.Invoke(entry.value);
                                variableFound = true;
                                break;
                            }
                        }

                        tempDescription = "";

                        if (!variableFound)
                            Debug.LogWarning("Config file contains variable (" + variableName + "), which isn't declared inside application.");
                    }
                    else if (currentString.StartsWith("(string)"))
                    {
                        string[] substrings = currentString.Split('=');
                        string variableName = substrings[0].Remove(0, 9).Trim();
                        bool variableFound = false;

                        foreach (StringConfigElement entry in stringElements)
                        {
                            if (variableName == entry.name)
                            {
                                try
                                {
                                    entry.value = substrings[1].Trim();

                                    if (!string.IsNullOrEmpty(tempDescription))
                                        entry.optionalDescription = tempDescription;
                                }
                                catch
                                {
                                    Debug.LogWarning("Error reading (" + variableName + ") value. Using default value.");
                                }

                                entry.loaded = true;
                                entry.OnElementLoaded.Invoke(entry.value);
                                variableFound = true;
                                break;
                            }
                        }

                        tempDescription = "";

                        if (!variableFound)
                            Debug.LogWarning("Config file contains variable (" + variableName + "), which isn't declared inside application.");
                    }
                    else if (currentString.StartsWith("(int)"))
                    {
                        string[] substrings = currentString.Split('=');
                        string variableName = substrings[0].Remove(0, 6).Trim();
                        bool variableFound = false;

                        foreach (IntConfigElement entry in intElements)
                        {
                            if (variableName == entry.name)
                            {
                                try
                                {
                                    int variableValue = Convert.ToInt32(substrings[1].Trim());
                                    entry.value = variableValue;

                                    if (!string.IsNullOrEmpty(tempDescription))
                                        entry.optionalDescription = tempDescription;
                                }
                                catch
                                {
                                    Debug.LogWarning("Error reading (" + variableName + ") value. Using default value.");
                                }

                                entry.loaded = true;
                                entry.OnElementLoaded.Invoke(entry.value);
                                variableFound = true;
                                break;
                            }
                        }

                        tempDescription = "";

                        if (!variableFound)
                            Debug.LogWarning("Config file contains variable (" + variableName + "), which isn't declared inside application.");
                    }
                    else if (currentString.StartsWith("(float)"))
                    {
                        string[] substrings = currentString.Split('=');
                        string variableName = substrings[0].Remove(0, 8).Trim();
                        bool variableFound = false;

                        foreach (FloatConfigElement entry in floatElements)
                        {
                            if (variableName == entry.name)
                            {
                                try
                                {
                                    float variableValue = Convert.ToSingle(substrings[1].Trim(), CultureInfo.InvariantCulture);
                                    entry.value = variableValue;

                                    if (!string.IsNullOrEmpty(tempDescription))
                                        entry.optionalDescription = tempDescription;
                                }
                                catch
                                {
                                    Debug.LogWarning("Error reading (" + variableName + ") value. Using default value.");
                                }

                                entry.loaded = true;
                                entry.OnElementLoaded.Invoke(entry.value);
                                variableFound = true;
                                break;
                            }
                        }

                        tempDescription = "";

                        if (!variableFound)
                            Debug.LogWarning("Config file contains variable (" + variableName + "), which isn't declared inside application.");
                    }
                    else if (currentString.StartsWith("(Vector2)"))
                    {
                        string[] substrings = currentString.Split('=');
                        string variableName = substrings[0].Remove(0, 10).Trim();
                        bool variableFound = false;

                        foreach (Vector2ConfigElement entry in vector2Elements)
                        {
                            if (variableName == entry.name)
                            {
                                try
                                {
                                    Vector2 variableValue = UniversalConfigHelpers.StringToVector2(substrings[1].Trim());
                                    entry.value = variableValue;

                                    if (!string.IsNullOrEmpty(tempDescription))
                                        entry.optionalDescription = tempDescription;
                                }
                                catch
                                {
                                    Debug.LogWarning("Error reading (" + variableName + ") value. Using default value.");
                                }

                                entry.loaded = true;
                                entry.OnElementLoaded.Invoke(entry.value);
                                variableFound = true;
                                break;
                            }
                        }

                        tempDescription = "";

                        if (!variableFound)
                            Debug.LogWarning("Config file contains variable (" + variableName + "), which isn't declared inside application.");
                    }
                    else if (currentString.StartsWith("(Vector3)"))
                    {
                        string[] substrings = currentString.Split('=');
                        string variableName = substrings[0].Remove(0, 10).Trim();
                        bool variableFound = false;

                        foreach (Vector3ConfigElement entry in vector3Elements)
                        {
                            if (variableName == entry.name)
                            {
                                try
                                {
                                    Vector3 variableValue = UniversalConfigHelpers.StringToVector3(substrings[1].Trim());
                                    entry.value = variableValue;

                                    if (!string.IsNullOrEmpty(tempDescription))
                                        entry.optionalDescription = tempDescription;
                                }
                                catch
                                {
                                    Debug.LogWarning("Error reading (" + variableName + ") value. Using default value.");
                                }

                                entry.loaded = true;
                                entry.OnElementLoaded.Invoke(entry.value);
                                variableFound = true;
                                break;
                            }
                        }

                        tempDescription = "";

                        if (!variableFound)
                            Debug.LogWarning("Config file contains variable (" + variableName + "), which isn't declared inside application.");
                    }
                    else if (currentString.StartsWith("(Color)"))
                    {
                        string[] substrings = currentString.Split('=');
                        string variableName = substrings[0].Remove(0, 8).Trim();
                        bool variableFound = false;

                        foreach (ColorConfigElement entry in colorElements)
                        {
                            if (variableName == entry.name)
                            {
                                try
                                {
                                    Color variableValue = UniversalConfigHelpers.StringToColor(substrings[1].Trim());
                                    entry.value = variableValue;

                                    if (!string.IsNullOrEmpty(tempDescription))
                                        entry.optionalDescription = tempDescription;
                                }
                                catch
                                {
                                    Debug.LogWarning("Error reading (" + variableName + ") value. Using default value.");
                                }

                                entry.loaded = true;
                                entry.OnElementLoaded.Invoke(entry.value);
                                variableFound = true;
                                break;
                            }
                        }

                        tempDescription = "";

                        if (!variableFound)
                            Debug.LogWarning("Config file contains variable (" + variableName + "), which isn't declared inside application.");
                    }
                    else
                    {
                        tempDescription = "";
                        Debug.LogWarning("Unknown line in config file. (line " + (i + 1).ToString() + ")");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Reading config file failure: " + e.Message);
            }

            OnConfigLoaded.Invoke();
            Debug.Log("Config (" + fullPath + ") loaded.");
        }

        [ContextMenu("Save Config")]
        public void SaveConfig()
        {
            CreateFullPath();

            if (string.IsNullOrEmpty(fullPath))
            {
                Debug.LogError("Error while saving config file.");
                return;
            }

            List<string> linesToSave = new List<string>();

            foreach (BoolConfigElement entry in boolElements)
            {
                if (!string.IsNullOrEmpty(entry.optionalDescription))
                    linesToSave.Add(@"// " + Regex.Replace(entry.optionalDescription, @"\r\n?|\n", " "));

                string currentLine = "(bool) " + entry.name + " = " + entry.value.ToString();
                linesToSave.Add(currentLine);
                linesToSave.Add("");
            }

            foreach (StringConfigElement entry in stringElements)
            {
                if (!string.IsNullOrEmpty(entry.optionalDescription))
                    linesToSave.Add(@"// " + Regex.Replace(entry.optionalDescription, @"\r\n?|\n", " "));

                string currentLine = "(string) " + entry.name + " = " + entry.value;
                linesToSave.Add(currentLine);
                linesToSave.Add("");
            }

            foreach (IntConfigElement entry in intElements)
            {
                if (!string.IsNullOrEmpty(entry.optionalDescription))
                    linesToSave.Add(@"// " + Regex.Replace(entry.optionalDescription, @"\r\n?|\n", " "));

                string currentLine = "(int) " + entry.name + " = " + entry.value.ToString();
                linesToSave.Add(currentLine);
                linesToSave.Add("");
            }

            foreach (FloatConfigElement entry in floatElements)
            {
                if (!string.IsNullOrEmpty(entry.optionalDescription))
                    linesToSave.Add(@"// " + Regex.Replace(entry.optionalDescription, @"\r\n?|\n", " "));

                string currentLine = "(float) " + entry.name + " = " + entry.value.ToString("0.###");
                linesToSave.Add(currentLine);
                linesToSave.Add("");
            }

            foreach (Vector2ConfigElement entry in vector2Elements)
            {
                if (!string.IsNullOrEmpty(entry.optionalDescription))
                    linesToSave.Add(@"// " + Regex.Replace(entry.optionalDescription, @"\r\n?|\n", " "));

                string currentLine = "(Vector2) " + entry.name + " = " + UniversalConfigHelpers.Vector2ToString(entry.value);
                linesToSave.Add(currentLine);
                linesToSave.Add("");
            }

            foreach (Vector3ConfigElement entry in vector3Elements)
            {
                if (!string.IsNullOrEmpty(entry.optionalDescription))
                    linesToSave.Add(@"// " + Regex.Replace(entry.optionalDescription, @"\r\n?|\n", " "));

                string currentLine = "(Vector3) " + entry.name + " = " + UniversalConfigHelpers.Vector3ToString(entry.value);
                linesToSave.Add(currentLine);
                linesToSave.Add("");
            }

            foreach (ColorConfigElement entry in colorElements)
            {
                if (!string.IsNullOrEmpty(entry.optionalDescription))
                    linesToSave.Add(@"// " + Regex.Replace(entry.optionalDescription, @"\r\n?|\n", " "));

                string currentLine = "(Color) " + entry.name + " = " + UniversalConfigHelpers.ColorToString(entry.value);
                linesToSave.Add(currentLine);
                linesToSave.Add("");
            }

            try
            {
                if (linesToSave.Count > 0)
                {
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    File.WriteAllLines(fullPath, linesToSave.ToArray(), Encoding.UTF8);
                    Debug.Log("Config (" + fullPath + ") saved.");
                }
                else
                {
                    Debug.LogWarning("Config is empty and it won't be saved.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to save config: " + e.Message);
            }
        }

        public bool GetConfigElement<T>(string name, ref T elementToOverride)
        {
            Type type = typeof(T);

            if (type == typeof(BoolConfigElement))
            {
                foreach (BoolConfigElement entry in boolElements)
                {
                    if (entry.name == name)
                    {
                        elementToOverride = (T)Convert.ChangeType(entry, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified element.");
                return false;
            }
            else if (type == typeof(StringConfigElement))
            {
                foreach (StringConfigElement entry in stringElements)
                {
                    if (entry.name == name)
                    {
                        elementToOverride = (T)Convert.ChangeType(entry, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified element.");
                return false;
            }
            else if (type == typeof(IntConfigElement))
            {
                foreach (IntConfigElement entry in intElements)
                {
                    if (entry.name == name)
                    {
                        elementToOverride = (T)Convert.ChangeType(entry, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified element.");
                return false;
            }
            else if (type == typeof(FloatConfigElement))
            {
                foreach (FloatConfigElement entry in floatElements)
                {
                    if (entry.name == name)
                    {
                        elementToOverride = (T)Convert.ChangeType(entry, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified element.");
                return false;
            }
            else if (type == typeof(Vector2ConfigElement))
            {
                foreach (Vector2ConfigElement entry in vector2Elements)
                {
                    if (entry.name == name)
                    {
                        elementToOverride = (T)Convert.ChangeType(entry, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified element.");
                return false;
            }
            else if (type == typeof(Vector3ConfigElement))
            {
                foreach (Vector3ConfigElement entry in vector3Elements)
                {
                    if (entry.name == name)
                    {
                        elementToOverride = (T)Convert.ChangeType(entry, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified element.");
                return false;
            }
            else if (type == typeof(ColorConfigElement))
            {
                foreach (ColorConfigElement entry in colorElements)
                {
                    if (entry.name == name)
                    {
                        elementToOverride = (T)Convert.ChangeType(entry, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified element.");
                return false;
            }
            else
            {
                Debug.LogError("Specified type is not supported.");
                return false;
            }
        }

        public bool GetConfigValue<T>(string name, ref T variableToOverride)
        {
            Type type = typeof(T);

            if (type == typeof(bool))
            {
                foreach (BoolConfigElement entry in boolElements)
                {
                    if (entry.name == name)
                    {
                        variableToOverride = (T)Convert.ChangeType(entry.value, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(string))
            {
                foreach (StringConfigElement entry in stringElements)
                {
                    if (entry.name == name)
                    {
                        variableToOverride = (T)Convert.ChangeType(entry.value, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(int))
            {
                foreach (IntConfigElement entry in intElements)
                {
                    if (entry.name == name)
                    {
                        variableToOverride = (T)Convert.ChangeType(entry.value, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(float))
            {
                foreach (FloatConfigElement entry in floatElements)
                {
                    if (entry.name == name)
                    {
                        variableToOverride = (T)Convert.ChangeType(entry.value, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(Vector2))
            {
                foreach (Vector2ConfigElement entry in vector2Elements)
                {
                    if (entry.name == name)
                    {
                        variableToOverride = (T)Convert.ChangeType(entry.value, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(Vector3))
            {
                foreach (Vector3ConfigElement entry in vector3Elements)
                {
                    if (entry.name == name)
                    {
                        variableToOverride = (T)Convert.ChangeType(entry.value, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(Color))
            {
                foreach (ColorConfigElement entry in colorElements)
                {
                    if (entry.name == name)
                    {
                        variableToOverride = (T)Convert.ChangeType(entry.value, type);
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else
            {
                Debug.LogError("Specified type is not supported.");
                return false;
            }
        }

        public bool SetConfigValue<T>(string name, T newValue, string newOptionalDescription = "")
        {
            Type type = typeof(T);

            if (type == typeof(bool))
            {
                foreach (BoolConfigElement entry in boolElements)
                {
                    if (entry.name == name)
                    {
                        entry.value = (bool)Convert.ChangeType(newValue, type);
                        entry.optionalDescription = newOptionalDescription;
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(string))
            {
                foreach (StringConfigElement entry in stringElements)
                {
                    if (entry.name == name)
                    {
                        entry.value = (string)Convert.ChangeType(newValue, type);
                        entry.optionalDescription = newOptionalDescription;
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(int))
            {
                foreach (IntConfigElement entry in intElements)
                {
                    if (entry.name == name)
                    {
                        entry.value = (int)Convert.ChangeType(newValue, type);
                        entry.optionalDescription = newOptionalDescription;
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(float))
            {
                foreach (FloatConfigElement entry in floatElements)
                {
                    if (entry.name == name)
                    {
                        entry.value = (float)Convert.ChangeType(newValue, type);
                        entry.optionalDescription = newOptionalDescription;
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(Vector2))
            {
                foreach (Vector2ConfigElement entry in vector2Elements)
                {
                    if (entry.name == name)
                    {
                        entry.value = (Vector2)Convert.ChangeType(newValue, type);
                        entry.optionalDescription = newOptionalDescription;
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(Vector3))
            {
                foreach (Vector3ConfigElement entry in vector3Elements)
                {
                    if (entry.name == name)
                    {
                        entry.value = (Vector3)Convert.ChangeType(newValue, type);
                        entry.optionalDescription = newOptionalDescription;
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else if (type == typeof(Color))
            {
                foreach (ColorConfigElement entry in colorElements)
                {
                    if (entry.name == name)
                    {
                        entry.value = (Color)Convert.ChangeType(newValue, type);
                        entry.optionalDescription = newOptionalDescription;
                        return true;
                    }
                }

                Debug.LogWarning("Cannot find specified variable.");
                return false;
            }
            else
            {
                Debug.LogError("Specified type is not supported.");
                return false;
            }
        }

        public bool AddNewConfigValue<T>(string newName, T newValue, string newOptionalDescription = "", Action<T> onLoadAction = null)
        {
            Type type = typeof(T);

            if (type == typeof(bool))
            {
                foreach (BoolConfigElement entry in boolElements)
                {
                    if (entry.name == newName)
                    {
                        Debug.LogWarning("Specified variable is already in config.");
                        return false;
                    }
                }

                BoolConfigElement newElement = new BoolConfigElement()
                {
                    name = newName,
                    optionalDescription = newOptionalDescription,
                    value = (bool)Convert.ChangeType(newValue, type),
                    OnElementLoaded = new BoolEvent()
                };

                if (onLoadAction != null)
                    newElement.OnElementLoaded.AddListener((x) => onLoadAction((T)Convert.ChangeType(newElement.value, type)));

                boolElements.Add(newElement);

                return true;
            }
            else if (type == typeof(string))
            {
                foreach (StringConfigElement entry in stringElements)
                {
                    if (entry.name == newName)
                    {
                        Debug.LogWarning("Specified variable is already in config.");
                        return false;
                    }
                }

                StringConfigElement newElement = new StringConfigElement()
                {
                    name = newName,
                    optionalDescription = newOptionalDescription,
                    value = (string)Convert.ChangeType(newValue, type),
                    OnElementLoaded = new StringEvent()
                };

                if (onLoadAction != null)
                    newElement.OnElementLoaded.AddListener((x) => onLoadAction((T)Convert.ChangeType(newElement.value, type)));

                stringElements.Add(newElement);

                return true;
            }
            else if (type == typeof(int))
            {
                foreach (IntConfigElement entry in intElements)
                {
                    if (entry.name == newName)
                    {
                        Debug.LogWarning("Specified variable is already in config.");
                        return false;
                    }
                }

                IntConfigElement newElement = new IntConfigElement()
                {
                    name = newName,
                    optionalDescription = newOptionalDescription,
                    value = (int)Convert.ChangeType(newValue, type),
                    OnElementLoaded = new IntEvent()
                };

                if (onLoadAction != null)
                    newElement.OnElementLoaded.AddListener((x) => onLoadAction((T)Convert.ChangeType(newElement.value, type)));

                intElements.Add(newElement);

                return true;
            }
            else if (type == typeof(float))
            {
                foreach (FloatConfigElement entry in floatElements)
                {
                    if (entry.name == newName)
                    {
                        Debug.LogWarning("Specified variable is already in config.");
                        return false;
                    }
                }

                FloatConfigElement newElement = new FloatConfigElement()
                {
                    name = newName,
                    optionalDescription = newOptionalDescription,
                    value = (float)Convert.ChangeType(newValue, type),
                    OnElementLoaded = new FloatEvent()
                };

                if (onLoadAction != null)
                    newElement.OnElementLoaded.AddListener((x) => onLoadAction((T)Convert.ChangeType(newElement.value, type)));

                floatElements.Add(newElement);

                return true;
            }
            else if (type == typeof(Vector2))
            {
                foreach (Vector2ConfigElement entry in vector2Elements)
                {
                    if (entry.name == newName)
                    {
                        Debug.LogWarning("Specified variable is already in config.");
                        return false;
                    }
                }

                Vector2ConfigElement newElement = new Vector2ConfigElement()
                {
                    name = newName,
                    optionalDescription = newOptionalDescription,
                    value = (Vector2)Convert.ChangeType(newValue, type),
                    OnElementLoaded = new Vector2Event()
                };

                if (onLoadAction != null)
                    newElement.OnElementLoaded.AddListener((x) => onLoadAction((T)Convert.ChangeType(newElement.value, type)));

                vector2Elements.Add(newElement);

                return true;
            }
            else if (type == typeof(Vector3))
            {
                foreach (Vector3ConfigElement entry in vector3Elements)
                {
                    if (entry.name == newName)
                    {
                        Debug.LogWarning("Specified variable is already in config.");
                        return false;
                    }
                }

                Vector3ConfigElement newElement = new Vector3ConfigElement()
                {
                    name = newName,
                    optionalDescription = newOptionalDescription,
                    value = (Vector3)Convert.ChangeType(newValue, type),
                    OnElementLoaded = new Vector3Event()
                };

                if (onLoadAction != null)
                    newElement.OnElementLoaded.AddListener((x) => onLoadAction((T)Convert.ChangeType(newElement.value, type)));

                vector3Elements.Add(newElement);

                return true;
            }
            else if (type == typeof(Color))
            {
                foreach (ColorConfigElement entry in colorElements)
                {
                    if (entry.name == newName)
                    {
                        Debug.LogWarning("Specified variable is already in config.");
                        return false;
                    }
                }

                ColorConfigElement newElement = new ColorConfigElement()
                {
                    name = newName,
                    optionalDescription = newOptionalDescription,
                    value = (Color)Convert.ChangeType(newValue, type),
                    OnElementLoaded = new ColorEvent()
                };

                if (onLoadAction != null)
                    newElement.OnElementLoaded.AddListener((x) => onLoadAction((T)Convert.ChangeType(newElement.value, type)));

                colorElements.Add(newElement);

                return true;
            }
            else
            {
                Debug.LogError("Specified type is not supported.");
                return false;
            }
        }

        public bool DeleteConfigValue<T>(string name)
        {
            Type type = typeof(T);

            if (type == typeof(bool))
            {
                BoolConfigElement temp = null;
                foreach (BoolConfigElement entry in boolElements)
                {
                    if (entry.name == name)
                    {
                        temp = entry;
                        break;
                    }
                }

                if (temp != null)
                {
                    boolElements.Remove(temp);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot delete specified variable. Item not found.");
                    return false;
                }
            }
            else if (type == typeof(string))
            {
                StringConfigElement temp = null;
                foreach (StringConfigElement entry in stringElements)
                {
                    if (entry.name == name)
                    {
                        temp = entry;
                        break;
                    }
                }

                if (temp != null)
                {
                    stringElements.Remove(temp);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot delete specified variable. Item not found.");
                    return false;
                }
            }
            else if (type == typeof(int))
            {
                IntConfigElement temp = null;
                foreach (IntConfigElement entry in intElements)
                {
                    if (entry.name == name)
                    {
                        temp = entry;
                        break;
                    }
                }

                if (temp != null)
                {
                    intElements.Remove(temp);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot delete specified variable. Item not found.");
                    return false;
                }
            }
            else if (type == typeof(float))
            {
                FloatConfigElement temp = null;
                foreach (FloatConfigElement entry in floatElements)
                {
                    if (entry.name == name)
                    {
                        temp = entry;
                        break;
                    }
                }

                if (temp != null)
                {
                    floatElements.Remove(temp);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot delete specified variable. Item not found.");
                    return false;
                }
            }
            else if (type == typeof(Vector2))
            {
                Vector2ConfigElement temp = null;
                foreach (Vector2ConfigElement entry in vector2Elements)
                {
                    if (entry.name == name)
                    {
                        temp = entry;
                        break;
                    }
                }

                if (temp != null)
                {
                    vector2Elements.Remove(temp);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot delete specified variable. Item not found.");
                    return false;
                }
            }
            else if (type == typeof(Vector3))
            {
                Vector3ConfigElement temp = null;
                foreach (Vector3ConfigElement entry in vector3Elements)
                {
                    if (entry.name == name)
                    {
                        temp = entry;
                        break;
                    }
                }

                if (temp != null)
                {
                    vector3Elements.Remove(temp);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot delete specified variable. Item not found.");
                    return false;
                }
            }
            else if (type == typeof(Color))
            {
                ColorConfigElement temp = null;
                foreach (ColorConfigElement entry in colorElements)
                {
                    if (entry.name == name)
                    {
                        temp = entry;
                        break;
                    }
                }

                if (temp != null)
                {
                    colorElements.Remove(temp);
                    return true;
                }
                else
                {
                    Debug.LogWarning("Cannot delete specified variable. Item not found.");
                    return false;
                }
            }
            else
            {
                Debug.LogError("Specified type is not supported.");
                return false;
            }
        }
        #endregion

        #region PostBuild
#if UNITY_EDITOR
        [UnityEditor.Callbacks.PostProcessBuildAttribute()]
        public static void OnPostprocessBuild(UnityEditor.BuildTarget target, string pathToBuiltProject)
        {
            string projectFullPath = Path.Combine(Directory.GetCurrentDirectory(), "Config.txt");
            string buildFileFullPath = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), "Config.txt");

            if (!File.Exists(projectFullPath) || File.Exists(buildFileFullPath))
                return;

            File.Copy(projectFullPath, buildFileFullPath);
            Debug.Log("Config copied to build location. (" + buildFileFullPath + ")");
        }
#endif
        #endregion
    }
}