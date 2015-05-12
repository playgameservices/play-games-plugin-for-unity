# NearbyConnections
 Google Play Games plugin for Unity: Nearby Connections

<strong>Copyright (c) 2015 Google Inc. All rights reserved.</strong>

This is a guide to the Nearby connections feature of the Google Play Games
plugin for Unity®.

<em>Note: this project is not in any way endorsed or supervised by Unity
Technologies.</em>

<em>Unity® is a trademark of Unity Technologies.</em>

The Nearby Connections API enables your app to easily discover other devices on
a local network and connect to each other and exchange messages in real-time. 

 This functionality is especially useful for two types of user experiences:

<strong>Local multiplayer gaming:</strong> Allows one player to set up a local multiplayer game and let other players on
the network join it. Your app can also allow a player to start an in-game
mission when enough nearby participants join.

<strong>Multi-screen gaming</strong>: Allows players to use their phone or tablet as a game controller to play
games displayed on a nearby large-screen Android device, such as Android TV.
Your app can also enable players to see a customized game screen on their
personal device while all nearby participants see a shared common view on a
table-top Android device.

For more in-depth information see [https://developers.google.com/games/services/android/nearby](https://developers.google.com/games/services/android/nearby).

# Requirements

The Nearby Connections API is implemented in version 7+ of Google Play Services
(also known as Google Play Services SDK 23).  At this time, Nearby Connections
is only available on Android.

# Using the API

## Configuring your application

Nearby connections works without requiring you to configure game services in
the Google Play Developer console.  This means players do not have to sign in
before playing.  If you want to use Play Game Services as well as Nearby
Connections, Both APIs need to be initialized and managed independently.

There is one additional configuration value that needs to be set for your game
in order to use Nearby Connections, the <em>Service ID</em>. This ID uniquely identifies the nearby connections for this game.  It should
be unique.  A good practice is to base it on the application bundle ID, or game
namespace, such as <em>com.<yourcompany>.awesomegame.</em>

The Service ID is set by going to <strong>Windows > Google Play Games > Setup > Nearby Connections Setup…</strong>

## Initialize the Nearby Connections API

The Nearby  Connections API needs to be initialized before using.  This is done
by calling Initialize:


```c#
void Awake() {
    PlayGamesPlatform.InitializeNearby((client) =>
    {
        Debug.Log("Nearby connections initialized");
    });
}
```
## Advertise

To establish a connection between two or more devices, one device must
advertise its service before  one or more devices connect to it. The
advertising device is the 'host' in a multiplayer game, while the connecting
device is the client.


```c#
List<string> appIdentifiers = new List&lt;string>();
appIdentifiers.Add(PlayGamesPlatform.Nearby.GetAppBundleId());
     PlayGamesPlatform.Nearby.StartAdvertising(
                "Awesome Game Host",  // User-friendly name
                 appIdentifiers,  // App bundle Id for this game
                TimeSpan.FromSeconds(0),// 0 = advertise forever
                (AdvertisingResult result) =>
        			{
            		    Debug.Log("OnAdvertisingResult: " + result);
        			},
               	(ConnectionRequest request) =>
        			{
            		    Debug.Log("Received connection request: " +
                            request.RemoteEndpoint.DeviceId + " " +
                            request.RemoteEndpoint.EndpointId + " " +
                            request.RemoteEndpoint.Name)
                    }
                );
```
## Discovery

The discovery process allows a device to find devices advertising nearby
connections for a specific service ID. The service ID parameter you pass into
Connections.startDiscovery() should match the value provided in the manifest of
the advertising app. See the previous section for information on how apps
advertise nearby games.

```c#
IDiscoveryListener listener;

PlayGamesPlatform.Nearby.StartDiscovery(
                PlayGamesPlatform.Nearby.GetServiceId(),
                TimeSpan.FromSeconds(0),
                listener);
```

The IDiscoveryListener interface defines two methods that are called as the
discovered endpoints change:

```c#
public void OnEndpointFound(EndpointDetails discoveredEndpoint)
{
    Debug.Log("Found Endpoint: " +
              discoveredEndpoint.DeviceId + " " +
              discoveredEndpoint.EndpointId + " " + 
              discoveredEndpoint.Name);
}
```

And when an endpoint stops advertising:

```c#
public void OnEndpointLost(string lostEndpointId)
{
    Debug.Log("Endpoint lost: " + lostEndpointId);
}
```

## Sending Connection Request

After your app discovers another app that is advertising the requested service
ID, you can initiate a connection between the devices.

```c#
 PlayGamesPlatform.Nearby.SendConnectionRequest(
                "Local Game player",  // the user-friendly name
                remote.EndpointId,	// the discovered endpoint	
                playerData,	// byte[] of data
                (response) => {
				        Debug.Log("response: " +
                                    response.ResponseStatus);
                },
                (IMessageListener)messageListener);
```

IMessageListener is an interface that is called when messages are received:

```c#
        void OnMessageReceived(string remoteEndpointId, byte[] data,
                       bool isReliableMessage);

        void OnRemoteEndpointDisconnected(string remoteEndpointId);
```

## Responding to Connection Request

One of the parameters to StartAdvertising is a callback for handling incoming
connection requests.  When a connection request is received, it must be
accepted or rejected.

To accept a request:

```c#
PlayGamesPlatform.Nearby.AcceptConnectionRequest(
                request.RemoteEndpoint.EndpointId,
                (byte[])responseData,
                (IMessageListener)messageListener);
```

To reject a request:

```c#
PlayGamesPlatform.Nearby.RejectConnectionRequest(
                request.RemoteEndpoint.EndpointId);
```
## Sending and Receiving Messages

After devices are connected, they can send and receive messages to update game
state and transfer input, data, or events from one device to another. Hosts can
send messages to any number of clients, and clients can send messages to the
host. If a client needs to communicate with other clients, you can send a
message to the host to relay the information to the recipient client.

To send a message, call <em><strong>SendReliable()</strong></em> and pass in the appropriate endpoint IDs.
The payload parameter is a byte array of up to <em><strong>MaxReliableMessagePayloadLength</strong></em> 
bytes long that holds your message data. Reliable messages are guaranteed to
be received in the order they were sent, and the system retries sending the
message until the connection ends.

Messages can be sent to multiple remote devices if the local device is acting
as the host (meaning StartAdvertising() was called).  Messages can only be sent
to the remote host if the local device called StartDiscovery()).


