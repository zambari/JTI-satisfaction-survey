using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    [DisallowMultipleComponent]
    public abstract class AbstractContent<T> : AbstractContent, IContent where T : BaseData, new()
    {
        [SerializeField, ConditionalHide("usesRegistry", false, true)]
        private string id;
        [SerializeField, HideInInspector]
        private string oid;
        [SerializeField, HideInInspector]
        private bool usesRegistry;
        [SerializeField, HideInInspector]
        private bool isCopy;
        [SerializeField, HideInInspector]
        private bool isClone;

        [SerializeField]
        private bool export = true;

        public ContentHandler ContentHandler
        {
            get { return GetComponentInParent<ContentHandler>(); }
        }

        public ContentHelpers ContentHelpers
        {
            get { return ContentHandler.GetComponentInChildren<ContentHelpers>(); }
        }

        private T data;
        public T Data
        {
            get
            {
                if (data == null)
                    data = new T();

                return data;
            }
            set
            {
                data = value;
            }
        }

        #region IContent implementation
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        public string OrginalID
        {
            get { return oid; }
        }

        BaseData IContent.Data
        {
            get { return (BaseData)Data; }
            set { Data = value as T; }
        }

        public bool UsesRegistry
        {
            get { return usesRegistry; }
            set { usesRegistry = value; }
        }

        public bool IsCopy
        {
            get { return isCopy; }
            set { isCopy = value; }
        }

        public bool IsClone
        {
            get { return isClone; }
            set { isClone = value; }
        }

        public bool ShouldExport
        {
            get { return export; }
            set { export = value; }
        }

        private void TryFetchRegistryID()
        {
            var registryComponent = GetComponent<ContentRegisteredID>();
            if (registryComponent && !IsCopy && usesRegistry)
                id = registryComponent.ID;
        }

        protected virtual void Awake()
        {

        }

        protected virtual void Reset()
        {
            var registryComponent = GetComponent<ContentRegisteredID>();
            usesRegistry = registryComponent && !IsCopy && !string.IsNullOrEmpty(registryComponent.ID);
        }

        public virtual void OnExportData()
        {
            TryFetchRegistryID();
            Data.id = id;
            Data.objectName = this.name;
            Data.type = Data.GetType().ToString();
            Data.isClone = isClone;

            if (!IsCopy)
            {
                Data.originalID = id;
            }

            var parentCommponent = Utilities.GetComponentInParent<IContentParent>(transform);
            Data.parentID = (parentCommponent != null) ? parentCommponent.ID : "-1";
        }

        public virtual void OnInitialize()
        {
            TryFetchRegistryID();
            if (string.IsNullOrEmpty(oid))
                oid = id;
        }

        public virtual void OnImportData()
        {
            if (!Data.isActive)
            {
                this.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(Data.alias))
            {
                this.name = Data.alias;
            }
            else if (!string.IsNullOrEmpty(Data.objectName))
            {
                this.name = Data.objectName;
            }
        }

        public virtual void OnLanguageChanged(int languageId)
        {

        }

        public virtual void OnContentChanged()
        {

        }
        #endregion
    }

    public abstract class AbstractContent : MonoBehaviour
    {

    }
}