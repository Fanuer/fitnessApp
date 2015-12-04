
using Android.App;
using Android.OS;
using Android.Views;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Util;
using fitnat.controllers;
using Android.Widget;
using System;

namespace fitnat.fragments
{
    // Implements some GoogleMaps specific functions for interacting with the map
    public class GMapFragment : Fragment
    {
        public GoogleMap Map { get; set; }
        public GMapHandler mapReadyCallback = new GMapHandler();
        private LocationHandler LocHandler;

        private MapFragment mapFragment;

        // before position is set initially
        private bool initZoom = true;
        private bool followPosition = true;

        private Location lastPosition;

        public GMapFragment() { } // obligatory default constructor

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_map, container, false);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            LocHandler = new LocationHandler(Activity.GetSystemService(Activity.LocationService) as LocationManager);
            LocHandler.LocationRequest += (sender, args) =>
            {
                if (args == LocationHandler.messageType.LOCATION)
                {
                    // sets the location initially
                    var loc = (Location)sender;
                    if (followPosition)
                    {
                        lastPosition = loc;
                        SetLocation(loc.Latitude, loc.Longitude);
                    }
                }
                else if (args == LocationHandler.messageType.DISABLED)
                {
                    Toast.MakeText(Activity, Resource.String.ENABLE_GPS, ToastLength.Long).Show();
                }
                else if (args == LocationHandler.messageType.ENABLED)
                {
                    Log.Debug("GPSPROVIDER", "Enabled");
                }
            };

            // initializes the LocationHandler and fires an event if any porblems occur
            LocHandler.Init();
        }

        public override void OnResume()
        {
            base.OnResume();
            SetUpMapIfNeeded();
            LocHandler.Init(); // needs to be reinitialized maybe
        }

        // gets the reference to the map instance
        private void SetUpMapIfNeeded()
        {
            mapFragment = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map9879468784687);
            if (Map == null)
            {
                // registers an event to the handler (implements the callback concept)
                mapReadyCallback.MapReady += (sender, args) =>
                {
                    Map = ((GMapHandler)sender).Map;
                    SetUpMap();
                };

                mapFragment.GetMapAsync(mapReadyCallback);
            }
        }

        // initial setup for the map
        private void SetUpMap()
        {
            Map.AddMarker(new MarkerOptions().SetPosition(new LatLng(0, 0)).SetTitle("Marker"));
            Map.MyLocationEnabled = true;
            Map.UiSettings.CompassEnabled = true;
            Map.UiSettings.MapToolbarEnabled = true;

            MapsInitializer.Initialize(Activity);

            Map.MyLocationButtonClick += (sender, args) => {
                followPosition = true;

                if(lastPosition != null)
                    SetLocation(lastPosition.Latitude, lastPosition.Longitude);
            };

            Map.CameraChange += (sender, args) =>
            {
                // if positions are equal, the CameraChange event was triggered by the fragment itself and not by the user ( => follow mode still active)
                if (lastPosition != null)
                {
                    //Toast.MakeText(Activity, args.Position.Target.Latitude + " == " + lastPosition.Latitude, ToastLength.Long).Show();
                    if (Math.Round(args.Position.Target.Latitude,4) == Math.Round(lastPosition.Latitude, 4) && Math.Round(args.Position.Target.Longitude,4) == Math.Round(lastPosition.Longitude,4))
                        followPosition = true;
                    else
                        followPosition = false;
                }
            };
        }

        // sets the shown area of the map (will be called once for setting the local position)
        public void SetLocation(double lat, double lng)
        {
            if (Map != null)
            {
                var latlng = new LatLng(lat, lng);

                if (initZoom)
                {
                    Map.MoveCamera(CameraUpdateFactory.NewLatLngZoom(latlng, 19));
                }
                else
                    Map.MoveCamera(CameraUpdateFactory.NewLatLng(latlng));

                initZoom = false;
            }
        }
    }
}