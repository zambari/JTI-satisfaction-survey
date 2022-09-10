using System;
using UnityEngine;

namespace ToucanApp.Config
{
    [Serializable]
    public class BaseConfigElement
    {
        public string name = "";
        [Multiline]
        public string optionalDescription = "";
        [HideInInspector]
        public bool loaded = false;
    }

    [Serializable]
    public class BoolConfigElement : BaseConfigElement
    {
        [HideInInspector]
        public bool value = false;
        public BoolEvent OnElementLoaded;
    }

    [Serializable]
    public class StringConfigElement : BaseConfigElement
    {
        [HideInInspector]
        public string value = "";
        public StringEvent OnElementLoaded;
    }

    [Serializable]
    public class IntConfigElement : BaseConfigElement
    {
        [HideInInspector]
        public int value = 0;
        public IntEvent OnElementLoaded;
    }

    [Serializable]
    public class FloatConfigElement : BaseConfigElement
    {
        [HideInInspector]
        public float value = 0;
        public FloatEvent OnElementLoaded;
    }

    [Serializable]
    public class Vector2ConfigElement : BaseConfigElement
    {
        [HideInInspector]
        public Vector2 value = Vector2.zero;
        public Vector2Event OnElementLoaded;
    }

    [Serializable]
    public class Vector3ConfigElement : BaseConfigElement
    {
        [HideInInspector]
        public Vector3 value = Vector3.zero;
        public Vector3Event OnElementLoaded;
    }

    [Serializable]
    public class ColorConfigElement : BaseConfigElement
    {
        [HideInInspector]
        public Color value = new Color(0, 0, 0, 0);
        public ColorEvent OnElementLoaded;
    }
}