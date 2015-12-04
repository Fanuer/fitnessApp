using System;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace fitnat.controllers
{
    class LocationHandler : Java.Lang.Object, ILocationListener
    {
        public enum messageType { DISABLED, ENABLED, LOCATION, STATUSCHANGED };
        public event EventHandler<messageType> LocationRequest;
        public LocationManager LocManager { get; private set; }

        public LocationHandler(LocationManager locMan)
        {
            LocManager = locMan;
        }

        public void Init()
        {
            string Provider = LocationManager.GpsProvider;

            if (LocManager.IsProviderEnabled(Provider))
            {
                LocManager.RequestLocationUpdates(Provider, 2000, 1, this);
            }
            else
            {
                var handler = LocationRequest;
                if (handler != null)
                    handler(this, messageType.DISABLED);
            }
        }

        public void OnLocationChanged(Location location)
        {
            var handler = LocationRequest;
            if (handler != null)
                handler(location, messageType.LOCATION);
        }

        public void OnProviderDisabled(string provider)
        {
            var handler = LocationRequest;
            if (handler != null)
                handler(this, messageType.DISABLED);
        }

        public void OnProviderEnabled(string provider)
        {
            var handler = LocationRequest;
            if (handler != null)
                handler(this, messageType.ENABLED);
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            var handler = LocationRequest;
            if (handler != null)
                handler(status, messageType.STATUSCHANGED);
        }
    }
}