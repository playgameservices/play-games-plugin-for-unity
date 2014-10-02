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

#import "GPGSTurnBasedMatchDelegate.h"

static GPGSTurnBasedMatchDelegate *_sInstance = nil;

@interface GPGSTurnBasedMatchDelegate()
@property (nonatomic) NSMutableDictionary *pendingMatchesDictionary;
@end

@implementation GPGSTurnBasedMatchDelegate

+ (GPGSTurnBasedMatchDelegate *)sharedInstance
{
  if (!_sInstance) {
    _sInstance = [[GPGSTurnBasedMatchDelegate alloc] init];
  }
  return _sInstance;
}


+ (NSString *)getJsonStringFromMatch:(GPGTurnBasedMatch *)match {
  // TODO: Consider using a third-party library like JSONModel here. This seems a bit
  // fragile

  NSMutableDictionary *matchAsDictionary =
  [@{
     @"matchId": match.matchId,
     @"matchData": (match.data) ? [match.data base64EncodedStringWithOptions:0] : [NSNull null],
     @"canRematch": @YES,  // TODO: Fix this
     @"availableAutomatchSlots": (match.matchConfig.minAutoMatchingPlayers) ? @(match.matchConfig.minAutoMatchingPlayers) : @0,
     @"localParticipantId": match.localParticipantId,
     @"pendingParticipantId": (match.pendingParticipant) ? match.pendingParticipant.participantId : @"",
     @"turnStatus": @(match.userMatchStatus),
     @"matchStatus": @(match.status),
     @"variant": @(match.matchConfig.variant)
     } mutableCopy];

  
  NSMutableArray *participantArray = [[NSMutableArray alloc] initWithCapacity:match.participants.count];
  for (GPGTurnBasedParticipant *participant in match.participants) {
    NSDictionary *participantAsDictionary =
    @{
      @"displayName": participant.player.displayName,
      @"participantId": participant.participantId,
      @"status": @(participant.status),
      @"player": @{
          @"playerDisplayName": participant.player.displayName,
          @"playerId": ((participant.player.playerId) ? participant.player.playerId : @""),
          },
      @"isConnectedToRoom": @(YES) // TODO: Fix this
      };
    [participantArray addObject:participantAsDictionary];
  }

  [matchAsDictionary setObject:participantArray forKey:@"participants"];

  NSError *error = nil;
  NSData *jsonifiedData = [NSJSONSerialization dataWithJSONObject:matchAsDictionary options:0 error:&error];
  if (error) {
    NSLog(@"Error! Coldn't jsonify data. %@",[error localizedDescription]);
  }
  return [[NSString alloc] initWithData:jsonifiedData encoding:NSUTF8StringEncoding];
}

- (NSMutableDictionary *)pendingMatchesDictionary {
  if (! _pendingMatchesDictionary) {
    _pendingMatchesDictionary = [[NSMutableDictionary alloc] initWithCapacity:5];
  }
  return _pendingMatchesDictionary;
}


- (void)setupWithParent:(UIViewController *)parent callbackId:(int32_t)callbackId unityCallback:(GPGTurnBasedMatchCreateCallback)callback {
  self.parentVc = parent;
  self.callbackId = callbackId;
  self.getMatchCallback = callback;
}

#pragma mark - GPGTurnBasedMatchListLauncherDelegate

- (void)turnBasedMatchListLauncherDidJoinMatch:(GPGTurnBasedMatch *)match {
  self.getMatchCallback([[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:match] UTF8String], GPGSFALSE, self.callbackId);
}

- (void)turnBasedMatchListLauncherDidSelectMatch:(GPGTurnBasedMatch *)match {
  NSLog(@"Match selected.");
  self.getMatchCallback([[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:match] UTF8String], GPGSFALSE, self.callbackId);
}

- (void)turnBasedMatchListLauncherDidRematch:(GPGTurnBasedMatch *)match {
  NSLog(@"Did rematch was picked");
  self.getMatchCallback([[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:match] UTF8String], GPGSFALSE, self.callbackId);
}

