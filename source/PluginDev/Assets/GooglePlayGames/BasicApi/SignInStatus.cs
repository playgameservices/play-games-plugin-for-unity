namespace GooglePlayGames.BasicApi
{
    public enum SignInStatus
    {
        /// <summary>The operation was successful.</summary>
        Success,

        /// <summary>
        /// The client attempted to connect to the service but the user is not signed in. The client may
        /// choose to continue without using the API. Alternately, if {@link Status#hasResolution} returns
        /// {@literal true} the client may call {@link Status#startResolutionForResult(Activity, int)} to
        /// prompt the user to sign in. After the sign in activity returns with {@link Activity#RESULT_OK}
        /// further attempts should succeed.
        /// </summary>
        UiSignInRequired,

        /// <summary>
        /// The application is misconfigured. This error is not recoverable and will be treated as fatal.
        /// The developer should look at the logs after this to determine more actionable information.
        /// </summary>
        DeveloperError,

        /// <summary>A network error occurred. Retrying should resolve the problem.</summary>
        NetworkError,

        /// <summary>An internal error occurred.</summary>
        InternalError,

        /// <summary>The sign in was canceled.</summary>
        Canceled,

        /// <summary>
        /// A sign in process is currently in progress and the current one cannot continue. e.g. the user
        /// clicks the SignInButton multiple times and more than one sign in intent was launched.
        /// </summary>
        AlreadyInProgress,

        /// <summary>
        /// Failure reason is unknown. Check adb log to see details if any.
        /// </summary>
        Failed,

        /// <summary>
        /// Currently not authenticated. Silent or interactive sign in is required.
        /// </summary>
        NotAuthenticated,
    }
}
