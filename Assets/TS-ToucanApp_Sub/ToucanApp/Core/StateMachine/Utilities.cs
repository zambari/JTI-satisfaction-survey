using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    public class Utilities 
    {      
        public static T GetParent<T>(Transform me)
        { 
            if (me.parent != null)
                return me.parent.GetComponentInParent<T>(); 

            return default(T);
        }

        public static U GetNextChild<T, U>(T parent, U current, bool rotate = false) where T : Component where U : Component
        {
			List<U> children;
            GetChildren<T, U>(parent, out children);

            var idx = children.IndexOf(current);
            if (idx == children.Count - 1)
            {
                if (rotate)
                    idx = 0;
                else
                    return default(U);
            }
            else
            {
                idx++;
            }

            return children[idx];
        }

        public static U GetPrevChild<T, U>(T parent, U current, bool rotate = false) where T : Component where U : Component
        {
			List<U> children;
            GetChildren<T, U>(parent, out children);

            var idx = children.IndexOf(current);
            if (idx == 0)
            {
                if (rotate)
                    idx = children.Count - 1;
                else
                    return default(U);
            }
            else
            {
                idx--;
            }

            return children[idx];
        }

        public static void GetChildren<T, U>(T parent, out List<U> children) where T : Component where U : Component
        {
			var allchildren = parent.GetComponentsInChildren<U>();

            children = new List<U>();

            foreach (var c in allchildren)
            {
                if (c == parent)
                    continue;

                var childParent = c.transform.parent.GetComponentInParent<T>();
                if (Equals(childParent, parent))
                {
                    children.Add(c);
                }
            }
        }

        public static int GetActiveSiebelingIdx(Transform transform)
        {
            int idx = 0;
            var parent = transform.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                var nextChild = parent.GetChild(i);
                if (nextChild.gameObject.activeSelf)
                {
                    if (transform == nextChild)
                        return idx;

                    idx++;
                }
            }

            return -1;
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent,
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            material.SetFloat("_Mode", (float)blendMode);

            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
            }
        }

        public enum EffectDirection
        {
            Forward,
            Backward,
        }

        public static IEnumerator DelegateLerpDuration(System.Action<float> action, EffectDirection direction = EffectDirection.Forward, float duration = 1, float delay = 0, bool useTimeScale = true)
        {
            action.Invoke((direction == EffectDirection.Forward) ? 0 : 1);

            if (delay > 0)
            {
                if (useTimeScale)
                    yield return new WaitForSeconds(delay);
                else
                    yield return new WaitForSecondsRealtime(delay);
            }

            for (float i = 0; i < 1; i += (useTimeScale? Time.deltaTime : Time.unscaledDeltaTime) / duration)
            {
                float val = (direction == EffectDirection.Forward) ? i : 1 - i;
                action.Invoke(val);
                yield return 0;
            }

            action.Invoke((direction == EffectDirection.Forward) ? 1 : 0);
        }

        public static IEnumerator DelegateLerpSpeed(System.Action<float> action, EffectDirection direction = EffectDirection.Forward, float speed = 1, float delay = 0, bool useTimeScale = true)
        {
            action.Invoke((direction == EffectDirection.Forward) ? 0 : 1);

            if (delay > 0)
            {
                if (useTimeScale)
                    yield return new WaitForSeconds(delay);
                else
                    yield return new WaitForSecondsRealtime(delay);
            }

            for (float i = 0; i < 1; i += (useTimeScale? Time.deltaTime : Time.unscaledDeltaTime) * speed)
            {
                float val = (direction == EffectDirection.Forward) ? i : 1 - i;
                action.Invoke(val);
                yield return 0;
            }

            action.Invoke((direction == EffectDirection.Forward) ? 1 : 0);
        }

        public static IEnumerator WaitExecute(System.Action action, float delay = 0, bool useTimeScale = true)
        {
            if (delay > 0)
            {
                if (useTimeScale)
                    yield return new WaitForSeconds(delay);
                else
                    yield return new WaitForSecondsRealtime(delay);
            }

            action.Invoke();
        }
    }
}