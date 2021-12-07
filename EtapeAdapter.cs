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
using System.MyExtension;

namespace Chronos_2
{
    public class EtapeAdapter : BaseAdapter<Etape>
    {
        private readonly MainActivity _context;
        private readonly List<Etape> _etapes;

        public EtapeAdapter(MainActivity activity, List<Etape> etapes)
        {
            _context = activity;
            _etapes = etapes;
        }

        public override Etape this[int position] 
        {
            get { return _etapes[position]; }
        }

        public override int Count
        {
            get { return _etapes.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convert, ViewGroup parent)
        {
            View view = _context.LayoutInflater.Inflate(Resource.Layout.Etape_Cell, parent, false);
            Etape etape = this[position];
            var id = etape.IdEtape;
            var nom = _context.Coordonnées_Data[id].Nom;
            view.FindViewById<TextView>(Resource.Id.idetape_cell).Text = etape.IdEtape.ToString();
            view.FindViewById<TextView>(Resource.Id.etape_nom_cell).Text = nom;
            view.FindViewById<TextView>(Resource.Id.arivee_etape).Text = etape.Heure.ToStringFormat();
           // view.FindViewById<TextView>(Resource.Id.parcour_cell).Text = etape.Distance.ToString();
            return view;
        }
    }
}