- (void)turnBasedMatchListLauncherDidDeclineMatch:(GPGTurnBasedMatch *)match {
  NSLog(@"Did decline match was picked. No further action required");
}

#pragma mark - GPGPlayerPickerDelegate

- (int)minPlayersForPlayerPickerLauncher {
  return self.minPlayers;
}

- (int)maxPlayersForPlayerPickerLauncher {
  return self.maxPlayers;
}

/**
 * Called when the user finishes picking people.
 *
 *      @param players An array of IDs with type |NSString| corresponding to the player chosen.
 *      @param autoPickPlayerCount The number of auto-pick players selected by the user.
 */
- (void)playerPickerLauncherDidPickPlayers:(NSArray *)players
                       autoPickPlayerCount:(int)autoPickPlayerCount {
  for (NSString *nextPlayerId in players) {
    NSLog(@"This is who we picked %@", nextPlayerId);
  }

  GPGMultiplayerConfig *matchConfigForCreation = [[GPGMultiplayerConfig alloc] init];
  matchConfigForCreation.invitedPlayerIds = players;
  matchConfigForCreation.minAutoMatchingPlayers = autoPickPlayerCount;
  matchConfigForCreation.maxAutoMatchingPlayers = autoPickPlayerCount;


  [GPGTurnBasedMatch createMatchWithConfig:matchConfigForCreation completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    if (error) {
      self.getMatchCallback(NULL, GPGSTRUE, self.callbackId);
    } else {
      self.getMatchCallback([[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:match] UTF8String], GPGSFALSE, self.callbackId);
    }
  }];
}

#pragma mark - GPGLauncherDelegate

- (void)launcherDismissed {
  self.getMatchCallback(NULL, GPGSTRUE, self.callbackId);
}

# pragma mark - Invitation Delegate methods

- (void)didReceiveTurnBasedInviteForMatch:(GPGTurnBasedMatch *)match
                              participant:(GPGTurnBasedParticipant *)participant
                     fromPushNotification:(BOOL)fromPushNotification
{
  NSLog(@"Received turn based invite.");
  if (fromPushNotification && self.invitationReceivedCallback) {

    GPGTurnBasedParticipant *invitingParticipant =
    [match participantForId:match.lastUpdateParticipant.participantId];
    [self.pendingMatchesDictionary setObject:match forKey:match.matchId];

    self.invitationReceivedCallback(GPGSFALSE,
                                    [match.matchId UTF8String],
                                    [invitingParticipant.participantId UTF8String],
                                    [invitingParticipant.player.displayName UTF8String],
                                    match.matchConfig.variant
                                    );
  }
}

- (void)didReceiveTurnEventForMatch:(GPGTurnBasedMatch *)match
                        participant:(GPGTurnBasedParticipant *)participant
               fromPushNotification:(BOOL)fromPushNotification
{
  NSLog(@"Received notification that it's my turn!");
  if (fromPushNotification && self.yourTurnCallback) {
    self.yourTurnCallback(GPGSFALSE,
                          [[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:match] UTF8String]);
  }

}

- (void)matchEnded:(GPGTurnBasedMatch *)match
       participant:(GPGTurnBasedParticipant *)participant
fromPushNotification:(BOOL)fromPushNotification
{
  NSLog(@"Received notification that the game is over!");
  if (fromPushNotification && self.yourTurnCallback) {
    self.yourTurnCallback(GPGSTRUE,
                          [[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:match] UTF8String]);
  }

}

- (void)failedToProcessMatchUpdate:(GPGTurnBasedMatch *)match error:(NSError *)error
{
  
}

- (GPGTurnBasedMatch *)extractMatchWithId:(NSString *)matchId {
  GPGTurnBasedMatch *match = (GPGTurnBasedMatch *)[self.pendingMatchesDictionary objectForKey:matchId];
  [self.pendingMatchesDictionary removeObjectForKey:matchId];
  return match;
}



@end
