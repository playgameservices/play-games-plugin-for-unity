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

#import "GPGSAppController.h"
#import "GPGSRealTimeRoomDelegate.h"
#import <GooglePlus/GooglePlus.h>
#import <GooglePlayGames/GooglePlayGames.h>

@implementation GPGSAppController

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
  [super application:application didFinishLaunchingWithOptions:launchOptions];
  // Override point for customization after application launch.

  // Let's specify our manager for incoming notifications and other real-time events
  [GPGManager sharedInstance].realTimeRoomDelegate = [GPGSRealTimeRoomDelegate sharedInstance];

  // Handle the case where our game was "cold-started" from a notification
  NSDictionary *remoteNotification =
  [launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];

  if (remoteNotification) {
    if ([[GPGManager sharedInstance] tryHandleRemoteNotification:remoteNotification]) {
      NSLog(@"Handling notification %@ after sign-in is complete", remoteNotification);
      // Looks like we got a Google Play match invite! No other action is requied. Our
      // invite delegate will receive a didReceiveRealTimeInviteForRoon just as soon
      // as sign-in is finished.
    } else {
      // You probably want to do other notification checking here.
    }
  }

  return YES;
}

- (BOOL)application:(UIApplication *)application
            openURL:(NSURL *)url
  sourceApplication:(NSString *)sourceApplication
         annotation:(id)annotation {
    
    [super application:application openURL:url sourceApplication:sourceApplication annotation:annotation];
    
    return [GPPURLHandler handleURL:url
                  sourceApplication:sourceApplication
                         annotation:annotation];
}


- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo {
  NSLog(@"Received a remote notification %@", userInfo);
  if ([[GPGManager sharedInstance] tryHandleRemoteNotification:userInfo]) {
    // No action is required. Our inviteDelegate is receiving a didReceiveRealTimeInviteForRoom
    // method right now.
    NSLog(@"Handling remote notification from the GPGManager");
  }
  [super application:application didReceiveRemoteNotification:userInfo];
}

- (void)application:(UIApplication *)application
didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
  NSLog(@"Got deviceToken from APNS! %@", deviceToken);
  [[GPGManager sharedInstance] registerDeviceToken:deviceToken
                                    forEnvironment:GPGPushNotificationEnvironmentSandbox];

  [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
}

- (void)application:(UIApplication *)application didFailToRegisterForRemoteNotificationsWithError:(NSError *)error {
  NSLog(@"I was unable to register for a remote notification: %@", [error localizedDescription]);
  [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
}


@end


