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
#import "UnityAppController.h"
#import "GPGSAchOrLbDelegate.h"
#import <PlayGameServices/PlayGameServices.h>

// This file defines entry points to be called from C#.

void GPGSAuthenticateWithCallback(GPGSSuccessCallback cb) {
    LOGD((@"GPGSAuthenticateWithCallback."));
    [[GPGSManager instance] authenticateWithCallback: cb];
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
    GPGAchievementMetadata *metaData = [[GPGManager sharedInstance].applicationModel.achievement        metadataForAchievementId:achId];

    if (!metaData) {
        return GPGSFALSE;
    }

    if (NULL != outIsIncremental) {
        *outIsIncremental = ([metaData type] == GPGAchievementTypeIncremental);
    }

    if (NULL != outIsRevealed) {
        *outIsRevealed = ([metaData state] == GPGAchievementStateRevealed ||
                      [metaData state] == GPGAchievementStateUnlocked);
    }

    if (NULL != outIsUnlocked) {
        *outIsUnlocked = ([metaData state] == GPGAchievementStateUnlocked);
    }

    if (NULL != outCurSteps) {
        *outCurSteps = [metaData completedSteps];
    }

    if (NULL != outTotalSteps) {
        *outTotalSteps = [metaData numberOfSteps];
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
    UIViewController *vc = GetUnityViewController();
    GPGAchievementController *achController = [[GPGAchievementController alloc] init];
    GPGSAchOrLbDelegate *delegate = [[GPGSAchOrLbDelegate alloc] initWithViewController: vc];
    achController.achievementDelegate = delegate;
    [vc presentViewController:achController animated:YES completion:nil];
}

void GPGSShowLeaderboardsUI(const char *lbId_s) {
    LOGD((@"GPGSShowLeaderboardsUI %s", lbId_s));

    NSString* lbId = nil;
    if (lbId_s) {
        lbId = [[NSString alloc] initWithUTF8String:lbId_s];
    }

    UIViewController *vc = GetUnityViewController();
    GPGSAchOrLbDelegate *delegate = [[GPGSAchOrLbDelegate alloc] initWithViewController: vc];

    if (lbId) {
        GPGLeaderboardController *lbController = [[GPGLeaderboardController alloc] initWithLeaderboardId:lbId];
        lbController.leaderboardDelegate = delegate;
        [vc presentViewController:lbController animated:YES completion:nil];
    } else {
        GPGLeaderboardsController *lbsController = [[GPGLeaderboardsController alloc] init];
        lbsController.leaderboardsDelegate = delegate;
        [vc presentViewController:lbsController animated:YES completion:nil];
    }
}

void GPGSSubmitScore(const char *lbId_s, int64_t score64, GPGSSuccessCallback callback, int32_t userdata) {
    long score = (long) score64;
    LOGD((@"GPGSSubmitScore %s, score %ld", lbId_s, score));
    NSString *lbId = [[NSString alloc] initWithUTF8String:lbId_s];
    GPGScore *myScore = [[GPGScore alloc] initWithLeaderboardId:lbId];
    myScore.value = score;
    [myScore submitScoreWithCompletionHandler:^(GPGScoreReport *report, NSError *error) {
        if (error) {
            LOGE((@"Error submitting score %ld to lb %s: %@", score, lbId_s, [error userInfo]));
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





/*

// ---- DEBUG: --------
static void DebugCallback(GPGSBOOL succ) {
    LOGD((@"DebugCallback called, %d", succ));
}

static unsigned int DebugS(const char *s) {
    unsigned int i = 0;
    while (*s) {
        i <<= 8;
        i |= ((unsigned int) *s++);
    }
    return i;
}
static int DebugCloudData = 0;
void DebugUpdateStateCallback(GPGSBOOL success, int slot) {
    LOGD((@"DebugUpdateStateCallback: %s", success ? "SUCCESS" : "FAILURE"));
    DebugCloudData = success ? DebugS("U=OK") : DebugS("U=ER");
}



void DebugLoadStateCallback(GPGSBOOL success, int slot, GPGSCBUF buf, int dataSize) {
    LOGD((@"DebugLoadStateCallback: %s, data=%s", success ? "SUCCESS" : "FAILURE",
          buf ? buf : "(NULL)"));
    if (!success) {
        DebugCloudData = DebugS("L=ER");
    }
    else {
        if (buf && dataSize >= sizeof(int)) {
            DebugCloudData = *((int*) buf);
        } else {
            DebugCloudData = DebugS("NONE");
        }
    }
}

unsigned int DebugFunc(int x) {
    GPGSEnableDebugLog(GPGSTRUE);
    if (x == 1) {
        GPGSAuthenticateWithCallback(DebugCallback);
        return DebugS("AUTH");
    } else if (x == 2) {
        GPGSBOOL isInc, isRev, isUnl;
        int cur, tot;
        LOGD((@"Querying..."));
        GPGSQueryAchievement("CgkI9qnBz_4LEAIQBQ", &isInc, &isRev, &isUnl, &cur, &tot);
        LOGD((@"Result: inc=%d, rev=%d, unl=%d, cur=%d, tot=%d", isInc, isRev, isUnl, cur, tot));
        return DebugS("OK..");
    } else if (x == 3) {
        GPGSShowAchievementsUI();
        return DebugS("OK..");
    } else if (x == 4) {
        GPGSShowLeaderboardsUI("CgkI9qnBz_4LEAIQAg");
        return DebugS("OK..");
    } else if (x == 5) {
        GPGSShowLeaderboardsUI(NULL);
        return DebugS("OK..");
    } else if (x == 6) {
        GPGSCBUF buf = "LOREM IPSUM";
        GPGSUpdateState(0, buf, strlen(buf), DebugUpdateStateCallback, NULL);
        return DebugS("UPD.");
    } else if (x == 7) {
        GPGSLoadState(0, DebugLoadStateCallback, NULL);
        return DebugS("LOD.");
    } else if (x == 8) {
        return DebugCloudData;
    } else {
        return DebugS("HUH?");
    }
}
*/
unsigned int DebugFunc(int x) {
    return 0;
}
