namespace Final.InfluencerMatch.Inputs
{
    /// <summary>
    /// App-wide input facade that surfaces high-level, device-agnostic input events.
    /// </summary>
    public interface IInputService
    {
        event CancelRequestedHandler CancelRequested;
    }

    public delegate void CancelRequestedHandler();

}
