namespace GooglePlayGames.BasicApi
{
    /// <summary>
    /// Enum to specify the sign in status.
    /// </summary>
    public enum SignInStatus
    {
        /// <summary>The operation was successful.</summary>
        Success,

        /// <summary>An internal error occurred.</summary>
        InternalError,

        /// <summary>The sign in was canceled.</summary>
        Canceled,
    }
}
