using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Chronos_2.Adapters;
using Android.Util;
using System.IO;
using Systéme.Calendrier;

namespace Chronos_2
{
    [Activity(Label = "Historique", MainLauncher = false)]
    public class HistoriqueActivity : AppCompatActivity
    {

      //  private string NAMEFILE_HISTORIQUE = "Coordonnées__SEM_.Dat";
        private Semaine semaine;

        private string _PathFile;
        private string _NameFile;
        private Historique historique = null;
        private ListView ListView_historique;
        //  private AdapterView AdapterView_histo;
        private TextView TextView_semaine;
        private Button bt_prveviou;
        private Button bt_next;

        //private string name = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_historique);

            //Intent
            _PathFile = Intent.GetStringExtra("NameFile");
            var data = Intent.GetBundleExtra("Data");
            var numsem =  data.GetInt("Semaine");
            var numan = data.GetInt("Année");
            semaine = new Semaine(numsem);
            _NameFile = _PathFile.Replace("_SEM_", numsem.ToString());
            //historique = new Historique(_NameFile);
            historique = new Historique(semaine);

            //UI Button
            bt_prveviou = FindViewById<Button>(Resource.Id.button_previou);
            bt_prveviou.Click += Bt_prveviou_Click;

            bt_next = FindViewById<Button>(Resource.Id.button_next);
            bt_next.Click += Bt_next_Click;

            TextView_semaine = FindViewById<TextView>(Resource.Id.textView_Semaine);
            TextView_semaine.Text = semaine.NumSemaine.ToString();

            //UI Barre de menu
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar_course);
            SetActionBar(toolbar);
            toolbar.InflateMenu(Resource.Menu.menu_historique);
            toolbar.NavigationOnClick += Toolbar_NavigationOnClick;
            toolbar.MenuItemClick += Toolbar_MenuItemClick;

            //UI liste des course
            ListView_historique = FindViewById<ListView>(Resource.Id.listView_course);
            ListView_historique.Adapter = new Course_Adaptater(this, historique);
        }

        private void Bt_next_Click(object sender, EventArgs e)
        {
            semaine =  semaine.GetNext();
            TextView_semaine.Text = semaine.NumSemaine.ToString();

            _NameFile = _PathFile.Replace("_SEM_", semaine.NumSemaine.ToString());
            historique = new Historique(_NameFile);
            ListView_historique.Adapter = new Course_Adaptater(this, historique);
        }

        private void Bt_prveviou_Click(object sender, EventArgs e)
        {
            semaine = semaine.GetPreviou();
            TextView_semaine.Text = semaine.NumSemaine.ToString();

            _NameFile = _PathFile.Replace("_SEM_", semaine.NumSemaine.ToString());
            historique = new Historique(_NameFile);
            ListView_historique.Adapter = new Course_Adaptater(this, historique);
        }
/*
        private void ListView_historique_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            Log.Info("HistoriqueActivity.ListView_historique_ItemLongClick", $"=> {e.ToString()}");
        }
*/
        private void Toolbar_MenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            int id = (int)e.Item.ItemId;
            switch (id)
            {
                case Resource.Id.histo_envoi:
                    EnvoiHistorique();
                    break;
                case Resource.Id.histo_reset:
                    ResetHistorique();
                    break;
            }
        }
/*
        private void ListView_historique_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Log.Info("HistoriqueActivity.ListView_historique_ItemClick", $"=> {e.ToString()}");
        }

        private void ListView_historique_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //throw new NotImplementedException();
            Log.Info("HistoriqueActivity.ItemSelected", $"=> {e.ToString()}");
        }
*/
        private void Toolbar_NavigationOnClick(object sender, EventArgs e)
        {
            this.Finish();
            Log.Info("HistoriqueActivity", "Finish");
        }

        public void EnvoiHistorique()
        {
            string BodyMessage = historique.Edit_Fiche();
            var email = new Intent(Android.Content.Intent.ActionSend);
            email.PutExtra(Android.Content.Intent.ExtraEmail, new string[] { "p_penchenat@orange.fr" });
            email.PutExtra(Android.Content.Intent.ExtraSubject, "Historique");
            email.PutExtra(Android.Content.Intent.ExtraText, "Historique de la tournée \r\n"+ BodyMessage);
            email.SetType("message/rfc822");
            StartActivity(email);
        }

        private void ResetHistorique()
        {
            if (_NameFile != "")
            {
                if (File.Exists(_NameFile))
                {
                    File.Delete(_NameFile);
                }
            }
        }
    }
}