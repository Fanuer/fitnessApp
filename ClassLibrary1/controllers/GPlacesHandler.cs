using Android.App;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Util;
using Android.Widget;
using System;
using System.Json;
using System.Threading.Tasks;

namespace fitnat.controllers {
    public class GPlacesHandler {
        // https://developers.google.com/places/web-service/search
        // request: https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=-33.8670522,151.1957362&radius=500&types=gym&key=AIzaSyBfVaiXt2v5Hz52SJwP8eV4nQoswlQXasE

        private LocationHandler LocHandler;
        private Location lastPosition;

        public event EventHandler PlacesResponse;

        public GPlacesHandler() {
        }

        // gets all gyms at the given location and calculate a radius
        public async Task<JsonValue> GetPlacesAsJSON(double lat, double lng, LatLngBounds llb)
        {
            var radius = LatLngDist.distance(llb.Northeast.Latitude, llb.Northeast.Longitude, llb.Southwest.Latitude, llb.Southwest.Longitude, 'K') * 1000 / 2;
            Log.Debug("JSON", radius + "");
            return await GetPlacesAsJSON(lat, lng, radius);
        }

        // gets all gyms at the given location and radius
        public async Task<JsonValue> GetPlacesAsJSON(double lat, double lng, double radius)
        {
            String sLat = lat.ToString().Replace(',', '.');
            String sLng = lng.ToString().Replace(',', '.');
            String sRad = radius.ToString().Replace(',', '.');
            JsonValue geojson = await HTTRequest.GetJSONAsync("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + sLat + "," + sLng + "&radius=" + sRad + "&types=gym&key=AIzaSyBfVaiXt2v5Hz52SJwP8eV4nQoswlQXasE");
            return geojson;
        }

        // Activates the location listener
        // distance: enforces to set a minimum distance that must be reached before the places api will be requested
        public void ActivateLocationChangeRequests(Activity Activity,float distance)
        {
            LocHandler = new LocationHandler(Activity.GetSystemService(Activity.LocationService) as LocationManager);
            LocHandler.LocationRequest += async (sender, args) =>
            {
                if (args == LocationHandler.messageType.LOCATION)
                {
                    // sets the location initially
                    var loc = (Location)sender;
                    var calcDist = double.MaxValue;

                    if(lastPosition != null)
                    {
                        calcDist = LatLngDist.distance(loc.Latitude, loc.Longitude, lastPosition.Latitude, lastPosition.Longitude, 'K');
                    }

                    if (distance < calcDist)
                    {
                        JsonValue x = await GetPlacesAsJSON(loc.Latitude, loc.Longitude, 200);

                        var handler = PlacesResponse;
                        if (handler != null)
                        {
                            handler(x, EventArgs.Empty);
                        }

                        lastPosition = loc;
                    }

                }
                else if (args == LocationHandler.messageType.DISABLED)
                {
                    Toast.MakeText(Activity, Resource.String.ENABLE_GPS, ToastLength.Long).Show();
                }
            };

            // initializes the LocationHandler and fires an event if any porblems occur
            LocHandler.Init();
        }
    }
}