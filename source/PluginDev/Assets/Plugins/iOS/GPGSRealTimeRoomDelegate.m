/*
 * Copyright (C) 2014 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#import "GPGSManager.h"
#import "GPGSLogging.h"
#import "GPGSRealTimeRoomDelegate.h"

static GPGSRealTimeRoomDelegate *_sInstance = nil;

@interface GPGSRealTimeRoomDelegate()
@property (nonatomic) GPGRealTimeRoom *roomToTrack;
@property (nonatomic) NSMutableDictionary *roomDataDictionary;

@end


@implementation GPGSRealTimeRoomDelegate


+ (GPGSRealTimeRoomDelegate *)sharedInstance
{
  if (!_sInstance) {
    _sInstance = [[GPGSRealTimeRoomDelegate alloc] init];
  }
  return _sInstance;
}

- (NSMutableDictionary *)roomDataDictionary {
  if (! _roomDataDictionary) {
    _roomDataDictionary = [[NSMutableDictionary alloc] initWithCapacity:5];
  }
  return _roomDataDictionary;
}


- (void)logRoomStatus:(GPGRealTimeRoomStatus)status {
  if (status == GPGRealTimeRoomStatusDeleted) {
    LOGD((@"RoomStatusDeleted. Game is over"));
  } else if (status == GPGRealTimeRoomStatusConnecting) {
    LOGD((@"RoomStatusConnecting"));
  } else if (status == GPGRealTimeRoomStatusActive) {
    LOGD((@"RoomStatusActive. Room is ready to go"));
  } else if (status == GPGRealTimeRoomStatusAutoMatching) {
    LOGD((@"RoomStatusAutoMatching. Waiting for auto-matched players"));
  } else if (status == GPGRealTimeRoomStatusInviting) {
    LOGD((@"RoomStatusInviting! Waiting for invites to be accepted."));
  } else {
    LOGD((@"Unknown room status %d", status));
  }
}

- (void)room:(GPGRealTimeRoom *)room didChangeStatus:(GPGRealTimeRoomStatus)status {
  LOGD((@"Room changed status"));
  [self logRoomStatus:status];
  self.roomToTrack = room;
  if (status == GPGRealTimeRoomStatusDeleted) {
    // This very likely happened because the user hit cancel in the RealTimeVC
    [[GPGLauncherController sharedInstance] dismissAnimated:YES completionHandler:nil];
  }
  if (status == GPGRealTimeRoomStatusActive) {
    [[GPGLauncherController sharedInstance] dismissAnimated:YES completionHandler:nil];
  }
  if (self.statusChangedCallback) {
    self.statusChangedCallback(status);
  }
  [self sendPartipantListToUnity];
}



- (void)sendPartipantListToUnity {
  LOGD((@"Sending updated participant list to unity"));
  NSMutableArray *fakeArray = [[NSMutableArray alloc] init];
  [self.roomToTrack enumerateParticipantsUsingBlock:^(GPGRealTimeParticipant *nextPart) {
    BOOL isConnectedToRoom = (nextPart.status == GPGRealTimeParticipantStatusJoined ||
                              nextPart.status == GPGRealTimeParticipantStatusConnectionEstablished);

    NSDictionary *participantDict = @{@"participantId": nextPart.participantId,
                                      @"displayName": nextPart.displayName,
                                      @"status": @(nextPart.status),
                                      @"playerName": nextPart.displayName,
                                      @"playerId": nextPart.participantId,
                                      @"connectedToRoom": (isConnectedToRoom) ? @1 : @0};
    [fakeArray addObject:participantDict];
  }];

  NSError *error = nil;
  NSData *jsonifiedString =  [NSJSONSerialization dataWithJSONObject:fakeArray options:0 error:&error];

  if (error) {
    LOGD((@"ERROR! Couldn't Jsonify our participant %@", [error localizedDescription]));
  } else {
    NSString *returnMe = [[NSString alloc] initWithData:jsonifiedString encoding:NSUTF8StringEncoding];
    if (self.participantsChangedCallback) {
      self.participantsChangedCallback([returnMe UTF8String]);
    }
  }


}



- (void)room:(GPGRealTimeRoom *)room didChangeConnectedSet:(BOOL)connected {
  LOGD((@"Room changed connected set to %@", (connected) ? @"YES" : @"NO"));

}

- (void)room:(GPGRealTimeRoom *)room
 participant:(GPGRealTimeParticipant *)participant
didChangeStatus:(GPGRealTimeParticipantStatus)status {
  LOGD((@"Participant id %@ changed status to %d", participant.participantId, status));
  [self sendPartipantListToUnity];
}

- (void)room:(GPGRealTimeRoom *)room
     didReceiveData:(NSData *)data
    fromParticipant:(GPGRealTimeParticipant *)participant
           dataMode:(GPGRealTimeDataMode)dataMode {
  LOGD((@"Received data from %@", participant.participantId));
  if (self.dataReceivedCallback) {
    self.dataReceivedCallback([participant.participantId UTF8String], [data bytes], data.length,
                              (dataMode == GPGRealTimeDataModeReliable) ? GPGSTRUE : GPGSFALSE);
  }
}

- (void)room:(GPGRealTimeRoom *)room
didSendReliableId:(int)reliableId
toParticipant:(GPGRealTimeParticipant *)participant
     success:(BOOL)success {
  LOGD((@"Reliable data sent to %@? %@", participant.participantId, (success) ? @"YES" : @"NO"));
}

- (void)room:(GPGRealTimeRoom *)room didFailWithError:(NSError *)error {
  LOGD((@"Room failed with error"));
  if (self.roomErrorCallback) {
    self.roomErrorCallback([error.localizedDescription UTF8String], error.code);
  }
}

- (void)sendMessageReliable:(BOOL)isRealible
                       data:(NSData *)data
                toEverybody:(BOOL)toEveryone
            orParticipantId:(NSString *)participantId {
  if (toEveryone) {
    if (isRealible) {
      [self.roomToTrack sendReliableDataToAll:data];
    } else {
      [self.roomToTrack sendUnreliableDataToAll:data];
    }
  } else {
    NSArray *participantsToReceivedData = @[ participantId ];
    if (isRealible) {
      [self.roomToTrack sendReliableData:data toParticipants:participantsToReceivedData];
    } else {
      [self.roomToTrack sendUnreliableData:data toParticipants:participantsToReceivedData];
    }
  }
}

- (NSString *)getLocalParticpantId {
  return self.roomToTrack.localParticipant.participantId;
}

- (void)didReceiveRealTimeInviteForRoom:(GPGRealTimeRoomData *)roomData {

  [self.roomDataDictionary setObject:roomData forKey:roomData.roomID];
  self.pushNotificationCallback(GPGSTRUE, [roomData.roomID UTF8String],
                                [roomData.creationDetails.participant.participantId UTF8String],
                                [roomData.creationDetails.participant.displayName UTF8String],
                                roomData.config.variant);


}

- (void)declineRoomWithId:(NSString *)roomId {
  GPGRealTimeRoomData *roomData = (GPGRealTimeRoomData *)[self.roomDataDictionary objectForKey:roomId];
  [self.roomDataDictionary removeObjectForKey:roomId];
  NSLog(@"Declining room with Id %@", roomId);
  [GPGRealTimeRoomMaker declineRoomFromData:roomData completionHandler:^(GPGRealTimeRoomData *data, NSError *error) {
    if (error) {
      NSLog(@"**ERROR declining room: %@", [error localizedDescription]);
    } else {
      NSLog(@"Successfully declined room");
    }
  }];

}

- (void)acceptRoomWithId:(NSString *)roomId {
  GPGRealTimeRoomData *roomData = (GPGRealTimeRoomData *)[self.roomDataDictionary objectForKey:roomId];
  [self.roomDataDictionary removeObjectForKey:roomId];
  NSLog(@"Joining room with Id %@", roomId);
  NSLog(@"Room desription is %@", roomData.roomDescription);
  [GPGRealTimeRoomMaker joinRoomFromData:roomData];
}


- (void)leaveRoom {
  if (self.roomToTrack && self.roomToTrack.status != GPGRealTimeRoomStatusDeleted) {
    [self.roomToTrack leave];
  }
}

@end
