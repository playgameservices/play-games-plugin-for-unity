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

#import <Foundation/Foundation.h>
#import <GooglePlayGames/GooglePlayGames.h>
#import "GPGSTypedefs.h"

@interface GPGSManager : NSObject <GPGStatusDelegate> {
    GPGSSuccessCallback mAuthCallback;
    NSString* mPlayerName;
    NSString* mPlayerId;
}
- (GPGSManager*)init;
+ (GPGSManager*)instance;
- (BOOL) authenticateWithCallback:(GPGSSuccessCallback)callback silently:(BOOL)trySilent;
- (void) signOut;
- (NSString*) playerId;
- (NSString*) playerName;
@end


