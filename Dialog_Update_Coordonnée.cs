using Android.App;
using Android.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Linq;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Locations;
using Android.Util;

namespace Chronos_2
{
    public class Dialog_Update_Coordonnée : AppCompatActivity
    {

        private string _namefile;
        readonly Activity _activity;
        private Coordonnée _coordonnée = null;
        private Type_Etape _type_Etape = Type_Etape.Null;
        private Boolean _RetourDialog = false;
        //private Location _location;
        private Dialog dialog;
        private View dialogview;
        private EditText EditText_Name;
        private EditText EditText_Tour;
        //private TextView TextView_Id;
       // private bool IsModif = false;
        private int IdEtape = -1;

        public Boolean RetourDialog
        {
            get { return _RetourDialog; }
        }

        public Coordonnée coordonnée
        {
            get { return _coordonnée; }
        }

        public Dialog_Update_Coordonnée(Activity activity, string namefile)
        {
            _activity = activity;
            _namefile = namefile;

            var builder = new AlertDialog.Builder(_activity);
            var inflater = _activity.LayoutInflater;
            dialogview = inflater.Inflate(Resource.Layout.Dialog_Etape, null);
            if (dialogview != null)
            {
                var group = dialogview.FindViewById<RadioGroup>(Resource.Id.radioGroup1);
                var bt1 = dialogview.FindViewById<RadioButton>(Resource.Id.button_chauff);
                EditText_Name = dialogview.FindViewById<EditText>(Resource.Id.nom_etape);
                EditText_Tour = dialogview.FindViewById<EditText>(Resource.Id.Code_Tour);
                builder.SetView(dialogview);
                builder.SetPositiveButton("Modifier", HandlePositiveButtonClick);
                builder.SetNegativeButton("Annuler", HandleNegativeButtonClick);
                group.CheckedChange += Group_CheckedChange;
            }
            dialog = builder.Create();
        }

        private void Group_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            switch (e.CheckedId)
            {
                case Resource.Id.button_chauff:
                    _type_Etape = Type_Etape.Dom_Chauffeur;
                    break;
                case Resource.Id.button_enfant:
                    _type_Etape = Type_Etape.Dom_Enfant;
                    break;
                case Resource.Id.button_Scolaire:
                    _type_Etape = Type_Etape.Ets_Scolaire;
                    break;
            }
        }

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            // throw new NotImplementedException();
            _RetourDialog = false;
            dialog.Cancel();
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            Log.Info("Dialog_Update_Coordonnée", "PositiveButton_Click");
            _coordonnée.Tour = dialog.FindViewById<EditText>(Resource.Id.Code_Tour).Text;
            _coordonnée.Nom = dialog.FindViewById<EditText>(Resource.Id.nom_etape).Text;
            _coordonnée.Type = (byte)_type_Etape;
            _RetourDialog = true;

            Log.Info("Dialog_Update_Coordonnée", $"Nouvelle etape {IdEtape} ");

            dialog.Cancel();
        }


        public void ShowDialog (Coordonnée data)
        {
            _coordonnée = data;

            //IsModif = true;
            IdEtape = data.Id;

            dialog.Show();
            var s1 = _activity.Resources.GetString(Resource.String.Dialog_Coord_Etape, data.Latitude, data.Longitude);
            dialog.FindViewById<TextView>(Resource.Id.coord_etape).Text = s1;
            dialog.FindViewById<TextView>(Resource.Id.id_coord_etape).Text = data.Id.ToString();
            dialog.FindViewById<EditText>(Resource.Id.nom_etape).Text = data.Nom.Trim();
            dialog.FindViewById<EditText>(Resource.Id.Code_Tour).Text = data.Tour.Trim();

            var x = (Enum.GetValues(typeof(Type_Etape)));
            var z = x.GetValue((int)data.Type);

            switch (z)
            {
                case Type_Etape.Null:
                    break;
                case Type_Etape.Dom_Chauffeur:
                    dialogview.FindViewById<RadioButton>(Resource.Id.button_chauff).Checked = true;
                    break;
                case Type_Etape.Dom_Enfant:
                    dialogview.FindViewById<RadioButton>(Resource.Id.button_enfant).Checked = true;
                    break;
                case Type_Etape.Ets_Scolaire:
                    dialogview.FindViewById<RadioButton>(Resource.Id.button_Scolaire).Checked = true;
                    break;
                case Type_Etape.Dépôt:
                    break;
                case Type_Etape.Parking:
                    break;
                default:
                    break;
            }
        }
    }
}