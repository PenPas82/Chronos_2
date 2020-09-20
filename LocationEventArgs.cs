using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Gms.Location;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Chronos_2
{
    public class LocationEventArgs : EventArgs
    {
        public LocationEventArgs(Android.Locations.Location location, bool Mouvement) 
        {
            location = null;
            Mouvement = false;
        }
        public Android.Locations.Location location { get; } // readonly
        public bool Mouvement { get; } // readonly

    }
}