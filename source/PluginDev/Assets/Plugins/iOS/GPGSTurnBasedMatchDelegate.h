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

#import <Foundation/Foundation.h>
#import <GooglePlayGames/GooglePlayGames.h>
#import "GPGSTypedefs.h"


@interface GPGSTurnBasedMatchDelegate : NSObject<GPGTurnBasedMatchDelegate, GPGPlayerPickerLauncherDelegate, GPGTurnBasedMatchListLauncherDelegate, GPGLauncherDelegate>

@property (nonatomic) int32_t callbackId;
@property (nonatomic, weak) UIViewController *parentVc;
@property (nonatomic) GPGTurnBasedMatchCreateCallback getMatchCallback;
@property (nonatomic) GPGSPushNotificationCallback invitationReceivedCallback;
@property (nonatomic) GPGSTurnBasedMatchYourTurnCallback yourTurnCallback;
@property (nonatomic) int32_t minPlayers;
@property (nonatomic) int32_t maxPlayers;

+ (GPGSTurnBasedMatchDelegate *)sharedInstance;
+ (NSString *)getJsonStringFromMatch:(GPGTurnBasedMatch *)match;

- (void)setupWithParent:(UIViewController *)parent callbackId:(int32_t)callbackId unityCallback:(GPGTurnBasedMatchCreateCallback)callback;
- (GPGTurnBasedMatch *)extractMatchWithId:(NSString *)matchId;
@end
