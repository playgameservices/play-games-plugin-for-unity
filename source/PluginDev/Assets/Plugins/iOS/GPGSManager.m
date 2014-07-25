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
// Author: Bruno Oliveira

#import "GPGSManager.h"
#import "GPGSLogging.h"
#import "GPGSParams.h"
#import <GooglePlus/GooglePlus.h>
#import <GooglePlayGames/GooglePlayGames.h>

static GPGSManager *sInstance = NULL;

@implementation GPGSManager
- (GPGSManager*)init
{
    if ((self = [super init])) {
        mPlayerName = @"User";
        mPlayerId = @"0";
    }
    return self;
}

- (void) authenticateWithCallback:(GPGSSuccessCallback)callback
{
    LOGD((@"GPGSManager initializing and authenticating."));
    mAuthCallback = callback;
    [GPGManager sharedInstance].statusDelegate = self;

    // Let's not break anybody currently using app state
    [GPGManager sharedInstance].appStateEnabled = YES;
    [[GPGManager sharedInstance] signInWithClientID:@kClientID silently:NO];
}

- (void)didFinishGamesSignInWithError:(NSError *)error {
  if (error) {
    LOGE((@"Error signing in: %@", [error localizedDescription]));
    if (mAuthCallback) {
      LOGD((@"Calling auth callback with GPGSFALSE."));
      (*mAuthCallback)(GPGSFALSE, 0);
    }
  } else {
    LOGD((@"Games sign in successful"));
    // request information about the player
    [[[GPGManager sharedInstance] applicationModel] loadDataForKey:GPGModelLocalPlayerKey
                                                 completionHandler:^(NSError *error) {
      LOGD((@"GPGSManager got player data callback"));
      if (error) {
        // Error
        LOGE((@"Failed to retrieve player name/id: %@", error));
      } else {
        // Retrieve that information from the GPGApplicationModel
        GPGPlayerModel *playerModel = [[[GPGManager sharedInstance] applicationModel] player];
        GPGPlayer *localPlayer = playerModel.localPlayer;
        mPlayerName = [localPlayer.displayName copy];
        mPlayerId = [localPlayer.playerId copy];
        LOGD((@"Player name %@, player id %@", localPlayer.displayName, localPlayer.playerId));
      }

      // Notify auth callback
      if (mAuthCallback) {
        LOGD((@"GPGSManager Calling auth callback with GPGSTRUE."));
        (*mAuthCallback)(GPGSTRUE, 0);
      }
    }];
  }

}

- (void)didFinishGamesSignOutWithError:(NSError *)error {
  LOGD((@"GPGSManager Player successfully signed out"));

}


+ (GPGSManager*)instance
{
    if (!sInstance) {
        sInstance = [[GPGSManager alloc] init];
    }
    return sInstance;
}

- (NSString*) playerId
{
    return mPlayerId;
}

- (NSString*) playerName
{
    return mPlayerName;
}

- (void)signOut
{
    LOGD((@"GPGSManager Signing out."));
    [[GPGManager sharedInstance] signOut];
}


@end

