# Google Play Games plugin for Unity:<br> Real-Time Multiplayer
_Copyright (c) 2014 Google Inc. All rights reserved._

This is a guide to the Real-Time Multiplayer features of the Google Play Games plugin
for Unity&reg; This documents assumes that you have thoroughly read
the [Getting Started Guide](README.md) and are familiar with how to set up your
project, sign in and perform basic API operations such as unlocking an
achievement and posting a score to a leaderboard.

Note: this project is not in any way endorsed or supervised by
Unity Technologies.

_Unity&reg; is a trademark of Unity Technologies._

_iOS is a trademark of Apple, Inc._

## Real-Time Multiplayer Overview

Real-Time multiplayer in Google Play Games is a feature that allows you to set up real-time games on the internet with  up to 4-players. The platform deals with the connection and networking infrastructure and exposes an API that allows you to send messages from one player to the other. Therefore, your game does not need to implement any low-level network connectivity code. All you have to do is implement your game logic based on exchanges of messages between players. To know more about the underlying multiplayer system, you can read the [real-time multiplayer conceptual overview](https://developers.google.com/games/services/common/concepts/realtimeMultiplayer) in the Google Play Games documentation.

To begin, implement sign-in as explained in the [Getting Started Guide](README.md). After the user is signed in, your game will typically show a menu screen where, amongst other options, the player can create or join a multiplayer game. 

In the Google Play Games real-time multiplayer terminology, a game happens in a **room**. A room is a virtual place where players get together to play a real-time game. These are the ways in which you can start or join a multiplayer room:

* **Create a Quick Game**. This means setting up or joining a room with random opponents (also called "automatch"). When you do this, you specify the minimum and maximum number of opponents to play against, and the Google Play Games platform automatically puts the user in a room with the given number of anonymous opponents.
* **Create With Invitation Screen**. The plugin will show the standard Google Play Games invitation screen to the user, where they can pick which friends they want to play with. This screen also allows the player to add automatch opponents, so they can even mix and match (for example, they can choose to play with two specific friends and one random opponent).
* **Accept From Inbox**. The plugin will show the invitation inbox to the user, which is a standard Google Play Games screen containing all the pending invitations that the user has received. The user can then accept one of those invitations to join a room.
* **Accept Invitation**. Accept a particular invitation whose ID you know. This is typically done in response to receiving an invitation to a room (we will cover this in mode detail later).

Each of these options corresponds to an API method, as we will see later. After calling one of those API methods, your game should show a wait screen (possibly with a "Cancel" button) and wait until the callback is called to indicate that you are successfully connected to the room (or that an error occurred).

After you are successfully connected to the room, you can send and receive messages to other participants. These messages are simply arrays of bytes that your game is responsible for encoding and decoding in a format of your choice. You should also handle connection/disconnection events appropriately for your game (for example, if a peer gets disconnected from the room, you should make their character disappear from the game). When the game finishes, all players must leave the room, at which point you are free to create or join a different room. You can only be in one room at any given time.

Another important part of a real-time game implementation is handling invitations. When a user receives an invitation to play a real-time game, that is reflected in the system as a push notification (on Android, it appears in the notification bar, while on iOS it appears as notification banner at the top of the screen). When they click that notification, your game is notified through an invitation delegate. Your code should implement this delegate to accept the invitation and start the game.

## Create or Join a Room

Each of the API calls below allow you to create or join a room, as described in the introduction above. In each of them, there is a `listener` parameter which is a reference to your `RealTimeMultiplayerListener`. If your class implements that interface, you can substitute `this` for this parameter. Also, the `GameVariant` parameter represents the particular variant of your game that is being played (you could establish different codes to mean different styles of gameplay -- co-op, capture-the-flag, etc). For simplicity, we use 0 to mean "default variant".

### Create a quick game (random opponents)

To start a quick game with a certain number of automatch opponents, call `CreateQuickGame`:

```csharp
    const int MinOpponents = 1, MaxOpponents = 3;
    const int GameVariant = 0;
    PlayGamesPlatform.Instance.RealTime.CreateQuickGame(MinOpponents, MaxOpponents,
                GameVariant, listener);
```

### Create with invitation screen

To show an invitation screen where the user can choose the friends they wish to play with, use `CreateWithInvitationScreen`:

```csharp
    const int MinOpponents = 1, MaxOpponents = 3;
    const int GameVariant = 0;
    PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen(MinOpponents, MaxOpponents,
                GameVariant, listener);
```

### Accept from inbox

To show the user their invitation inbox so they can pick an invitation to accept, use `AcceptFromInbox`:

```csharp
    PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(listener);
```

Once an invitation is accepted, the invitation can be accessed by using:

```csharp
    PlayGamesPlatform.Instance.RealTime.GetInvitation();
```

### Accept invitation

To accept an invitation whose ID you know (for example, an invitation delivered to you via the invitation delegate), use `AcceptInvitation`:

```csharp
    Invitation invitation = ....;  // (obtained via delegate)
    PlayGamesPlatform.Instance.RealTime.AcceptInvitation(invitationId, invitation.InvitationId);
```

We will cover this in more detail later.


## List All Invitations

To get a list of all the invitations use:

```csharp

    PlayGamesPlatform.Instance.RealTime.GetAllInvitations(
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


## Wait for Connection

After calling any of the room creation/joining methods described above,
you should show a waiting screen in your game and wait until you get a call
to the `OnRoomConnected` method of your listener.

If you want to show a progress bar while waiting, you can implement the
`OnRoomSetupProgress` method of the listener:

```csharp
    public void OnRoomSetupProgress(float progress) {
        // update progress bar
        // (progress goes from 0.0 to 100.0)
    }
```

Alternatively, you can display the standard waiting room UI by calling
ShowWaitingRoomUI() from the OnRoomSetupProgress().  When the waiting is
over, OnRoomConnected() is called as normal.

```csharp
    private bool showingWaitingRoom = false;

    public void OnRoomSetupProgress(float progress) {
        // show the default waiting room.
        if (!showingWaitingRoom) {
            showingWaitingRoom = true;
            PlayGamesPlatform.Instance.RealTime.ShowWaitingRoomUI();
        }
    }
```

If a participant declines an invitation, the callback `OnParticipantLeft` is
called.  Since this happens before the game has begun, this can cause unbalanced
calls relative to OnPeersConnected().

When the room setup is complete (or has failed), you will receive a call to
your listener's `OnRoomConnected` method.

```csharp
    public void OnRoomConnected(bool success) {
        if (success) {
            // Successfully connected to room!
            // ...start playing game...
        } else {
            // Error!
            // ...show error message to user...
        }
    }
```

## Participant IDs

Participants in a room are identified by a participant ID. This ID is only relevant and valid within the particular room, and should not be stored across games. To obtain the player's participant ID, you can call:

```csharp
    Participant myself = PlayGamesPlatform.Instance.RealTime.GetSelf();
    Debug.Log("My participant ID is " + myself.ParticipantId);
```

## List Participants

After the room is connected, you can list the room's participants by calling `GetConnectedParticipants`:

```csharp
    using System.Collections.Generic;
    
    List<Participant> participants = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants();
```

This list is guaranteed to be sorted by participant ID, so all participants in the game will get this list in the same order.

You can determine which participant is the current user by comparing the `ParticipantId` property with the participant ID of the player.

## Send Messages

To send a message to all other participant, use `SendMessageToAll`:

```csharp
    byte[] message = ....; // build your message
    bool reliable = true;
    PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliable, message);
