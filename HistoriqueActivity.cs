using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace Chronos_2
{
    [Activity(Label = "Historique", MainLauncher = false)]
    public class HistoriqueActivity : Activity, BottomNavigationView.IOnNavigationItemSelectedListener
    {


        private string[] doc = new string[] { };
        private string texte = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_historique);

            // var button = FindViewById<FloatingActionButton>(Resource.Id.fab);
            // button.Click += Button_Click;

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            ListView listView = FindViewById<ListView>(Resource.Id.listView1);

            doc = Intent.GetStringArrayExtra("Doc");
            if (doc.Length > 0)
            {
                listView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, doc);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            //var secondary
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    StartActivity(typeof(MainActivity));
                    return true;
                case Resource.Id.navigation_dashboard:

                    var email = new Intent(Android.Content.Intent.ActionSend);
                    email.PutExtra(Android.Content.Intent.ExtraEmail, new string[] { "p_penchenat@orange.fr" });


                    foreach (var w in doc)
                    {
                        texte += w + "\r\n";
                    }
                    email.PutExtra(Android.Content.Intent.ExtraSubject, "Historique");
                    email.PutExtra(Android.Content.Intent.ExtraText, texte);
                    email.SetType("message/rfc822");

                    // email.PutExtra(Android.Content.Intent.ExtraStream, filename);
                    StartActivity(email);

                    return true;
                case Resource.Id.navigation_notifications:
                    // textMessage.SetText(Resource.String.title_notifications);
                    return true;
            }
            return false;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }
    }
}