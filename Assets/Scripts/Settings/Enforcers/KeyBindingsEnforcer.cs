namespace Settings.Enforcers
{
    public class KeyBindingsEnforcer :
        ISettingEnforcer<string>
    {
        public string GetKey()
        {
            return "Key Bindings";
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