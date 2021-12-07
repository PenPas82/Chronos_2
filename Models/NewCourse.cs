using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Chronos_2
{
    public class NewCourse
    {
        public DateTime Jour { get; set; } = new DateTime();
        public byte     Sens { get; set; } = 0;
        public byte     Tournée { get; set; } = 0;
        public TimeSpan Départ { get; set; }   = new TimeSpan();
        public TimeSpan Arrivée { get; set; }  = new TimeSpan();
        public int      Effectif { get; set; }= 0;
        public List<NewEtape> Etapes { get; set; } = new List<NewEtape>();

        private NewEtape etape = null;
        private int id;
        private TimeSpan time;

        private static int _PositionBuffeur = 0;
        private static int _Index = 0;

        public int PositionBuffeur
        {
            get { return _PositionBuffeur; }
        }

        public NewCourse()
        {
           // _Index = 0;
        }

        public NewCourse( Course arg, List<string> arg1)
        {
            this.Jour = arg.Jour;
            this.Sens = arg.Sens;
            this.Tournée = arg.Tournée;
            this.Départ = arg.Départ;
            this.Arrivée = arg.Arrivée;
            this.Effectif = arg.Effectif;

            foreach (var item in arg.Etapes)
            {
                
                //var IdTour = arg1.FindIndex(x=> x== )

                this.Etapes.Add(new NewEtape() {
                    IdEtape = item.IdEtape, 
                    Heure = item.Heure,
                    Distance = item.Distance
                });
            }
        }


        public NewCourse(byte[] collection)
        {
            byte[] tempo = new byte[12];
            Jour = new DateTime(BitConverter.ToInt64(collection,0));       // 8
            Sens = collection[8];                                           // 1
            Tournée = collection[9];                                        // 1
            Départ = new TimeSpan(BitConverter.ToInt64(collection,10));    // 8
            Arrivée = new TimeSpan(BitConverter.ToInt64(collection,18));   // 8
            Effectif = BitConverter.ToInt32(collection,26);                // 4
            _PositionBuffeur = 30;
            while (_PositionBuffeur < collection.Length)
            {
                etape = new NewEtape(collection, _PositionBuffeur);
                Etapes.Add(etape);
                _PositionBuffeur += etape.lenght;
            }
            _Index += 1;
        }

        public override string ToString()
        {
            string texte = "";
            texte = $"jour:{Jour.ToShortDateString()},";
            texte += $"sens:{Sens.ToString()},";
            texte += $"départ:{Départ.ToString()},";
            texte += $"arrivé:{Arrivée.ToString()},";
            texte += $"effectif:{Effectif.ToString()}";
            return texte;
        }

        public string ToEdit(bool titre = false)
        {

            string texte = "";
            string[] Listetournée = new string[] { "2.9","1.2" };

            StringBuilder stringBuilder = new StringBuilder();
            string s1 = Enum.GetName(typeof(EnumCourse), Sens);
            string s2 = Listetournée[Tournée];
            string s3 = $"{Départ.Hours,2}:{Départ.Minutes,2}";
            string s4 = $"{Arrivée.Hours,2}:{Arrivée.Minutes,2}";

            if (titre)
            {
                texte += "|   Date    |      |Tourné | Dep. | Arr. |\r\n";
            }
            texte += $"|{ Jour.ToShortDateString(),11}|{ s1,6}|{ s2,7}|{ s3,6}|{s4,6}|\r\n";
            foreach (var item in Etapes)
            {
                texte += $"|          |      |{item.IdEtape,7}|{item.Heure,12}|\r\n";
            }
            return texte;

        }

        public byte[] ToByte()
        {
            List<byte> retour = new List<byte>();
            Effectif = Etapes.Count;

            retour.AddRange(BitConverter.GetBytes(Jour.Ticks));     //long 8
            retour.Add(Sens);                                       //byte 1
            retour.Add(Tournée);                                    //byte 1
            retour.AddRange(BitConverter.GetBytes(Départ.Ticks));   //long 8
            retour.AddRange(BitConverter.GetBytes(Arrivée.Ticks));  //long 8
            retour.AddRange(BitConverter.GetBytes(Effectif));       //Int 4
            for (int i = 0; i < Effectif; i++)
            {
                retour.AddRange(Etapes[i].ToByte());
            }
            retour.InsertRange(0,BitConverter.GetBytes(retour.Count));
            
            return retour.ToArray();
        }


        public void ToFiche()
        {
          // var q1 = Etapes.GroupBy(x=> x.)
        }
    }
}
