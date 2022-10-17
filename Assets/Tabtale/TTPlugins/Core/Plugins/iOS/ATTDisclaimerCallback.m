//
//  ATTDisclaimerCallback.m
//  Unity-iPhone
//
//  Created by ShmulikA on 04/05/2021.
//

#import "ATTDisclaimerCallback.h"
#import "TTPUnityServiceManager.h"

@implementation ATTDisclaimerCallback

+(void)notify
{
    TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
    id<TTPIunityMessenger> messengerService = [serviceManager get:@protocol(TTPIunityMessenger)];
    
    [messengerService unitySendMessage:"OnAskedForATT" message:""];
}

@end
