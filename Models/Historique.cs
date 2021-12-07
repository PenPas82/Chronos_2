using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chronos_2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Systéme.Calendrier;
using System.MyExtension;
namespace Chronos_2
{
    public class Historique : List<Course>
    {

        private const string NAMEFILE_HISTORIQUE = "Historique__SEM_.Dat";

        //private const int LENGHT = 20;

        private static string NameFile { get; set; } = "";

        private static Semaine Sem {get;set;}

        private Dictionary<DateTime, Dictionary<byte, Dictionary<string, Models.Item_Fiche>>> datafiche = null;

        private Course _course;

        public Historique() { }


        public Historique(Semaine sem)
        {
            var namefilehistosem = NAMEFILE_HISTORIQUE.Replace("_SEM_", sem.NumSemaine.ToString());
            NameFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), namefilehistosem);
            Sem = sem;
            load();
            ToFiche();
        }

        public Historique(string name)
        {
            NameFile = name;
            if (NameFile == "") return;

            if (NameFile.Contains("_"))
            {
                var p1 = NameFile.IndexOf("_");
                var p2 = NameFile.IndexOf(".");
                var s1 = NameFile.Substring(p1, p2 - p1);
            }

            if (!File.Exists(NameFile))
            {
                File.Create(NameFile);
            }
            else
            {
                load();
                /*
                int lenght = 0;
                byte[] bufferheader = new byte[4];

                byte[] buffer; // = new byte[0];
                int start = 0;
                using (var filestream = new FileStream(NameFile, FileMode.Open))
                {
                    while (start < filestream.Length)
                    {
                        filestream.Position = start;
                        filestream.Read(bufferheader, 0, 4);
                        lenght = BitConverter.ToInt32(bufferheader);
                        buffer = new byte[lenght];
                        filestream.Read(buffer, 0,lenght);
                        _course = new Course(buffer);
                        this.Add(_course);
                        start += lenght+4;
                    }
                    filestream.Close();
                }
                */
            }
        }

        private void load()
        {
            int lenght = 0;
            byte[] bufferheader = new byte[4];

            byte[] buffer; // = new byte[0];
            int start = 0;
            using (var filestream = new FileStream(NameFile, FileMode.Open))
            {
                while (start < filestream.Length)
                {
                    filestream.Position = start;
                    filestream.Read(bufferheader, 0, 4);
                    lenght = BitConverter.ToInt32(bufferheader);
                    buffer = new byte[lenght];
                    filestream.Read(buffer, 0, lenght);
                    _course = new Course(buffer);
                    this.Add(_course);
                    start += lenght + 4;
                }
                filestream.Close();
            }
        }

        public override string ToString()
        {
            string texte = "";
            foreach (var item in this)
            {
                texte = texte + item.ToEdit();
            }
            return texte;
        }


        private void ToFiche()
        {
            datafiche = new Dictionary<DateTime, Dictionary<byte, Dictionary<string, Item_Fiche>>>();
            foreach (var item in this)
            {
                if (!datafiche.ContainsKey(item.Jour))
                    datafiche.Add(item.Jour, new Dictionary<byte, Dictionary<string, Item_Fiche>>());

                if (!datafiche[item.Jour].ContainsKey((byte)item.Sens))
                    datafiche[item.Jour].Add((byte)item.Sens, new Dictionary<string, Item_Fiche>());

                var qx = item.GetNameTour();
                var nametour = qx.Key;

                if (datafiche[item.Jour][(byte)item.Sens].ContainsKey(nametour))
                {
                    datafiche[item.Jour][(byte)item.Sens][nametour].effectif += qx.Value;
                }
                else
                {
                    datafiche[item.Jour][(byte)item.Sens].Add(nametour, new Item_Fiche() { début = item.Départ, fin = item.Arrivée, effectif = qx.Value });
                }
            }
        }

        public string Edit_Fiche() 
        {
            if (datafiche == null) ToFiche();
            string[] doc = new string[25];

            string fiche = "";

            DateTime jour = Sem.FirstDayWeek;
            int nl = 0;
            for (int i = 0; i < 5; i++)
            {
                doc[nl] = jour.ToString("dddd dd");

                if (datafiche.ContainsKey(jour))
                {
                    foreach (var item in datafiche[jour])
                    {                        
                        nl += 1;
                        var s1 = Enum.GetName(typeof(EnumSens), item.Key);
                        /*
                        if ((EnumSens)item.Key == EnumSens.Aller)
                        {
                            nl = i + 1;
                           // doc[i + 1] = Enum.GetName(typeof(EnumSens), item.Key);
                        }
                        else if ((EnumSens)item.Key == EnumSens.Retour)
                        {
                            nl = i + 2;
                        }
                        */

                        doc[nl] = Enum.GetName(typeof(EnumSens), item.Key);
                        foreach (var item2 in item.Value)
                        {
                            nl += 1;
                            var line = $"S:{item2.Key,20}{item2.Value.début.ToStringFormat(),20}{item2.Value.fin.ToStringFormat(),20}{item2.Value.effectif}";
                            doc[nl] = line;
                        }
                    }
                    nl += 1;
                }
                else
                {
                    nl += 3;
                }
                
                jour = jour.AddDays(1);
            }

            foreach (var item in doc)
            {
                fiche += item + "\r\n";
            }
            
            return fiche;
        }

        public string To_Line_Fiche()
        {
            if (datafiche == null) ToFiche();

            string fiche = "";
            foreach (var item in datafiche)
            {
                var j = item.Key;
                int n = (int)j.DayOfWeek;
                fiche = $"[{n}";
                foreach (var item2 in item.Value)
                {
                    var x = Enum.GetName(typeof(EnumSens), item2.Key);
                    fiche += $"[{x}";
                    foreach (var item3 in item2.Value)
                    {
                        fiche += $"[{item3.Key}{item3.Value.ToString()}]";
                    }
                    fiche += "]";
                }
                fiche += "]";
            }
            return fiche;

        }
    }
}