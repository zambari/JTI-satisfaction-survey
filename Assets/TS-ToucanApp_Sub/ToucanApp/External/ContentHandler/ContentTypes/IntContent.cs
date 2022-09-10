using ToucanApp.Config;

namespace ToucanApp.Data
{
    public class IntContent : AbstractContent<IntData>
    {
        public int value = 0;
        public IntEvent onImported;

        public override void OnExportData()
        {
    		base.OnExportData();
            Data.value = value;
        }

        public override void OnImportData()
        {
            base.OnImportData();
            value = Data.value;

            if (onImported != null)
                onImported.Invoke(value);
        }
    }
}
