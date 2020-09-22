using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Locations;

namespace Chronos_2
{
    public class FusedLocationProviderCallback : LocationCallback
    {

        //public delegate void LocationEventHandler(object sender, LocationEventArgs e);
        //public event LocationEventHandler OnLocationChanged;

        readonly MainActivity activity;
        private static Location _StartLocation = null;
        private static Location _location = null;
        //private static float _distanceparcourue = 0;

        private string graphe = "";

        public FusedLocationProviderCallback(MainActivity activity)
        {
            this.activity = activity;
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }

       
        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();
                if (_location != null)
                {
                    var delta = location.DistanceTo(_location);
                    if (delta > 0)
                    {
                        activity.distanceparcourue += delta;
                        activity.radiostop.Checked = true;
                        activity.radiostop.Text = activity.distanceparcourue.ToString("N0");
                    }
                    else
                    {
                        activity.radiostop.Checked = false;
                    }
                    /*
                    if (_location.Latitude != location.Latitude && _location.Longitude != location.Longitude)
                    {
                        activity.radiostop.Checked = true;
                        var distance = location.DistanceTo(_StartLocation);
                        activity.radiostop.Text = distance.ToString();
                       // activity.textMessage.Text = $"Distance {distance} ";
                    } 
                    else
                    {
                        activity.radiostop.Checked = false;
                    }
                    */
                }
                else
                {
                    _StartLocation = location;
                    activity.startcoordonnées = location;
                }

                _location = location;
                activity.coordonnées = location;

                activity.latitude2.Text = activity.Resources.GetString(Resource.String.latitude_string, location.Latitude);
                activity.longitude2.Text = activity.Resources.GetString(Resource.String.longitude_string, location.Longitude);
               // activity.provider2.Text = activity.Resources.GetString(Resource.String.requesting_updates_provider_string, location.Provider);
            }
            else
            {
                activity.latitude2.SetText(Resource.String.location_unavailable);
                activity.longitude2.SetText(Resource.String.location_unavailable);
               // activity.provider2.SetText(Resource.String.could_not_get_last_location);
               // activity.requestLocationUpdatesButton.SetText(Resource.String.request_location_button_text);
            }
        }
    }
}