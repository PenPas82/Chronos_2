using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chronos_2
{
    public class Position : IEquatable<Position>
    {
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;

        public bool IsNullorZero()
        {
            if (Latitude == 0 & Longitude == 0) return true; else return false;
        }

        public Position() { }

        public Position(Location arg)
        {
            Latitude = arg.Latitude;
            Longitude = arg.Longitude;
        }
        public void Zero()
        {
            Latitude = 0;
            Longitude = 0;
        }

        public void Set(Location arg)
        {
            Latitude = arg.Latitude;
            Longitude = arg.Longitude;
        }

        public bool Equals(Position other)
        {
            if (other.Latitude == this.Latitude & other.Longitude == this.Longitude)
                return true;
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            Location item = null;
            if (obj is Location)
            {
                item = (Location)obj;
                if (item.Latitude == this.Latitude & item.Longitude == this.Longitude)
                    return true;
                else
                    return false;    
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            Random r = new Random();
            return r.Next();
        }
    }
}