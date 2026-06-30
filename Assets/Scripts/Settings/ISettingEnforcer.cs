namespace Settings
{
    public interface ISettingEnforcer<T>
    {
        string GetKey();
        void Enforce(string fullName, T value);
    }
}