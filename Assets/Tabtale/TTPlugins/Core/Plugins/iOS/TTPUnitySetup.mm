//
//  TTPUnitySetup.mm
//  Unity-iPhone
//
//  Created by Tabtale on 21/11/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import "TTPUnityViewController.h"

#import <TT_Plugins_Core/TTPIConversion.h>
#import <TT_Plugins_Core/TTPConfiguration.h>
#import <TT_Plugins_Core/TTPIgeoService.h>

extern "C" {

    void ttpSetup()
    {
        [[TTPUnityServiceManager sharedInstance] setup:[[TTPUnityViewController alloc] init] unityMessenger:[[TTPUnityServiceManager alloc] init]];
        id<TTPIappLifeCycleMgr> appLifeCycleMgr = [[TTPUnityServiceManager sharedInstance] get:@protocol(TTPIappLifeCycleMgr)];
        if(appLifeCycleMgr != nil){
            [appLifeCycleMgr onResume];
        }
    }

    const char * ttpGetPackageInfo()
    {
        NSString *packageInfo = [[TTPUnityServiceManager sharedInstance] getPackageInfo];
        return strdup([packageInfo UTF8String]);
    }

    const char * ttpGetConfigurationJson(const char * serviceName)
    {
        NSDictionary* dictionary = [TTPConfiguration getConfiguration:[[NSString alloc] initWithUTF8String:serviceName]];
        NSError *error;
        if (dictionary != nil){
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dictionary
                                                               options:NSJSONWritingPrettyPrinted
                                                                 error:&error];
            if (! jsonData) {
                NSLog(@"TTPUnitySetup::ttpGetConfigurationJson:error creating json string: %@", error);
            } else {
                NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                return strdup([jsonString UTF8String]);
            }
        }
        return strdup("");
    }

    void ttpCrashApp()
    {
        NSLog(@"TTPUnitySetup::ttpCrashApp");
        dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
            NSArray* arr = [NSArray new];
            NSObject* o = [arr objectAtIndex:10];
        });
    }
    
    bool ttpIsConnected()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            return [serviceManager isConnectedToTheInternet];
        }
        return false;
    }
    
    long ttpGetSessionNumber()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            id<TTPISessionMgr> sessionMgr = [serviceManager get:@protocol(TTPISessionMgr)];
            if(sessionMgr != nil){
                return (long)[sessionMgr getSessionNumber];
            }
        }
        return 0;
    }

    void ttpSetGeoServiceAlwaysReturnedLocation(const char * location){
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            id<TTPIgeoService> geoService = [serviceManager get:@protocol(TTPIgeoService)];
            if(geoService != nil){
                [geoService setAlwaysReturnedLocation:[[NSString alloc] initWithUTF8String:location]];
            }
        }
    }

    void ttpClearGeoServiceAlwaysReturnedLocation(){
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            id<TTPIgeoService> geoService = [serviceManager get:@protocol(TTPIgeoService)];
            if(geoService != nil){
                [geoService clearAlwaysReturnedLocation];
            }
        }
    }

    void ttpAskForIDFA(){
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            [serviceManager askForIDFA];
        }
    }

    void ttpRefuseToAskForIDFA(){
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            [serviceManager refuseToAskForIDFA];
        }
    }

    bool ttpShouldWaitForIDFA(){
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            return [serviceManager shouldAskForIDFA];
        }
        return NO;
    }

    bool ttpShouldShowATTDisclaimer(){
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            return [serviceManager shouldShowATTDisclaimer];
        }
        return false;
    }

    bool ttpIsRemoteConfigExistAndEnabled()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            return [serviceManager isRemoteConfigExistAndEnabled];
        }
        return false;
    }

    void ttpReportIAPToConversion(const char *currency, float price, const char *productId, bool consumable)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        if(serviceManager != nil){
            id<TTPIConversion> conversion = [serviceManager get:@protocol(TTPIConversion)];
            [conversion iapPurchased:[NSString stringWithUTF8String:currency]
                               price:price
                           productId:[NSString stringWithUTF8String:productId]
                          consumable:consumable ? YES : NO];
        }
    }
}
