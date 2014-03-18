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
    GPPSignIn *signIn = [GPPSignIn sharedInstance];
    // You set kClientID in a previous step
    signIn.clientID = @kClientID;
    signIn.scopes = [NSArray arrayWithObjects:
                     @"https://www.googleapis.com/auth/games",
                     @"https://www.googleapis.com/auth/appstate",
                     nil];
    signIn.language = [[NSLocale preferredLanguages] objectAtIndex:0];
    signIn.delegate = self;
    signIn.shouldFetchGoogleUserID =YES;
    LOGD((@"GPPSignIn initialized."));
    LOGD((@"GPPSignIn attempting sign in now."));
    [[GPPSignIn sharedInstance] authenticate];
}

-(void)startGoogleGamesSignIn
{
    LOGD((@"GPGSManager Starting Google Games sign in."));

    // The GPPSignIn object has an auth token now. Pass it to the GPGManager.
    // NOTE: if you get compile errors in this function, it may be because your Google
    // Play Games SDK for iOS is not recent enough. If so, update it and try again.
    // Check https://developers.google.com/games/services/downloads
    //[GPGManager sharedInstance].sdkTag = 0xa227;
    [[GPGManager sharedInstance] signIn:[GPPSignIn sharedInstance]
                     reauthorizeHandler:^(BOOL requiresKeychainWipe, NSError *error) {
        // If you hit this, auth has failed and you need to authenticate.
        // Most likely you can refresh behind the scenes
        if (requiresKeychainWipe) {
            [[GPPSignIn sharedInstance] signOut];
        }
        [[GPPSignIn sharedInstance] authenticate];
    }];

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
            mPlayerName = [localPlayer.name copy];
            mPlayerId = [localPlayer.playerId copy];
            LOGD((@"Player name %@, player id %@", localPlayer.name, localPlayer.playerId));
        }

        // Notify auth callback
        if (mAuthCallback) {
            LOGD((@"GPGSManager Calling auth callback with GPGSTRUE."));
            (*mAuthCallback)(GPGSTRUE, 0);
        }
    }];
}

- (void)finishedWithAuth:(GTMOAuth2Authentication *)auth error:(NSError *)error
{
    LOGD((@"GPGSManager Finished with auth."));
    if (error == nil && auth) {
        LOGD((@"Success signing in to Google! Auth object is %@", auth));
        [self startGoogleGamesSignIn];
    } else {
        LOGD((@"Failed to log into Google\n\tError=%@\n\tAuthObj=%@",error,auth));
        if (mAuthCallback) {
            LOGD((@"Calling auth callback with GPGSFALSE."));
            (*mAuthCallback)(GPGSFALSE, 0);
        }
    }
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

