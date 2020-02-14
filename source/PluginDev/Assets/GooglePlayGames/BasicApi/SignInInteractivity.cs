namespace GooglePlayGames.BasicApi
{
    public enum SignInInteractivity
    {
        /// <summary>no UIs will be shown (if UIs are needed, it will fail rather than show them).</summary>
        NoPrompt,

        /// <summary>
        /// This may show UIs, consent dialogs, etc.
        /// At the end of the process, callback will be invoked to notify of the result.
        /// Once the callback returns true, the user is considered to be authenticated.
        /// </summary>
        CanPromptAlways,

        /// <summary>When this is selected, PlayGamesPlatform.Authenticate does the followings in order:
        /// 1. Attempt to silent sign in.
        /// 2. If silent sign in fails, check if user has previously declined to sign in and don’t prompt interactive
        /// sign in if they have.
        /// 3. Check the internet connection and fail with NO_INTERNET_CONNECTION if there is no internet connection.
        /// 4. Prompt interactive sign in.
        /// 5. If the interactive sign in is not successful (user declines or cancels), then
        /// remember this for step 2 the next time the user opens the game and don’t ask for sign-in.
        /// </summary>
        CanPromptOnce
    }
}
