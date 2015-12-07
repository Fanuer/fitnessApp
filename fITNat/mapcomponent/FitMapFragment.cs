using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using fitnat.controllers;
using System.Json;
using Android.Gms.Maps.Model;
using Newtonsoft.Json.Linq;
using fITNat;

namespace fitnat.fragments
{
    public class FitMapFragment : Fragment
    {
        private GMapFragment GMap;
        private GPlacesHandler GPlaces;

        public FitMapFragment() { }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_fitmap, container, false);
        }

        public override void OnResume()
        {
            base.OnResume();

            GMap = ChildFragmentManager.FindFragmentById<GMapFragment>(Resource.Id.gmap8485315486415);
            if (GMap == null)
            {
                GMap = FragmentManager.FindFragmentById<GMapFragment>(Resource.Id.gmap8485315486415);
            }
            GPlaces = new GPlacesHandler();

            // registers to map events for adding the places
            GMap.mapReadyCallback.MapReady += (sender, args) =>
            {
                var map = ((GMapHandler)sender).Map;

                map.CameraChange += async (s, a) => {
                    var jsonval = await GPlaces.GetPlacesAsJSON(map.CameraPosition.Target.Latitude, map.CameraPosition.Target.Longitude, 1000);
                    var gyms = Gym.InstatiateManyFromGeoJson(jsonval);

                    if (gyms != null)
                        foreach(var gym in gyms)
                        {
                            Log.Debug("JSON", "" + gym);
                            map.AddMarker(new MarkerOptions().SetPosition(new LatLng(gym.lat, gym.lng)).SetTitle(gym.name));
                        }
                };
            };
        }
    }

    // contains data for a given gym and instantiates itself by the given GeoJSON
    class Gym
    {
        public double lat;
        public double lng;
        public String name;

        public static Gym InstatiateFromGeoJson(JToken jGym)
        {
            var latlng = jGym["geometry"]["location"];
            var gym = new Gym();
            gym.lat = latlng["lat"].ToObject<double>();
            gym.lng = latlng["lng"].ToObject<double>();
            gym.name = jGym["name"].ToString();
            return gym;
        }

        public static Gym[] InstatiateManyFromGeoJson(JsonValue json)
        {
            var strJson = json.ToString();
            JObject geo = JObject.Parse(strJson);

            if (((JArray )geo["results"]).Count > 0)
            {
                var results = (JArray) geo["results"];
                var gyms = new List<Gym>();

                foreach (var data in results)
                {
                    gyms.Add(InstatiateFromGeoJson(data));
                }

                return gyms.ToArray();
            }

            return null;
        }

        public override string ToString()
        {
            return "[Gym: Lat - "+lat+", Lang - "+lng+", Name - "+name+"]";
        }
    }
}