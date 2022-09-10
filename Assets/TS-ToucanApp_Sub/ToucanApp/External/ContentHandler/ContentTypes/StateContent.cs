using UnityEngine;

namespace ToucanApp.Data
{
    public class StateContent : AbstractParentContent<HierarchyData>
    {
        public bool isPrototype;

        public override void OnContentChanged()
        {
            base.OnContentChanged();

            var contentInChildren = Utilities.GetAllComponentsInChildren<StateContent, StateContent>(transform);
            foreach (var content in contentInChildren)
            {
                var children = Data.PeekChildren<HierarchyData>();
                for (int i = 0; i < children.Length; i++)
                {
                    if ((children[i].isPrototype || children[i].isClone) && content.Data.id == children[i].id)
                    {
                        var monoContent = (MonoBehaviour)content;
                        monoContent.transform.SetSiblingIndex(i);
                    }
                }
            }
        }

        public override void OnImportData()
        {
            base.OnImportData();

            var contentHierarchy = Utilities.GetAllComponentsInChildren<StateContent, StateContent>(transform);
            for (int j = 0; j < contentHierarchy.Length; j++)
            {
                var childData = ContentHandler.CurrentBuilder.FindData(contentHierarchy[j].ID);
                if (childData != null)
                {
                    var childParentData = ContentHandler.CurrentBuilder.FindData(childData.parentID);
                    if (childParentData != null)
                    {
                        var dataHierarchy = childParentData.PeekChildren<HierarchyData>();
                        if (dataHierarchy != null)
                        {
                            for (int i = 0; i < dataHierarchy.Length; i++)
                            {
                                if (!clonedChildren.ContainsKey(dataHierarchy[i].id))
                                {
                                    if (contentHierarchy[j].ID == dataHierarchy[i].id)
                                    {
                                        clonedChildren.Add(dataHierarchy[i].id, null);
                                        continue;
                                    }

                                    if (contentHierarchy[j].OrginalID == dataHierarchy[i].originalID)
                                    {
                                        var stateCopy = (StateContent)ContentHandler.CloneContentBlock(dataHierarchy[i], contentHierarchy[j], contentHierarchy[j].transform.parent);
                                        
                                        stateCopy.isPrototype = false;
                                        stateCopy.IsClone = true;
                                        clonedChildren.Add(dataHierarchy[i].id, stateCopy);
                                        RemoveCopies(stateCopy.transform);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void RemoveCopies(Transform transform)
        {
            var contentFound = Utilities.GetAllComponentsInChildren<IContent>(transform);
            foreach (var content in contentFound)
            {
                if (content.IsClone)
                {
                    var mono = (MonoBehaviour)content;
                    mono.transform.SetParent(null);
                    Destroy(mono.gameObject);
                }
            }
        }

        public override void OnExportData()
        {
            base.OnExportData();

            var subState = GetComponent<ToucanApp.States.SubStateMachine>();
            Data.linkable = (subState != null);
            Data.isPrototype = isPrototype;
        }
    }
}
