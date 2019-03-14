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
        }

        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitmask, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
                                        Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<UIStatus, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void GetAllInvitations(Action<Invitation[]> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void GetAllMatches(Action<TurnBasedMatch[]> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void AcceptFromInbox(Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void AcceptInvitation(string invitationId, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void RegisterMatchDelegate(MatchDelegate del)
        {
        }

        public void TakeTurn(TurnBasedMatch match, byte[] data, string pendingParticipantId,
                      Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public int GetMaxMatchDataSize()
        {
            return 0;
        }

        public void Finish(TurnBasedMatch match, byte[] data, MatchOutcome outcome, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void AcknowledgeFinished(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void Leave(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void LeaveDuringTurn(TurnBasedMatch match, string pendingParticipantId,
                             Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void Cancel(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void Rematch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
        }

        public void DeclineInvitation(string invitationId)
        {
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

