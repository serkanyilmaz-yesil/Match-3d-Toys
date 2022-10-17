//
//  TTPUnityRewardedInterstitials.m
//  Unity-iPhone
//
//  Created by Tabtale on 17/12/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPIrewardedInterstitial.h>

extern "C" {
    
    bool ttpRewardedInterstitialsShow(const char * location, BOOL force)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrewardedInterstitial> rewardedInterService = [serviceManager get:@protocol(TTPIrewardedInterstitial)];
        if(rewardedInterService != nil){
            return [rewardedInterService show:[[NSString alloc] initWithUTF8String:location] forceToShow:force];
        }
        return false;
    }

    bool ttpRewardedInterstitialsIsReady(BOOL force)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrewardedInterstitial> rewardedInterService = [serviceManager get:@protocol(TTPIrewardedInterstitial)];
        if(rewardedInterService != nil){
            return [rewardedInterService isReady:force];
        }
        return false;
    }

    void ttpRewardedInterstitialsPopupShown()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrewardedInterstitial> rewardedInterService = [serviceManager get:@protocol(TTPIrewardedInterstitial)];
        if(rewardedInterService != nil){
            [rewardedInterService popupShown];
        }
    }

    void ttpRewardedInterstitialsPopupCancelled()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIrewardedInterstitial> rewardedInterService = [serviceManager get:@protocol(TTPIrewardedInterstitial)];
        if(rewardedInterService != nil){
            [rewardedInterService popupCancelled];
        }
    }
}
