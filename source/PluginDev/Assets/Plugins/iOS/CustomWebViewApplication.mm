/*
 * Copyright (C) 2015 Google Inc. All Rights Reserved.
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

#import "CustomWebViewApplication.h"

#include "UnityAppController.h"

#import <GTMOAuth2Authentication.h>
#import <GooglePlus/GooglePlus.h>

@interface CustomWebViewApplication()

@end

@implementation CustomWebViewApplication

- (BOOL)openURL:(NSURL*)url {
    
    if( [ [url absoluteString] hasPrefix:@"googlechrome-x-callback:" ] )
    {
        return NO;
    }
    else if( [ [url absoluteString] hasPrefix:@"https://accounts.google.com/o/oauth2/auth" ] )
    {
        [self ShowWeb:url];
        
        return NO;
        
    }
    
    return [super openURL:url];
}

- (void) ShowWeb:(NSURL *)url {
    UnityPause(true);
    
    UIWebView *webview=[ [UIWebView alloc] initWithFrame:[ [UIScreen mainScreen] bounds ] ];
    NSURLRequest *nsrequest=[NSURLRequest requestWithURL:url];

    webview.backgroundColor = [UIColor whiteColor];
    webview.scalesPageToFit = YES;

    [webview setDelegate:self];
    [webview loadRequest:nsrequest];
    [GetAppController().rootViewController.view addSubview:webview];
    webview.delegate = self;
}

- (void)webView:(UIWebView *)webView didFailLoadWithError:(NSError *)error
{
    NSLog(@"Error : %@",error);
}

- (void)webViewDidFinishLoad:(UIWebView *)webView
{
    // finished loading, hide the activity indicator in the status bar
    [UIApplication sharedApplication].networkActivityIndicatorVisible = NO;
}

- (BOOL)            webView:(UIWebView *)webView
 shouldStartLoadWithRequest:(NSURLRequest *)request
             navigationType:(UIWebViewNavigationType)navigationType
{
    NSString* bundleID = [ [NSBundle mainBundle] bundleIdentifier ];

    if ( [ [ [request URL] absoluteString ] hasPrefix:[NSString stringWithFormat:@"%@:/oauth2callback", bundleID] ] ) {
        [GPPURLHandler handleURL:request.URL sourceApplication:@"com.apple.mobilesafari" annotation:nil];
        [GetAppController().rootViewController.navigationController popViewControllerAnimated:YES];
        [webView removeFromSuperview];
        UnityPause(false);
        return NO;
    }
    return YES;
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
    if (string == NULL)
    {
        return NULL;
    }

    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

#endif

extern "C"
{
    const char* _GooglePlayGetIdToken()
    {
        const char* idToken = nil;
        GPPSignIn* gp = [GPPSignIn sharedInstance];

        NSString* user;
        if( [gp authentication] )
        {
            idToken = __MakeStringCopy( [gp idToken] );
            user = [gp userID];
        }

        NSLog(@"\n\tOS: 'iOS',\n\tBundleId: '%@',\n\tUser: '%@',\n\t idToken: '%s'",
              [ [NSBundle mainBundle] bundleIdentifier ], user, idToken);
        return idToken ? idToken :  __MakeStringCopy(@"");
    }

    const char* _GooglePlayGetUserEmail()
    {
        const char* email = nil;
        GPPSignIn* gp = [GPPSignIn sharedInstance];

        if( [gp authentication] )
        {
            email = __MakeStringCopy( [gp userEmail] );
        }

        return email ? email : __MakeStringCopy(@"");
    }

    const char* _GooglePlayGetAccessToken()
    {
        const char* accessToken = nil;
        GPPSignIn* gp = [GPPSignIn sharedInstance];
        
        NSString* user;
        if( [gp authentication] )
        {
            accessToken = __MakeStringCopy( [ [gp authentication] accessToken ] );
            user = [gp userID];
        }

        NSLog(@"\n\tOS: 'iOS',\n\tBundleId: '%@',\n\tUser: '%@',\n\t AccessToken: '%s'",
              [ [NSBundle mainBundle] bundleIdentifier ], user, accessToken);
        return accessToken ? accessToken :  __MakeStringCopy(@"");
    }

    void _GooglePlayEnableProfileScope()
    {
        GPPSignIn *signIn = [GPPSignIn sharedInstance];
        signIn.shouldFetchGoogleUserEmail = YES;
        signIn.scopes = @[@"profile"];
    }
}
