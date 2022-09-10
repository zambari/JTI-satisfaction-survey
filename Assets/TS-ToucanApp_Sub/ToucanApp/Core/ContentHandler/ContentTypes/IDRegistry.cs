using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ToucanApp.Data
{
    public class IDRegistry : ScriptableObject
    {
        public int id;
        public string[] storedIds;

        private void Awake()
        {
            RefreshID();
        }

        public void RefreshID()
        {
#if UNITY_EDITOR
            Debug.Log("Registry awaken");
            int hash = GetFileGuidHash();
            if (id != hash)
                id = hash;
#endif
        }

#if UNITY_EDITOR
        private int GetFileGuidHash()
        {
            string guid;
            long file;

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out guid, out file))
            {
                return guid.GetHashCode();
            }

            return -1;
        }
#endif

        public string GetID(int idx)
        {
            if (idx > -1 && idx < storedIds.Length)
                return string.Format("{0}_{1}", id, storedIds[idx]);

            return null;
        }
    }
}