```c#
List<string> endpointIds;
byte[] payload;
PlayGamesPlatform.Nearby.SendReliable(endpointIds, payload);
```
Smaller messages can be sent using <em><strong>SendUnreliable()</strong></em> the maximum message size
is limited to <em><strong>MaxUnreliableMessagePayloadLength</strong></em>.

```c#
List<string> endpointIds;
byte[] payload;
PlayGamesPlatform.Nearby.SendUnreliable(endpointIds, payload);
```
## Cleaning up

Once  the connections have been made, advertising is stopped by calling:

```c#
	PlayGamesPlatform.Nearby.StopAdvertising();
```

Likewise, discovery is stopped by calling:

```c#
PlayGamesPlatform.Nearby.StopDiscovery(serviceId);
```

Once the communication is done, the connection can be closed by calling:

```c#
	PlayGamesPlatform.Nearby.DisconnectFromEndpoint(endpointId);
```

To stop all Nearby connections activity, call:

```c#
    PlayGamesPlatform.Nearby.StopAllConnections();
```

# Sample: Nearby Droids

The Nearby connections sample is found in samples/NearbyDroids.  This is a
complete 2D game that demonstrates how to advertise and discover players,
establish the connections and maintain the gamestate across multiple levels.

<strong><u>Note</u></strong>: This sample requires additional tags, layers and sorting layers to be
defined.  If you are running Unity 5 or greater, this script will run
automatically and add the required elements.  If you are running an older
version of Unity, please refer to the script in
Assets/NearbyDroids/Editor/InitializeTagsAndLayers.cs  for the names of the
layers and tags to create.

## Setup

  1. Create a new 2D game in Unity
  2. Import the Google Play Games Services package (Assets > Import Package > Custom
Package).
  3. Import the NearbyDroids package
  4. Change the build type to Android (File > Build Settings, then Switch player to
Android).
  5. Add the scenes to the scene list in the build settings dialog.  There are 2
scenes in Assets/NearbyDroids.  MainMenu should be first in the list, and game
should be second.
  6. Set the player settings (click Player Settings) and change the bundle Id to
your bundle Id.  It should be unique to your game, and be something like
com.<yourcompany>.game
  7. Close the Player settings
  8. Set the service id for Nearby Connections. Click Window > Google Play Games >
Setup > Nearby Connections Setup...
  9. Enter the serviceId.  It should be similar (or the same) as the bundle Id.
  10. Press Setup and then close the dialog.

## Run

Build and run the sample - using two devices.

Menu Items that are displayed:

  * Single player - Plays the game standalone.
  * Start Multiplayer - Starts a multiplayer game as the host (advertising)
  * Join Multiplayer - Starts a multiple player game as a guest (discovery)

