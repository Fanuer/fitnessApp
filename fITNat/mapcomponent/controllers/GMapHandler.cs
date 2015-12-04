using System;
using Android.Gms.Maps;

namespace fitnat {
    public class GMapHandler : Java.Lang.Object, IOnMapReadyCallback {
        public GoogleMap Map { get; private set; }

        public event EventHandler MapReady;

        public void OnMapReady(GoogleMap googleMap) {
            Map = googleMap;
            var handler = MapReady;
            if (handler != null)
                handler(this, EventArgs.Empty); //sends the Map object back when completed*/
        }
    }
}