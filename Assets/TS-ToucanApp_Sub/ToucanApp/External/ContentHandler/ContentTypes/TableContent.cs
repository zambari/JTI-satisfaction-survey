using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    public class TableContent : AbstractParentContent<TableData>
	{
        public override void OnContentChanged()
        {
            base.OnContentChanged();

            var contentInChildren = Utilities.GetAllComponentsInChildren<TableContent, RowContent>(transform);
            foreach (var content in contentInChildren)
            {
                var children = Data.PeekChildren<RowData>();
                for (int i = 0; i < children.Length; i++)
                {
                    if (content.Data.id == children[i].id)
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

            var contentRows = Utilities.GetAllComponentsInChildren<IContent, RowContent>(transform);
            for (int j = 0; j < contentRows.Length; j++)
            {
                var childData = ContentHandler.CurrentBuilder.FindData(contentRows[j].ID);

                if (childData == null)
                    childData = ContentHandler.CurrentBuilder.FindOriginalData(contentRows[j].ID); // fix for removed prototype

                if (childData != null)
                {
                    var childParentData = ContentHandler.CurrentBuilder.FindData(childData.parentID);
                    if (childParentData != null)
                    {
                        var dataRows = childParentData.PeekChildren<RowData>();
                        if (dataRows != null)
                        {
                            for (int i = 0; i < dataRows.Length; i++)
                            {
                                if (!clonedChildren.ContainsKey(dataRows[i].id))
                                {
                                    if (contentRows[j].ID == dataRows[i].id)
                                    {
                                        clonedChildren.Add(dataRows[i].id, null);
                                        continue;
                                    }

                                    if (contentRows[j].OrginalID == dataRows[i].originalID)
                                    {
                                        var rowCopy = (RowContent)ContentHandler.CloneContentBlock(dataRows[i], contentRows[j], contentRows[j].transform.parent);
                                        rowCopy.IsClone = true;
                                        clonedChildren.Add(dataRows[i].id, rowCopy);
                                    }
                                }
                            }
                        }
                    }

                    foreach (var item in contentRows)
                    {
                        if (!clonedChildren.ContainsKey(item.ID))
                            item.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
