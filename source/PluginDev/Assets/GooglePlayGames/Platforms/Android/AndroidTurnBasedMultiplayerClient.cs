#if UNITY_ANDROID
#pragma warning disable 0168 // The variable 'var' is declared but never used
#pragma warning disable 0642 // Possible mistaken empty statement

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
        private volatile AndroidClient mAndroidClient;
        private volatile Action<TurnBasedMatch, bool> mMatchDelegate;

        public AndroidTurnBasedMultiplayerClient(AndroidClient androidClient, AndroidJavaObject account)
        {
            mAndroidClient = androidClient;
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games"))
            {
                mClient = gamesClass.CallStatic<AndroidJavaObject>("getTurnBasedMultiplayerClient",
                    AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
            Action<bool, TurnBasedMatch> callback)
        {
            CreateQuickMatch(minOpponents, maxOpponents, variant, /* exclusiveBitMask= */ 0, callback);
        }

        public void CreateQuickMatch(uint minOpponents, uint maxOpponents, uint variant,
            ulong exclusiveBitmask, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);

            using (var matchConfigClass =
                new AndroidJavaClass("com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatchConfig"))
            using (var matchConfigBuilder = matchConfigClass.CallStatic<AndroidJavaObject>("builder"))
            using (var autoMatchCriteria = matchConfigClass.CallStatic<AndroidJavaObject>("createAutoMatchCriteria",
                (int) minOpponents, (int) maxOpponents, (long) exclusiveBitmask))
            {
                matchConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria", autoMatchCriteria);

                if (variant != 0)
                {
                    matchConfigBuilder.Call<AndroidJavaObject>("setVariant", (int) variant);
                }

                using (var matchConfig = matchConfigBuilder.Call<AndroidJavaObject>("build"))
                using (var task = mClient.Call<AndroidJavaObject>("createMatch", matchConfig))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task,
                        turnBasedMatch =>
                            callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch)));

                    AndroidTaskUtils.AddOnFailureListener(task, e => callback(false, null));
                }
            }
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<bool, TurnBasedMatch> callback)
        {
            CreateWithInvitationScreen(minOpponents, maxOpponents, variant, (status, match) =>
                callback(status == UIStatus.Valid, match)
            );
        }

        public void CreateWithInvitationScreen(uint minOpponents, uint maxOpponents, uint variant,
            Action<UIStatus, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);

            AndroidHelperFragment.ShowTbmpSelectOpponentsUI(minOpponents, maxOpponents,
                (status, result) =>
                {
                    if (status == UIStatus.NotAuthorized)
                    {
                        mAndroidClient.SignOut((() => callback(status, null)));
                        return;
                    }

                    if (status != UIStatus.Valid)
                    {
                        callback(status, null);
                        return;
                    }

                    using (var matchConfigClass =
                        new AndroidJavaClass("com.google.android.gms.games.multiplayer.turnbased.TurnBasedMatchConfig"))
                    using (var matchConfigBuilder = matchConfigClass.CallStatic<AndroidJavaObject>("builder"))
                    {
                        if (result.MinAutomatchingPlayers > 0)
                        {
                            using (var autoMatchCriteria = matchConfigClass.CallStatic<AndroidJavaObject>(
                                "createAutoMatchCriteria", result.MinAutomatchingPlayers,
                                result.MaxAutomatchingPlayers, /* exclusiveBitMask= */ (long) 0))
                            using (matchConfigBuilder.Call<AndroidJavaObject>("setAutoMatchCriteria",
                                autoMatchCriteria))
                                ;
                        }

                        if (variant != 0)
                        {
                            using (matchConfigBuilder.Call<AndroidJavaObject>("setVariant", (int) variant)) ;
                        }

                        using (var invitedPlayersObject = new AndroidJavaObject("java.util.ArrayList"))
                        {
                            for (int i = 0; i < result.PlayerIdsToInvite.Count; ++i)
                            {
                                invitedPlayersObject.Call<bool>("add", result.PlayerIdsToInvite[i]);
                            }

                            using (matchConfigBuilder.Call<AndroidJavaObject>("addInvitedPlayers", invitedPlayersObject)
                            ) ;
                        }

                        using (var matchConfig = matchConfigBuilder.Call<AndroidJavaObject>("build"))
                        using (var task = mClient.Call<AndroidJavaObject>("createMatch", matchConfig))
                        {
                            AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                                task,
                                turnBasedMatch =>
                                    callback(UIStatus.Valid, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch)));

                            AndroidTaskUtils.AddOnFailureListener(
                                task,
                                exception => callback(UIStatus.InternalError, null));
                        }
                    }
                });
        }

        private AndroidJavaObject StringListToAndroidJavaObject(List<string> list)
        {
            AndroidJavaObject javaObject = new AndroidJavaObject("java.util.ArrayList");
            for (int i = 0; i < list.Count; ++i)
            {
                javaObject.Call<bool>("add", list[i]);
            }

            return javaObject;
        }

        public void GetAllInvitations(Action<Invitation[]> callback)
        {
            callback = ToOnGameThread(callback);

            using (var task = mClient.Call<AndroidJavaObject>("loadMatchesByStatus", new int[] {0, 1, 2, 3}))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var matchesResponse = annotatedData.Call<AndroidJavaObject>("get"))
                        using (var invitationsBuffer = matchesResponse.Call<AndroidJavaObject>("getInvitations"))
                        {
                            int count = invitationsBuffer.Call<int>("getCount");
                            Invitation[] invitations = new Invitation[count];
                            for (int i = 0; i < count; i++)
                            {
                                using (var invitation = invitationsBuffer.Call<AndroidJavaObject>("get", (int) i))
                                {
                                    invitations[i] = AndroidJavaConverter.ToInvitation(invitation);
                                }
                            }

                            callback(invitations);
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(task, e => callback(null));
            }
        }

        public void GetAllMatches(Action<TurnBasedMatch[]> callback)
        {
            callback = ToOnGameThread(callback);

            using (var task = mClient.Call<AndroidJavaObject>("loadMatchesByStatus", new int[] {0, 1, 2, 3}))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var matchesResponse = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            List<TurnBasedMatch> myTurnMatches, theirTurnMatches, completedMatches;
                            using (var myTurnMatchesObject =
                                matchesResponse.Call<AndroidJavaObject>("getMyTurnMatches"))
                            {
                                myTurnMatches = CreateTurnBasedMatchList(myTurnMatchesObject);
                            }

                            using (var theirTurnMatchesObject =
                                matchesResponse.Call<AndroidJavaObject>("getTheirTurnMatches"))
                            {
                                theirTurnMatches = CreateTurnBasedMatchList(theirTurnMatchesObject);
                            }

                            using (var completedMatchesObject =
                                matchesResponse.Call<AndroidJavaObject>("getCompletedMatches"))
                            {
                                completedMatches = CreateTurnBasedMatchList(completedMatchesObject);
                            }

                            List<TurnBasedMatch> matches = new List<TurnBasedMatch>(myTurnMatches);
                            matches.AddRange(theirTurnMatches);
                            matches.AddRange(completedMatches);
                            callback(matches.ToArray());
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(task, exception => callback(null));
            }
        }

        public void GetMatch(string matchId, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);

            using (var task = mClient.Call<AndroidJavaObject>("loadMatch", matchId))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var turnBasedMatch = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            if (turnBasedMatch == null)
                            {
                                OurUtils.Logger.e(string.Format("Could not find match {0}", matchId));
                                callback(false, null);
                            }
                            else
                            {
                                callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch));
                            }
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(task, e => callback(false, null));
            }
        }

        private void GetMatchAndroidJavaObject(string matchId, Action<bool, AndroidJavaObject> callback)
        {
            using (var task = mClient.Call<AndroidJavaObject>("loadMatch", matchId))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var turnBasedMatch = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            if (turnBasedMatch == null)
                            {
                                OurUtils.Logger.e(string.Format("Could not find match {0}", matchId));
                                callback(false, null);
                            }
                            else
                            {
                                callback(true, turnBasedMatch);
                            }
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(task, exception => callback(false, null));
            }
        }

        public void AcceptFromInbox(Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);

            AndroidHelperFragment.ShowInboxUI((status, turnBasedMatch) =>
            {
                if (status == UIStatus.NotAuthorized)
                {
                    mAndroidClient.SignOut(() => callback(false, null));
                    return;
                }

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

            FindInvitationWithId(invitationId, invitation =>
            {
                if (invitation == null)
                {
                    OurUtils.Logger.e("Could not find invitation with id " + invitationId);
                    callback(false, null);
                    return;
                }

                using (var task = mClient.Call<AndroidJavaObject>("acceptInvitation", invitationId))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task,
                        turnBasedMatch =>
                            callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch)));

                    AndroidTaskUtils.AddOnFailureListener(task, e => callback(false, null));
                }
            });
        }

        public void RegisterMatchDelegate(MatchDelegate del)
        {
            if (del == null)
            {
                mMatchDelegate = null;
            }
            else
            {
                mMatchDelegate =
                    ToOnGameThread<TurnBasedMatch, bool>(
                        (turnBasedMatch, autoAccept) => del(turnBasedMatch, autoAccept));

                TurnBasedMatchUpdateCallbackProxy callbackProxy = new TurnBasedMatchUpdateCallbackProxy(mMatchDelegate);
                AndroidJavaObject turnBasedMatchUpdateCallback =
                    new AndroidJavaObject("com.google.games.bridge.TurnBasedMatchUpdateCallbackProxy", callbackProxy);
                using (mClient.Call<AndroidJavaObject>("registerTurnBasedMatchUpdateCallback",
                    turnBasedMatchUpdateCallback)) ;
            }
        }

        public Action<TurnBasedMatch, bool> MatchDelegate
        {
            get { return mMatchDelegate; }
        }

        public void TakeTurn(TurnBasedMatch match, byte[] data, string pendingParticipantId,
            Action<bool> callback)
        {
            callback = ToOnGameThread(callback);
            FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback,
                (pendingParticipant, foundMatch) =>
                {
                    using (var task =
                        mClient.Call<AndroidJavaObject>("takeTurn", foundMatch.MatchId, data, pendingParticipantId))
                    {
                        AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                            task,
                            turnBasedMatch => callback(true));

                        AndroidTaskUtils.AddOnFailureListener(
                            task,
                            exception =>
                            {
                                OurUtils.Logger.d("Taking turn failed");
                                callback(false);
                            });
                    }
                });
        }

        public int GetMaxMatchDataSize()
        {
            return 128 * 1024 * 1024;
        }

        public void Finish(TurnBasedMatch match, byte[] data, MatchOutcome outcome, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);

            GetMatchAndroidJavaObject(match.MatchId, (status, foundMatch) =>
            {
                if (!status)
                {
                    callback(false);
                    return;
                }

                using (var results = new AndroidJavaObject("java.util.ArrayList"))
                {
                    Dictionary<string, AndroidJavaObject> idToResult = new Dictionary<string, AndroidJavaObject>();

                    using (var participantList = foundMatch.Call<AndroidJavaObject>("getParticipants"))
                    {
                        int size = participantList.Call<int>("size");
                        for (int i = 0; i < size; ++i)
                        {
                            try
                            {
                                using (var participantResult = participantList.Call<AndroidJavaObject>("get", i)
                                    .Call<AndroidJavaObject>("getResult"))
                                {
                                    string id = participantResult.Call<string>("getParticipantId");
                                    idToResult[id] = participantResult;
                                    results.Call<AndroidJavaObject>("add", participantResult);
                                }
                            }
                            catch (Exception e)
                            {
                                // if getResult returns null.
                            }
                        }
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
                            using (var participantResult = new AndroidJavaObject(
                                "com.google.android.gms.games.multiplayer.ParticipantResult", participantId,
                                (int) result, (int) placing))
                            {
                                results.Call<bool>("add", participantResult);
                            }
                        }
                    }

                    using (var task = mClient.Call<AndroidJavaObject>("finishMatch", match.MatchId, data, results))
                    {
                        AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                            task,
                            turnBasedMatch => callback(true));

                        AndroidTaskUtils.AddOnFailureListener(task, e => callback(false));
                    }
                }
            });
        }

        public void AcknowledgeFinished(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);

            FindEqualVersionMatch(match, (success, foundMatch) =>
            {
                if (!success)
                {
                    callback(false);
                    return;
                }

                using (var task = mClient.Call<AndroidJavaObject>("dismissMatch", foundMatch.MatchId))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(task, v => callback(true));
                    AndroidTaskUtils.AddOnFailureListener(task, e => callback(false));
                }
            });
        }

        public void Leave(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);

            FindEqualVersionMatch(match, (success, foundMatch) =>
            {
                if (!success)
                {
                    callback(false);
                }

                using (var task = mClient.Call<AndroidJavaObject>("leaveMatch", match.MatchId))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task,
                        v => callback(true));

                    AndroidTaskUtils.AddOnFailureListener(task, exception => callback(false));
                }
            });
        }

        public void LeaveDuringTurn(TurnBasedMatch match, string pendingParticipantId,
            Action<bool> callback)
        {
            callback = ToOnGameThread(callback);

            FindEqualVersionMatchWithParticipant(match, pendingParticipantId, callback,
                (pendingParticipant, foundMatch) =>
                {
                    using (var task =
                        mClient.Call<AndroidJavaObject>("leaveMatchDuringTurn", match.MatchId, pendingParticipantId))
                    {
                        AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                            task,
                            v => callback(true));

                        AndroidTaskUtils.AddOnFailureListener(task, e => callback(false));
                    }
                });
        }

        public void Cancel(TurnBasedMatch match, Action<bool> callback)
        {
            callback = ToOnGameThread(callback);

            FindEqualVersionMatch(match, (success, foundMatch) =>
            {
                if (!success)
                {
                    callback(false);
                    return;
                }

                using (var task = mClient.Call<AndroidJavaObject>("cancelMatch", match.MatchId))
                {
                    AndroidTaskUtils.AddOnSuccessListener<string>(
                        task,
                        v => callback(true));

                    AndroidTaskUtils.AddOnFailureListener(task, e => callback(false));
                }
            });
        }

        public void Dismiss(TurnBasedMatch match)
        {
            FindEqualVersionMatch(match, (success, foundMatch) =>
            {
                if (success)
                {
                    using (mClient.Call<AndroidJavaObject>("dismissMatch", match.MatchId)) ;
                }
            });
        }

        public void Rematch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback)
        {
            callback = ToOnGameThread(callback);

            FindEqualVersionMatch(match, (success, foundMatch) =>
            {
                if (!success)
                {
                    callback(false, null);
                    return;
                }

                using (var task = mClient.Call<AndroidJavaObject>("rematch", match.MatchId))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task,
                        turnBasedMatch =>
                            callback(true, AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch)));

                    AndroidTaskUtils.AddOnFailureListener(
                        task,
                        e => callback(false, null));
                }
            });
        }

        public void DeclineInvitation(string invitationId)
        {
            FindInvitationWithId(invitationId, invitation =>
            {
                if (invitation == null)
                {
                    return;
                }

                using (mClient.Call<AndroidJavaObject>("declineInvitation", invitationId)) ;
            });
        }

        private void FindInvitationWithId(string invitationId, Action<Invitation> callback)
        {
            using (var task = mClient.Call<AndroidJavaObject>("loadMatchesByStatus", new int[] {0, 1, 2, 3}))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var matchesResponse = annotatedData.Call<AndroidJavaObject>("get"))
                        using (var invitationsBuffer = matchesResponse.Call<AndroidJavaObject>("getInvitations"))
                        {
                            int count = invitationsBuffer.Call<int>("getCount");
                            for (int i = 0; i < count; ++i)
                            {
                                Invitation invitation =
                                    AndroidJavaConverter.ToInvitation(
                                        invitationsBuffer.Call<AndroidJavaObject>("get", (int) i));
                                if (invitation.InvitationId == invitationId)
                                {
                                    callback(invitation);
                                    return;
                                }
                            }
                        }

                        callback(null);
                    });

                AndroidTaskUtils.AddOnFailureListener(task, e => callback(null));
            }
        }


        private void FindEqualVersionMatch(TurnBasedMatch match, Action<bool, TurnBasedMatch> callback)
        {
            GetMatch(match.MatchId, (success, foundMatch) =>
            {
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

        private void FindEqualVersionMatchWithParticipant(TurnBasedMatch match, string participantId,
            Action<bool> onFailure, Action<Participant, TurnBasedMatch> onFoundParticipantAndMatch)
        {
            FindEqualVersionMatch(match, (success, foundMatch) =>
            {
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

        private Participant CreateAutomatchingSentinel()
        {
            return new Participant(
                /* displayName= */ "",
                /* participantId= */ "",
                Participant.ParticipantStatus.NotInvitedYet,
                new Player("", "", ""),
                /* connectedToRoom= */ false
            );
        }

        private List<TurnBasedMatch> CreateTurnBasedMatchList(AndroidJavaObject turnBasedMatchBuffer)
        {
            List<TurnBasedMatch> turnBasedMatches = new List<TurnBasedMatch>();
            int count = turnBasedMatchBuffer.Call<int>("getCount");
            for (int i = 0; i < count; ++i)
            {
                TurnBasedMatch match =
                    AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatchBuffer.Call<AndroidJavaObject>("get", (int) i));
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

        private class TurnBasedMatchUpdateCallbackProxy : AndroidJavaProxy
        {
            private Action<TurnBasedMatch, bool> mMatchDelegate;

            public TurnBasedMatchUpdateCallbackProxy(Action<TurnBasedMatch, bool> matchDelegate)
                : base("com/google/games/bridge/TurnBasedMatchUpdateCallbackProxy$Callback")
            {
                mMatchDelegate = matchDelegate;
            }

            public void onTurnBasedMatchReceived(AndroidJavaObject turnBasedMatch)
            {
                mMatchDelegate.Invoke(AndroidJavaConverter.ToTurnBasedMatch(turnBasedMatch), /* shouldAutoLaunch= */
                    false);
            }

            public void onTurnBasedMatchRemoved(string invitationId)
            {
            }
        }
    }
}
#endif
