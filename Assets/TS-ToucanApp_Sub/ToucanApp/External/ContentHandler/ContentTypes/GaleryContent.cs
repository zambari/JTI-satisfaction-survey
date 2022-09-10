using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    public class GaleryContent : AbstractParentContent<GaleryData>
	{
        //public override void OnContentChanged()
        //{
        //    base.OnContentChanged();

        //    var contentInChildren = Utilities.GetAllComponentsInChildren<GaleryContent, IContent>(transform);
        //    foreach (var content in contentInChildren)
        //    {
        //        var children = Data.PeekChildren<BaseData>();
        //        for (int i = 0; i < children.Length; i++)
        //        {
        //            if (content.Data.id == children[i].id)
        //            {
        //                var monoContent = (MonoBehaviour)content;
        //                monoContent.transform.SetSiblingIndex(i);
        //            }
        //        }
        //    }
        //}

        public override void OnImportData()
        {
            base.OnImportData();

            var contentElements = Utilities.GetAllComponentsInChildren<GaleryContent, IContent>(transform);

            for (int j = 0; j < contentElements.Length; j++)
            {
                var childData = ContentHandler.CurrentBuilder.FindData(contentElements[j].ID);
                if (childData != null)
                {
                    var childParentData = ContentHandler.CurrentBuilder.FindData(childData.parentID);
                    if (childParentData != null)
                    {
                        var baseData = childParentData.PeekChildren<BaseData>();
                        if (baseData != null)
                        {
                            for (int i = 0; i < baseData.Length; i++)
                            {
                                if (!clonedChildren.ContainsKey(baseData[i].id))
                                {
                                    if (contentElements[j].ID == baseData[i].id)
                                    {
                                        var mono = (MonoBehaviour)contentElements[j];

                                        mono.gameObject.SetActive(false);
                                        clonedChildren.Add(baseData[i].id, null);
                                        continue;
                                    }

                                    if (contentElements[j].OrginalID == baseData[i].originalID)
                                    {
                                        var mono = (MonoBehaviour)contentElements[j];
                                        var contentCopy = (IContent)ContentHandler.CloneContentBlock(baseData[i], contentElements[j], mono.transform.parent);
                                        var monoCopy = (MonoBehaviour)contentCopy;

                                        contentCopy.IsClone = true;
                                        monoCopy.gameObject.SetActive(true);
                                        clonedChildren.Add(baseData[i].id, mono);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
