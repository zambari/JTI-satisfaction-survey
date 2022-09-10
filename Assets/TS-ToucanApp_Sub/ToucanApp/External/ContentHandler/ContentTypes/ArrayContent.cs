using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ToucanApp.Data
{
    public class TextArrayContent : AbstractParentContent<TableData>
	{
        private int idx = 0;
        private RowData[] rows;

        public override void OnLanguageChanged(int languageId)
        {
            base.OnLanguageChanged(languageId);
            rows = ContentHelpers.GetTableData<RowData>(Data, "ArrayRow");
        }

        public override void OnExportData()
        {
            base.OnExportData();
            Data.AddChild<RowData>("ArrayRow", (RowData row) => {

                row.AddChild<TextData>("ArrayElement");

            });
        }

        public void SetIdx(int idx)
        {
            var t = GetComponent<Text>();
            if (t != null)
                t.text = GetCurrent();
        }

        public string GetCurrent()
        {
            return rows[idx].GetFirstChild<TextData>("ArrayElement").GetTranslation(ContentHandler.LanguageID);
        }

        public void RotateNext()
        {
            idx++;
            if (idx >= rows.Length)
                idx = 0;

            SetIdx(idx);
        }

        public void RotatePrev()
        {
            idx--;
            if (idx < 0)
                idx = rows.Length - 1;

            SetIdx(idx);
        }
    }
}
