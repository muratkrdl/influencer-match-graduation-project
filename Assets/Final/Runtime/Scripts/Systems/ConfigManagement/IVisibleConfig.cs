namespace Final.Systems.ConfigManagement
{
    /// <summary>
    /// Marker interface for ScriptableObject configs that should be visible in the Config Manager window.
    /// </summary>
    public interface IVisibleConfig
    {
        string ConfigName { get; }
        string Category { get; }
    }
}
