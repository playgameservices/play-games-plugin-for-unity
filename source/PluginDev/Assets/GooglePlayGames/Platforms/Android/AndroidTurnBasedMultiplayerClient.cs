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
            CreateQuickMatch(minOpponents, maxOpponents, variant, /* exclusiveBitMask= */ 0, callback);
        }

        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitmask, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            AndroidJavaObject matchConfig;
            // build room config
            using (var matchConfigClass = new AndroidJavaClass("com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatchConfig"))
            {
                using (var matchConfigBuilder = matchConfigClass.CallStatic<AndroidJavaObject>("builder"))
                {

                  var autoMatchCriteria = matchConfigClass.CallStatic<AndroidJavaObject>("createAutoMatchCriteria", (int) minOpponents, (int) maxOpponents, (long) exclusiveBitmask);
                  matchConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria", autoMatchCriteria);

                  if (variant != 0) {
                    matchConfigBuilder.Call<AndroidJavaObject>("setVariant", (int) variant);
                  }

                  matchConfig = matchConfigBuilder.Call<AndroidJavaObject>("build");
                }
            }

            // Task<TurnBasedMatch> createMatch(@NonNull TurnBasedMatchConfig config)
            using (var task = mClient.Call<AndroidJavaObject>("createMatch", matchConfig))
            {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    turnBasedMatch => {
                      callback(true, createTurnBasedMatch(turnBasedMatch));
                    }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(false, null);
                    }
                ));
            }
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
            using (var task = mClient.Call<AndroidJavaObject>("loadMatchesByStatus", new int[]{0, 1, 2, 3}))
            {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    annotatedData => {
                      using (var matchesResponse = annotatedData.Call<AndroidJavaObject>("get"))
                      {
                        List<TurnBasedMatch> myTurnMatches = createTurnBasedMatchList(matchesResponse.Call<AndroidJavaObject>("getMyTurnMatches"));
                        List<TurnBasedMatch> theirTurnMatches = createTurnBasedMatchList(matchesResponse.Call<AndroidJavaObject>("getTheirTurnMatches"));
                        List<TurnBasedMatch> completedMatches = createTurnBasedMatchList(matchesResponse.Call<AndroidJavaObject>("getCompletedMatches"));

                        List<TurnBasedMatch> matches = new List<TurnBasedMatch>(myTurnMatches);
                        matches.AddRange(theirTurnMatches);
                        matches.AddRange(completedMatches);
                        callback(matches.ToArray());
                      }
                    }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(null);
                    }
                ));
            }
        }

        public void GetMatch(string matchId, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // public Task<AnnotatedData<TurnBasedMatch>> loadMatch(@NonNull String matchId)

            using (var task = mClient.Call<AndroidJavaObject>("loadMatch"))
            {
              task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    annotatedData => {
                      using (var turnBasedMatch = annotatedData.Call<AndroidJavaObject>("get"))
                      {
                        if (turnBasedMatch == null) {
                          OurUtils.Logger.e(string.Format("Could not find match {0}", matchId));
                          callback(false, null);
                        }
                        else
                        {
                          callback(true, createTurnBasedMatch(turnBasedMatch));
                        }
                      }
                    }));
              task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                exception => {
                  callback(false, null);
                }
              ));
            }
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
            GetMatch(match.MatchId, (success, foundMatch) => {
              if (success) {
                if (foundMatch.Version != match.Version) {
                  OurUtils.Logger.e(string.Format("Attempted to update a stale version of the " +
                        "match. Expected version was {0} but current version is {1}.",
                        match.Version, foundMatch.Version));
                  return;
                }
                mClient.Call<AndroidJavaObject>("dismissMatch", match.MatchId);
              }
            });
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

        private List<TurnBasedMatch> createTurnBasedMatchList(AndroidJavaObject turnBasedMatchBuffer) {
                List<TurnBasedMatch> turnBasedMatches = new List<TurnBasedMatch>();
                int count = turnBasedMatchBuffer.Call<int>("getCount");
                for (int i=0; i<count; i++) {
                  TurnBasedMatch match = createTurnBasedMatch(turnBasedMatchBuffer.Call<AndroidJavaObject>("get", (int) i));
                  turnBasedMatches.Add(match);
                }
                return turnBasedMatches;
        }

        private TurnBasedMatch createTurnBasedMatch(AndroidJavaObject turnBasedMatch)
        {
            string matchId = turnBasedMatch.Call<string>("getMatchId");
            byte[] data = turnBasedMatch.Call<byte[]>("getData");
            bool canRematch = turnBasedMatch.Call<bool>("canRematch");
            uint availableAutomatchSlots = (uint) turnBasedMatch.Call<int>("getAvailableAutoMatchSlots");
            string selfParticipantId = turnBasedMatch.Call<string>("getCreatorId");
            List<Participant> participants = createParticipants(turnBasedMatch);
            string pendingParticipantId = turnBasedMatch.Call<string>("getPendingParticipantId");
            TurnBasedMatch.MatchStatus turnStatus = AndroidJavaConverter.ToTurnStatus(turnBasedMatch.Call<int>("getStatus"));
            TurnBasedMatch.MatchTurnStatus matchStatus = AndroidJavaConverter.ToMatchTurnStatus(turnBasedMatch.Call<int>("getTurnStatus"));
            uint variant = (uint) turnBasedMatch.Call<int>("getVariant");
            uint version = (uint) turnBasedMatch.Call<int>("getVersion");
            DateTime creationTime = AndroidJavaConverter.ToDateTime(turnBasedMatch.Call<long>("getCreationTimestamp"));
            DateTime lastUpdateTime = AndroidJavaConverter.ToDateTime(turnBasedMatch.Call<long>("getLastUpdatedTimestamp"));

            return new TurnBasedMatch(matchId, data, canRematch, selfParticipantId, participants, availableAutomatchSlots, pendingParticipantId, matchStatus, turnStatus, variant, version, creationTime, lastUpdateTime);
        }

        private List<Participant> createParticipants(AndroidJavaObject turnBasedMatch)
        {
            AndroidJavaObject participantsObject = turnBasedMatch.Call<AndroidJavaObject>("getParticipantIds");
            List<Participant> participants = new List<Participant>();
            int size = participantsObject.Call<int>("size");

            for (int i = 0; i < size ; i++)
            {
                string participantId = participantsObject.Call<string>("get", i);
                AndroidJavaObject participantObject = turnBasedMatch.Call<AndroidJavaObject>("getParticipant", participantId);
                participants.Add(AndroidJavaConverter.ToParticipant(participantObject));
            }
            return participants;
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
