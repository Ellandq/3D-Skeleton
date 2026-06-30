using Settings;

namespace Settings.Enforcers
{
    public class MonitorEnforcer :
        ISettingEnforcer<string>
    {
        public string GetKey()
        {
            return "Video/Display/Monitor";
        }


        public void Enforce(
            string fullName,
            string value)
        {
        }

        void ISettingEnforcer.Enforce(
            string fullName,
            object value)
        {
            Enforce(
                fullName,
                (string)value);
        }
    }
}