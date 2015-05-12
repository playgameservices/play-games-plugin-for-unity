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
// Author: Bruno Oliveira

#import "GPGSAppController.h"

#import <GooglePlus/GooglePlus.h>
#import <gpg/ios_support.h>

@implementation GPGSAppController

- (BOOL)application:(UIApplication *)application
            openURL:(NSURL *)url
  sourceApplication:(NSString *)sourceApplication
         annotation:(id)annotation {

    [super application:application
               openURL:url
     sourceApplication:sourceApplication
            annotation:annotation];

    return [GPPURLHandler handleURL:url
                  sourceApplication:sourceApplication
                         annotation:annotation];
}

- (BOOL)application:(UIApplication *)application
        didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    [super application:application didFinishLaunchingWithOptions:launchOptions];

    //-- Set Notification
    // iOS 8 Notifications
    if ([application
         respondsToSelector:@selector(isRegisteredForRemoteNotifications)]) {
        [application registerUserNotificationSettings:
         [UIUserNotificationSettings
          settingsForTypes:(UIUserNotificationTypeSound |
                            UIUserNotificationTypeAlert |
                            UIUserNotificationTypeBadge)
          categories:nil]];

        [application registerForRemoteNotifications];
    } else {
        // iOS < 8 Notifications
        [application
         registerForRemoteNotificationTypes:(UIRemoteNotificationTypeBadge |
                                             UIRemoteNotificationTypeAlert |
                                             UIRemoteNotificationTypeSound)];
    }

    gpg::TryHandleNotificationFromLaunchOptions(launchOptions);
    return YES;
}

- (void)application:(UIApplication *)application
            didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {

    NSLog(@"Got Token for APNS: %@", deviceToken);

    // send the token to GPGS server so invitations can be sent to the local player
    // NOTE: false indicates this is using the production APNS service.  true indicates
    // that the sandbox service should be used.  This value needs to match the cooresponding
    // certificate registered in the play app console, under linked apps > ios in
    // the section for push notifications.
    gpg::RegisterDeviceToken(deviceToken, false);
}


- (void)application:(UIApplication *)application
        didFailToRegisterForRemoteNotificationsWithError:(NSError *)error {
    NSLog(@"Error registering for remote notifications! %@", error);
}


- (void)application:(UIApplication *)application
        didReceiveRemoteNotification:(NSDictionary *)userInfo {
    // this returns a bool if it was handled (here you might pass off to another
    // company's sdk for example).
    NSLog(@"Received notification: %@", userInfo);
    gpg::TryHandleRemoteNotification(userInfo);
}


@end
