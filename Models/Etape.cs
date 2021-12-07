using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Android.Locations;
using System.MyExtension;

//using System.
namespace Chronos_2
{
    /*
    public enum ModeAction
    {
        Null,
        Départ,
        Pause,
        Arrivée,
    }
    */
    public class Etape
    {
        private const int _lgid = 4;
        private const int _lgheure = 8;
        private const int _lgdépart = 8;
        private const int _lgidtour = 4;
        private const int _lgInfo = 1;
        private const int _lgtournée = 5;

        private const int _posid = 0;
        private const int _posheure = _lgid;
        private const int _posdépart = _posheure + _lgheure;
        private const int _postidtour = _posdépart + _lgdépart;
        private const int _postinfo = _postidtour + _lgidtour;
        private const int _posttournée = _postinfo + _lgInfo;
        private const int _lenght = _posttournée + _lgtournée;

        public int lenght
        {
            get { return _lenght; }
        }

        public int IdEtape { get; set; }
        public TimeSpan Heure { get; set; }             // Heure d'arrivée
        public TimeSpan départ { get; set; }            // Modif Distance --> heure d'arrivée de pause 
        public int IdTour { get; set; }                 // ajout code tournée pour item fiche  //
        public Type_Etape Info_Etape { get; set; }      // Ajout type etape (Domicile Chauffeur , Ets Scolaire, Domicile Enfant, ... ) pour item fiche
        public string NameTour { get; set; }             // Nom de la tournée


        public bool isEnfant
        {
            get { return Info_Etape == Type_Etape.Dom_Enfant;}
        }

        public Etape() { }

        public Etape(int id)
        {
            IdEtape = id;
            Heure = DateTime.Now.TimeOfDay;
        }
       
        public Etape(byte[] collection, int index = 0)
        {
            IdEtape = BitConverter.ToInt32(collection, index + _posid);
            Heure = new TimeSpan(BitConverter.ToInt64(collection, index + _posheure));
            départ = new TimeSpan(BitConverter.ToInt64(collection, index + _posdépart));
            IdTour = BitConverter.ToInt32(collection, index + _postidtour);
            Info_Etape = (Type_Etape)collection[index + _postinfo];
            NameTour = collection.ToString(index + _posttournée, _lgtournée);
        }

        public byte[] ToByte()
        {
            List<byte> retour = new List<byte>();
            retour.AddRange(BitConverter.GetBytes(IdEtape));        //4
            retour.AddRange(BitConverter.GetBytes(Heure.Ticks));    //8
            retour.AddRange(BitConverter.GetBytes(départ.Ticks));   //8
            retour.AddRange(BitConverter.GetBytes(IdTour));         //4
            retour.Add((Byte)Info_Etape);                           //1
            retour.AddRange(NameTour.GetBytes(_lgtournée));
            return retour.ToArray();
        }

        public string ToFiche()
        {
            return $"Id : {IdEtape} Tour :{NameTour} Arrivée : {Heure.ToStringFormat()} Départ : {départ.ToStringFormat()}";
        }
    }
}
