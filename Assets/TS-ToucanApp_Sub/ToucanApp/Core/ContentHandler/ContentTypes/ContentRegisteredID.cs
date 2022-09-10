using UnityEngine;

namespace ToucanApp.Data
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AbstractContent))]
    public class ContentRegisteredID : MonoBehaviour
    {
        public IDRegistry registry;

        [SerializeField]
        private int registryIndex = -1;

        public string ID
        {
            get { return registry != null ? registry.GetID(registryIndex) : null; }
        }

        public IContent Owner
        {
            get { return GetComponent<IContent>(); }
        }

        private void OnDestroy()
        {
            var owner = Owner;
            if (owner != null)
                owner.UsesRegistry = false;
        }

        private void OnValidate()
        {
            var owner = Owner;
            if (owner != null)
                owner.UsesRegistry = !string.IsNullOrEmpty(ID);
        }
    }
}