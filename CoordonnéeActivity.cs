using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using Android.Support.V7.App;
using Android.Util;

namespace Chronos_2
{

    [Activity(Label = "CoordonnéeActivity")]
    public class CoordonnéeActivity : AppCompatActivity
    {

        private ListView listView_coordonnée;
        private Coordonnées _Coordonnées;

        public string _NameFile;

       // public Dialog_Update_Coordonnée Dialog_Update_Coordonnée = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {

            Log.Info("CoordonnéeActivity", "OnCreate ");

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_coordonnee);

            // ui toolbar -> Navigation & Menu
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.my_toolbar);
          //  SetActionBar(toolbar);

            toolbar.InflateMenu(Resource.Menu.menu_etape);
            toolbar.MenuItemClick += Toolbar_MenuItemClick;
           // toolbar.SetNavigationIcon(Resource.Drawable.ic_home_black_24dp);
            toolbar.NavigationOnClick += Toolbar_NavigationOnClick;
           
            //Ui listView_coordonnée
            listView_coordonnée = FindViewById<ListView>(Resource.Id.listView_coordonnee);
          //  listView_coordonnée.ItemSelected += ListView_coordonnée_ItemSelected;
           // listView_coordonnée.ItemClick += ListView_coordonnée_ItemClick;
            //listView_coordonnée.LongClick += ListView_coordonnée_LongClick;

            var toto = Intent.GetBundleExtra("Data");
           // var titi = toto.GetByteArray("Data");
            //_Coordonnées = new Coordonnées(titi);
            _NameFile = Intent.GetStringExtra("NameFile");
            _Coordonnées = new Coordonnées(_NameFile);

            var adapter = new CoordonéeAdapter(this, _Coordonnées);
            listView_coordonnée.Adapter = adapter;// new CoordonéeAdapter(this, _Coordonnées);

           // Dialog_Update_Coordonnée = new Dialog_Update_Coordonnée(this, _NameFile);
        }
/*
        private void ListView_coordonnée_LongClick(object sender, View.LongClickEventArgs e)
        {
            Log.Info("ListView_coordonnée_LongClick",e.ToString());
        }

        private void ListView_coordonnée_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Log.Info("ListView_coordonnée_ItemClick", e.Id.ToString());
        }

        private void ListView_coordonnée_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Log.Info("ListView_coordonnée_ItemSelected", e.Id.ToString());
        }
*/
        private void Toolbar_MenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            int id = (int)e.Item.ItemId;
            switch (id)
            {
                case Resource.Id.etape_export:

                    break;
                case Resource.Id.etape_reset:
                    _Coordonnées.Clear();
                    ResetEtape();
                    break;
            }
        }
        private void ExportEtape()
        {
            if (_NameFile != "")
            {
                if (File.Exists(_NameFile))
                {
                   // var  backingfile_dest = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Coordonnées.Dat");

                    var path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath;

                    var backingfile_dest = path + @"\Coordonnées.Dat";
                   
                    File.Copy(_NameFile, backingfile_dest);
                }
            }
        }

        private void ResetEtape()
        {
            if (_NameFile != "")
            {
                if (File.Exists(_NameFile))
                {
                    File.Delete(_NameFile);
                }
            }
        }

        private void Toolbar_NavigationOnClick(object sender, EventArgs e)
        {
            this.Finish();
            Log.Info("CoordonnéeActivity", "Finish");
        }
    }
}