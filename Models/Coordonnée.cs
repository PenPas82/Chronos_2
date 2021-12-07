using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.MyExtension;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Locations;

namespace Chronos_2
{
    public enum Type_Etape
    {
        Null,
        Dom_Chauffeur,
        Dom_Enfant,
        Ets_Scolaire,
        Dépôt,
        Parking
    }

    public class SampleEventArgs
    {
        public SampleEventArgs(object arg) { Arg = arg; }
        public object Arg { get; } // readonly
    }


    public class Coordonnée
    {
        public delegate void SampleEventHandler(object sender, SampleEventArgs e);

        // Declare the event.
        public event SampleEventHandler OnValueChanged;

        private const int _lenght = 60;
        private const int _posid = 0;
        private const int _poslat = 4; 
        private const int _poslon = 12;
        private const int _postyp = 20;
        private const int _postour = 21;
        private const int _posnom = 30;

        private const int _lgid = 4;
        private const int _lglat = 8;
        private const int _lglon = 8;
        private const int _lgtyp = 1;
        private const int _lgtour = 9;
        private const int _lgnom = 30;

        private int _Id;            //  0 - 4
        private double _Latitude;   //  4 - 8
        private double _Longitude;  // 12 - 8
        private byte _Type;         // 20 - 1
        private string _Tour = "";       // 21 - 9 (max)
        private string _Nom = "";        // 30 - 30 (max)

        private Location _location = new Location("");

        private float _Distance = 0;

        public int Id 
        {
            get { return _Id; }
            set { _Id = value; }
        }
        
        public double Latitude 
        {
            get { return _Latitude; }
            set { 
                _Latitude = value;
                _location.Latitude = value;
            }
        }   
        
        public double Longitude
        {
            get { return _Longitude; }
            set { 
                _Longitude = value;
                _location.Longitude = value;
            }
        }

        public byte Type
        {
            get { return _Type; }
            set { 
                if (value != _Type)
                {
                    //if (_Type != 0)
                   // {
                        _Type = value;
                        OnValueChanged?.Invoke(this, new SampleEventArgs(value));
                   // }
                    //else
                    //{
                     //   _Type = value;
                    //}
                }

            }
        }
        public string Nom
        {
            get { return _Nom; }
            set { 
                if(value != _Nom)
                {
                    if(_Nom != "")
                    {
                        _Nom = value;
                        OnValueChanged?.Invoke(this, new SampleEventArgs(value));
                    }
                    else
                    {
                        _Nom = value;
                    }
                }
            }
        }

        public string Tour
        {
            get { return _Tour; }
            set {
                if (value != _Tour)
                {
                    if (_Tour != "")
                    {
                        _Tour = value;
                        OnValueChanged?.Invoke(this, new SampleEventArgs(value));
                    }
                    else
                    {
                        _Tour = value;
                    }
                }
            }
        }

        public string StrType
        {
            get { return Enum.GetName(typeof(Type_Etape), _Type); }
        }

        public bool IsDomicile
        {
            get { if (_Type == (byte)Type_Etape.Dom_Chauffeur) return true; return false; }
        }

        public bool IsEtape
        {
            get
            {
                bool ok = false;
                switch ((Type_Etape)_Type)
                {
                    case Type_Etape.Null:
                    case Type_Etape.Dom_Chauffeur:
                    case Type_Etape.Dépôt:
                        ok = false;
                        break;
                    case Type_Etape.Dom_Enfant:
                    case Type_Etape.Ets_Scolaire:
                    case Type_Etape.Parking:
                        ok = true;
                        break;
                    default:
                        break;
                }
                return ok;
            }
        }


        public Location Location
        {
            get { return _location; }
        }

        public float Distance
        {
            get { return _Distance; }
        }

        private double delta = 0.000005;

        public Coordonnée() { }

        public Coordonnée(Coordonnée arg) 
        {
            this.Id = arg.Id;
            this.Nom = arg.Nom;
            this.Latitude = arg.Latitude;
            this.Longitude = arg.Longitude;
            this.Tour = arg.Tour;
            this.Type = arg.Type;
        }

        public Coordonnée(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Coordonnée(byte[] buffeur, int index = 0)
        {
            Id = BitConverter.ToInt32(buffeur,index + _posid);
            Latitude = BitConverter.ToDouble(buffeur, index + _poslat);
            Longitude = BitConverter.ToDouble(buffeur, index + _poslon);
            if (buffeur.Length > 20)
            {
                Type = buffeur[index + _postyp];
                Tour = buffeur.ToString(index + _postour, _lgtour);
                Nom = buffeur.ToString(index + _posnom, _lgnom);
            }
        }

        public byte[] ToByte()
        {
            List<byte> retour = new List<byte>();
            retour.AddRange(BitConverter.GetBytes(Id));
            retour.AddRange(BitConverter.GetBytes(Latitude));
            retour.AddRange(BitConverter.GetBytes(Longitude));
            retour.Add(Type);
            retour.AddRange(Tour.GetBytes(9));
            retour.AddRange(Nom.GetBytes(30));
            return retour.ToArray();
        }

        public float DistanceTo(Location other)
        {
            _Distance = this._location.DistanceTo(other);
            return _Distance;
        }


        public bool Equals(Coordonnée other)
        {
            if (other.Latitude == this.Latitude & other.Longitude == this.Longitude)
                return true;
            else
                return false;
        }

        public bool Equals(double latitude, double longitude)
        {

            double delta1 = Math.Abs(this.Latitude - latitude);
            double delta2 = Math.Abs(this.Longitude - longitude);
            if (delta1 < delta & delta2 < delta)
                return true;
            else
                return false;
        }
    }
}