using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Chronos_2
{
    public partial class MainActivity : AppCompatActivity
    {

        private Coordonnée _coordonnée = null;
        private Type_Etape _type_Etape = Type_Etape.Null;

        private void DialodEtape()
        {

            var builder = new AlertDialog.Builder(this);
            var inflater = this.LayoutInflater;
            var dialogview = inflater.Inflate(Resource.Layout.Dialog_Etape, null);
            
            if (dialogview != null)
            {
                var s1 = this.Resources.GetString(Resource.String.Dialog_Coord_Etape, coordonnées.Latitude,coordonnées.Longitude);
                dialogview.FindViewById<TextView>(Resource.Id.coord_etape).Text = s1;
                var group = dialogview.FindViewById<RadioGroup>(Resource.Id.radioGroup1);
                var bt1 =  dialogview.FindViewById<RadioButton>(Resource.Id.button_chauff);

                if (Nb_Coord_Etapes == 0)
                {
                    bt1.Checked = true;
                    group.Enabled = true;
                    dialogview.FindViewById<EditText>(Resource.Id.nom_etape).Text = "Domicile";
                }
                else
                {
                    bt1.Checked = false;
                    bt1.Enabled = true;
                }
                builder.SetView(dialogview);
                builder.SetPositiveButton("Valider", HandlePositiveButtonClick);
                builder.SetNegativeButton("Annuler", HandleNegativeButtonClick);

                group.CheckedChange += Group_CheckedChange;
                
                dialogview.FindViewById<RadioButton>(Resource.Id.button_chauff).Checked = true;
            }
            var dialog = builder.Create();

            dialog.Show();
        }

        private void Group_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
           // _type_Etape = CtrlRadioBuuton(e.CheckedId);
        }

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (Dialog)sender;
            dialog.Dismiss();
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (Dialog)sender;
            _coordonnée = new Coordonnée()
            {
                Id = -1,
                Latitude = coordonnées.Latitude,
                Longitude = coordonnées.Longitude,
                Nom = dialog.FindViewById<EditText>(Resource.Id.nom_etape).Text,
                Type = (byte)_type_Etape,
            };

            var beep = 1; ;
            int idetape = Coordonnées_Data.GetInsert(_coordonnée);
            startcoordonnées = _coordonnée.Location;
            dialog.Dismiss();
        }

        private Type_Etape CtrlRadioBuuton(int e)
        {
            Type_Etape result = Type_Etape.Null;
            switch (e)
            {
                case Resource.Id.button_chauff:
                    result = Type_Etape.Dom_Chauffeur;
                    break;
                case Resource.Id.button_enfant:
                    result = Type_Etape.Dom_Enfant;
                    break;
                case Resource.Id.button_Scolaire:
                    result = Type_Etape.Ets_Scolaire;
                    break;
            }
            return result;
        }
    }
}