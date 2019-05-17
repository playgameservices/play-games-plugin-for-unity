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
                      callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch));
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
            CreateWithInvitationScreen(minOpponents, maxOpponents, variant, (status, match) => {
              callback(status == UIStatus.Valid, match);
            });
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<UIStatus, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);

            AndroidHelperFragment.InvitePlayerUI(minOpponents, maxOpponents, (status, result) => {
                if (status != UIStatus.Valid)
                {
                  callback(status, null);
                  return;
                }

                AndroidJavaObject matchConfig;
                using (var matchConfigClass = new AndroidJavaClass("com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatchConfig"))
                {
                    using (var matchConfigBuilder = matchConfigClass.CallStatic<AndroidJavaObject>("builder"))
                    {

                      if (result.MinAutomatchingPlayers > 0)
                      {
                        var autoMatchCriteria = matchConfigClass.CallStatic<AndroidJavaObject>("createAutoMatchCriteria", result.MinAutomatchingPlayers, result.MaxAutomatchingPlayers, /* exclusiveBitMask= */ (long) 0);
                        matchConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria", autoMatchCriteria);
                      }

                      if (variant != 0) {
                        matchConfigBuilder.Call<AndroidJavaObject>("setVariant", (int) variant);
                      }

                      AndroidJavaObject invitedPlayersObject = new AndroidJavaObject("java.util.ArrayList");
                      for (int i=0;i<result.PlayerIdsToInvite.Count;i++)
                      {
                          invitedPlayersObject.Call<bool>("add", result.PlayerIdsToInvite[i]);
                      }

                      matchConfigBuilder.Call<AndroidJavaObject>("addInvitedPlayers", invitedPlayersObject);

                      matchConfig = matchConfigBuilder.Call<AndroidJavaObject>("build");
                    }
                }

                // Task<TurnBasedMatch> createMatch(@NonNull TurnBasedMatchConfig config)
                using (var task = mClient.Call<AndroidJavaObject>("createMatch", matchConfig))
                {
                    task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                        turnBasedMatch => {
                          callback(UIStatus.Valid, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch));
                        }
                    ));
                    task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                        exception => {
                          callback(UIStatus.InternalError, null);
                        }
                    ));
                }
            });
        }

        private AndroidJavaObject StringListToAndroidJavaObject(List<string> list)
        {
          AndroidJavaObject javaObject = new AndroidJavaObject("java.util.ArrayList");
          for (int i=0;i<list.Count;i++)
          {
            javaObject.Call<bool>("add", list[i]);
          }
          return javaObject;
        }

        public void GetAllInvitations(Action<Invitation[]> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<AnnotatedData<InvitationBuffer>> InvitationsClient.loadInvitations()

            // Task<AnnotatedData<LoadMatchesResponse>> loadMatchesByStatus(@InvitationSortOrder int invitationSortOrder, @NonNull @MatchTurnStatus int[] matchTurnStatuses))
            using (var task = mClient.Call<AndroidJavaObject>("loadMatchesByStatus", new int[]{0, 1, 2, 3}))
            {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    annotatedData => {
                      // LoadMatchesResponse
                      using (var matchesResponse = annotatedData.Call<AndroidJavaObject>("get"))
                      {
                        AndroidJavaObject invitationsBuffer = matchesResponse.Call<AndroidJavaObject>("getInvitations");
                        int count = invitationsBuffer.Call<int>("getCount");
                        Invitation[] invitations = new Invitation[count];
                        for (int i=0; i<count; i++) {
                          Invitation invitation = AndroidJavaConverter.ToInvitation(invitationsBuffer.Call<AndroidJavaObject>("get", (int) i));
                          invitations[i] = invitation;
                        }
                        callback(invitations);
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
                        List<TurnBasedMatch> myTurnMatches = CreateTurnBasedMatchList(matchesResponse.Call<AndroidJavaObject>("getMyTurnMatches"));
                        List<TurnBasedMatch> theirTurnMatches = CreateTurnBasedMatchList(matchesResponse.Call<AndroidJavaObject>("getTheirTurnMatches"));
                        List<TurnBasedMatch> completedMatches = CreateTurnBasedMatchList(matchesResponse.Call<AndroidJavaObject>("getCompletedMatches"));

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

            using (var task = mClient.Call<AndroidJavaObject>("loadMatch", matchId))
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
                          callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch));
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

        private void GetMatchAndroidJavaObject(string matchId, Action<bool, AndroidJavaObject> callback)
        {
            // public Task<AnnotatedData<TurnBasedMatch>> loadMatch(@NonNull String matchId)
            using (var task = mClient.Call<AndroidJavaObject>("loadMatch", matchId))
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
                          callback(true, turnBasedMatch);
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
            AndroidHelperFragment.ShowInboxUI((status, turnBasedMatch) => {
              if (status != UIStatus.Valid)
              {
                callback(false, null);
                return;
              }

              OurUtils.Logger.d("Passing converted match to user callback:" + turnBasedMatch);
              callback(true, turnBasedMatch);
            });
        }

        public void AcceptInvitation(string invitationId, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> acceptInvitation(@NonNull String invitationId)
            FindInvitationWithId(invitationId, invitation => {
              if (invitation == null) {
                OurUtils.Logger.e("Could not find invitation with id " + invitationId);
                callback(false, null);
                return;
              }

              using (var task = mClient.Call<AndroidJavaObject>("acceptInvitation", invitationId))
              {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                  turnBasedMatch => {
                    callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch));
                  }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(false, null);
                    }
                ));
              }
            });
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
            FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback, (pendingParticipant, foundMatch) => {
              using (var task = mClient.Call<AndroidJavaObject>("takeTurn", foundMatch.MatchId, data, pendingParticipantId))
              {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                  turnBasedMatch => {
                    callback(true);
                  }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      OurUtils.Logger.d("Taking turn failed");
                      callback(false);
                    }
                ));
              }
            });
        }

        public int GetMaxMatchDataSize()
        {
            // Task<Integer> getMaxMatchDataSize()
            return 128 * 1024 * 1024;
        }

        public void Finish(TurnBasedMatch match, byte[] data, MatchOutcome outcome, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);

            GetMatchAndroidJavaObject(match.MatchId, (status, foundMatch) => {
              if (!status)
              {
                callback(false);
                return;
              }

              AndroidJavaObject results = new AndroidJavaObject("java.util.ArrayList");
              Dictionary<string, AndroidJavaObject> idToResult = new Dictionary<string, AndroidJavaObject>();

              AndroidJavaObject participantList = foundMatch.Call<AndroidJavaObject>("getParticipants");
              int size = participantList.Call<int>("size");
              for (int i=0;i<size;i++)
              {
                AndroidJavaObject participantResult = participantList.Call<AndroidJavaObject>("get", i).Call<AndroidJavaObject>("getResult");
                if (participantResult == null) {
                  continue;
                }
                string id = participantResult.Call<string>("getParticipantId");
                idToResult[id] = participantResult;
                results.Call<AndroidJavaObject>("add", participantResult);
              }

              foreach (string participantId in outcome.ParticipantIds)
              {
                MatchOutcome.ParticipantResult result = outcome.GetResultFor(participantId);
                uint placing = outcome.GetPlacementFor(participantId);

                if (idToResult.ContainsKey(participantId))
                {
                  var existingResult = idToResult[participantId].Get<int>("result");
                  uint existingPlacing = (uint) idToResult[participantId].Get<int>("placing");

                  if (result != (MatchOutcome.ParticipantResult) existingResult || placing != existingPlacing)
                  {
                    OurUtils.Logger.e(string.Format("Attempted to override existing results for " +
                            "participant {0}: Placing {1}, Result {2}",
                            participantId, existingPlacing, existingResult));
                    callback(false);
                    return;
                  }
                }
                else
                {
                  AndroidJavaObject participantResult = new AndroidJavaObject("com.google.android.gms.games.multiplayer.ParticipantResult", participantId, (int) result, (int) placing);
                  results.Call<bool>("add", participantResult);
                }
              }

              // Task<TurnBasedMatch> finishMatch(@NonNull String matchId, @Nullable byte[] matchData, @Nullable List<ParticipantResult> results)
              using (var task = mClient.Call<AndroidJavaObject>("finishMatch", match.MatchId, data, results))
              {
                  task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    turnBasedMatch => {
                      callback(true);
                    }
                  ));
                  task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                      exception => {
                        callback(false);
                      }
                  ));
              }
            });
        }

        public void AcknowledgeFinished(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Void> dismissMatch(@NonNull String matchId)
            FindEqualVersionMatch(match, (success, foundMatch) => {
              if (!success)
              {
                callback(false);
                return;
              }

              using (var task = mClient.Call<AndroidJavaObject>("dismissMatch", foundMatch.MatchId))
              {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                  v => {
                    callback(true);
                  }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(false);
                    }
                ));
              }
            });
        }

        public void Leave(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);

            // Task<Void> leaveMatch(@NonNull String matchId)
            FindEqualVersionMatch(match, (success, foundMatch) => {
              if(!success) {
                callback(false);
              }

              using (var task = mClient.Call<AndroidJavaObject>("leaveMatch", match.MatchId))
              {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                  v => {
                    callback(true);
                  }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(false);
                    }
                ));
              }
            });
        }

        public void LeaveDuringTurn(TurnBasedMatch match, string pendingParticipantId,
                             Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<Void> leaveMatchDuringTurn(@NonNull String matchId, @Nullable String pendingParticipantId)
            FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback, (pendingParticipant, foundMatch) => {
                using (var task = mClient.Call<AndroidJavaObject>("leaveMatchDuringTurn", match.MatchId, pendingParticipantId))
                {
                  task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    v => {
                      callback(true);
                    }
                  ));
                  task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                      exception => {
                        callback(false);
                      }
                  ));
                }
            });
        }

        public void Cancel(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<String> cancelMatch(@NonNull String matchId)
            FindEqualVersionMatch(match, (success, foundMatch) => {
              if (!success)
              {
                callback(false);
                return;
              }
              mClient.Call<AndroidJavaObject>("cancelMatch", match.MatchId);
              callback(true);
            });
        }

        public void Dismiss(TurnBasedMatch match)
        {
          FindEqualVersionMatch(match, (success, foundMatch) => {
              if (success)
              {
                mClient.Call<AndroidJavaObject>("dismissMatch", match.MatchId);
              }
          });
        }

        public void Rematch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);
            // Task<TurnBasedMatch> rematch(@NonNull String matchId)
            FindEqualVersionMatch(match, (success, foundMatch) => {
              if (!success)
              {
                callback(false, null);
                return;
              }

              using (var task = mClient.Call<AndroidJavaObject>("rematch", match.MatchId))
              {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                  turnBasedMatch => {
                    callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch));
                  }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(false, null);
                    }
                ));
              }
            });
        }

        public void DeclineInvitation(string invitationId)
        {
            //Task<Void> declineInvitation(@NonNull String invitationId)
            FindInvitationWithId(invitationId, invitation =>
              {
                  if (invitation == null)
                  {
                      return;
                  }

                  mClient.Call<AndroidJavaObject>("declineInvitation", invitationId);
            });
        }

        private void FindInvitationWithId(string invitationId, Action<Invitation> callback)
        {
            // Task<AnnotatedData<LoadMatchesResponse>> loadMatchesByStatus(@InvitationSortOrder int invitationSortOrder, @NonNull @MatchTurnStatus int[] matchTurnStatuses))
            using (var task = mClient.Call<AndroidJavaObject>("loadMatchesByStatus", new int[]{0, 1, 2, 3}))
            {
                task.Call<AndroidJavaObject>("addOnSuccessListener", new TaskOnSuccessProxy<AndroidJavaObject>(
                    annotatedData => {
                      // LoadMatchesResponse
                      using (var matchesResponse = annotatedData.Call<AndroidJavaObject>("get"))
                      {
                        AndroidJavaObject invitationsBuffer = matchesResponse.Call<AndroidJavaObject>("getInvitations");
                        int count = invitationsBuffer.Call<int>("getCount");
                        for (int i=0; i<count; i++) {
                          Invitation invitation = AndroidJavaConverter.ToInvitation(invitationsBuffer.Call<AndroidJavaObject>("get", (int) i));
                          if (invitation.InvitationId == invitationId) {
                            callback(invitation);
                            return;
                          }
                        }
                      }
                      callback(null);
                    }
                ));
                task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(
                    exception => {
                      callback(null);
                    }
                ));
            }
        }


        private void FindEqualVersionMatch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback) {
            GetMatch(match.MatchId, (success, foundMatch) => {
              if (!success)
              {
                callback(false, null);
                return;
              }
              if (match.Version != foundMatch.Version)
              {
                OurUtils.Logger.e(string.Format("Attempted to update a stale version of the " +
                        "match. Expected version was {0} but current version is {1}.",
                        match.Version, foundMatch.Version));
                callback(false, null);
                return;
              }
              callback(true, foundMatch);
            });
        }

        private void FindEqualVersionMatchWithParticipant(TurnBasedMatch match, string participantId, Action<bool> onFailure, Action<Participant, TurnBasedMatch> onFoundParticipantAndMatch)
        {
            FindEqualVersionMatch(match, (success, foundMatch) => {
                if (!success)
                {
                    onFailure(true);
                }

                // If we received a null participantId, we're using an automatching player instead -
                // issue the callback using that.
                if (participantId == null)
                {
                    onFoundParticipantAndMatch(CreateAutomatchingSentinel(), foundMatch);
                    return;
                }

                Participant participant = foundMatch.GetParticipant(participantId);
                if (participant == null)
                {
                    OurUtils.Logger.e(string.Format("Located match {0} but desired participant with ID " +
                            "{1} could not be found", match.MatchId, participantId));
                    onFailure(false);
                    return;
                }

                onFoundParticipantAndMatch(participant, foundMatch);
              });
        }

        private Participant CreateAutomatchingSentinel() {
            return new Participant(
              /* displayName= */ "",
              /* participantId= */ "",
              Participant.ParticipantStatus.NotInvitedYet,
              new Player(null, null, null), // ????????????????????????/
              /* connectedToRoom= */ false
            );
        }

        private List<TurnBasedMatch> CreateTurnBasedMatchList(AndroidJavaObject turnBasedMatchBuffer) {
                List<TurnBasedMatch> turnBasedMatches = new List<TurnBasedMatch>();
                int count = turnBasedMatchBuffer.Call<int>("getCount");
                for (int i=0; i<count; i++) {
                  TurnBasedMatch match = AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatchBuffer.Call<AndroidJavaObject>("get", (int) i));
                  turnBasedMatches.Add(match);
                }
                return turnBasedMatches;
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
