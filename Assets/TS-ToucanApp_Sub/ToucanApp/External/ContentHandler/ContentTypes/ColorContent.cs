using UnityEngine;
using ToucanApp.Config;

namespace ToucanApp.Data
{
    public class ColorContent : AbstractContent<ColorData>
    {
        [SerializeField]
        private Color color;

        public ColorEvent OnColorImported = new ColorEvent();

        public override void OnExportData()
        {
            base.OnExportData();
            Data.SetColor(color);
        }

        public override void OnImportData()
        {
            base.OnImportData();
            color = Data.GetColor(color);
            OnColorImported.Invoke(color);
        }
    }
}