//
//  TTPUnityAppsFlyer.m
//  Unity-iPhone
//
//  Created by Tabtale on 26/11/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"


@interface TTPUnityAppsFlyer : NSObject

+ (NSDictionary *) ttpAppsFlyerDictionaryFromJsonStr: (const char*) json;

@end

@implementation TTPUnityAppsFlyer

+ (NSDictionary *) ttpAppsFlyerDictionaryFromJsonStr: (const char*) json {
    if (json == nullptr) return nil;
    NSData *data = [[[NSString alloc] initWithUTF8String:json] dataUsingEncoding:NSUnicodeStringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    return dict;
}

extern "C" {

    void ttpAppsFlyerLogEvent(const char* eventName, const char* eventParamsJsonStr)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIappsFlyer> appsflyerService = [serviceManager get:@protocol(TTPIappsFlyer)];
        if(appsflyerService != nil){
            [appsflyerService logEvent:[[NSString alloc] initWithUTF8String:eventName]
                                params:[TTPUnityAppsFlyer ttpAppsFlyerDictionaryFromJsonStr:eventParamsJsonStr]];
        }
    }
}

@end
