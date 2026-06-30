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
    }
}