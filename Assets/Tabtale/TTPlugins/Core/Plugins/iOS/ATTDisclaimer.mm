//
//  TTPATTDisclaimer.m
//  Unity-iPhone
//
//  Created by ShmulikA on 04/05/2021.
//

#import "TTPUnityServiceManager.h"
#import "ATTDisclaimerCallback.h"

static NSTimer *attTimer;

extern "C"
{
    void ttpAttDisclaimer()
    {
      NSString *defaultTitle = @"Ads keep this game free";
      NSString *defaultMessage = @"On the next popup, allow tracking to help support the game and keep it free for everyone. It will allow us to provide you with the best personalized ads for you.";
      NSString *title = [[NSBundle mainBundle] localizedStringForKey:@"ATT_Disclaimer_Title"
                                                               value:defaultTitle
                                                               table:@"InfoPlist"];
      NSString *message = [[NSBundle mainBundle] localizedStringForKey:@"ATT_Disclaimer_Message"
                                                                 value:defaultMessage
                                                                 table:@"InfoPlist"];
      NSString *nextTitle = [[NSBundle mainBundle] localizedStringForKey:@"ATT_Disclaimer_Next"
                                                                   value:@"Next"
                                                                   table:@"InfoPlist"];
      
      UIAlertController* alert = [UIAlertController
                                  alertControllerWithTitle:title
                                  message:message
                                  preferredStyle:UIAlertControllerStyleAlert];

        UIAlertAction* yesButtonAction = [UIAlertAction actionWithTitle:nextTitle
                                                                  style:UIAlertActionStyleDefault
                                                                handler:^(UIAlertAction* action)
        {
            [ATTDisclaimerCallback notify];
        }];
        [alert addAction:yesButtonAction];
        
        TTPServiceManager* serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrootViewController> rootViewController = [serviceManager get:@protocol(TTPIrootViewController)];
        UIViewController* vc = [rootViewController get];
        if (vc != nil)
        {
            if(vc.isViewLoaded && vc.view.window){
                NSLog(@"displaying correct view controller");
                [vc presentViewController:alert animated:YES completion:nil];
                return;
            }
            //in case ttp is initiated while unity splash is still on screen
            attTimer = [NSTimer scheduledTimerWithTimeInterval:0.5 repeats:YES block:^(NSTimer * _Nonnull timer) {
                if(vc.isViewLoaded && vc.view.window){
                    [vc presentViewController:alert animated:YES completion:nil];
                    [attTimer invalidate];
                    attTimer = nil;
                }
                
            }];
            
        }
    }
}
