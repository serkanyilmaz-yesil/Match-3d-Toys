//
//  TTPUnityRateUs.m
//  Unity-iPhone
//
//  Created by Shmulik Armon on 15/04/2019.
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
#import <TT_Plugins_Core/TTPAppStoreLauncher.h>
#import "TTPUnityServiceManager.h"

extern "C"
{
    void ttpRateUsGoToStore()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        [TTPAppStoreLauncher openApp:nil appUrl:nil appId:nil storeId:[serviceManager getAppId] serviceIdentifier:@"rateUs" delegate:nil];
    }

    void ttpRateUsDisplayRateUsModal()
    {
        NSLog(@"ttpRateUsDisplayRateUsModal::");
        if ([SKStoreReviewController class]) {
            #ifdef __IPHONE_14_0
            if (@available(iOS 14.0, *)) {
                UIWindowScene* scene = (UIWindowScene*)[[[UIApplication sharedApplication].connectedScenes allObjects] objectAtIndex: 0];
                [SKStoreReviewController requestReviewInScene:scene];
            } else {
                [SKStoreReviewController requestReview];
            }
            #else
            [SKStoreReviewController requestReview];
            #endif
        } else {
            NSLog(@"ttpRateUsDisplayRateUsModal:: SKStoreReviewController class does not exist. will not display Rate Us model view");
            ttpRateUsGoToStore();
        }
    }
}
