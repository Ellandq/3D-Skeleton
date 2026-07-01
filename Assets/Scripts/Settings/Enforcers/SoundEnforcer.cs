namespace Settings.Enforcers
{
    public class SoundEnforcer :
        ISettingEnforcer<float>
    {
        public string GetKey()
        {
            return "Sound";
        }


        public void Enforce(
            string fullName,
            float value)
        {
        }

        void ISettingEnforcer.Enforce(
            string fullName,
            object value)
        {
            Enforce(
                fullName,
                (float)value);
        }
    }
}