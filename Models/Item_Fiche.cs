using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chronos_2;
using System.MyExtension;
namespace Chronos_2.Models
{
    public class Item_Fiche
    {
        public TimeSpan début { get; set; } = new TimeSpan();
        public TimeSpan fin { get; set; } = new TimeSpan();
        public int effectif { get; set; }
        public string Remarque { get; set; } = "";

        public override string ToString()
        {
            string r = $"[{début.ToString()},{fin.ToString()},{effectif}";
            if (Remarque != "")
            {
                r += $",{Remarque}";
            }
            r += "]";
            return r;
        }
    }
}