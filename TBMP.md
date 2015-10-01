# Google Play Games plugin for Unity:<br> Turn-Based Multiplayer
_Copyright (c) 2014 Google Inc. All rights reserved._

This is a guide to the Turn-Based Multiplayer features of the Google Play Games plugin
for Unity&reg; This documents assumes that you have thoroughly read
the [Getting Started Guide](README.md) and are familiar with how to set up your
project, sign in and perform basic API operations such as unlocking an
achievement and posting a score to a leaderboard.

Note: this project is not in any way endorsed or supervised by
Unity Technologies.

_Unity&reg; is a trademark of Unity Technologies._

_iOS is a trademark of Apple, Inc._

## Turn-Based Multiplayer Overview

Turn-based multiplayer allows you to write games where players take turns asynchronously. Turn-based matches can have up to 8 participants and users can play against their friends and also against randomly chosen opponents in the internet. The Google Play Games API implements all the underlying networking and notification infrastructure and exposes an API to your game that allows you to perform high-level actions such as create matches, play a turn, finish a match and react to invitations.

For more information about turn-based concepts in the Google Play Games API, please refer to the [documentation](https://developers.google.com/games/services/common/concepts/turnbasedMultiplayer).

To begin, implement sign-in as explained in the [Getting Started Guide](README.md). After the user is signed in, your game will typically show a menu screen where, amongst other options, the player can create or join a turn-based game. 

Typical choices in a turn-based game's main screen are:

* **Create a Quick Match**. This means setting up or joining a match with random opponents (also called "automatch"). When you do this, you specify the minimum and maximum number of opponents to play against, and the Google Play Games platform automatically puts the user in a match with the given number of anonymous opponents.
* **Invite Friends**. The plugin will show the standard Google Play Games invitation screen to the user, where they can pick which friends they want to play with. This screen also allows the player to add automatch opponents, so they can even mix and match (for example, they can choose to play with two specific friends and one random opponent).
* **Accept From Inbox**. The plugin will show the invitation inbox to the user, which is a standard Google Play Games screen containing all the pending invitations and ongoing matches that the user has. The user can then accept one of those invitations or matches.
* **Accept Invitation**. Accept a particular invitation whose ID you know. This is typically done in response to receiving an invitation to a match (we will cover this in mode detail later).

Each of these options corresponds to an API method, as we will see later.

## Create a Quick Match (Random Opponents)

To start a quick match with a given number of random opponents, call `CreateQuickMatch`:

```csharp
    const int MinOpponents = 1;
    const int MaxOpponents = 7;
    const int Variant = 0;  // default
    PlayGamesPlatform.Instance.TurnBased.CreateQuickMatch(MinOpponents, MaxOpponents,
        Variant, OnMatchStarted);

    // Callback:
    void OnMatchStarted(bool success, TurnBasedMatch match) {
        if (success) {
            // go to the game screen and play!
        } else {
            // show error message
        }
    }
```

## Create with Invitation Screen ("Invite friends...")

To start a match with an invitation screen where the player can choose friends to play against, call `CreateWithInvitationScreen`:

```csharp
    const int MinOpponents = 1;
    const int MaxOpponents = 7;
    const int Variant = 0;  // default
    PlayGamesPlatform.Instance.TurnBased.CreateWithInvitationScreen(MinOpponents, MaxOpponents,
        Variant, OnMatchStarted);

    // Callback:
    void OnMatchStarted(bool success, TurnBasedMatch match) {
        if (success) {
            // go to the game screen and play!
        } else {
            // show error message
        }
    }
```

## Accept From Inbox

To show the user's invitation/match inbox and allow them to select an incoming invitation to accept, call `AcceptFromInbox`.

```csharp
    PlayGamesPlatform.Instance.TurnBased.AcceptFromInbox(OnMatchStarted);
```

The inbox also allows the user to select any of their ongoing matches.

## Accept an Invitation

To accept a specific invitation whose ID you know (for example, an invitation you received via an invitation delegate), use `AcceptInvitation`:

```csharp
    Invitation invitation = ....;  // received from invitation delegate
    PlayGamesPlatform.Instance.TurnBased.AcceptInvitation(invitation.InvitationId, OnMatchStarted);
```


## List All Invitations

To get a list of all the invitations use:

```csharp

    PlayGamesPlatform.Instance.TurnBased.GetAllInvitations(
        (invites) =>
        {
            Debug.Log("Got " + invites.Length + " invites");
            string logMessage = "";
            foreach(Invitation invite in invites)
            {
                logMessage += " " + invite.InvitationId + " (" +
                    invite.InvitationType + ") from " +
                    invite.Inviter + "\n";
                if (mFirstInvite == null) {
                    mFirstInvite = invite;
                }
            }
            Debug.Log(logMessage);
    });
```

## The Match Callback

In the previous examples, the match callback we created was called `OnMatchStarted`. It receives two parameters:
a boolean indicating whether the match setup or loading was successful, and a `TurnBasedMatch` object
representing the match itself.

A typical implementation checks for failures, then launches the game screen:

```csharp
    void OnMatchStarted(bool success, TurnBasedMatch match) {
        if (success) {
            // get the match data
            byte[] myData = match.Data;
            
            // I can only make a move if the match is active and it's my turn!
            bool canPlay = (match.Status == TurnBasedMatch.MatchStatus.Active &&
                match.TurnStatus == TurnBasedMatch.MatchTurnStatus.MyTurn);
            
            // Deserialize game state from myData into scene and
            // go to gameplay screen. If canPlay == true, let user play a move; 
            // if not, they can only see the current state of the game but can't play.
        } else {
            // show error message, return to main menu
        }
    }
```

## Playing Your Turn

If it is the current user's turn in a match, you can play a turn by calling `TakeTurn`.

```csharp
    TurnBasedMatch match = ....; // received in OnMatchStarted
    
    // your representation of the new state of the game after the player
    // has taken their turn:
    byte[] myData = .....;
    
    // this indicates whose turn is next (a participant ID)
    string whoIsNext = .....;
    
    PlayGamesPlatform.Instance.TurnBased.TakeTurn(match.MatchId, myData, whoIsNext, (bool success) => {
        if (success) {
            // turn successfully submitted!
        } else {
            // show error
        }
    });
```

Deciding which participant is the next to play depends on your game's logic. You can query for a list of participants using the `Participants` property of the `TurnBasedMatch` object. It's important to take into account how automatch works: if there are open automatch slots in a game, you can call `TakeTurn` with `null` as the `whoIsNext` parameter to indicate that the next participant to play should be one of the automatch participants.

A typical implementation is to pass the turn to an automatch participant while there are automatch slots open. When there are no more automatch slots open (everyone has joined), pass the turn to an actual participant:

```csharp
    string DecideWhoIsNext(TurnBasedMatch match) {
        if (match.AvailableAutomatchSlots > 0) {
            // hand over to an automatch player
            return null;
        } else {
            // pick a player among match.Participants,
            // according to the game's logic
        }
    }
```

In a two-player game, deciding who is next can be done simply by finding a participant whose ID is not equal to the current participant' s ID:

```csharp
    string DecideWhoIsNext2p(TurnBasedMatch match) {
        if (match.AvailableAutomatchSlots > 0) {
            // there is an automatch slot open, so hand over to automatch
            return null;
        }
        
        // who is my opponent?
        foreach (Participant p in match.Participants) {
    		if (!p.ParticipantId.Equals(match.SelfParticipantId)) {
    			return p.ParticipantId;
    		}
    	}
    	
    	// log error (something is wrong!)
    }
```

For more complex games with more participants, the logic might be more complicated. When writing your game's logic to decide who is the next player to play, keep in mind that players may leave the game before it is finished, so you must take extra care not to pass the turn to a player who has left the game (always check the `Status` property of the `Participant` objects). Also, beware that until all automatch players have joined (that is, until there are no more automatch slots available), the list of participants will change.

## Finishing a Match

If, during a player's turn, you determine that the match has come to an end, call `FinishMatch`.

```csharp
    using GooglePlayGames.BasicApi.Multiplayer;

    .....;

    TurnBasedMatch match = .....;  // our current match
    byte[] finalData = .....; // match data representing the final state of the match
    
    // define the match's outcome
    MatchOutcome outcome = new MatchOutcome();
    foreach (Participant p in match.Participants) {
        // decide if participant p has won, lost or tied, and
        // their ranking (1st, 2nd, 3rd, ...):
        MatchOutcome.ParticipantResult result = .....;
        int placement = ......;
        
        outcome.SetParticipantResult(p.ParticipantId, result, placement);
    }
   
    // finish the match
    PlayGamesPlatform.Instance.TurnBased.Finish(match.MatchId, finalData, outcome, (bool success) => {
        if (success) {
            // sent successfully
        } else {
            // an error occurred
        }
    });
```

Note that when setting the outcome of the match, you can also call `SetParticipantResult` with only a result (win, lose, tie) or only a placement (1st, 2nd, 3rd). You do not necessarily have to specify both.

## Acknowledging a Finished Match

When a player finishes a match, the other participants in the match are notified to view the match results. After they do so, they each must acknoledge that they have seen the final match results. This will cause the match to no longer appear in the list of active matches for each player.

When your game starts with a match that is in the finished state, call `AcknowledgeFinish`:

```csharp
    void OnMatchStarted(bool success, TurnBasedMatch match) {
        
        // ....(other logic)....
        
        // if the match is in the completed state, acknowledge it
        if (success && match.Status == TurnBasedMatch.MatchStatus.Complete) {
            PlayGamesPlatform.Instance.TurnBased.AcknowledgeFinished(match.MatchId,
                    (bool success) => {
                if (success) {
                    // success!
                } else {
                    // show error
                }
            });
        }
```

## If Necessary, Leave the Match

If the player decides to leave an ongoing match, you can call `Leave` or `LeaveMatchDuringTurn`.

If it is **not** the player's turn, use `Leave`:

```csharp
    PlayGamesPlatform.Instance.TurnBased.Leave(match.MatchId, (bool success) => {
        if (success) {
            // successfully left
        } else {
            // error leaving match
    });
```

If it **is** the player's turn, use `LeaveDuringTurn`. You must specify the next participant to play:

```csharp
    string whoIsNext = ......; // determine who plays next
    
    PlayGamesPlatform.Instance.TurnBased.LeaveDuringTurn(match.MatchId, whoIsNext, (bool success) => {
        if (success) {
            // successfully left
        } else {
            // error leaving match
    });
```

## Register an Invitation Delegate

To be notified of incoming turn-based invitations (and also to allow the user to accept an invitation they received via push notification), register an invitation delegate in the plugin instance configuration:

```csharp

    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using UnityEngine.SocialPlatforms;

    PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
	    // registers a callback to handle game invitations.
    	.WithInvitationDelegate(OnInvitationReceived) 

	    // registers a callback for turn based match notifications.
	    .WithMatchDelegate(OnGotMatch) 

    	.Build();

    PlayGamesPlatform.InitializeInstance(config);

    // recommended for debugging:
    PlayGamesPlatform.DebugLogEnabled = true;

    // Activate the Google Play Games platform
    PlayGamesPlatform.Activate();

```

Whenever the player receives an invitation, this delegate will be invoked. The delegate is invoked with two arguments: the invitation and a boolean flag indicating whether or not the invitation should be immediately accepted:

```csharp
    // called when an invitation is received:
    public void OnInvitationReceived(Invitation invitation, bool shouldAutoAccept) {
        ...
    }
```

If `shouldAutoAccept` is `true`, accept the invitation immediately and start the game. If it is `false`, store the invitation for later and show an in-game invitation popup to alert the user that there is a pending invitation to accept. For example:

```csharp
    Invitation mIncomingInvitation = null;
    
    // called when an invitation is received:
    public void OnInvitationReceived(Invitation invitation, bool shouldAutoAccept) {
        if (shouldAutoAccept) {
            // Invitation should be accepted immediately. This happens if the user already
            // indicated (through the notification UI) that they wish to accept the invitation,
            // so we should not prompt again.
            ShowMyCustomWaitingScreen();
            PlayGamesPlatform.Instance.TurnBased.AcceptInvitation(invitation.InvitationId, OnMatchStarted);
        } else {
            // The user has not yet indicated that they want to accept this invitation.
            // We should *not* automatically accept it. Rather we store it and 
            // display an in-game popup:
            mIncomingInvitation = invitation;
        }
    }
```

If `mIncomingInvitation` is not `null`, show an in-game invitation popup on the main menu:

```csharp
    void OnGUI() {
        if (mIncomingInvitation != null) {
            // show the popup
            string who = (mIncomingInvitation.Inviter != null && 
                mIncomingInvitation.Inviter.DisplayName != null) ?
                    mIncomingInvitation.Inviter.DisplayName : "Someone";
            GUI.Label(labelRect, who + " is challenging you to a match!");
            if (GUI.Button(acceptButtonRect, "Accept!")) {
                // user wants to accept the invitation!
                ShowMyCustomWaitingScreen();
                PlayGamesPlatform.Instance.TurnBased.AcceptInvitation(
                    mIncomingInvitation.InvitationId, OnMatchStarted);
            }
            if (GUI.Button(declineButtonRect, "Decline")) {
                // user wants to decline the invitation
                PlayGamesPlatform.Instance.TurnBased.DeclineInvitation(
                    mIncomingInvitation.InvitationId);
            }
        }
    }
```

## Register a Match Delegate

Apart from invitation, your game might receive matches to play. This happens, for example, when it's the player's turn to play on a given match that was already happening. To handle these, register a match delegate:

```csharp
     
    // match delegate:
    void OnGotMatch(TurnBasedMatch match, bool shouldAutoLaunch) {
        if (shouldAutoLaunch) {
        	// if shouldAutoLaunch is true, we know the user has indicated (via an external UI)
        	// that they wish to play this match right now, so we take the user to the
        	// game screen without further delay:
            OnMatchStarted(true, match);
        } else {
        	// if shouldAutoLaunch is false, this means it's not clear that the user
        	// wants to jump into the game right away (for example, we might have received
        	// this match from a background push notification). So, instead, we will
        	// calmly hold on to the match and show a prompt so they can decide
        	mIncomingMatch = match;
        }
    }
```

The `mIncomingMatch` member variable, in this case, stores an incoming match until the player decides to act on it. You can display it as a banner on the main menu:

```csharp
    void OnGUI() {
        
        // ....(other logic)....
        
        if (mIncomingMatch != null) {
            // show banner saying ("It's your turn against [opponent]!") and
            // put show buttons for "play" and "not now".
            if (clickedPlay) {
                // play now!
                OnMatchStarted(true, mIncomingMatch);
                mIncomingMatch = null
            } else if (clickedNotNow) {
                // stop showing banner:
                mIncomingMatch = null;
            }
        }
    }
```
