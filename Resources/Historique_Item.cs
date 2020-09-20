using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Chronos_2.Resources
{

    public enum AllerRetour
    {
        Null,
        Matin,
        Midi,
        Soir
    }

    public class Historique_Item
    {
        public DateTime jour { get; set; } 
        public AllerRetour mode { get; set; }
        public TimeSpan depart { get; set; }
        public TimeSpan retour { get; set; }
        public int nbenfant { get; set; }
    
        public Historique_Item()
        {

        }

        public void load(string value)
        {
            var items = value.Split(';', StringSplitOptions.None);

            jour = DateTime.Parse(items[0]);
            mode = (AllerRetour)Enum.Parse(typeof(AllerRetour), items[1]);
            depart = TimeSpan.Parse(items[2]);
            retour = TimeSpan.Parse(items[3]);
            nbenfant = int.Parse(items[4]);
        }

        public string ToCsv()
        {
            string texte = "";
            texte = $"{jour};";
            texte += $"{mode};";
            texte += $"{depart};";
            texte += $"{retour};";
            texte += $"{nbenfant}";
            return texte;
        }

        public override string ToString()
        {
            string texte = "";
            texte = $"{jour.ToString("dddd, dd mm yyyy")}";
            texte += $" {mode.ToString()}";
            texte += $" Départ {depart.ToString("hh mm ss")}";
            texte += $" Retour {retour.ToString("hh mm ss")}";
            texte += $" Nb Enfants {nbenfant}";
            return texte;
        }
    }
    
}