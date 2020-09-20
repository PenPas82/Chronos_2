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

        public delegate void LocationEventHandler(object sender, LocationEventArgs e);
        public event LocationEventHandler OnLocationChanged;

        readonly MainActivity activity;

        private static Location _location = null;
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
                    if (!_location.Equals(location)) 
                    {
                        activity.radiostop.Checked = true;
                    } 
                    else
                    {
                        activity.radiostop.Checked = false;
                    }
                }
                else
                {
                    _location = location;
                }
               
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