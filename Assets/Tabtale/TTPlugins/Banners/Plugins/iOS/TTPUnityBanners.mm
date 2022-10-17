//
//  TTPUnityBanners.m
//  Unity-iPhone
//
//  Created by Dmytro-MacBookPro on 25.02.19.
//  Copyright © 2019 Ariel Vardy. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPIbanners.h>

extern "C" {

    bool ttpBannersShow()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbanners> bannersService = [serviceManager get:@protocol(TTPIbanners)];
        if(bannersService != nil){
            return [bannersService show];
        } else {
            [serviceManager markPendingBannersShow];
        }
        return false;
    }
    
    bool ttpBannersHide()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbanners> bannersService = [serviceManager get:@protocol(TTPIbanners)];
        if(bannersService != nil){
            return [bannersService hide];
        }
        return false;
    }
    
    int ttpBannersGetAdHeight()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbanners> bannersService = [serviceManager get:@protocol(TTPIbanners)];
        if(bannersService != nil){
            return [bannersService getAdHeight];
        }
        return 0;
    }

}
