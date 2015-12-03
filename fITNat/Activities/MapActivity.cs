using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;

namespace fITNat.Activities
{
    [Activity(Label = "MapActivity")]
    public class MapActivity : Activity, IOnMapReadyCallback
    {
        private GoogleMap mMap;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.MapLayout);
            SetUpMap();
        }

        private void SetUpMap()
        {
            if (mMap == null)
            {
                var map = Resource.Id.map;
                var fragment = FragmentManager.FindFragmentById<MapFragment>(map);
                fragment.GetMapAsync(this);
            }
        }

        public void OnMapReady(GoogleMap googleMap)
        {
            mMap = googleMap;
        }
    }
}