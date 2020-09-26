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
using System.Globalization;
using Xamarin.Android;

namespace Chronos_2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, 
        BottomNavigationView.IOnNavigationItemSelectedListener
    {
        const long INTERVAL = 1;

        const long FASTESTINTERVAL = INTERVAL * 500;
        const long LOCATIONINTERVAL = INTERVAL * 1000;

       // const long TEN_SECONDS = 10 * 1000;
      //  const long FIVE_SECONDES = 5 * 1000;

      //  const long ONE_MINUTE = 60 * 1000;
      //  const long FIVE_MINUTES = 5 * ONE_MINUTE;
      //  const long TWO_MINUTES = 2 * ONE_MINUTE;

        private const string DISTANCE_KEY = "distanceparcourue";
        private const string DTSTART_KEY = "DtStart";
        private const string REQUESTING_KEY = "isRequestingLocationUpdates";
        private const string TOURNEE_KEY = "tournée";

       // private const string LATITUDESTART_KEY = "LatitudeStart";
      //  private const string LONGITUDESTART_KEY = "LongitudeStart";
      //  private const string LATITUDELAST_KEY = "LatitudeLast";
      //  private const string LONGITUDELAST_KEY = "LongitudeLast";

       // private const string TIME_KEY = "Time";

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
        TextView datejour;

        internal TextView textdistance;
        internal TextView textheure;
        internal TextView latitude2;
        internal TextView longitude2;
        internal TextView textMessage;

        internal Android.Locations.Location startcoordonnées = null;
        private double LatitudeStart = 0;
        private double LongitudeStart = 0;

        private double LatitudeLast = 0;
        private double LongitudeLast = 0;

        internal Boolean Mouvement = false;
        internal Android.Locations.Location coordonnées = null;
        internal Android.Locations.Location lastcoordonnées = null;

        //internal float distance = 0;
        internal long distanceparcourue = 0;

      //  static private CultureInfo myCIintl = new CultureInfo("fr-FR", false);
        //static System.

        bool isGooglePlayServicesInstalled;
        bool isRequestingLocationUpdates;

        // Variable date 
        private DateTime dtjour = new DateTime();      
        private DateTime dtstart = new DateTime();
        private DateTime dtstop = new DateTime();
        private TimeSpan heure = new TimeSpan();
        private TimeSpan DeltaTime = new TimeSpan();

        private long _max = new TimeSpan(4,0,0).Ticks; // temps max de conduite

        private int numberToCompute = 1;

        private BackgroundWorker backgroundWorker1;

        internal Button startandstop;
        internal RadioButton radiostop;

        private string proges = "";
        private string backingFile;
        private string backingFileEtape;

        System.Collections.ArrayList historique = new System.Collections.ArrayList();

        public Tour tournée = null;


        public List<Tour> Data = new List<Tour>();
       // public List<Etape> Etapes = new List<Etape>();
        public List<Détail> Détails = new List<Détail>();

        private string filename;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

           // xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            rootLayout = FindViewById(Resource.Id.root_layout);
            isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();

            //UI Bouton lancement de la tournée
            startandstop = FindViewById<Button>(Resource.Id.button_start);
            startandstop.Click += Startandstop_Click;

            //UI Date du jour
            datejour = FindViewById<TextView>(Resource.Id.datejour);
            dtjour = DateTime.Now.Date;
            string strdt = dtjour.ToString("dd-MM-yyyy");
            datejour.Text = dtjour.ToString("dddd, dd MMMM yyyy");

            //ui heure
            textheure = FindViewById<TextView>(Resource.Id.heure);

            //UI distance affichage dans fusedlocation
            textdistance = FindViewById<TextView>(Resource.Id.distance);

            //UI Message
            textMessage = FindViewById<TextView>(Resource.Id.message);

            //UI to display Run off Stop 
            radiostop = FindViewById<RadioButton>(Resource.Id.radiostop);
            radiostop.CheckedChange += Radiostop_CheckedChange;

            // UI to display location updates
            latitude2 = FindViewById<TextView>(Resource.Id.latitude2);
            longitude2 = FindViewById<TextView>(Resource.Id.longitude2);

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
            
            backingFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Historique.dat");
            backingFileEtape = backingFile + "/" + strdt;

            isRequestingLocationUpdates = false;

            if (isGooglePlayServicesInstalled)
            {
                locationRequest = new LocationRequest()
                                  .SetPriority(LocationRequest.PriorityHighAccuracy)
                                  .SetInterval(LOCATIONINTERVAL)
                                  .SetFastestInterval(FASTESTINTERVAL);

                locationCallback = new FusedLocationProviderCallback(this);

                fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            }
            else
            {
                Snackbar.Make(rootLayout, Resource.String.missing_googleplayservices_terminating, Snackbar.LengthIndefinite)
                        .SetAction(Resource.String.ok, delegate { FinishAndRemoveTask(); })
                        .Show();
            }

            if (savedInstanceState != null)
            {
                var beep = 1;
            }


            InitializeBackgroundWorker();

            //ResetHistorique();

        }

        /// <summary>
        /// Changement de conduite 
        /// création d'une  ligne détail et mise a jour de la tournée en cour
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Radiostop_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Détail item = null;
            if (e.IsChecked)   // Stop -> Start
            {
                item = new Détail()
                {
                    modeaction = StopOffSart.Start,
                    heure = DateTime.Now.TimeOfDay,
                    distance = distanceparcourue,
                    durée =  DeltaTime,
                    latitude = coordonnées.Altitude,
                    longitude = coordonnées.Longitude,
                };
            }
            else
            {
                item = new Détail()
                {
                    modeaction = StopOffSart.Stop,
                    heure = DateTime.Now.TimeOfDay,
                    distance =distanceparcourue,
                    durée = DeltaTime,
                    latitude = coordonnées.Altitude,
                    longitude = coordonnées.Longitude,
                };
            }
            if(tournée != null && item != null)
            {
                Détails.Add(item);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutLong(DTSTART_KEY, dtstart.Ticks);
            outState.PutLong(DISTANCE_KEY, distanceparcourue);
            outState.PutBoolean(REQUESTING_KEY, isRequestingLocationUpdates);
            //outState.PutByteArray(TOURNEE_KEY, tournée.ToByte());
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            var n = savedInstanceState.GetLong(DTSTART_KEY);
            dtstart = new DateTime(n);
            distanceparcourue = savedInstanceState.GetLong(DISTANCE_KEY);
            isRequestingLocationUpdates = savedInstanceState.GetBoolean(REQUESTING_KEY);
          //  var buffer = savedInstanceState.GetByteArray(TOURNEE_KEY);
            //tournée = new Tour(buffer);
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
           // email.PutExtra(Android.Content.Intent.ExtraText, EditEtapes());
            email.SetType("message/rfc822");

           // email.PutExtra(Android.Content.Intent.ExtraStream, filename);
            StartActivity(email);
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            
            Log.Info("MainActivity", "On Restart -> "+dtstart.ToString());
            
            /*
            if(isRequestingLocationUpdates)
                await StartRequestingLocationUpdates();
            */
        }

        protected override void OnResume()
        {
            base.OnResume();
            Log.Info("MainActivity", "On Resume");

        }
        protected override void OnStart()
        {
            base.OnStart();
            Log.Info("MainActivity", "On Start");

        }
        protected override void OnPause()
        {
            base.OnPause();
            Log.Info("MainActivity", "On Pause");
            SaveHistorique();

            //Bundle outstate = null;
            // this.OnSaveInstanceState(outstate);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
           // SaveHistorique();
        }
        protected override void OnStop()
        {
            base.OnStop();
            Log.Info("MainActivity", "On Stop");
            SaveHistorique();

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
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
                {
                    await StartRequestingLocationUpdates();
                    isRequestingLocationUpdates = true;
                }
                else
                {
                    RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
                }
                backgroundWorker1.RunWorkerAsync(numberToCompute);
               
                // creation nouvelle tournée //
                EnumTour modetour = EnumTour.Matin;
                if (dtstart.Hour > 12) modetour = EnumTour.Soir;
                tournée = new Tour()
                {
                    Jour = dtstart.Date,
                    Départ = dtstart.TimeOfDay,
                    Sens = modetour,
                };
                Détails = new List<Détail>();
            }
            else
            {
                this.backgroundWorker1.CancelAsync();

                startandstop.SetText(Resource.String.title_home);
                dtstop = DateTime.Now;
                if (tournée != null)
                {
                    tournée.Arrivée = dtstop.TimeOfDay;
                    tournée.Effectif = 3;
                }
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
        private long Incremment(int n, BackgroundWorker worker, DoWorkEventArgs e)
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
        private async void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string texte = "";
            int reste;
            DeltaTime = (TimeSpan)e.UserState;
            BackgroundWorker ctl = (BackgroundWorker)sender;
            if (ctl.IsBusy) 
            {
                double hh = ((TimeSpan)e.UserState).Hours;
                double mm = ((TimeSpan)e.UserState).Minutes;
                double ss = ((TimeSpan)e.UserState).Seconds;
                texte = $"{hh}:{mm}:{ss}";
                Math.DivRem((int)ss, (int)INTERVAL, out reste);
                if(reste == 0)
                {
                    await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
                }
            }
            else
            {
                texte = "Domicile";
            }
            startandstop.Text = texte;//((TimeSpan)e.UserState).ToString("hh:mm:ss");
            DateTime dtvar = DateTime.Now;
            textheure.Text = dtvar.ToShortTimeString();
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            startandstop.Text = "Domicile";
            await RemoveRequestingLocationUpdates();
            SaveHistorique();

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
           // Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

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
        /// 
        private async Task GetLastLocationFromDevice()
        {
            coordonnées = await fusedLocationProviderClient.GetLastLocationAsync();

            if (coordonnées == null)
            {
                latitude2.SetText(Resource.String.location_unavailable);
                longitude2.SetText(Resource.String.location_unavailable);
            }
            else
            {
                if(lastcoordonnées != null)
                {
                    if (coordonnées.Latitude == lastcoordonnées.Latitude && coordonnées.Longitude == lastcoordonnées.Longitude)
                    {
                        radiostop.Checked = false;
                    }
                    else
                    {
                        if(radiostop.Checked == false)
                        {
                            radiostop.Checked = true;
                        }
                    }
                }
                else
                {
                    LatitudeStart = coordonnées.Latitude;
                    LongitudeStart = coordonnées.Longitude;
                    var etape = new Etape() { jour = dtstart.Date, heure = DateTime.Now.TimeOfDay, lieu = coordonnées.ToString(), commentaire = "Depart de la tournée" };
                }
                lastcoordonnées = coordonnées;
                latitude2.Text = Resources.GetString(Resource.String.latitude_string, coordonnées.Latitude);
                longitude2.Text = Resources.GetString(Resource.String.longitude_string, coordonnées.Longitude);
            }
        }

        async Task StartRequestingLocationUpdates()
        {
           await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);      
        }
        async  Task RemoveRequestingLocationUpdates()
        {
            await fusedLocationProviderClient.RemoveLocationUpdatesAsync(locationCallback);
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
                    return true;
                case Resource.Id.navigation_dashboard:

                    LoadHistorique();

                    var activity = new Intent(this, typeof(HistoriqueActivity));
                    activity.PutExtra("Doc", Data.ToString());
                    StartActivity(activity);
                    return true;
                case Resource.Id.navigation_notifications:
                    ResetHistorique();
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Lecture de l'historique
        /// </summary>
        public void LoadHistorique()
        {
            if (backingFile == null || !File.Exists(backingFile))
            {
                File.CreateText(backingFile);
                textMessage.Text = "Creation du fichier " + backingFile;
            }
            else
            {
                textMessage.Text = "Lecture du fichier " + backingFile;
                using (var filestream = new FileStream(backingFile, FileMode.Open))
                {
                    if (filestream.Length > 0)
                    {
                        byte[] buffer = new byte[filestream.Length];
                        filestream.Read(buffer);
                        Data = new Tournées(buffer);
                    }
                }
            }
        }

        /// <summary>
        /// Sauvegarde de l'historique
        /// </summary> 
        public void SaveHistorique()
        {
            if(filename != "" || tournée.ToByte().Length > 0)
            {
                if (backingFile == null || !File.Exists(backingFile))
                {
                    File.CreateText(backingFile);
                    textMessage.Text = "Creation du fichier " + backingFile;
                }

                textMessage.Text = "Ecriture du fichier " + backingFile ;
                using (var filestream = new FileStream(backingFile,FileMode.Append) )
                {
                    filestream.Write(tournée.ToByte());
                }

                if (!Directory.Exists(backingFileEtape))
                {
                    Directory.CreateDirectory(backingFileEtape);
                    textMessage.Text = "Creation du repertoire " + backingFileEtape;
                }
                using (var filestream = new FileStream(backingFileEtape+"/Détail.dat", FileMode.Append))
                {
                    textMessage.Text = "Ecriture du fichier " + backingFileEtape + "/Détail.dat";

                    foreach (var item in Détails)
                    {
                         filestream.Write(item.ToByte());
                    }
                }
            }
        }

        public void ResetHistorique()
        {
            if (backingFile == null)
            {
                if (File.Exists(backingFile))
                {
                    File.Delete(backingFile);
                }
            }
        }
        /*
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
        */
        /*
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
        */
    }
}