When Starting a Multiplayer game there are some settings to fill out:

  * Room name - this is the room name shown to players when joining.
  * Enable auto-connect - this allows players to connect automatically.
  * Allow joining during game play - this allows players to join the game in
progress.
  * Player Name - this is your player's name
  * Select Character - this is your player's avatar in the game.
  * Press start to start.

If you unchecked the "enable auto-connect", there is a lobby page where you can
select which players to allow to join the game.

When Joining a Multiplayer game:

  * Player Name - this is your player's name
  * Select Character - this is your player's avatar in the game.
  * Press start to start looking for games to join.  On the lobby screen, select
the game to join and then press start.

  1. Game play is to move (players move towards the tap) in the x and y directions
(no diagonal movement).
  2.   3. Avoid the green droids.  These droids will explode if they crash into each
other or are zapped.
  4.   5. Pick up coins for points.
  6.   7. When all the green droids are gone, the exit portal appears.  Use it to go to
the next level.
  8.   9. The game is over when all players have a negative score.

## Game Design

The game is comprised of 3 major parts:

## Menuing and setup

The menus are developed using the Unity's auto-layout features that
automatically layout child elements.  This allows for the dynamic addition of
prefab'ed elements for lists.  The multiplayer actions start and join cause the
start advertising and StartDiscovery methods respectively.  If the game does
not allow joining during game play, the callback for OnConnectionRequest
displays the player in the lobby, once the players are selected, the Accept
connection method is called to establish the connection.

Similarly, the join connection starts discovery and displays the "rooms"
discovered.  Once a room is selected, the scene is switched to the game scene.
If the connection is rejected, or there is some other error, the game is over
and the player can hit "Leave" to go back to the menu.

## Game play

Game play is modeled after a standard 2D turn based game where the player gets
a chance to move, then the enemies take a turn moving. If the enemies collide
with each other or with the "deadly" zapper, they explode.  Once all the
enemies are destroyed, the exit portal is displayed.

If the player collides with an enemy or a zapper, they lose points.  Once
points go below 0, the game is over for that player.  If another player
survives to get to the exits, the zapped players are healed on the next level.

The GameManager class controls the communication of the game state across all
the players.  The key events are:

When the game is hosting the game room:

  1. On level initialization the game state is recorded.  This is saved and sent to
all remote players.
  2. On a timer, all changes that have been recorded since the last change are sent
to all remote players.  These changes include all the changes made by all the
players.  This is how the changes between remote players is communicated to
each other.
  3. When a player exits the level, the scene is reloaded and the process starts
over.

When the game is joining another room:

  1. On level initialization the game state is used to build the level.
  2. If it is the players' turn to move, the local player is moved and the change is
recorded.
  3. On a timer, the changes are sent to the room.
  4. When a message is received, it is parsed and each change is applied to the
scene.  If the message contains a gamestate with a new level, the scene is
reloaded and the process starts again.

## Nearby Connections

The principal goal of the sample is how to use Nearby connections.  There are 3
main classes for how this works:

  1. NearbyPlayer

Represents an endpoint.  This is a tuple of deviceId, enpointId, and name.  It
represents the collection of identities that are used in Nearby.  There uses
are:

  * DeviceId is a stable ID for the device.  It never changes.  It is useful for
using as a key representing a player or a room.
  * EndpointId is specific to a connection.  The endpoint Id changes each time it
is discovered, or a connection is requested.  It is only used to identify a
specific connection.
  * Name is a human friendly name.  It is set on the call to StartDiscovery or
StartAdvertising.  It should only be used to display a label within the game
play.

  2. NearbyRoom

Represents the entity being advertised.  Players discover rooms and
subsequently join a room.  It serves as the central "hub" in the hub and spoke
model of Nearby connections.

  3. GameDataMessage

Contains the data structures that are sent and received.  This sample uses C#
serialization, but there is nothing special about it other than convenience.
It could be replaced with JSON, or some other serialization.  There are two
fields in the message.  The first is a game state indicating the scores, level,
and serial number of the message.  The second is a list of ItemState objects
which contain the name, enabled state, and transform information.  This list is
only the items that have changed since the last message.

## Conclusion

Some other design choices could be explored:

  1. Event driven communication vs. a timer.  The timer was used primarily for
simplicity. 
  2. Alternate message encodings (which will affect processing speed,
interoperability)
  3. Custom tiles/avatars - allow players to create their own avatar vs. using
predefined.
  4. More creative animations and sounds.
