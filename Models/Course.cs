using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.MyExtension;

namespace Chronos_2
{
    public enum EnumTour
    {
        Null,
        Matin,
        Midi,
        Soir
    }

    public enum EnumCourse
    {
        Null,
        Matin,
        Midi,
        Soir
    }

    public enum EnumSens
    {
        Null,
        Aller,
        Retour
    }

    public class Course :IEquatable<Course>
    {

        private const int _lgJour = 8;
        private const int _lgSens = 1;
        private const int _lgDépart = 8;
        private const int _lgArrivée = 8;
        private const int _lgIdDépart = 4;
        private const int _lgIdArrivée = 4;

        private const int _posjour = 0;                                 //0
        private const int _posSens = _lgJour;                           //8
        private const int _posDépart = _posSens+ _lgSens;               //9
        private const int _posArrivée = _posDépart+ _lgDépart;          //17
        private const int _posIdDépart = _posArrivée+ _lgArrivée;       //25
        private const int _posIdArrivée = _posIdDépart+ _lgIdDépart;    //29

        private const int _lenghtHeader = _lgJour + _lgSens + _lgDépart + _lgArrivée + _lgIdDépart + _lgIdArrivée;  //33


        public DateTime Jour { get; set; } = new DateTime();            // Date de la course 
        public EnumSens Sens { get; set; } = EnumSens.Null;             // Aller 1iére Etape Dom Enfant, Retour 1iére Etape Ets Scolaire
        public TimeSpan Départ { get; set; }   = new TimeSpan();        // Heure de départ de la course (Départ domicile)
        public TimeSpan Arrivée { get; set; }  = new TimeSpan();        // Heure de fin de course       (Arrivé domicile)
        public int IdDépart { get; set; } = -1;                         // Id Coordonnée de départ (Domicile Chaufeur,... 
        public int IdArrivée { get; set; } = -1;                         // Id Coordonnée d'arrivé (..., Domicile chauffeur)

        public List<Etape> Etapes { get; set; } = new List<Etape>();
       
        private Etape etape = null;
        private static int _PositionBuffeur = 0;
        private static int _Index = 0;

        public int Lenght
        {
            get { return _lenghtHeader; }
        }

        public int PositionBuffeur
        {
            get { return _PositionBuffeur; }
        }

        public Course()
        {
           // _Index = 0;
        }

        public Course(byte[] collection)
        {
            //byte[] tempo = new byte[12];
            Jour = new DateTime(BitConverter.ToInt64(collection,0));     
            Sens = (EnumSens)collection[_posSens];                                
            Départ = new TimeSpan(BitConverter.ToInt64(collection, _posDépart));    
            Arrivée = new TimeSpan(BitConverter.ToInt64(collection, _posArrivée));   
            IdDépart = BitConverter.ToInt32(collection, _posIdDépart);   
            IdArrivée = BitConverter.ToInt32(collection, _posIdArrivée);   
            _PositionBuffeur = _lenghtHeader;
            while (_PositionBuffeur < collection.Length)
            {
                etape = new Etape(collection, _PositionBuffeur);
                Etapes.Add(etape);
                _PositionBuffeur += etape.lenght;
            }
            _Index += 1;
        }

        public KeyValuePair<string,int> GetNameTour()
        {
            KeyValuePair<string, int> retour;

            var q1 = Etapes.GroupBy(x => x.NameTour,
                (a, b) =>  new {
                Key =a,
                Effectif = b.Where(y=> y.isEnfant).Count()
            });

            string s = "";
            int r = 0;
            if (q1.Any())
            {
                foreach (var item in q1)
                {
                    if (s != "") s += "+";
                    s += item.Key;
                    r += item.Effectif;
                }
            }
            retour = new KeyValuePair<string, int>(s, r);
            return retour;
        }

        public byte[] ToByte()
        {
            List<byte> retour = new List<byte>();
            retour.AddRange(BitConverter.GetBytes(Jour.Ticks));     //long 8
            retour.Add((byte)Sens);                                       //byte 1
            retour.AddRange(BitConverter.GetBytes(Départ.Ticks));   //long 8
            retour.AddRange(BitConverter.GetBytes(Arrivée.Ticks));  //long 8
            retour.AddRange(BitConverter.GetBytes(IdDépart));
            retour.AddRange(BitConverter.GetBytes(IdArrivée));

            foreach (var item in Etapes)
            {
                retour.AddRange(item.ToByte());
            }
            retour.InsertRange(0,BitConverter.GetBytes(retour.Count));
            
            return retour.ToArray();
        }

        public override string ToString()
        {
            string texte = "";
            texte = $"jour:{Jour.ToShortDateString()},";
            texte += $"sens:{Sens.ToString()},";
            texte += $"départ de {IdDépart} a {Départ.ToString()},";
            texte += $"arrivé a {IdArrivée} a {Arrivée.ToString()},";
            return texte;
        }

        public string ToEdit(bool titre = false)
        {

            string texte = "";
            string[] Listetournée = new string[] { "2.9","1.2" };

            StringBuilder stringBuilder = new StringBuilder();
            string s1 = Enum.GetName(typeof(EnumCourse), Sens);
            // string s2 = Listetournée[Tournée];
            string s2 = "";
            string s3 = $"{Départ.Hours,2}:{Départ.Minutes,2}";
            string s4 = $"{Arrivée.Hours,2}:{Arrivée.Minutes,2}";

            if (titre)
            {
                texte += "|   Date    |      |Tourné | Dep. | Arr. |\r\n";
            }
            texte += $"|{ Jour.ToShortDateString(),11}|{ s1,6}|{ s2,7}|{ s3,6}|{s4,6}|\r\n";
           /*
            foreach (var item in Etapes)
            {
                texte += $"|          |      |{item.IdEtape,7}|{item.Heure,12}|\r\n";
            }
           */
            return texte;
        }

        public string ToFiche()
        {
            string RN =  "\r\n";
            string texte = "---- Course -----"+RN;
            texte += $"Jour : {Jour.ToShortDateString()}{RN}";
            texte += $"Sens : {Enum.GetName(typeof(EnumSens),Sens)}{RN}";
            texte += $"Départ : {Départ.ToStringFormat()}{RN}";
            texte += $"Arrivé : {Arrivée.ToStringFormat()}{RN}";
           // texte += $"Effectif : {Effectif}{RN}";
            texte += "Détail " + RN;

            foreach (var item in Etapes)
            {
                texte += "\t"+ item.ToFiche() + RN;
            }
            return texte;
        }

        public bool Equals(Course other)
        {
            if (Jour == other.Jour & Sens == other.Sens & Départ == other.Départ)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
      
        public bool IsNull()
        {
            if (Jour == new DateTime() || Sens == 0 || Départ == new TimeSpan())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
