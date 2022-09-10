using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    public abstract class AbstractParentContent<T> : AbstractContent<T>, IContentParent where T : BaseData, new()
    {
        protected Dictionary<string, MonoBehaviour> clonedChildren = new Dictionary<string, MonoBehaviour>();

        public override void OnExportData()
        {
            base.OnExportData();

            var allChildren = new List<BaseData>();
            var contentChildren = Utilities.GetAllComponentsInChildren<IContentParent, IContent>(transform);
            var dataChildren = Data.PeekChildren<BaseData>();

            foreach (var child in contentChildren)
            {
                if (child.ShouldExport)
                {
                    allChildren.Add(child.Data);
                }
            }

            if (dataChildren != null)
            {
                foreach (var child in dataChildren)
                {
                    allChildren.Add(child);
                }
            }

            Data.SetChildren(allChildren.ToArray());
        }

        #region IContentParent implementation
        public void ClearChildren()
        {
            foreach (var clone in clonedChildren.Values)
            {
                if (clone != null)
                {
                    clone.transform.SetParent(null);
                    Destroy(clone.gameObject);
                }
            }

            clonedChildren.Clear();

        }
        #endregion
    }
}
