using Settings;

namespace Settings.Enforcers
{
    public class SoundEnforcer :
        ISettingEnforcer<string>
    {
        public string GetKey()
        {
            return "Sound";
        }


        public void Enforce(
            string fullName,
            string value)
        {
        }
    }
}