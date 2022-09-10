using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.States
{
    public class AppMeshFadeEffect : AbstractTransitionEffect 
    {
        public enum MeshFadeDirection
        {
            FadeToDefault,
            FadeOut,
        }

        public class MaterialValues
        {
            public Material material;
            public float alpha;
            public Utilities.BlendMode mode;
            public int renderQueue;
        }

        private List<MaterialValues> _materialAwakeValues = new List<MaterialValues>();

        [SerializeField]
        private MeshRenderer[] _meshRenderers = null;

        [SerializeField]
        private SkinnedMeshRenderer[] _skinMeshRenderers = null;

        [SerializeField]
        private MeshFadeDirection _direction = MeshFadeDirection.FadeToDefault;

        [SerializeField]
        private float _duration = 1;

        [SerializeField]
        private Utilities.BlendMode _blendMode = Utilities.BlendMode.Fade;

        private const string MATERIAL_CLONE_NAME = "_MC_";

        private void Awake()
        {
            foreach (var mr in _meshRenderers)
                for (int i = 0; i < mr.sharedMaterials.Length; i++)
                {
                    if (mr.sharedMaterials[i].name != MATERIAL_CLONE_NAME)
                    {
                        mr.materials[i].name = MATERIAL_CLONE_NAME;
                    }

                    AddMaterials(mr.sharedMaterials[i]);
                }

            foreach (var mr in _skinMeshRenderers)
                for (int i = 0; i < mr.sharedMaterials.Length; i++)
                {
                    if (mr.sharedMaterials[i].name != MATERIAL_CLONE_NAME)
                    {
                        mr.materials[i].name = MATERIAL_CLONE_NAME;
                    }

                    AddMaterials(mr.sharedMaterials[i]);
                }
        }

        private void Start()
        {
            foreach (var values in _materialAwakeValues)
            {
                Utilities.SetupMaterialWithBlendMode(values.material, Utilities.BlendMode.Fade); 
                var t = values.material.color;
                t.a = 0;
                values.material.color = t;
            }
        }

        private void AddMaterials(Material m)
        {
            Utilities.BlendMode blendMode = Utilities.BlendMode.Transparent;

            if (m.HasProperty("_Mode"))
            {
                blendMode = (Utilities.BlendMode)m.GetFloat("_Mode");
            }

            _materialAwakeValues.Add(new MaterialValues {

                material = m,
                mode = blendMode,
                alpha = m.color.a,
                renderQueue = m.renderQueue
            });
                    
        }

        #region implemented abstract members of AbstractTransitionEffect
        public override IEnumerator LaunchStart()
        {
            foreach (var values in _materialAwakeValues)
            {
                Utilities.SetupMaterialWithBlendMode(values.material, _blendMode);
                values.material.renderQueue = values.renderQueue;
            }

            yield return StartCoroutine(Utilities.DelegateLerpDuration((float v) => {
                
                foreach (var values in _materialAwakeValues)
                {
                    var mat = values.material;
                    if ((_direction == MeshFadeDirection.FadeOut && mat.color.a > v * values.alpha) || (_direction == MeshFadeDirection.FadeToDefault& mat.color.a < v * values.alpha))
                    {
                        var t = mat.color;
                        t.a = v * values.alpha;
                        mat.color = t;
                    }
                }

            }, (Utilities.EffectDirection)_direction, _duration, _delay, _useTimeScale));

            foreach (var values in _materialAwakeValues)
            {
                if (_direction == MeshFadeDirection.FadeToDefault)
                    Utilities.SetupMaterialWithBlendMode(values.material, values.mode);
                values.material.renderQueue = values.renderQueue;
            }
        }
        #endregion
    }
}
