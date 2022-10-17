using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tabtale.TTPlugins
{
    public class TTPEvents
    {

        public const string TRANSACTION = "transaction";
        public const string SUBSCRIPTION_STARTED = "subscriptionStarted";
        
        public const string TUTORIAL_STEP = "tutorialStep";
        public const string MAIN_SCREEN = "mainScreen";
        public const string EXCLUDE_FROM_AB_TEST = "excludeFromABTest";
        
        public const string LEVEL_UP = "levelUp";
        public const string MISSION_STARTED = "missionStarted";
        public const string MISSION_COMPLETED = "missionCompleted";
        public const string MISSION_ABANDONED = "missionAbandoned";
        public const string MISSION_FAILED = "missionFailed";

        public const string BLENDED_OBJECT_ON_SCREEN = "blendedObjectOnScreen";
        public const string BLENDED_OBJECT_OFF_SCREEN = "blendedObjectOffScreen";

        public const string AD_SHOW = "adShow";

        public const string RATE_US_BUTTON = "rateUsButton";
        public const string RATE_US_POPUP = "rateUsPopup";
    }
}
