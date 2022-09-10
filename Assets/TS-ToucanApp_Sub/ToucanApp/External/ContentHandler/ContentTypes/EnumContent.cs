using ToucanApp.Config;

namespace ToucanApp.Data
{
    public class EnumContent : AbstractContent<EnumData>
    {
        public string[] options;
        public int value = 0;
        public IntEvent onImported;

        public override void OnExportData()
        {
    		base.OnExportData();
            Data.value = value;
            Data.options = options;
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
