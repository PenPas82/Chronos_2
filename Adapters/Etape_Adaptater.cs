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

namespace Chronos_2.Adapters
{
    public class Etape_Adaptater : BaseAdapter<Etape>
    {
        private readonly Activity _context;
        private readonly List<Etape> _etapes;

        public Etape_Adaptater(Activity activity, List<Etape> etapes)
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

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(Resource.Layout.Etape_Cell, parent, false);
            }
            Etape etape = this[position];
            view.FindViewById<TextView>(Resource.Id.idetape_cell).Text = etape.IdEtape.ToString();
            view.FindViewById<TextView>(Resource.Id.arivee_etape).Text = etape.Heure.ToString("mm hh");
           //view.FindViewById<TextView>(Resource.Id.)
            return view;
        }
    }
}