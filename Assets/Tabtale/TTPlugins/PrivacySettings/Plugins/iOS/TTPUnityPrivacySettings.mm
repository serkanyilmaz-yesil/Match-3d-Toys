//
//  TTPUnityPrivacySettings.mm
//  Unity-iPhone
//
//  Created by TabTale on 06/11/2018.
//

#import "TTPUnityServiceManager.h"

@interface TTPUnityPrivacySettings : NSObject

@end

@implementation TTPUnityPrivacySettings

extern "C" {
    
    const char * ttpPrivacySettingsGetConsent()
    {
        
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIprivacySettings> privacySettings = [serviceManager get:@protocol(TTPIprivacySettings)];
        TtConsentMode consentMode = [privacySettings getConsent];
        NSString *consentModeStr = [privacySettings stringFromConsentMode:consentMode];
        return strdup([consentModeStr UTF8String]);
    }
    
    void ttpPrivacySettingsSetConsent(const char *consent)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIprivacySettings> privacySettings = [serviceManager get:@protocol(TTPIprivacySettings)];
        TtConsentMode mode = [privacySettings consentFromString:[[NSString alloc] initWithUTF8String:consent]];
        [privacySettings setConsent:mode];
    }
    
    void ttpPrivacySettingsForgetUser()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIprivacySettings> privacySettings = [serviceManager get:@protocol(TTPIprivacySettings)];
        [privacySettings forgetUser];
    }
    
    void ttpPrivacySettingsShowPrivacySettings()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIprivacySettings> privacySettings = [serviceManager get:@protocol(TTPIprivacySettings)];
        [privacySettings showPrivacySettings];
    }
    
    void ttpPrivacySettingsSetAge(int age)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIprivacySettings> privacySettings = [serviceManager get:@protocol(TTPIprivacySettings)];
        [privacySettings setAge:age];
    }
    
    bool ttpPrivacyShouldShowAgeGate()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIprivacySettings> privacySettings = [serviceManager get:@protocol(TTPIprivacySettings)];
        return [privacySettings shouldShowAgeGate];
    }
    
    bool ttpIsCcpaJurisdiction()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIprivacySettings> privacySettings = [serviceManager get:@protocol(TTPIprivacySettings)];
        return [privacySettings isCcpaJurisdiction];
    }
    
}

@end
