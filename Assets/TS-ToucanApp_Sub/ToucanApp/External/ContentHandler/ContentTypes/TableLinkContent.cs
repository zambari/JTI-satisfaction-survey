using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ToucanApp.Data
{
    public class TableLinkContent : AbstractContent<TableLinkData>
    {
        public TableContent linkedTable;
        public RowContent rowPrototype;
        public bool setClonesActive = true;

        private Dictionary<string, IContent> clonedChildren = new Dictionary<string, IContent>();

        public override void OnExportData()
        {
            base.OnExportData();

            Data.linkedTable = linkedTable.ID;
        }

        public override void OnImportData()
        {
            base.OnImportData();

            foreach (var row_id in Data.linkedRows)
            {
                var rowData = BaseData.FindData<RowData>(ContentHandler.RootContent.Data, (BaseData data) => { return data.id == row_id; });

                if (!clonedChildren.ContainsKey(row_id))
                {
                    var clone = ContentHandler.CloneContentBlock(rowData, rowPrototype, this.transform) as RowContent;
                    clone.ShouldExport = false;
                    if (setClonesActive)
                        clone.gameObject.SetActive(true);

                    clonedChildren.Add(clone.ID, clone);
                }
            }
        }
    }
}
