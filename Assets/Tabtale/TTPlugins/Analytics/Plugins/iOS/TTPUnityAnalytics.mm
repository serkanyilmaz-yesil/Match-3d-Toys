//
//  TTPUnityAnalytics.m
//  Unity-iPhone
//
//  Created by Tabtale on 26/11/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"


@interface TTPUnityAnalytics : NSObject

+ (NSDictionary *) ttpAnalyticsDictionaryFromJsonStr: (const char*) json;

@end

@implementation TTPUnityAnalytics

+ (NSDictionary *) ttpAnalyticsDictionaryFromJsonStr: (const char*) json {
    if (json == nullptr) return nil;
    NSData *data = [[[NSString alloc] initWithUTF8String:json] dataUsingEncoding:NSUnicodeStringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    return dict;
}

extern "C" {

    void ttpLogEvent(int64_t targets, const char* eventName, const char* eventParamsJsonStr, BOOL timed, BOOL internal)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            [analyticsService logEvent:(AnalyticsType)targets
                                  name:[[NSString alloc] initWithUTF8String:eventName]
                                params:[TTPUnityAnalytics ttpAnalyticsDictionaryFromJsonStr:eventParamsJsonStr]
                                 timed:timed
                              internal:internal];
        }
    }
    
    void ttpEndLogEvent(const char* eventName, const char* eventParamsJsonStr)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            [analyticsService endTimedEvent:[[NSString alloc] initWithUTF8String:eventName]
                                     params:[TTPUnityAnalytics ttpAnalyticsDictionaryFromJsonStr:eventParamsJsonStr]
             ];
        }
    }

    bool ttpGetRemoteValue(const char* key, double timeoutInSecs)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            return [analyticsService valueForKey:[[NSString alloc] initWithUTF8String:key] timeout:timeoutInSecs];
        }
        return false;
    }

    bool ttpGetRemoteDictionary(const char* keys, double timeoutInSecs)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            NSArray *keysArray = [[[NSString alloc] initWithUTF8String:keys] componentsSeparatedByCharactersInSet:
                [NSCharacterSet characterSetWithCharactersInString:@";,{}[]"]];

            return [analyticsService dictionaryForKeyArray:keysArray
                                     timeout:timeoutInSecs];
        }
        return false;
    }
    
    
    void ttpAddExtras(const char* extrasStr)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil && nullptr != extrasStr){
            [analyticsService addExtras:[TTPUnityAnalytics ttpAnalyticsDictionaryFromJsonStr:extrasStr]];
        }
    }
    
    
     void ttpRemoveExtras(const char* extrasStr)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil && nullptr != extrasStr){
            NSArray *keys = [[[NSString alloc] initWithUTF8String:extrasStr] componentsSeparatedByString:@";"];
            [analyticsService removeExtras:keys];
        }
        
    }
    
    bool ttpDidFetchComplete()
    {
       TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
       id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
       if(analyticsService != nil){
           return [analyticsService didFetchComplete];
       }
        
       return false;
    }
    
    const char * ttpStringForKey(const char* key)
    {
       TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
       id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
       if(analyticsService != nil && nullptr != key){
           return strdup([[analyticsService stringForKey:[NSString stringWithUTF8String:key]] UTF8String]);
       }
        
       return strdup("");
    }
    
    const char * ttpGetFirebaseInstanceId()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            return strdup([[analyticsService getFirebaseInstanceId] UTF8String]);
        }
        return strdup("");
    }
    
    void ttpSetFirebaseInstanceId(const char* firebaseInstanceId)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            [analyticsService setFirebaseInstanceId:[[NSString alloc] initWithUTF8String:firebaseInstanceId]];
        }
    }
    
    const char * ttpGetCurrentConfig()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            NSError *error;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:[analyticsService getCurrentFirebaseRemoteConfig]
                                                               options:NSJSONWritingPrettyPrinted // Pass 0 if you don't care about the readability of the generated string
                                                                 error:&error];

            if (! jsonData) {
                NSLog(@"ttpGetCurrentConfig error: %@", error);
            } else {
                NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                return strdup([jsonString UTF8String]);
            }
        }
        return strdup("");
    }
    
    void ttpDdnaIsReady(BOOL isReady, const char* userId)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            [analyticsService ddnaIsReady:isReady userId:[[NSString alloc] initWithUTF8String:userId]];
        }
    }
    
    void ttpGetGeo()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            [analyticsService getAndSendGeoCodeAsync];
        }
    }

    const char * ttpGetAdditionalParams()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIanalytics> analyticsService = [serviceManager get:@protocol(TTPIanalytics)];
        if(analyticsService != nil){
            NSError *error;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:[analyticsService getAdditionalEventParams]
                                                               options:NSJSONWritingPrettyPrinted // Pass 0 if you don't care about the readability of the generated string
                                                                 error:&error];

            if (!jsonData) {
                NSLog(@"ttpGetAdditionalParams error: %@", error);
            } else {
                NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                return strdup([jsonString UTF8String]);
            }
        }
        return strdup("");
    }
    
}

@end
