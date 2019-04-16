#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    using System;
    using System.Collections.Generic;
    using GooglePlayGames.BasicApi;
    using GooglePlayGames.BasicApi.Multiplayer;
    using GooglePlayGames.OurUtils;
    using UnityEngine;

    internal class AndroidTurnBasedMultiplayerClient : ITurnBasedMultiplayerClient
    {
        private volatile AndroidJavaObject mClient;

        public AndroidTurnBasedMultiplayerClient(AndroidJavaObject account) 
        {
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games")) 
            {
                mClient = gamesClass.CallStatic<AndroidJavaObject>("getTurnBasedMultiplayerClient", AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
                              Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> createMatch(@NonNull TurnBasedMatchConfig config)
        }

        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitmask, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> createMatch(@NonNull TurnBasedMatchConfig config)
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
                                        Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Intent> getSelectOpponentsIntent(@IntRange(from = 1) int minPlayers, @IntRange(from = 1) int maxPlayers)
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<UIStatus, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Intent> getSelectOpponentsIntent(@IntRange(from = 1) int minPlayers, @IntRange(from = 1) int maxPlayers)
        }

        public void GetAllInvitations(Action<Invitation[]> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<AnnotatedData<InvitationBuffer>> InvitationsClient.loadInvitations()
        }

        public void GetAllMatches(Action<TurnBasedMatch[]> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<AnnotatedData<LoadMatchesResponse>> loadMatchesByStatus(@InvitationSortOrder int invitationSortOrder, @NonNull @MatchTurnStatus int[] matchTurnStatuses))
        }

        public void GetMatch(string matchId, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void AcceptFromInbox(Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Intent> getInboxIntent()
        }

        public void AcceptInvitation(string invitationId, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> acceptInvitation(@NonNull String invitationId)
        }

        public void RegisterMatchDelegate(MatchDelegate del)
        {
            // Task<Void> registerTurnBasedMatchUpdateCallback(@NonNull TurnBasedMatchUpdateCallback callback) 
            // Task<Boolean> unregisterTurnBasedMatchUpdateCallback(@NonNull TurnBasedMatchUpdateCallback callback)
        }

        public void TakeTurn(TurnBasedMatch match, byte[] data, string pendingParticipantId,
                      Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> takeTurn(@NonNull String matchId, @Nullable byte[] matchData, @Nullable String pendingParticipantId)
        }

        public int GetMaxMatchDataSize()
        {
            // Task<Integer> getMaxMatchDataSize()
            return 128 * 1024 * 1024;
        }

        public void Finish(TurnBasedMatch match, byte[] data, MatchOutcome outcome, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> finishMatch(@NonNull String matchId)
        }

        public void AcknowledgeFinished(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Void> dismissMatch(@NonNull String matchId)
        }

        public void Leave(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Void> leaveMatch(@NonNull String matchId)
        }

        public void LeaveDuringTurn(TurnBasedMatch match, string pendingParticipantId,
                             Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Void> leaveMatchDuringTurn(@NonNull String matchId, @Nullable String pendingParticipantId)
        }

        public void Cancel(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<String> cancelMatch(@NonNull String matchId)
        }

        public void Dismiss(TurnBasedMatch match)
        {

        }

        public void Rematch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> rematch(@NonNull String matchId)
        }

        public void DeclineInvitation(string invitationId)
        {
            //Task<Void> declineInvitation(@NonNull String invitationId)
        }

        private static Action<T> ToOnGameThread<T>(Action<T> toConvert)
        {
            return (val) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val));
        }        

        private static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert)
        {
            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }        

    }
}
#endif

