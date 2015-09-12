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

#import <GIDSignIn.h>
#import <GIDGoogleUser.h>
#import <GIDAuthentication.h>
#import <GIDProfileData.h>
#import <gpg/ios_support.h>
#import <gpg/GPGEnums.h>

@interface GPGSAppController() <GIDSignInUIDelegate>

@end

@implementation GPGSAppController

- (BOOL)application:(UIApplication *)application
            openURL:(NSURL *)url
  sourceApplication:(NSString *)sourceApplication
         annotation:(id)annotation {
  [super application:application
             openURL:url
   sourceApplication:sourceApplication
          annotation:annotation];
  return [[GIDSignIn sharedInstance] handleURL:url
                             sourceApplication:sourceApplication
                                    annotation:annotation];
}

- (BOOL)application:(UIApplication *)application
didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
  [super application:application didFinishLaunchingWithOptions:launchOptions];

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

  [GIDSignIn sharedInstance].uiDelegate = self;
  
  gpg::TryHandleNotificationFromLaunchOptions(launchOptions);
  return YES;
}

- (void)application:(UIApplication *)application
didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {

  [super application:application didRegisterForRemoteNotificationsWithDeviceToken:deviceToken];
  NSLog(@"Got Token for APNS: %@", deviceToken);


  // send the token to GPGS server so invitations can be sent to the local player
  // NOTE: GPGPushNotificationEnvironmentProduction indicates this is
  // using the production APNS service.
  // GPGPushNotificationEnvironmentSandbox indicates that the sandbox
  // service should be used.  This value needs to match the cooresponding
  // certificate registered in the play app console, under linked apps > ios in
  // the section for push notifications.
  gpg::RegisterDeviceToken(deviceToken, GPGPushNotificationEnvironmentProduction);
}

- (void)application:(UIApplication *)application
didFailToRegisterForRemoteNotificationsWithError:(NSError *)error {
  [super application:application didFailToRegisterForRemoteNotificationsWithError:error];
  NSLog(@"Error registering for remote notifications! %@", error);
}


- (void)application:(UIApplication *)application
didReceiveRemoteNotification:(NSDictionary *)userInfo {
  [super application:application didReceiveRemoteNotification:userInfo];
  // this returns a bool if it was handled (here you might pass off to another
  // company's sdk for example).
  NSLog(@"Received notification: %@", userInfo);
  gpg::TryHandleRemoteNotification(userInfo);
}

- (void)signIn:(GIDSignIn *)signIn presentViewController:(UIViewController *)viewController {

   UnityPause(true);
  [[self rootViewController] presentViewController:viewController animated:YES completion:nil];
}


- (void)signIn:(GIDSignIn *)signIn dismissViewController:(UIViewController *)viewController {
   UnityPause(false);
  [[self rootViewController] dismissViewControllerAnimated:YES completion:nil];
}

- (void)signInWillDispatch:(GIDSignIn *)signIn error:(NSError *)error {

  NSLog(@"signInWillDispatch: %@", error);
}

@end

#ifndef CUSTOM_MAKE_STRING_COPY
#define CUSTOM_MAKE_STRING_COPY

char* __MakeStringCopy(NSString* nstring)
{
  if( (!nstring) || (nil == nstring) || ( nstring == (id)[NSNull null] ) || (0 == nstring.length) )
  {
    return NULL;
  }

  const char* string = [nstring UTF8String];
  if (string == NULL) {
    return NULL;
  }

  char* res = (char*)malloc(strlen(string) + 1);
  strcpy(res, string);
  return res;
}

#endif

extern "C" {
  const char* _GooglePlayGetIdToken() {
    const char* idToken = nil;
    GIDGoogleUser* guser = [GIDSignIn sharedInstance].currentUser;

    if(guser)
    {
      idToken = __MakeStringCopy( [guser.authentication idToken] );

      NSString* user = [guser userID];

      NSLog(@"\n\tOS: 'iOS',\n\tBundleId: '%@',\n\tUser: '%@',\n\t idToken: '%s'",
            [ [NSBundle mainBundle] bundleIdentifier ], user, idToken);
    } else {
      NSLog(@"Current user is not set");
    }
    return idToken ? idToken :  __MakeStringCopy(@"");
  }

  const char* _GooglePlayGetUserEmail() {
    const char* email = nil;
    GIDGoogleUser* guser = [GIDSignIn sharedInstance].currentUser;

    if(guser) {
      email = __MakeStringCopy( [guser.profile email] );
    } else {
      NSLog(@"Current user is not set");
    }

    return email ? email : __MakeStringCopy(@"");
  }

  const char* _GooglePlayGetAccessToken() {
    const char* accessToken = nil;
    NSString* user;
    GIDGoogleUser* guser = [GIDSignIn sharedInstance].currentUser;

    if(guser) {
      accessToken = __MakeStringCopy( [guser.authentication accessToken] );
      user = [guser userID];

      NSLog(@"\n\tOS: 'iOS',\n\tBundleId: '%@',\n\tUser: '%@',\n\t AccessToken: '%s'",
            [ [NSBundle mainBundle] bundleIdentifier ], user, accessToken);
    } else {
      NSLog(@"Current user is not set");
    }
    return accessToken ? accessToken :  __MakeStringCopy(@"");
  }

  void _GooglePlayEnableProfileScope() {
    GIDSignIn *signIn = [GIDSignIn sharedInstance];
    signIn.shouldFetchBasicProfile = YES;
  }
}
