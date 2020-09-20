using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using FicheLiaison.Models;
using Android.Content;
using Android.Gms.Common;
using Android.Util;
using Android.Gms.Location;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using System.Threading.Tasks;
using Android.Support.V4.App;

namespace Chronos_2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {

        const long ONE_MINUTE = 60 * 1000;
        const long FIVE_MINUTES = 5 * ONE_MINUTE;
        const long TWO_MINUTES = 2 * ONE_MINUTE;
        private const string DTSTART_KEY = "DtStart";
        private const string TIME_KEY = "Time";

        //private DateTime _DtStart;
       // private TimeSpan _Time;
        private static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;
        private static readonly int RC_LOCATION_UPDATES_PERMISSION_CHECK = 1100;

        private static readonly string KEY_REQUESTING_LOCATION_UPDATES = "requesting_location_updates";
        private FusedLocationProviderClient fusedLocationProviderClient;
        private LocationCallback locationCallback;
        private LocationRequest locationRequest;

        View rootLayout;
        TextView provider;
        TextView latitude;
        TextView longitude;

        internal Boolean Mouvement = false;

        internal TextView latitude2;
        internal TextView longitude2;
        bool isGooglePlayServicesInstalled;
        bool isRequestingLocationUpdates;

        TextView textMessage;
        DateTime dtstart = new DateTime();
        DateTime dtstop = new DateTime();
        private long _max = new TimeSpan(4,0,0).Ticks; // temps max de conduite

        private int numberToCompute = 1;

        private BackgroundWorker backgroundWorker1;

        internal Button startandstop;
        internal RadioButton radiostop;

        private string backingFile;
        private string backingFileEtape;

        System.Collections.ArrayList historique = new System.Collections.ArrayList();

        public List<Tour> Data = new List<Tour>();
        public List<Etape> Etapes = new List<Etape>();

        private string filename;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            rootLayout = FindViewById(Resource.Id.root_layout);
            isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();

            //UI Bouton lancement de la tournée
            startandstop = FindViewById<Button>(Resource.Id.button_start);
            startandstop.Click += Startandstop_Click;

            textMessage = FindViewById<TextView>(Resource.Id.message);

            //UI to display Run off Stop 
            radiostop = FindViewById<RadioButton>(Resource.Id.radiostop);
            radiostop.CheckedChange += Radiostop_CheckedChange;

            // UI to display location updates
            // requestLocationUpdatesButton = FindViewById<Button>(Resource.Id.request_location_updates_button);
            latitude2 = FindViewById<TextView>(Resource.Id.latitude2);
            longitude2 = FindViewById<TextView>(Resource.Id.longitude2);
           // provider2 = FindViewById<TextView>(Resource.Id.provider2);

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            
            backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Historique.csv");
            backingFileEtape = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Etapes.csv");

            LoadEtapes();

            isRequestingLocationUpdates = false;

