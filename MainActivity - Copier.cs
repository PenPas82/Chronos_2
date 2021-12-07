using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Android.Content;
using Android.Gms.Common;
using Android.Util;
using Android.Locations;
using Android.Gms.Location;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using System.Threading.Tasks;
using Android.Support.V4.App;
using System.Globalization;
using System.Linq;
using Systéme.Calendrier;
using System.MyExtension;

using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Chronos_2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public partial class MainActivity_Copie : AppCompatActivity, 
        Toolbar.IOnMenuItemClickListener
    // BottomNavigationView.IOnNavigationItemSelectedListener
    {
        const long INTERVAL = 1;

        const long FASTESTINTERVAL = INTERVAL * 500;
        const long LOCATIONINTERVAL = INTERVAL * 1000;
        private const float PRESITION_ETAPE = 10;

        private const string DISTANCE_KEY = "distanceparcourue";
        private const string DTSTART_KEY = "DtStart";
        private const string REQUESTING_KEY = "isRequestingLocationUpdates";
        private const string TOURNEE_KEY = "course";

        private const string NAMEFILE_COORDONNEES = "Coordonnées.Dat";
        private const string NAMEFILE_HISTORIQUE = "Coordonnées__SEM_.Dat";

        private static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;

        private FusedLocationProviderClient fusedLocationProviderClient;
        private LocationCallback locationCallback;
        private LocationRequest locationRequest;

        View rootLayout;
        TextView datejour;

        internal TextView textdistance;
        internal TextView textdistancecourse;
        internal TextView textheure;
        internal TextView latitude2;
        internal TextView longitude2;
        internal TextView textMessage;
        internal Location startcoordonnées = null;
        internal Location etapecoordonnées = null;
        internal Location coordonnées = null;
        internal Location lastcoordonnées = null;

        internal long distanceparcourue = 0;

        bool isGooglePlayServicesInstalled;
        bool isRequestingLocationUpdates;

        // Variable date 
        private DateTime dtjour = new DateTime();      
        private DateTime dtstart = new DateTime();
        private DateTime dtstop = new DateTime();
       // private TimeSpan heure = new TimeSpan();
        private TimeSpan timestart = new TimeSpan();

        private TimeSpan DeltaTime = new TimeSpan();
        private Semaine semaine;

        //variable tache de fond
        private long _max = new TimeSpan(4,0,0).Ticks; // temps max de conduite
        private int numberToCompute = 1;
        private BackgroundWorker backgroundWorker1;
        private BackgroundWorker backgroundLocation;

        internal Button buttonetape;
        internal Button startandstop;
        internal ListView ListView_etapes;

        private string backingFile_historique;
        private string backingfile_coordonnées;
        
        public Course course = null;

        public List<Course> Historique_Semaine = new List<Course>();
        internal Coordonnées Coordonnées_Data = new Coordonnées();
        public int Nb_Coord_Etapes = 0;

        public List<Etape> Etapes = new List<Etape>();

        public Dialog_New_Coordonnée Dialog_New_Coordonnée = null;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            // Initialisation 
            dtjour = DateTime.Now.Date;
            semaine = new Semaine(dtjour);
            var s1 = NAMEFILE_HISTORIQUE.Replace("_SEM_", semaine.NumSemaine.ToString());

            //
            backingFile_historique = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), s1);
            backingfile_coordonnées = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), NAMEFILE_COORDONNEES);

            Dialog_New_Coordonnée = new Dialog_New_Coordonnée(this, backingfile_coordonnées);

           // ResetHistorique();
            
            Coordonnées_Data = new Coordonnées(backingfile_coordonnées);

            // xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            rootLayout = FindViewById(Resource.Id.root_layout);
            isGooglePlayServicesInstalled = IsGooglePlayServicesInstalled();

            //UI barre de menu
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar_main);
            toolbar.NavigationOnClick += Toolbar_NavigationOnClick;
            toolbar.InflateMenu(Resource.Menu.navigation);
            toolbar.MenuItemClick += Toolbar_MenuItemClick;

            //Menu menu = FindViewById<Menu>(Resource.Id.navigation);

            //UI Bouton lancement de la course
            startandstop = FindViewById<Button>(Resource.Id.button_start);
            startandstop.Click += Startandstop_Click;
            startandstop.Text = "";

            //UI Liste des étapes
            ListView_etapes = FindViewById<ListView>(Resource.Id.etape_listView);


            //UI Bonton Etape
            buttonetape = FindViewById<Button>(Resource.Id.button_etape);
            buttonetape.Click += Buttonetape_Click;

            //UI Date du jour
            datejour = FindViewById<TextView>(Resource.Id.datejour);

            string strdt = dtjour.ToString("dd-MM-yyyy");
            datejour.Text = $"{dtjour.ToString("dddd, dd MMMM yyyy")} Sem:{semaine.NumSemaine.ToString()}";
            var jour = dtjour.DayOfWeek;

            //ui heure
            textheure = FindViewById<TextView>(Resource.Id.heure);
            textheure.Text = dtjour.TimeOfDay.ToString();

            //UI distance affichage dans fusedlocation
            textdistance = FindViewById<TextView>(Resource.Id.distance);

            //UI distance affichage dans fusedlocation
            textdistancecourse = FindViewById<TextView>(Resource.Id.distance_course);

            //UI Message
            textMessage = FindViewById<TextView>(Resource.Id.message);

            // UI to display location updates
            latitude2 = FindViewById<TextView>(Resource.Id.latitude2);
            longitude2 = FindViewById<TextView>(Resource.Id.longitude2);


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

            InitialLocation();

            backgroundLocation.RunWorkerAsync(numberToCompute);

        }

        private void Toolbar_MenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            // throw new NotImplementedException();
            int id = (int)e.Item.ItemId;
            Intent activity;
            Bundle bundle;
            switch (id)
            {
                case Resource.Id.navigation_home:
                    activity = new Intent(this, typeof(HistoriqueActivity));
                    activity.PutExtra("NameFile", backingFile_historique);
                    StartActivity(activity);
                    break;
                case Resource.Id.navigation_dashboard:
                    activity = new Intent(this, typeof(CoordonnéeActivity));
                    bundle = new Bundle();
                    bundle.PutByteArray("Data", Coordonnées_Data.ToByte());
                    activity.PutExtra("Data", bundle);
                    StartActivity(activity);
                    break;
                case Resource.Id.navigation_notifications:
                    ResetHistorique();
                    break;
            }
        }

        private void Toolbar_NavigationOnClick(object sender, EventArgs e)
        {
            this.Finish();
        }

        private void ListView_coordonées_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            textMessage.Text = "Etape " + e.ToString();
        }

        private void Buttonetape_Click(object sender, EventArgs e)
        {
            Dialog_New_Coordonnée.ShowDialog(coordonnées);

            if (Dialog_New_Coordonnée.RetourDialog)
            {
                if (course != null)
                {
                    Etape etape = new Etape() { Heure = DateTime.Now.TimeOfDay, IdEtape = Dialog_New_Coordonnée.coordonnée.Id };
                    Etapes.Add(etape);
                    ListView_etapes.Adapter = new EtapeAdapter(this,Etapes);
                }
                lastcoordonnées = coordonnées;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutLong(DTSTART_KEY, dtstart.Ticks);
            outState.PutLong(DISTANCE_KEY, distanceparcourue);
            outState.PutBoolean(REQUESTING_KEY, isRequestingLocationUpdates);
            //outState.PutByteArray(TOURNEE_KEY, course.ToByte());
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            var n = savedInstanceState.GetLong(DTSTART_KEY);
            dtstart = new DateTime(n);
            distanceparcourue = savedInstanceState.GetLong(DISTANCE_KEY);
            isRequestingLocationUpdates = savedInstanceState.GetBoolean(REQUESTING_KEY);
            var buffer = savedInstanceState.GetByteArray(TOURNEE_KEY);
            //course = new Tour(buffer);
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
          //  SaveHistorique();

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
        }
        
        /// <summary>
        /// Initialisation de la position du départ de la course
        /// (Domicile Etape 0)
        /// </summary>
        private async void InitialLocation()
        {
            bool ok = false;

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await StartRequestingLocationUpdates();
                isRequestingLocationUpdates = true;
            }
            else
            {
                RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
            }


            /*
            if(startcoordonnées == null)
            {
                await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
                
                if(coordonnées != null)
                {
                    ok = CtrlStartCoordonnéAsync();

                    if (!ok)
                    {
                        backgroundLocation.RunWorkerAsync(numberToCompute);
                    }
                    else
                    {
                        backgroundWorker1.RunWorkerAsync(numberToCompute);
                    }
                } 
            }
            */
        }

        /// <summary>
        /// Controle de la position
        /// </summary>
        /// <returns></returns>
        private bool CtrlStartCoordonnéAsync()
        {
            bool ok = false;
            if (Coordonnées_Data.Count > 0)
            {
                var r = Coordonnées_Data.GetCoordonnée(coordonnées);
                if (r != null)
                {
                    textMessage.Text = $"etape {r.Nom}";
                    if (r.IsDomicile & startcoordonnées==null)
                    {
                        startandstop.Text = r.Nom;
                        dtstart = new DateTime();
                        startcoordonnées = coordonnées;
                        lastcoordonnées = coordonnées;
                        ok = true;
                    }
                }
            }
            else
            {
                // var p = await Dialog_New_Coordonnée.ShowDialogasync(coordonnées);
                Dialog_New_Coordonnée.ShowDialog(coordonnées);
                if (Dialog_New_Coordonnée.RetourDialog)
                {
                    var beep=1;
                }
                //DialodEtape();
                //if (startcoordonnées != null) ok = true;
            }
            return ok;
        }
       
        /// <summary>
        /// Evenement clik sur bouton Start 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Startandstop_Click(object sender, System.EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                if (backgroundLocation.IsBusy)
                {
                    backgroundLocation.CancelAsync();
                }
                dtstart = DateTime.Now;
                backgroundWorker1.RunWorkerAsync(numberToCompute);
                InitialCourse(dtstart);
            }
            else
            {
                this.backgroundWorker1.CancelAsync();
                dtstop = DateTime.Now;
                if (course != null)
                {
                    course.Arrivée = dtstop.TimeOfDay;
                }
                dtstart = new DateTime();
            }
        }
      
        /// <summary>
        ///  Chrono en tache de fond
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
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int reste;
            TimeSpan timecurent = DateTime.Now.TimeOfDay;
            TimeSpan deltatime;

            DeltaTime = (TimeSpan)e.UserState;
            BackgroundWorker ctl = (BackgroundWorker)sender;
            if (ctl.IsBusy) 
            {
                textheure.Text = timecurent.ToStringFormat();
                double hh = ((TimeSpan)e.UserState).Hours;
                double mm = ((TimeSpan)e.UserState).Minutes;
                double ss = ((TimeSpan)e.UserState).Seconds;
                Math.DivRem((int)ss, (int)INTERVAL, out reste);
                if(reste == 0)
                {
                    await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);

                    if(startcoordonnées != null)
                    {
                       // var delta = (long)coordonnées.DistanceTo(lastcoordonnées);
                        if (coordonnées.DistanceTo(lastcoordonnées)>0)
                        {
                            if(dtstart == new DateTime())
                            {
                                dtstart = DateTime.Now;
                                //InitialCourse(dtstart);
                                EnumTour modetour = EnumTour.Matin;
                                if (dtstart.Hour > 12) modetour = EnumTour.Soir;
                                course = new Course()
                                {
                                    Jour = dtstart.Date,
                                    Départ = dtstart.TimeOfDay,
                                    Sens = (byte)modetour,
                                };
                                rootLayout.FindViewById<TextView>(Resource.Id.sens_cell).Text = course.Sens.ToString();
                                rootLayout.FindViewById<TextView>(Resource.Id.depart_cell).Text = dtstart.TimeOfDay.ToString();

                                lastcoordonnées = coordonnées;
                            }
                            else 
                            {
                                deltatime = timecurent.Subtract(dtstart.TimeOfDay);
                                startandstop.Text = deltatime.ToStringFormat();
                                if (course != null)
                                {
                                    var co = Coordonnées_Data.GetCoordonnée(coordonnées);
                                    if (co.IsDomicile)
                                    {
                                        CloseCourse();
                                    }
                                    /*
                                    else if ((Type_Etape)co.Type == Type_Etape.Dépôt)
                                    {
                                        // Dialog dialog = Dialog.b
                                        var builder = new AlertDialog.Builder(this);
                                        builder.SetTitle("Etape Dépôt");
                                        builder.SetMessage("Fin de course ?");
                                        builder.SetPositiveButton("Oui", (sender, args) => { CloseCourse(); });
                                        builder.SetNegativeButton("Non", (sender, args) => { AjoutEtape(co); });
                                        builder.Show();
                                    }
                                    */
                                    else
                                    {
                                        AjoutEtape(co);
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
            while (!worker.CancellationPending & dtstop == new DateTime()) // delta.Ticks <= _max)
            {
                Thread.Sleep(500);
                delta = DateTime.Now - DtDepart;
                result = delta.Ticks;
                int percentComplete = (int)((float)delta.Ticks / (float)_max * 100);
                worker.ReportProgress(percentComplete, delta);
            }
            return result;
        }


         /// <summary>
        /// Deplacement en tache de fond
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundLocation_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            e.Result = Deplacement((int)e.Argument, worker, e);
        }


        /// <summary>
        /// Incrementation du déplacement
        /// </summary>
        /// <param name="n"></param>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private Location Deplacement(int n,BackgroundWorker worker, DoWorkEventArgs e)
        {
           // DateTime DtDepart = DateTime.Now;
            TimeSpan delta = new TimeSpan(0, 0, 0);
            Location result = new Location("");
            while (!worker.CancellationPending & dtstop == new DateTime() )
            {
                Thread.Sleep(500);
                if(dtstart != new DateTime())
                {
                    delta = DateTime.Now - dtstart;
                }
                int percentComplete = (int)((float)delta.Ticks / (float)_max * 100);
                worker.ReportProgress(percentComplete, delta);
            }
            return result;
        }

        private async void backgroundLocation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // TimeSpan delta = new TimeSpan(0, 0, 0);
            double hh = ((TimeSpan)e.UserState).Hours;
            double mm = ((TimeSpan)e.UserState).Minutes;

            TimeSpan ti = DateTime.Now.TimeOfDay;

            // textheure.Text = $"{ti.Hours}:{ti.Minutes}:{ti.Seconds}";

            //Boolean ok = false;
            BackgroundWorker ctl = (BackgroundWorker)sender;
            if (ctl.IsBusy)
            {
                textheure.Text = $"{ti.Hours}:{ti.Minutes}:{ti.Seconds}";

                if (lastcoordonnées != null)
                {
                    await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
                    // textMessage.Text = $"distance depuis {coordonnées.DistanceTo(lastcoordonnées).ToString()} ";
                    // if(coordonnées.DistanceTo(lastcoordonnées)>)
                    //var q1 = Math.Round((double)coordonnées.DistanceTo(lastcoordonnées), 3);

                    var q1 = (long)coordonnées.DistanceTo(lastcoordonnées);

                    textdistance.Text = q1.ToString();

                    //if (!coordonnées.Equals(lastcoordonnées))
                    //if(q1>0)
                    // {
                    var r = Coordonnées_Data.GetCoordonnée(coordonnées);
                    if (r != null)
                    {
                        textMessage.Text = $"etape {r.Nom}";
                        if (startcoordonnées == null)
                        {
                            if (r.IsDomicile)
                            {
                                startandstop.Text = r.Nom;
                                dtstart = DateTime.Now;
                                timestart = dtstart.TimeOfDay;
                                startandstop.Text = "00:00";
                                startcoordonnées = coordonnées;

                                InitialCourse(dtstart);
                                // lastcoordonnées = coordonnées;
                                //ok = true;
                            }
                            else
                            {

                            }
                        }
                        else
                        {

                            if (q1 > PRESITION_ETAPE)
                            {
                                if (r.IsEtape)
                                {
                                    AjoutEtape(r);
                                }
                                else if (r.IsDomicile)
                                {
                                    CloseCourse();
                                }
                            }
                            else
                            {
                                var beep = 1;
                            }
                        }
                        /*
                        }
                        else
                        {

                        }
                        */
                        /*
                        if (CtrlStartCoordonnéAsync())
                        {
                            InitialCourse(DateTime.Now);
                            textMessage.Text = "Start coordonnée ok";
                        }
                        else
                        {
                            // déplacement 
                        }
                        */
                    }
                    else
                    {
                        if (startcoordonnées != null)
                        {

                        }
                        // pause
                    }
                }
                if (dtstart != new DateTime())
                {
                    var q2 = ti - timestart;
                    startandstop.Text = $"{q2.Hours}:{q2.Minutes}:{q2.Seconds}";

                }
                lastcoordonnées = coordonnées;
            }
        }


        /// <summary>
        /// creation nouvelle course
        /// </summary>
        /// <param name="date"></param>
        private void InitialCourse(DateTime date)
        {
            EnumTour modetour = EnumTour.Matin;
            if (date.Hour > 12) modetour = EnumTour.Soir;
            course = new Course()
            {
                Jour = date.Date,
                Départ = date.TimeOfDay,
                Sens = (byte)modetour,
            };
            rootLayout.FindViewById<TextView>(Resource.Id.sens_cell).Text = course.Sens.ToString();
            rootLayout.FindViewById<TextView>(Resource.Id.depart_cell).Text = date.TimeOfDay.ToStringFormat();
        }

        private void AjoutEtape(Coordonnée co)
        {
            if (!course.Etapes.Exists(c => c.IdEtape == co.Id))
            {
                Etape etape = new Etape() { Heure = DateTime.Now.TimeOfDay, IdEtape = co.Id };
                course.Etapes.Add(etape);
                ListView_etapes.Adapter = new EtapeAdapter(this, course.Etapes);
            }
        }

        private void CloseCourse()
        {
           // course.Arrivée = time;
            dtstop = DateTime.Now;
            course.Arrivée = dtstop.TimeOfDay;

            FindViewById<TextView>(Resource.Id.arrivee_cell).Text = dtstop.TimeOfDay.ToStringFormat();

            // sauvegarde 

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
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

            backgroundLocation = new BackgroundWorker();
            backgroundLocation.DoWork += new DoWorkEventHandler(backgroundLocation_DoWork);
            backgroundLocation.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundLocation_RunWorkerCompleted);
            backgroundLocation.ProgressChanged += new ProgressChangedEventHandler(backgroundLocation_ProgressChanged);
            backgroundLocation.WorkerSupportsCancellation = true;
        }

        private void backgroundLocation_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            numberToCompute = 1;
            backgroundWorker1.RunWorkerAsync(numberToCompute);

            textMessage.Text = "Depart chrono tache de fond";
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
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    this.OnStop();
                    return true;
                case Resource.Id.navigation_dashboard:
                    var activity = new Intent(this, typeof(CoordonnéeActivity));
                    Bundle bundle = new Bundle();
                    bundle.PutByteArray("Data", Coordonnées_Data.ToByte());
                    activity.PutExtra("Data", bundle);
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
        /// 
/*
        public void LoadHistorique()
        {
            int lenght ;
            Course course = new Course();

            if (backingFile_historique == null || !File.Exists(backingFile_historique))
            {
                File.CreateText(backingFile_historique);
                textMessage.Text = "Creation du fichier " + backingFile_historique;
            }
            else
            {
                textMessage.Text = "Lecture du fichier " + backingFile_historique;
                using (var filestream = new FileStream(backingFile_historique, FileMode.Open))
                {
                    lenght = (int)filestream.Length;
                    byte[] buffer = new byte[lenght];
                    filestream.Read(buffer, 0, lenght);
                    Historique_Semaine = Courses.SetCourses(buffer);
                }
            }
        }
*/
/*
        public string EditHistorique()
        {
            bool first = true;
            string Texte = "";
            foreach (var item in Historique_Semaine)
            {
                Texte += item.ToEdit(first);
                first = false;
            }
            return Texte;
        }
*/
        /// <summary>
        /// ok
        /// Sauvegarde de l'historique
        /// </summary> 
        public void SaveHistorique()
        {
            if(backingFile_historique != "" & course != null)
            {
                if (!File.Exists(backingFile_historique))
                {
                    textMessage.Text = "Creation du fichier " + backingFile_historique;
                    using (var filestream = new FileStream(backingFile_historique, FileMode.Create))
                    {
                        filestream.Write(course.ToByte());
                    }
                }
                else
                {
                    textMessage.Text = "Ecriture du fichier " + backingFile_historique;
                    using (var filestream = new FileStream(backingFile_historique, FileMode.Append))
                    {
                        filestream.Write(course.ToByte());
                    }
                }
            }
        }

        /// <summary>
        /// Remise a zero des fichiers Dat
        /// </summary>
        public void ResetHistorique()
        {
            if (backingFile_historique != null)
            {
                if (File.Exists(backingFile_historique))
                {
                    File.Delete(backingFile_historique);
                }
            }
            if(backingfile_coordonnées != null)
            {
                if (File.Exists(backingfile_coordonnées))
                {
                    File.Delete(backingfile_coordonnées);
                }
            }
        }

        public void EnvoiHistorique()
        {
            var email = new Intent(Android.Content.Intent.ActionSend);
            email.PutExtra(Android.Content.Intent.ExtraEmail, new string[] { "p_penchenat@orange.fr" });
            email.PutExtra(Android.Content.Intent.ExtraSubject, "Historique");
            email.PutExtra(Android.Content.Intent.ExtraText, "Historique de la tournée");
            email.SetType("message/rfc822");
            email.PutExtra(Android.Content.Intent.ExtraStream, backingFile_historique);
            StartActivity(email);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    this.OnStop();
                    return true;
                case Resource.Id.navigation_dashboard:
                    var activity = new Intent(this, typeof(CoordonnéeActivity));
                    Bundle bundle = new Bundle();
                    bundle.PutByteArray("Data", Coordonnées_Data.ToByte());
                    activity.PutExtra("Data", bundle);
                    StartActivity(activity);
                    return true;
                case Resource.Id.navigation_notifications:
                    ResetHistorique();
                    return true;
            }
            return false;
            return true;
        }
    }
}

