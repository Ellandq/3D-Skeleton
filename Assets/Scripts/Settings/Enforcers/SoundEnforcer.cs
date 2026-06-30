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