namespace SaveAndLoad
{
    public interface ISaveable
    {
        string GetSaveData();
        void LoadSaveData(string data);
    }
}