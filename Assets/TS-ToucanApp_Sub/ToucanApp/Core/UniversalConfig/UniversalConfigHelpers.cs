using System;
using System.Globalization;
using UnityEngine;

namespace ToucanApp.Config
{
    public static class UniversalConfigHelpers
    {
        public static string Vector2ToString(Vector2 value)
        {
            string result = "";
            result = value.x.ToString("0.###") + ", " + value.y.ToString("0.###");
            return result;
        }

        public static string Vector3ToString(Vector3 value)
        {
            string result = "";
            result = value.x.ToString("0.###") + ", " + value.y.ToString("0.###") + ", " + value.z.ToString("0.###");
            return result;
        }

        public static string ColorToString(Color value)
        {
            string result = "";
            result = value.r.ToString("0.###") + ", " + value.g.ToString("0.###") + ", " + value.b.ToString("0.###") + ", " + value.a.ToString("0.###");
            return result;
        }

        public static Vector2 StringToVector2(string value)
        {
            Vector2 result = Vector2.zero;

            try
            {
                string[] substrings = value.Split(',');
                result = new Vector2(Convert.ToSingle(substrings[0].Trim(), CultureInfo.InvariantCulture), 
                    Convert.ToSingle(substrings[1].Trim(), CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return result;
        }

        public static Vector3 StringToVector3(string value)
        {
            Vector3 result = Vector3.zero;

            try
            {
                string[] substrings = value.Split(',');
                result = new Vector3(Convert.ToSingle(substrings[0].Trim(), CultureInfo.InvariantCulture), 
                    Convert.ToSingle(substrings[1].Trim(), CultureInfo.InvariantCulture), 
                    Convert.ToSingle(substrings[2].Trim(), CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return result;
        }

        public static Color StringToColor(string value)
        {
            Color result = Color.white;

            try
            {
                string[] substrings = value.Split(',');
                result = new Color(Convert.ToSingle(substrings[0].Trim(), CultureInfo.InvariantCulture), 
                    Convert.ToSingle(substrings[1].Trim(), CultureInfo.InvariantCulture), 
                    Convert.ToSingle(substrings[2].Trim(), CultureInfo.InvariantCulture), 
                    Convert.ToSingle(substrings[3].Trim(), CultureInfo.InvariantCulture));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return result;
        }
    }
}