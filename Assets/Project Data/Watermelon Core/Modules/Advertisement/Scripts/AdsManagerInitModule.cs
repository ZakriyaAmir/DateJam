using System.Collections;
using UnityEngine;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace Watermelon
{
    [RegisterModule("Monetization/Ads Manager")]
    public class AdsManagerInitModule : InitModule
    {
        public AdsSettings Settings;
        public GameObject DummyCanvasPrefab;
        public GameObject GDPRPrefab;

        [Space]
        public bool LoadAdOnStart = true;

        public AdsManagerInitModule()
        {
            moduleName = "Ads Manager";
        }

        public override void CreateComponent(Initialiser initialiser)
        {
            AdsManager.Initialise(this, LoadAdOnStart);

#if UNITY_IOS
            if (Settings.IsIDFAEnabled && !AdsManager.IsIDFADetermined())
            {
                if (Settings.SystemLogs)
                    Debug.Log("[Ads Manager]: Requesting IDFA..");

                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif
        }
    }
}