            if (isGooglePlayServicesInstalled)
            {
                locationRequest = new LocationRequest()
                                  .SetPriority(LocationRequest.PriorityHighAccuracy)
                                  .SetInterval(FIVE_MINUTES)
                                  .SetFastestInterval(TWO_MINUTES);
                locationCallback = new FusedLocationProviderCallback(this);
             
                //locationCallback.
                fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);

                
               // getLastLocationButton.Click += GetLastLocationButtonOnClick;
               // requestLocationUpdatesButton.Click += RequestLocationUpdatesButtonOnClick;
            }
            else
            {
               // Snackbar.Make(rootLayout)
                // If there is no Google Play Services installed, then this sample won't run.
                Snackbar.Make(rootLayout, Resource.String.missing_googleplayservices_terminating, Snackbar.LengthIndefinite)
                        .SetAction(Resource.String.ok, delegate { FinishAndRemoveTask(); })
                        .Show();
            }
            InitializeBackgroundWorker();
        }

        private void Radiostop_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            //throw new NotImplementedException();
            var beep = 1;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutLong("DtStart", dtstart.Ticks);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            var n = savedInstanceState.GetLong(DTSTART_KEY);
            dtstart = new DateTime(n);

        }
        private void HandleState(Bundle bundle)
        {

        }
        /// <summary>
        /// Envoie message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FabOnClick(object sender, EventArgs e)
        {
            var email = new Intent(Android.Content.Intent.ActionSend);
            email.PutExtra(Android.Content.Intent.ExtraEmail,new string[] {"p_penchenat@orange.fr"});

            email.PutExtra(Android.Content.Intent.ExtraSubject, "Historique");
            email.PutExtra(Android.Content.Intent.ExtraText, EditEtapes());
            email.SetType("message/rfc822");

           // email.PutExtra(Android.Content.Intent.ExtraStream, filename);
            StartActivity(email);
        }

        protected override void OnRestart()
        {
            base.OnRestart();
        }
        protected override void OnStart()
        {
            base.OnStart();
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnPause()
        {
            base.OnPause();
            SaveEtapes();
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            SaveEtapes();
        }
        /// <summary>
        /// Evenement clik sur bouton Start 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Startandstop_Click(object sender, System.EventArgs e)
        {
            if (dtstart == new DateTime())// & !backgroundWorker1.IsBusy)
            {
                dtstart = DateTime.Now;
                var q1 = ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation);

                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                {
                    //await GetLastLocationFromDevice();
                    await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);

                }
                else
                {
                    RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
                }

                // startandstop.Text = "course";
                // historique.Add($"depart domicile a {dtstart}");
                backgroundWorker1.RunWorkerAsync(numberToCompute);
                Etapes.Add(new Etape() { jour = dtstart.Date, heure = dtstart.TimeOfDay, modeaction = ModeAction.Départ });
            }
            else
            {
                this.backgroundWorker1.CancelAsync();

                startandstop.SetText(Resource.String.title_home);
                dtstop = DateTime.Now;
                Etapes.Add(new Etape() { jour = dtstart.Date, heure = dtstop.TimeOfDay, modeaction = ModeAction.Arrivée });
                EnumTour modetour = EnumTour.Matin;
                if (dtstart.Hour > 12) modetour = EnumTour.Soir;
                Tour tour = new Tour() { Jour = dtstart.Date, Départ = dtstart.TimeOfDay, Arrivée = dtstop.TimeOfDay,Sens = modetour, Effectif =3 };
                TimeSpan durée = dtstop - dtstart;
                dtstart = new DateTime();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            e.Result = Incremment((int)e.Argument, worker, e);
        }

        /// <summary>
        /// Incrementation du chrono
        /// </summary>
        /// <param name="n"></param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        long Incremment(int n, BackgroundWorker worker, DoWorkEventArgs e)
        {
            long result = DateTime.Now.Ticks;
            DateTime DtDepart = DateTime.Now;
            TimeSpan delta = new TimeSpan(0, 0, 0);
            while (!worker.CancellationPending & delta.Ticks <= _max)
            {
                Thread.Sleep(500);
                delta = DateTime.Now - DtDepart;
                result = delta.Ticks;
                int percentComplete = (int)((float)delta.Ticks / (float)_max * 100);

                //var activiy = Intent.
                worker.ReportProgress(percentComplete, delta);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string texte = "";

            BackgroundWorker ctl = (BackgroundWorker)sender;

            if (ctl.IsBusy) 
            { 
                double hh = ((TimeSpan)e.UserState).Hours;
                double mm = ((TimeSpan)e.UserState).Minutes;
                double ss = ((TimeSpan)e.UserState).Seconds;
                texte = $"{hh}:{mm}:{ss}";
            }
            else
            {
                texte = "Domicile";
            }
            startandstop.Text = texte;//((TimeSpan)e.UserState).ToString("hh:mm:ss");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            startandstop.Text = "Domicile";
        }
        /// <summary>
        /// Inititialisation des taches de travail de fond 
        /// </summary>
        private void InitializeBackgroundWorker()
        {
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RC_LAST_LOCATION_PERMISSION_CHECK || requestCode == RC_LOCATION_UPDATES_PERMISSION_CHECK)
            {
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
                {
                    if (requestCode == RC_LAST_LOCATION_PERMISSION_CHECK)
                    {
                        await GetLastLocationFromDevice();
                    }
                    else
                    {
                        await StartRequestingLocationUpdates();
                        isRequestingLocationUpdates = true;
                    }
                }
                else
                {
                    Snackbar.Make(rootLayout, Resource.String.permission_not_granted_termininating_app, Snackbar.LengthIndefinite)
                            .SetAction(Resource.String.ok, delegate { FinishAndRemoveTask(); })
                            .Show();
                    return;
                }
            }
            else
            {
                Log.Debug("FusedLocationProviderSample", "Don't know how to handle requestCode " + requestCode);
            }


            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void RequestLocationPermission(int requestCode)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
            {
                Snackbar.Make(rootLayout, Resource.String.permission_location_rationale, Snackbar.LengthIndefinite)
                        .SetAction(Resource.String.ok,
                                   delegate
                                   {
                                       ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
                                   })
                        .Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
            }
        }

        /// <summary>
        /// Controle si Google Service est Installé 
        /// </summary>
        /// <returns></returns>
        bool IsGooglePlayServicesInstalled()
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                          queryResult, errorString);
            }
            return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task GetLastLocationFromDevice()
        {
            //getLastLocationButton.SetText(Resource.String.getting_last_location);
            var location = await fusedLocationProviderClient.GetLastLocationAsync();

            if (location == null)
            {
                latitude2.SetText(Resource.String.location_unavailable);
                longitude2.SetText(Resource.String.location_unavailable);
                //provider.SetText(Resource.String.could_not_get_last_location);
            }
            else
            {
                latitude2.Text = Resources.GetString(Resource.String.latitude_string, location.Latitude);
                longitude2.Text = Resources.GetString(Resource.String.longitude_string, location.Longitude);
               // provider.Text = Resources.GetString(Resource.String.provider_string, location.Provider);
                //getLastLocationButton.SetText(Resource.String.get_last_location_button_text);
            }
        }

        async Task StartRequestingLocationUpdates()
        {
            //requestLocationUpdatesButton.SetText(Resource.String.request_location_in_progress_button_text);
            await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
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
                    //textMessage.SetText(Resource.String.title_home);
                    return true;
                case Resource.Id.navigation_dashboard:
                    //textMessage.SetText(Resource.String.title_dashboard);
                    var activity = new Intent(this, typeof(HistoriqueActivity));
                    var doc = EditEtapes();
                    activity.PutExtra("Doc", doc);
                    StartActivity(activity);
                    return true;
                case Resource.Id.navigation_notifications:
                   // textMessage.SetText(Resource.String.title_notifications);
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Lecture de l'historique
        /// </summary>
        public void LoadHistorique()
        {
           // var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Historique.csv");

            if (backingFile == null || !File.Exists(backingFile))
            {
                File.CreateText(backingFile);
                textMessage.Text = "Creation du fichier " + backingFile;
            }

            using (var reader = new StreamReader(backingFile, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Data.Add(new Tour(line));
                }
            }
        }

        /// <summary>
        /// Sauvegarde de l'historique
        /// </summary> 
        public void SaveHistorique()
        {
            textMessage.Text = "Ecriture du fichier " + backingFile ;

            using (var writer = File.CreateText(backingFile))
            {
                foreach (var item in Data)
                {
                    writer.WriteLine(item.ToCsv());
                }
            }
        }

        /// <summary>
        /// Lecture du fichier Etape.csv
        /// </summary>
        public void LoadEtapes()
        {
            // var backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Historique.csv");

            if (backingFileEtape == null || !File.Exists(backingFileEtape))
            {
                File.CreateText(backingFileEtape);
                textMessage.Text = "Creation du fichier " + backingFileEtape;
            }

            using (var reader = new StreamReader(backingFileEtape, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Etapes.Add(new Etape(line));
                }
            }
        }

        /// <summary>
        /// Sauvegarde de l'historique
        /// </summary> 
        public void SaveEtapes()
        {
            textMessage.Text = "Ecriture du fichier " + backingFileEtape;

            using (var writer = File.CreateText(backingFileEtape))
            {
                foreach (var item in Etapes)
                {
                    writer.WriteLine(item.ToCsv());
                }
            }
        }

        public void EditHistorique()
        {

        }

        public string[] EditEtapes()
        {
            List<string> doc = new List<string>();
            foreach (var item in Etapes)
            {
                 doc.Add(item.ToEdit()) ;
            }
            return doc.ToArray();
        }
    }
}

