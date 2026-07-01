namespace Settings
{
    public interface ISettingEnforcer
    {
        string GetKey();

        void Enforce(string fullName, object value);
    }

    public interface ISettingEnforcer<T> : ISettingEnforcer
    {
        void Enforce(string fullName, T value);
    }
}