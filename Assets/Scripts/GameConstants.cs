/// <summary>
/// Central location for game-wide constants.
/// </summary>
public static class GameConstants
{
    /// <summary>Maximum number of wrong attempts allowed per sequence.</summary>
    public const int MaxAttempts = 3;

    /// <summary>Seconds to display feedback before advancing to the next level.</summary>
    public const float FeedbackDelay = 1.0f;

    /// <summary>Seconds to display the "safe locked" message before hard-resetting.</summary>
    public const float HardResetDelay = 2.2f;
}
