using System.Linq;
using Android.Gms.Location;
using Android.Util;
using Android.Locations;
using System;

namespace Chronos_2
{
    public class FusedLocationProviderCallback : LocationCallback
    {

        readonly MainActivity activity;
       // private static Location _StartLocation = null;
        private static Location _location = null;
       // private static long _Mouvement = 0;

        public FusedLocationProviderCallback(MainActivity activity)
        {
            this.activity = activity;
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
           //Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }
       
        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                if (_location != null)
                {
                    var delta = (long)location.DistanceTo(_location);
                    activity.distance = delta;
                    if (delta > 0)
                    {
                        activity.distanceparcourue += delta;

                        double km = (double)activity.distanceparcourue / (double)1000;
                        activity.textdistancecourse.Text = $"{km} Km";
                    }
                }
                _location = location;
                activity.coordonnées = location;
                activity.latitude2.Text = activity.Resources.GetString(Resource.String.latitude_string, location.Latitude);
                activity.longitude2.Text = activity.Resources.GetString(Resource.String.longitude_string, location.Longitude);
            }
            else
            {
                activity.latitude2.SetText(Resource.String.location_unavailable);
                activity.longitude2.SetText(Resource.String.location_unavailable);
            }
        }
    }
}