#import "CustomWebViewApplication.h"

#include "UnityAppController.h"

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
    const char* _GooglePlayGetAccessToken()
    {
        const char* accessToken = nil;
        GPPSignIn* gp = [GPPSignIn sharedInstance];
        
        NSString* user;
        if( [gp authentication] )
        {
            accessToken = __MakeStringCopy( [gp idToken] );
            user = [gp userID];
        }
        
        NSLog(@"\n\tOS: 'iOS',\n\tBundleId: '%@',\n\tUser: '%@',\n\t ServiceAccessToken: '%s'",
              [ [NSBundle mainBundle] bundleIdentifier ], user, accessToken);
        return accessToken ? accessToken : "";
    }
}
