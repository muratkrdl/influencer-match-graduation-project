namespace Final.InfluencerMatch.UI
{
    /// <summary>
    /// Contract for any UI panel that can be shown or hidden by the <see cref="UIManager"/>.
    /// </summary>
    public interface IUIPanel
    {
        void Show();
        void Hide();
    }
}
