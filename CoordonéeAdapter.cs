using Android.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace Chronos_2
{
    public class CoordonéeAdapter : BaseAdapter<Coordonnée>
    {
        private readonly Activity _context;
       // private readonly List<Coordonnée> _coordonnées;
        private readonly Coordonnées _coordonnées;

        private CheckBox check;
        private EditText editText;
        private Coordonnée _curent = null;

        private Dialog_Update_Coordonnée Dialog_Update_Coordonnée = null;

        public CoordonéeAdapter(Activity activity, Coordonnées coordonnée)
        {
            _context = activity;
            _coordonnées = coordonnée;
            _curent = new Coordonnée();

            _curent.OnValueChanged += _curent_OnValueChanged;

            Dialog_Update_Coordonnée = new Dialog_Update_Coordonnée(activity, "");
        }

        private void _curent_OnValueChanged(object sender, SampleEventArgs e)
        {
        }
        public override Coordonnée this[int position]
        { 
            get
            {
                _curent = _coordonnées[position];
                return _coordonnées[position]; 
            }
        }

        public override int Count 
        {
            get { return _coordonnées.Count; }
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
                view = _context.LayoutInflater.Inflate(Resource.Layout.CoordonneeCell, parent, false);
            }
            Coordonnée coordonnée = this[position];

            check = view.FindViewById<CheckBox>(Resource.Id.checkBox1);
            check.Tag = position;

            check.CheckedChange += Check_CheckedChange;

            editText = view.FindViewById<EditText>(Resource.Id.Coord_code_Tour_Cell);

            view.FindViewById<TextView>(Resource.Id.Coord_id_cell).Text = coordonnée.Id.ToString();
            view.FindViewById<TextView>(Resource.Id.Coord_Type_Cell).Text = coordonnée.StrType;
            view.FindViewById<TextView>(Resource.Id.Coord_code_Tour_Cell).Text = coordonnée.Tour.Trim();
            view.FindViewById<TextView>(Resource.Id.Coord_Nom_Cell).Text = coordonnée.Nom.Trim();
            view.FindViewById<TextView>(Resource.Id.Coord_latitude_cell).Text = coordonnée.Latitude.ToString("#0.000000");
            view.FindViewById<TextView>(Resource.Id.Coord_longitude_cell).Text = coordonnée.Longitude.ToString("#0.000000");

            return view;
        }

        private void Check_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Log.Info("CoordonéeAdapter", "Check_CheckedChange ");
            CheckBox ctl = (CheckBox)sender;
            _curent = new Coordonnée(this[(int)ctl.Tag]);
            _curent.OnValueChanged += _curent_OnValueChanged1;
            Dialog_Update_Coordonnée.ShowDialog(_curent);
        }

        private void _curent_OnValueChanged1(object sender, SampleEventArgs e)
        {
            Coordonnée ctl = (Coordonnée)sender;
            _coordonnées.Update(ctl);
        }
    }
}