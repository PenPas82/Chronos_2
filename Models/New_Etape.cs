using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Android.Locations;

//using System.
namespace Chronos_2
{

    public class NewEtape 
    {
        private const int _posid = 0;
        private const int _posheure = 4;
        private const int _posdistance = 12;
        private const int _postidtour = 20;
        private const int _lgid = 4;
        private const int _lgheure = 8;
        private const int _lgdistance = 8;
        private const int _lgidtour = 4;
       // private const int _lenght = _lgid + _lgheure + _lgdistance;
        private const int _lenght = _lgid + _lgheure + _lgdistance + _lgid;

        public int lenght
        {
            get { return _lenght; }
        }

        public int IdEtape { get; set; }
        public TimeSpan Heure { get; set; }
        public double Distance { get; set; }
        public int IdTour { get; set; }         // ajout code tournée pour item fiche  //

        public NewEtape() { }

        public NewEtape(int id)
        {
            IdEtape = id;
            Heure = DateTime.Now.TimeOfDay;
        }


        public NewEtape(byte[] collection, int index = 0)
        {
            IdEtape = BitConverter.ToInt32(collection, index + _posid);
            Heure = new TimeSpan(BitConverter.ToInt64(collection, index + _posheure));
            Distance = BitConverter.ToDouble(collection, index + _posdistance);
            IdTour = BitConverter.ToInt32(collection, index + _postidtour);
        }

        public byte[] ToByte()
        {
            List<byte> retour = new List<byte>();
            retour.AddRange(BitConverter.GetBytes(IdEtape));        //4
            retour.AddRange(BitConverter.GetBytes(Heure.Ticks));    //8
            retour.AddRange(BitConverter.GetBytes(Distance));       //8
            retour.AddRange(BitConverter.GetBytes(IdTour));         //4
            return retour.ToArray();
        }
    }
}
