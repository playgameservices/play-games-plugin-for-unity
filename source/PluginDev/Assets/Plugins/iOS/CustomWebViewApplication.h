#ifndef Unity_iPhone_CustomWebViewApplication_h
#define Unity_iPhone_CustomWebViewApplication_h

#import <UIKit/UIKit.h>

#define ApplicationOpenGoogleAuthNotification @"ApplicationOpenGoogleAuthNotification"

@interface CustomWebViewApplication : UIApplication<UIWebViewDelegate>
{
}

- (BOOL)openURL:(NSURL*)url;

@end

#endif
