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
//using Android.Support.V4.App;
//using Android.Support.V7.App;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Chronos_2
{
    public class Etape_Dialogue : Android.Support.V4.App.DialogFragment
    {
        private EditText nom_etape;
        private EditText num_etape;
        private RadioButton bt1;
        private RadioButton bt2;
        private RadioButton bt3;
        //private Button bt4;
        //private Button bt5;

        public string result = "";

        public Etape_Dialogue() { }

        public Dialog onCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //var builder = new AlertDialog.Builder();

            var inflater = Activity.LayoutInflater;

            var dialogview = inflater.Inflate(Resource.Layout.Dialog_Etape, null);

            if(dialogview != null)
            {

                nom_etape = dialogview.FindViewById<EditText>(Resource.Id.nom_etape);
                num_etape = dialogview.FindViewById<EditText>(Resource.Id.num_etape);
                bt1 = dialogview.FindViewById<RadioButton>(Resource.Id.button_chauff);
                bt2 = dialogview.FindViewById<RadioButton>(Resource.Id.button_Scolaire);
                bt3 = dialogview.FindViewById<RadioButton>(Resource.Id.button_enfant);
               // bt4 = dialogview.FindViewById<Button>(Resource.Id.button_annuler);
               // bt5 = dialogview.FindViewById<Button>(Resource.Id.button_Validier);

                bt1.CheckedChange += Bt1_CheckedChange;
                bt2.CheckedChange += Bt2_CheckedChange;
                bt3.CheckedChange += Bt3_CheckedChange;
               // bt4.Click += Bt4_Click;
               // bt5.Click += Bt5_Click;

                builder.SetView(dialogview);
                builder.SetPositiveButton("Valider", HandlePositiveButtonClick);
                builder.SetNegativeButton("Annuler", HandleNegativeButtonClick);
            }
            /*

            */
            return builder.Create();
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (Dialog)sender;
            result = "Valider";
            dialog.Dismiss();
        }
        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (Dialog)sender;
            result = "Annuler";
            dialog.Dismiss();
        }


        private void Bt3_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Bt2_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void Bt1_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
           // throw new NotImplementedException();
        }
    }
}