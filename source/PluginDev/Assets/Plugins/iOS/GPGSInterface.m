/*
 * Copyright (C) 2013 Google Inc.
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

#import "GPGSInterface.h"
#import "GPGSManager.h"
#import "GPGSLogging.h"
#import "GPGSRealTimeRoomDelegate.h"
#import "GPGSTurnBasedMatchDelegate.h"
#import "UnityAppController.h"
#import "GPGSTypedefs.h"

#import <GooglePlayGames/GooglePlayGames.h>

// This file defines entry points to be called from C#.

GPGSBOOL GPGSAuthenticateWithCallback(GPGSSuccessCallback cb, BOOL silently) {
  LOGD((@"GPGSAuthenticateWithCallback."));
  BOOL tryingSilentSignIn = [[GPGSManager instance] authenticateWithCallback:cb silently:silently];
  return (tryingSilentSignIn) ? GPGSTRUE : GPGSFALSE;
}

void GPGSEnableDebugLog(GPGSBOOL enable) {
  GPGSDebugLogEnabled = enable ? YES : NO;
  if (enable) {
    LOGD((@"Debug logs enabled!"));
  }
}

void _NSStringToUTF8Buf(NSString *s, char *buf, int32_t bufSize) {
  memset(buf, 0, bufSize);
  NSData *data = [s dataUsingEncoding:NSUTF8StringEncoding];
  if (data != NULL && [data bytes] != NULL) {
    int dataLength = [data length];
    const char* dataBytes = [data bytes];
    int to_copy = bufSize < dataLength ? bufSize : dataLength;
    memcpy(buf, dataBytes, to_copy);
    buf[bufSize-1] = 0;
  }
}

void GPGSGetPlayerId(char *buf, int32_t bufSize) {
  _NSStringToUTF8Buf([[GPGSManager instance] playerId], buf, bufSize);
}

void GPGSGetPlayerName(char *buf, int32_t bufSize) {
  _NSStringToUTF8Buf([[GPGSManager instance] playerName], buf, bufSize);
}

void GPGSSignOut() {
  LOGD((@"GPGSSignOut"));
  [[GPGSManager instance] signOut];
}

GPGSBOOL GPGSQueryAchievement(const char *achId_s, GPGSBOOL *outIsIncremental,
                              GPGSBOOL *outIsRevealed, GPGSBOOL *outIsUnlocked,
                              int32_t *outCurSteps, int32_t *outTotalSteps) {
  LOGD((@"GPGSQueryAchievement %s", achId_s));
  NSString *achId = [[NSString alloc] initWithUTF8String:achId_s];
  __block GPGAchievementMetadata *aMetadata;
  dispatch_sync(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
    [GPGAchievementMetadata metadataForAchievementId:achId completionHandler:^(GPGAchievementMetadata *metadata, NSError *error) {
      aMetadata = metadata;
    }];
  });

  if (NULL != outIsIncremental) {
    *outIsIncremental = ([aMetadata type] == GPGAchievementTypeIncremental);
  }

  if (NULL != outIsRevealed) {
    *outIsRevealed = ([aMetadata state] == GPGAchievementStateRevealed ||
                      [aMetadata state] == GPGAchievementStateUnlocked);
  }

  if (NULL != outIsUnlocked) {
    *outIsUnlocked = ([aMetadata state] == GPGAchievementStateUnlocked);
  }

  if (NULL != outCurSteps) {
    *outCurSteps = [aMetadata completedSteps];
  }

  if (NULL != outTotalSteps) {
    *outTotalSteps = [aMetadata numberOfSteps];
  }

  return GPGSTRUE;
}

void GPGSUnlockAchievement(const char *achId_s, GPGSSuccessCallback callback, int32_t userdata) {
  LOGD((@"GPGSUnlockAchievement: %s", achId_s));
  NSString *achId = [[NSString alloc] initWithUTF8String:achId_s];
  [[GPGAchievement achievementWithId:achId] unlockAchievementWithCompletionHandler:^(BOOL newlyUnlocked, NSError *error) {
    if (error) {
      LOGE((@"Error unlocking achievement %s: %@", achId_s, [error userInfo]));
    }
    if (callback) {
      (*callback)(error ? GPGSFALSE : GPGSTRUE, userdata);
    }
  }];
}

void GPGSRevealAchievement(const char *achId_s, GPGSSuccessCallback callback, int32_t userdata) {
  LOGD((@"GPGSRevealAchievement: %s", achId_s));
  NSString *achId = [[NSString alloc] initWithUTF8String:achId_s];
  [[GPGAchievement achievementWithId:achId] revealAchievementWithCompletionHandler:^(GPGAchievementState state, NSError *error) {
    if (error) {
      LOGE((@"Error revealing achievement %s: %@", achId_s, [error userInfo]));
    }
    if (callback) {
      (*callback)(error ? GPGSFALSE : GPGSTRUE, userdata);
    }
  }];
}

void GPGSIncrementAchievement(const char *achId_s, int32_t steps, GPGSSuccessCallback callback, int32_t userdata) {
  LOGD((@"GPGSIncrementAchievement: %s, %d steps", achId_s, steps));
  NSString *achId = [[NSString alloc] initWithUTF8String:achId_s];
  [[GPGAchievement achievementWithId:achId] incrementAchievementNumSteps:steps
                                                       completionHandler:^(BOOL newlyUnlocked, int currentSteps, NSError *error) {
                                                         if (error) {
                                                           LOGE((@"Error incrementing achievement %s: %@", achId_s, [error userInfo]));
                                                         }
                                                         if (callback) {
                                                           (*callback)(error ? GPGSFALSE : GPGSTRUE, userdata);
                                                         }
                                                       }];
}

static UIViewController* GetUnityViewController() {
  return ((UnityAppController*)[UIApplication sharedApplication].delegate).rootViewController;
}

void GPGSShowAchievementsUI() {
  LOGD((@"GPGSShowAchievementsUI"));
  [[GPGLauncherController sharedInstance] presentAchievementList];
}

void GPGSShowLeaderboardsUI(const char *lbId_s) {
  LOGD((@"GPGSShowLeaderboardsUI %s", lbId_s));

  NSString* lbId = nil;
  if (lbId_s) {
    lbId = [[NSString alloc] initWithUTF8String:lbId_s];
  }

  if (lbId) {
    [[GPGLauncherController sharedInstance] presentLeaderboardWithLeaderboardId:lbId];
  } else {
    [[GPGLauncherController sharedInstance] presentLeaderboardList];
  }
}

void GPGSSubmitScore(const char *lbId_s, int64_t score, GPGSSuccessCallback callback, int32_t userdata) {
  LOGD((@"GPGSSubmitScore %s, score %lld", lbId_s, (long long)score));
  NSString *lbId = [[NSString alloc] initWithUTF8String:lbId_s];
  GPGScore *myScore = [[GPGScore alloc] initWithLeaderboardId:lbId];
  myScore.value = score;
  [myScore submitScoreWithCompletionHandler:^(GPGScoreReport *report, NSError *error) {
    if (error) {
      LOGE((@"Error submitting score %lld to lb %s: %@", (long long)score, lbId_s, [error userInfo]));
    }
    if (callback) {
      (*callback)(error ? GPGSFALSE : GPGSTRUE, userdata);
    }
  }];
}


NSData* _GPGSResolveConflict(int32_t slot, NSData *localState, NSData *remoteState, GPGSStateConflictCallback cb) {
  LOGD((@"GPGSResolveConflict: cloud save conflict!"));
  if (cb) {
    LOGD((@"GPGSResolveConflict: attempting to resolve."));
    GPGSBUF resolvBuf = (GPGSBUF) malloc(kCloudSaveSize);

    GPGSCBUF localBuf = localState ? [localState bytes] : NULL;
    GPGSCBUF remoteBuf = remoteState ? [remoteState bytes] : NULL;
    int localBufSize = localState ? [localState length] : 0;
    int remoteBufSize = remoteState ? [remoteState length] : 0;

    int resolvedDataSize = (*cb)(slot, localBuf, localBufSize, remoteBuf, remoteBufSize,
                                 resolvBuf, kCloudSaveSize);
    LOGD((@"GPGSResolveConflict: conflict callback returned, resolved data %p, size %d",
          resolvBuf, resolvedDataSize));
    NSData *result = [NSData dataWithBytes:resolvBuf length:resolvedDataSize];
    free(resolvBuf);
    return result;
  } else {
    return localState ? localState : remoteState;
  }
}

void GPGSUpdateState(int32_t slot, GPGSCBUF buf, int32_t dataSize, GPGSUpdateStateCallback updateCb,
                     GPGSStateConflictCallback conflictCb) {
  LOGD((@"GPGSUpdateState slot %d, data %p, len %d, cb %p,%p", slot, buf, dataSize, updateCb, conflictCb));
  NSData *data = [NSData dataWithBytes:buf length:dataSize];

  GPGAppStateModel *model = [GPGManager sharedInstance].applicationModel.appState;
  NSNumber *slotNumber = [NSNumber numberWithInt:slot];
  [model setStateData:data forKey:slotNumber];

  LOGD((@"GPGSUpdateState: attempting to save to the cloud."));
  [model updateForKey:slotNumber completionHandler:^(GPGAppStateWriteStatus status, NSError *error) {
    if (status == GPGAppStateWriteStatusSuccess) {
      LOGD((@"GPGSUpdateState: cloud write success!"));
      if (updateCb) {
        (*updateCb)(GPGSTRUE, slot);
      }
    } else {
      LOGD((@"GPGSUpdateState: cloud write failure, %d", status));
      if (error) {
        LOGD((@"GPGSUpdateState: error: %@", [error userInfo]));
      }
      LOGW((@"Cloud save failed (%d).", status));
      if (updateCb) {
        (*updateCb)(GPGSFALSE, slot);
      }
    }
  } conflictHandler:^NSData *(NSNumber *key, NSData *localState, NSData *remoteState) {
    return _GPGSResolveConflict(slot, localState, remoteState, conflictCb);
  }];
}

void GPGSLoadState(int32_t slot, GPGSLoadStateCallback loadCb, GPGSStateConflictCallback conflictCb) {
  LOGD((@"GPGSLoadState slot %d, cb %p, %p", slot, loadCb, conflictCb));
  GPGAppStateModel *model = [GPGManager sharedInstance].applicationModel.appState;
  NSNumber *slotNumber = [NSNumber numberWithInt:slot];

  LOGD((@"GPGSLoadState attempting to load state."));
  [model loadForKey:slotNumber completionHandler:^(GPGAppStateLoadStatus status, NSError *error) {
    if (status == GPGAppStateLoadStatusNotFound) {
      // Data doesn't exist yet. We interpret it as a success, but with null data.
      LOGD((@"GPGSLoadState: cloud load success (but no data there)"));
      if (loadCb) {
        (*loadCb)(GPGSTRUE, slot, NULL, 0);
      }
    } else if (status == GPGAppStateLoadStatusSuccess) {
      NSData *data = [model stateDataForKey:slotNumber];
      GPGSCBUF buf = data ? [data bytes] : NULL;
      int bufLen = data ? [data length] : 0;
      LOGD((@"GPGSLoadState: cloud load success, %p, %d bytes.", buf, bufLen));
      if (loadCb) {
        (*loadCb)(GPGSTRUE, slot, buf, bufLen);
      }
    } else {
      LOGD((@"GPGSLoadState: cloud load failed (%d)", status));
      LOGW((@"Cloud load failed (%d).", status));
      if (loadCb) {
        (*loadCb)(GPGSFALSE, slot, NULL, 0);
      }
    }
  } conflictHandler:^NSData *(NSNumber *key, NSData *localState, NSData *remoteState) {
    return _GPGSResolveConflict(slot, localState, remoteState, conflictCb);
  }];
}

# pragma mark - RTMP calls
void GPGSRegisterInviteDelegate(GPGSPushNotificationCallback pushNotificationCallback) {
  // Request push notifications!
  NSLog(@"Requesting a push notification and registering delegates!");
  [[UIApplication sharedApplication] registerForRemoteNotificationTypes:
   (UIRemoteNotificationTypeBadge | UIRemoteNotificationTypeAlert)];

  [GPGManager sharedInstance].realTimeRoomDelegate = [GPGSRealTimeRoomDelegate sharedInstance];
  [GPGSRealTimeRoomDelegate sharedInstance].pushNotificationCallback = pushNotificationCallback;
  [GPGLauncherController sharedInstance].turnBasedMatchListLauncherDelegate = [GPGSTurnBasedMatchDelegate sharedInstance];
  [GPGManager sharedInstance].turnBasedMatchDelegate = [GPGSTurnBasedMatchDelegate sharedInstance];
  [GPGSTurnBasedMatchDelegate sharedInstance].invitationReceivedCallback = pushNotificationCallback;

}


void GPGSRtmpCreateWithInviteScreen(
    int32_t minOpponents, int32_t maxOpponents, int32_t variant,
    GPGRealTimeRoomStatusChangedCallback statusChangedCallback,
    GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
    GPGRealTimeDataReceivedCallback dataReceivedCallback,
    GPGRealTimeRoomErrorCallback roomErrorCallback) {
  LOGD((@"GPGSRtmpCreateWithInviteScreen"));
  // Fun fact: The unity plugin interprets the DEFAULT variant as 0, not -1;
  if (variant == 0) { variant = -1; }
  [GPGManager sharedInstance].realTimeRoomDelegate = [GPGSRealTimeRoomDelegate sharedInstance];
  [GPGSRealTimeRoomDelegate sharedInstance].statusChangedCallback = statusChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].participantsChangedCallback =
      participantsChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].dataReceivedCallback = dataReceivedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].roomErrorCallback = roomErrorCallback;

  UIViewController *vc = GetUnityViewController();
  [GPGSRealTimeRoomDelegate sharedInstance].parentOfRealTimeVC = vc;
  [[GPGLauncherController sharedInstance]
      presentRealTimeInviteWithMinPlayers:minOpponents
                               maxPlayers:maxOpponents
                         exclusiveBitMask:0
                                  variant:variant];
}

void GPGSRtmpDeclineRoomWithId(const char *roomId_s) {
  NSString *roomId = [[NSString alloc] initWithUTF8String:roomId_s];
  [[GPGSRealTimeRoomDelegate sharedInstance] declineRoomWithId:roomId];
}

void GPGSRtmpAcceptRoomWithId(const char *roomId_s,
                              GPGRealTimeRoomStatusChangedCallback statusChangedCallback,
                              GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
                              GPGRealTimeDataReceivedCallback dataReceivedCallback,
                              GPGRealTimeRoomErrorCallback roomErrorCallback) {
  NSString *roomId = [[NSString alloc] initWithUTF8String:roomId_s];

  [GPGManager sharedInstance].realTimeRoomDelegate = [GPGSRealTimeRoomDelegate sharedInstance];
  [GPGSRealTimeRoomDelegate sharedInstance].statusChangedCallback = statusChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].participantsChangedCallback =
  participantsChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].dataReceivedCallback = dataReceivedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].roomErrorCallback = roomErrorCallback;
  [[GPGSRealTimeRoomDelegate sharedInstance] acceptRoomWithId:roomId];

}


void GPGSRtmpCreateQuickGame(int32_t minOpponents, int32_t maxOpponents, int32_t variant,
                             GPGRealTimeRoomStatusChangedCallback statusChangedCallback,
                             GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
                             GPGRealTimeDataReceivedCallback dataReceivedCallback,
                             GPGRealTimeRoomErrorCallback roomErrorCallback) {
  LOGD((@"GPGSRtmpCreateQuickGame"));
  // Fun fact: The unity plugin interprets the DEFAULT variant as 0, not -1;
  if (variant == 0) { variant = -1; }

  [GPGSRealTimeRoomDelegate sharedInstance].statusChangedCallback = statusChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].participantsChangedCallback =
      participantsChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].dataReceivedCallback = dataReceivedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].roomErrorCallback = roomErrorCallback;

  GPGMultiplayerConfig *config = [[GPGMultiplayerConfig alloc] init];
  config.minAutoMatchingPlayers = minOpponents;
  config.maxAutoMatchingPlayers = maxOpponents;
  config.variant = variant;

  [GPGManager sharedInstance].realTimeRoomDelegate = [GPGSRealTimeRoomDelegate sharedInstance];
  [[GPGLauncherController sharedInstance] presentRealTimeWaitingRoomWithConfig:config];
}

void GPGSRtmpShowAllInvitations(
    GPGRealTimeRoomStatusChangedCallback statusChangedCallback,
    GPGRealTimeParticipantListChangedCallback participantsChangedCallback,
    GPGRealTimeDataReceivedCallback dataReceivedCallback,
    GPGRealTimeRoomErrorCallback roomErrorCallback) {

  [GPGSRealTimeRoomDelegate sharedInstance].statusChangedCallback = statusChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].participantsChangedCallback =
      participantsChangedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].dataReceivedCallback = dataReceivedCallback;
  [GPGSRealTimeRoomDelegate sharedInstance].roomErrorCallback = roomErrorCallback;

   [GPGRealTimeRoomMaker listRoomsWithMaxResults:50 completionHandler:^(NSArray *rooms, NSError *error) {
     NSMutableArray *roomsWithInvites = [NSMutableArray array];
     for (GPGRealTimeRoomData *roomData in rooms) {
       NSLog(@"Found a room %@", roomData);
       if (roomData.status == GPGRealTimeRoomStatusInviting) {
         [roomsWithInvites addObject:roomData];
       }
     }

     [[GPGLauncherController sharedInstance] presentRealTimeInvitesWithRoomDataList:roomsWithInvites];
   }];
}

void GPGSRtmpGetLocalParticipantId(char *buf, int32_t bufSize) {
  _NSStringToUTF8Buf([[GPGSRealTimeRoomDelegate sharedInstance] getLocalParticpantId], buf,
                     bufSize);
}

void GPGSRtmpSendMessage(BOOL isReliable, GPGSCBUF data, int32_t dataLen, BOOL toEveryone,
                         const char *participantId) {
  NSData *dataToSend = [NSData dataWithBytes:data length:dataLen];
  NSString *sendingParticipantId;
  if (participantId != NULL) {
    sendingParticipantId = [NSString stringWithUTF8String:participantId];
  }
  [[GPGSRealTimeRoomDelegate sharedInstance] sendMessageReliable:isReliable
                                                            data:dataToSend
                                                     toEverybody:toEveryone
                                                 orParticipantId:sendingParticipantId];
}

void GPGSRtmpLeaveRoom() {
  [[GPGSRealTimeRoomDelegate sharedInstance] leaveRoom];
}

# pragma mark - Turn-based match calls

void GPGSTBMPCreateQuickMatch(int32_t minOpponents, int32_t maxOpponents, int32_t variant,
                              int32_t callbackId, GPGTurnBasedMatchCreateCallback callback) {

  GPGMultiplayerConfig *gameConfigForAutoMatch = [[GPGMultiplayerConfig alloc] init];
  gameConfigForAutoMatch.minAutoMatchingPlayers = minOpponents;
  gameConfigForAutoMatch.maxAutoMatchingPlayers = maxOpponents;

  [GPGTurnBasedMatch createMatchWithConfig:gameConfigForAutoMatch completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    if (error) {
      NSLog(@"Received an error trying to create a match %@", [error localizedDescription]);
      callback([@"" UTF8String], GPGSTRUE, callbackId);
    } else {
      callback([[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:match] UTF8String], GPGSFALSE,
               callbackId);
    }
  }];
}

void GPGSTBMPCreateWithInviteScreen(int32_t minOpponents, int32_t maxOpponents, int32_t variant,
                              int32_t callbackId, GPGTurnBasedMatchCreateCallback callback) {
  // This can be a 2-4 player game
  GPGSTurnBasedMatchDelegate *tbmpDelegate = [GPGSTurnBasedMatchDelegate sharedInstance];
  tbmpDelegate.minPlayers = minOpponents;
  tbmpDelegate.maxPlayers = maxOpponents;
  [GPGLauncherController sharedInstance].playerPickerLauncherDelegate = tbmpDelegate;
  [GPGLauncherController sharedInstance].launcherDelegate = tbmpDelegate;
  UIViewController *vc = GetUnityViewController();
  [tbmpDelegate setupWithParent:vc callbackId:callbackId unityCallback:callback];
  [[GPGLauncherController sharedInstance] presentPlayerPicker];
}

void GPGSTBMPAcceptMatchWithId(const char *invitationId_s, int32_t callbackId, GPGTurnBasedMatchCreateCallback callback) {
  NSString *matchId = [[NSString alloc] initWithUTF8String:invitationId_s];
  GPGTurnBasedMatch *matchToAccept = [[GPGSTurnBasedMatchDelegate sharedInstance] extractMatchWithId:matchId];
  if (matchToAccept) {
    [matchToAccept joinWithCompletionHandler:^(NSError *error) {
      NSLog(@"Accepting an invitation. Got back error? %@", [error localizedDescription]);
      //(const char *matchAsJson, GPGSBOOL isError, int32_t callbackId);
      if (error) {
        callback([@"" UTF8String], GPGSTRUE, callbackId);
      } else {
        callback([[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:matchToAccept] UTF8String], GPGSFALSE, callbackId);
      }
    }];
  } else {
    // For some reason, the match couldn't be found
    callback([@"" UTF8String], GPGSTRUE, callbackId);
  }

}


void GPGSTBMPDeclineMatchWithId(const char *invitationId_s) {
  NSString *matchId = [[NSString alloc] initWithUTF8String:invitationId_s];
  GPGTurnBasedMatch *matchToDecine = [[GPGSTurnBasedMatchDelegate sharedInstance] extractMatchWithId:matchId];
  if (matchToDecine) {
    [matchToDecine declineWithCompletionHandler:^(NSError *error) {
      if (error) {
        NSLog(@"Error declining a match %@", [error localizedDescription]);
      } else {
        NSLog(@"Match declined");
      }
    }];
  }
}



void GPGSTBMPTakeTurnInMatch(const char *matchId_s, GPGSCBUF buf, int32_t dataSize,
                             const char *nextPlayerId_s, int32_t callbackId,
                             GPGTurnBasedSuccessCallback callback) {
  NSString *matchId = [[NSString alloc] initWithUTF8String:matchId_s];
  NSString *nextPlayerId =
      (nextPlayerId_s == NULL) ? nil : [[NSString alloc] initWithUTF8String:nextPlayerId_s];
  NSData *matchData = [NSData dataWithBytes:buf length:dataSize];
  NSLog(@"I am attempting to take a turn with matchID %@ and player %@", matchId, nextPlayerId);
  [GPGTurnBasedMatch fetchMatchWithId:matchId includeMatchData:YES completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    NSLog(@"I have found the match! Here it is: %@", match);
    NSLog(@"Tetting ready to take my turn in match %@", match.matchId);

    [match takeTurnWithNextParticipantId:nextPlayerId data:matchData results:nil completionHandler:^(NSError *error) {
      NSLog(@"Taking my turn. Got back error? %@", [error localizedDescription]);
      if (error) {
        callback(GPGSTRUE, [error code], callbackId);
      } else {
        callback(GPGSFALSE, 0, callbackId);
      }
    }];
  }];
}

void GPGSTBMPShowInvitesAndFindMatch(int32_t callbackId, GPGTurnBasedMatchCreateCallback callback) {
  LOGD((@"GPGSTBMPShowInvitesAndFindMatch"));
  UIViewController *vc = GetUnityViewController();
  [[GPGSTurnBasedMatchDelegate sharedInstance] setupWithParent:vc callbackId:callbackId unityCallback:callback];
  [GPGLauncherController sharedInstance].turnBasedMatchListLauncherDelegate = [GPGSTurnBasedMatchDelegate sharedInstance];
  [[GPGLauncherController sharedInstance] presentTurnBasedMatchList];
}

void GPGSTBMPLeaveDuringTurn(const char *matchId_s, const char *nextParticipantId_s, int32_t callbackId, GPGTurnBasedSuccessCallback callback) {
  LOGD((@"GPGSTBMPLeaveDuringTurn"));
  NSString *matchId =  [[NSString alloc] initWithUTF8String:matchId_s];
  NSString *nextPlayerId = (nextParticipantId_s == NULL) ? nil : [[NSString alloc] initWithUTF8String:nextParticipantId_s];
  NSLog(@"I am attempting to leave a turn with matchID %@ and player %@", matchId, nextPlayerId);
  [GPGTurnBasedMatch fetchMatchWithId:matchId includeMatchData:YES completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    [match leaveDuringTurnWithNextParticipantId:nextPlayerId completionHandler:^(NSError *error) {
      NSLog(@"Leaving during turn. Got back error? %@", [error localizedDescription]);
      if (error) {
        callback(GPGSTRUE, [error code], callbackId);
      } else {
        callback(GPGSFALSE, 0, callbackId);
      }
    }];
  }];
}

void GPGSTBMPLeaveOutofTurn(const char *matchId_s, int32_t callbackId, GPGTurnBasedSuccessCallback callback) {
  LOGD((@"GPGSTBMPLeaveDuringTurn"));
  NSString *matchId =  [[NSString alloc] initWithUTF8String:matchId_s];
  NSLog(@"I am attempting to leave OUT OF turn with matchID %@ ", matchId);
  [GPGTurnBasedMatch fetchMatchWithId:matchId includeMatchData:YES completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    [match leaveOutOfTurnWithCompletionHandler:^(NSError *error) {
      NSLog(@"Leave out of turn. Got back error? %@", [error localizedDescription]);
      if (error) {
        callback(GPGSTRUE, [error code], callbackId);
      } else {
        callback(GPGSFALSE, 0, callbackId);
      }
    }];
  }];
}

void GPGSTBMPCancelMatch(const char *matchId_s, int32_t callbackId, GPGTurnBasedSuccessCallback callback) {
  LOGD((@"GPGSTBMPCancelMatch"));
  NSString *matchId =  [[NSString alloc] initWithUTF8String:matchId_s];
  NSLog(@"I am attempting to cancel with matchID %@ ", matchId);
  [GPGTurnBasedMatch fetchMatchWithId:matchId includeMatchData:YES completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    [match dismissWithCompletionHandler:^(NSError *error) {
      NSLog(@"Dismiss. Got back error? %@", [error localizedDescription]);
      if (error) {
        callback(GPGSTRUE, [error code], callbackId);
      } else {
        callback(GPGSFALSE, 0, callbackId);
      }
    }];
  }];
}


void GPGSTBMPFinishMatch(const char *matchId_s, GPGSCBUF buf, int32_t dataSize, const char *resultsAsJson, int32_t callbackId, GPGTurnBasedSuccessCallback callback) {
  LOGD((@"GPGSTBMPFinishMatch"));
  NSString *matchId =  [[NSString alloc] initWithUTF8String:matchId_s];
  NSData *matchData = [NSData dataWithBytes:buf length:dataSize];
  NSLog(@"I am attempting to finish with matchID %@ and data %s", matchId, resultsAsJson);
  NSError *error = nil;;

  // TODO: Can I simplify this at all?
  NSData *jsonData = [[[NSString alloc] initWithUTF8String:resultsAsJson] dataUsingEncoding:NSUTF8StringEncoding];
  NSArray *resultArray = [NSJSONSerialization JSONObjectWithData:jsonData options:0 error:&error];
  if (error) {
    callback(GPGSTRUE, [error code], callbackId);
  }

  NSMutableArray *finalResults = [[NSMutableArray alloc] initWithCapacity:resultArray.count];
  for (NSDictionary *tempResult in resultArray) {
    GPGTurnBasedParticipantResult *playerResult = [[GPGTurnBasedParticipantResult alloc] init];
    playerResult.participantId = (NSString *)[tempResult objectForKey:@"participantId"];
    playerResult.result = [(NSString *)[tempResult objectForKey:@"result"] integerValue];
    playerResult.placing =[(NSString *)[tempResult objectForKey:@"placing"] integerValue];
    [finalResults addObject:playerResult];
  }

  [GPGTurnBasedMatch fetchMatchWithId:matchId includeMatchData:YES completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    [match finishWithData:matchData results:finalResults completionHandler:^(NSError *error) {
      NSLog(@"Finishing match. Got back error %@", [error localizedDescription]);
      if (error) {
        callback(GPGSTRUE, [error code], callbackId);
      } else {
        callback(GPGSFALSE, 0, callbackId);
      }
    }];
  }];
}

void GPGSTBMPAcknowledgeFinish(const char *matchId_s, int32_t callbackId, GPGTurnBasedSuccessCallback callback) {
  NSString *matchId =  [[NSString alloc] initWithUTF8String:matchId_s];
  LOGD((@"Acknowledging finish for match %@", matchId));
  // For now, we are just acknowledging the results

  [GPGTurnBasedMatch fetchMatchWithId:matchId includeMatchData:YES completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    [match finishWithData:nil results:nil completionHandler:^(NSError *error) {
      if (error) {
        callback(GPGSTRUE, [error code], callbackId);
      } else {
        callback(GPGSFALSE, 0, callbackId);
      }
    }];
  }];
}

void GPGSTBMPRemach(const char *matchId_s, int32_t callbackId, GPGTurnBasedMatchCreateCallback callback) {
  NSString *matchId =  [[NSString alloc] initWithUTF8String:matchId_s];
  LOGD((@"Requesting rematch for match %@", matchId));
  [GPGTurnBasedMatch fetchMatchWithId:matchId includeMatchData:YES completionHandler:^(GPGTurnBasedMatch *match, NSError *error) {
    [match rematchWithCompletionHandler:^(GPGTurnBasedMatch *rematch, NSError *error) {
      if (error) {
        NSLog(@"Received an error trying to rematch %@", [error localizedDescription]);
        callback([@"" UTF8String], GPGSTRUE, callbackId);
      } else {
        callback([[GPGSTurnBasedMatchDelegate getJsonStringFromMatch:rematch] UTF8String], GPGSFALSE,
                 callbackId);
      }
    }];
  }];
}

void GPGSTBMPRegisterYourTurnNotificationCallback(GPGSTurnBasedMatchYourTurnCallback callback) {
  [GPGSTurnBasedMatchDelegate sharedInstance].yourTurnCallback = callback;
}


unsigned int DebugFunc(int x) {
  return 0;
}
