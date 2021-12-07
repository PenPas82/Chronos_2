using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Chronos_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chronos_2
{
    public partial class MainActivity : AppCompatActivity
    {
        private void BuildFiche()
        {

            Dictionary<DateTime, Dictionary<byte, Dictionary<string, Models.Item_Fiche>>> datafiche = new Dictionary<DateTime, Dictionary<byte, Dictionary<string, Item_Fiche>>>();
            var collection = new Historique(backingFile_historique);

            foreach (var item in collection)
            {
                var j = item.Jour;
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
                    datafiche[item.Jour][(byte)item.Sens].Add(nametour, new Item_Fiche() { début = item.Départ,fin = item.Arrivée,  effectif = qx.Value });
                }
            }


        }
/*
        private string Build_Course_Fiche(Course course)
        {
            String r = "";
            var q1 = course.Etapes.GroupBy(x => x.IdTour, (a, b) => new { key = a, value = b });

            foreach (var item in q1)
            {
               r =  Coordonnées_Data.tournées[item.key];
            }
            return r;
        }
*/
    }
}