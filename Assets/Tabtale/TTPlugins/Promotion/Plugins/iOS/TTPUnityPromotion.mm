//
//  TTPUnityPromotion.mm
//  Unity-iPhone
//
//  Created by TabTale on 06/11/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPIpromotion.h>

extern "C" {

  bool ttpPromotionShow(const char * location)
  {
      TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
      id<TTPIpromotion> promotionService = [serviceManager get:@protocol(TTPIpromotion)];
      if(promotionService != nil){
          return [promotionService showStand:[[NSString alloc] initWithUTF8String:location]];
      }
      return false;
  }

  bool ttpPromotionIsReady()
  {
      TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
      id<TTPIpromotion> promotionService = [serviceManager get:@protocol(TTPIpromotion)];
      if(promotionService != nil){
          return [promotionService isStandReady];
      }
      return false;
  }

}