```

The `reliable` parameter controls whether the message will be sent reliably or unreliably.

* **Reliable messages**: are guaranteed to arrive at the destination, and always arrive in order.
* **Unreliable messages**: are not guaranteed to arrive at the destination (but usually arrive). They may arrive out of order.

Unreliable messages can be considerably faster than reliable messages. Typically a game should send transient updates via unreliable messages (where missing a few updates does not affect gameplay), and send important messages  via reliable messages.

You can also send a message to a particular participant:

```csharp
    byte[] message = ....;
    bool reliable = true;
    string participantId = ....;
    PlayGamesPlatform.Instance.RealTime.SendMessage(reliable, participantId, message);
````

## Receive Messages

When you receive a message from another participant, your listener's `OnRealTimeMessageReceived` method will be called.

```csharp
    public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data) {
        // handle message! (e.g. update player's avatar)
    }
```

## Handle Connection Events

During gameplay the user may get disconnected from the room, and other participants may get disconnected or connected. Your game should handle these events and respond appropriately.

If the user gets disconnected from the room, your listener's `OnLeftRoom` method will be called:

```csharp
    public void OnLeftRoom() {
        // display error message and go back to the menu screen
        
        // (do NOT call PlayGamesPlatform.Instance.RealTime.LeaveRoom() here --
        // you have already left the room!)
    }
```

**IMPORTANT:** Due to the way the underlying platform works, the user's connection to the room will be severed if the user sends the game into the background. In this case, `OnLeftRoom()` will be called. It is not possible to reconnect to a room if this happens.

If someone else gets connected or disconnected, your listener's `OnPeersConnected` and `OnPeersDisconnected` methods will be called:

```csharp
    public void OnPeersConnected(string[] participantIds) {
        // react appropriately (e.g. add new avatars to the game)
    }
    
    public void OnPeersDisconnected(string[] participantIds) {
        // react appropriately (e.g. remove avatars from the game)
    }
```

## Leave the Room

When finished with your real-time game, leave the room:

```csharp
    PlayGamesPlatform.Instance.RealTime.LeaveRoom();
```

This will trigger a call to your listener's `OnLeftRoom`.

## Register an Invitation Delegate

To be notified of incoming invitations (and also to allow the user to accept an invitation they received via push notification), register an invitation delegate in the plugin instance configuration:

```csharp

    using GooglePlayGames;
    using GooglePlayGames.BasicApi;
    using UnityEngine.SocialPlatforms;

    PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
	    // registers a callback to handle game invitations.
    	.WithInvitationDelegate(OnInvitationReceived) 
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
            ShowWaitScreen();
            PlayGamesPlatform.Instance.RealTime.AcceptInvitation(invitation.InvitationId, listener);
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
            GUI.Label(labelRect, who + " is challenging you to a race!");
            if (GUI.Button(acceptButtonRect, "Accept!")) {
                // user wants to accept the invitation!
                ShowWaitScreen();
                PlayGamesPlatform.Instance.RealTime.AcceptInvitation(
                    mIncomingInvitation.InvitationId, listener);
            }
            if (GUI.Button(declineButtonRect, "Decline")) {
                // user wants to decline the invitation
                PlayGamesPlatform.Instance.RealTime.DeclineInvitation(
                    mIncomingInvitation.InvitationId, listener);
            }
        }
    }
```


