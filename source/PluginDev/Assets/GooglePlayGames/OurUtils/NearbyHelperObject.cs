#if UNITY_ANDROID

namespace GooglePlayGames.OurUtils
{
    using BasicApi.Nearby;
    using System;
    using UnityEngine;

    public class NearbyHelperObject : MonoBehaviour
    {
        // our (singleton) instance
        private static NearbyHelperObject instance = null;

        // timers to keep track of discovery and advertising
        private static double mAdvertisingRemaining = 0;
        private static double mDiscoveryRemaining = 0;

        // nearby client to stop discovery and to stop advertising
        private static INearbyConnectionClient mClient = null;

        public static void CreateObject(INearbyConnectionClient client)
        {
            if (instance != null)
            {
                return;
            }

            mClient = client;
            if (Application.isPlaying)
            {
                // add an invisible game object to the scene
                GameObject obj = new GameObject("PlayGames_NearbyHelper");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<NearbyHelperObject>();
            }
            else
            {
                instance = new NearbyHelperObject();
            }
        }

        private static double ToSeconds(TimeSpan? span)
        {
            if (!span.HasValue)
            {
                return 0;
            }

            if (span.Value.TotalSeconds < 0)
            {
                return 0;
            }

            return span.Value.TotalSeconds;
        }

        public static void StartAdvertisingTimer(TimeSpan? span)
        {
            mAdvertisingRemaining = ToSeconds(span);
        }

        public static void StartDiscoveryTimer(TimeSpan? span)
        {
            mDiscoveryRemaining = ToSeconds(span);
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public void Update()
        {
            // check if currently advertising
            if (mAdvertisingRemaining > 0)
            {
                mAdvertisingRemaining -= Time.deltaTime;
                if (mAdvertisingRemaining < 0)
                {
                    mClient.StopAdvertising();
                }
            }

            // check if currently discovering
            if (mDiscoveryRemaining > 0)
            {
                mDiscoveryRemaining -= Time.deltaTime;
                if (mDiscoveryRemaining < 0)
                {
                    mClient.StopDiscovery(mClient.GetServiceId());
                }
            }
        }
    }
}
#endif