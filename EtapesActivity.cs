using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chronos_2
{
    [Activity(Label = "EtapesActivity")]
    public class EtapesActivity :  ListActivity
    {
        private ListView listView_etape;
        private FloatingActionButton Button_Plus;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_etape);

            //Ui View Liste Etape
            listView_etape = FindViewById<ListView>(Resource.Id.listView_etape);
           // listView_etape.Adapter = new CoordonéeAdapter(this,MainActivity.Coordonnées_Data.data)



            //Ui Bouton Ajout
            Button_Plus = FindViewById<FloatingActionButton>(Resource.Id.fabplus);



        }

    }
}