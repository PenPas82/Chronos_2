//#define TESTHISTORIQUE

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
using Chronos_2.Models;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using AlertDialog = Android.Support.V7.App.AlertDialog;

/********************************************************************************/
/* Version 4 du 07/12/2021 */
/**/
namespace Chronos_2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public partial class MainActivity : AppCompatActivity //,Toolbar.IOnMenuItemClickListener
    {
       // static readonly int NOTIFICATION_ID = 1000;
       // static readonly string CHANNEL_ID = "location_notification";
        internal static readonly string COUNT_KEY = "count";

        const long INTERVAL = 1;
        private long TIMEPAUSEETAPE = TimeSpan.FromSeconds(30).Ticks;
        const long FASTESTINTERVAL = INTERVAL * 500;
        const long LOCATIONINTERVAL = INTERVAL * 1000;
        private const float PRESITION_ETAPE = 10;

        private const string DISTANCE_KEY = "distanceparcourue";
        private const string DTSTART_KEY = "DtStart";
        private const string TIMESTART_KEY = "timestart";
        private const string REQUESTING_KEY = "isRequestingLocationUpdates";
        private const string TOURNEE_KEY = "course";
        private const string STARTLOCATION_KEY = "startlocation";
        private const string COURSESTART_KEY = "isCourseStart";
        private const string ALERTPOSITION_KEY = "isAlertPosition";
        private const string CTRLPOSITION_KEY = "isCtrlPosition";
        private const string ETAPELOCATION_KEY = "etapelocation";

        private const string NAMEFILE_COORDONNEES = "Coordonnées.Dat";
        private const string NAMEFILE_HISTORIQUE = "Historique__SEM_.Dat";
        private const string NAMEFILE_COURSE = "Course.Dat";

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
        internal Position etapelocation = new Position(); //
        internal Location startlocation = null;
        internal Location etapecoordonnées = null;
        internal Location coordonnées = null;
        internal Location lastcoordonnées = null;

        internal long distanceparcourue = 0;

        bool isGooglePlayServicesInstalled;
        bool isRequestingLocationUpdates = false;
        internal bool isCtrlPosition = false;                // si position déjà controlé
        internal bool isAlertPosition = false;               // si demande de nouvelle position
        internal bool isCourseStart = false;                 // Si lancement de la course

        // Variable date 
        private DateTime dtjour = new DateTime();
        //private DateTime dtstart = new DateTime();
        private DateTime dtstop = new DateTime();
        private TimeSpan timecurent = new TimeSpan();
        private TimeSpan timecourse = new TimeSpan();   // temps total de course
        private TimeSpan timepause = new TimeSpan();    // temps de pause;
        private TimeSpan timearret = new TimeSpan();     // temps d'arret
        private TimeSpan timestart = new TimeSpan();
        private Semaine semaine;

        //variable tache de fond
        private long _max = new TimeSpan(4, 0, 0).Ticks; // temps max de conduite
        private long _delta = new TimeSpan(0, 0, 0).Ticks;
        private int numberToCompute = 1;
        //private BackgroundWorker backgroundWorker1;
        private BackgroundWorker backgroundLocation;

        internal Button buttonetape;
        internal Button startandstop;
        internal ListView ListView_etapes;

        private string backingPath_historique;
        private string backingFile_historique;
        private string backingfile_coordonnées;
        private string backingfile_course;
      //  private string backingfile_test;
        private string namefilehistosem;

        public Course course = new Course();

        public List<Course> Historique_Semaine = new List<Course>();
        internal Coordonnées Coordonnées_Data = new Coordonnées();
        internal Coordonnée coordonnée_etape = new Coordonnée();
        internal Coordonnée last_coordonnée_etape = new Coordonnée();
        internal long distance = 0;
        internal long Pause;

        public int Nb_Coord_Etapes = 0;

        public List<Etape> Etapes = new List<Etape>();
        private List<string> Tournées = new List<string>();

        private Dialog_New_Coordonnée Dialog_New_Coordonnée = null;

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
            namefilehistosem = NAMEFILE_HISTORIQUE.Replace("_SEM_", semaine.NumSemaine.ToString());

            //
            backingPath_historique = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), NAMEFILE_HISTORIQUE);
            backingFile_historique = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), namefilehistosem);
            backingfile_coordonnées = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), NAMEFILE_COORDONNEES);
            backingfile_course = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), NAMEFILE_COURSE);

            CtrlVersion(backingFile_historique);
            CtrlVersion(backingfile_course);

            Dialog_New_Coordonnée = new Dialog_New_Coordonnée(this, backingfile_coordonnées);
            Coordonnées_Data = new Coordonnées(backingfile_coordonnées);

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
            startandstop.Background = GetDrawable(Resource.Color.colorStop);

            //UI Date du jour
            datejour = FindViewById<TextView>(Resource.Id.datejour);

            string strdt = dtjour.ToString("dd-MM-yyyy");
            datejour.Text = $"{dtjour.ToString("dddd, dd MMMM yyyy")} Sem:{semaine.NumSemaine.ToString()}";
            //var jour = dtjour.DayOfWeek;

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
        }

        private void Startandstop_Click(object sender, EventArgs e)
        {
            if (isCourseStart)
            {
                isAlertPosition = false;
                isCtrlPosition = false;
                isCourseStart = false;
                timestart = new TimeSpan();
                startandstop.Background = GetDrawable(Resource.Color.colorStop);
                ResetCourse();
                course = new Course();
            }
            else
            {
                startandstop.Background = GetDrawable(Resource.Color.colorRun);
                course = new Course();
                timecourse = new TimeSpan();
                isAlertPosition = false;
                isCtrlPosition = false;
                isCourseStart = true;
            }
        }

        private void Toolbar_MenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            int id = (int)e.Item.ItemId;
            Intent activity;
            Bundle bundle;
            switch (id)
            {
                case Resource.Id.navigation_home:
                    activity = new Intent(this, typeof(HistoriqueActivity));
                    activity.PutExtra("NameFile", backingPath_historique);
                    bundle = new Bundle();
                    bundle.PutInt("Semaine", semaine.NumSemaine);
                    bundle.PutInt("Année", semaine.NumAnnée);
                    activity.PutExtra("Data", bundle);
                    StartActivity(activity);
                    break;
                case Resource.Id.navigation_dashboard:
                    activity = new Intent(this, typeof(CoordonnéeActivity));
                    bundle = new Bundle();
                    bundle.PutByteArray("Data", Coordonnées_Data.ToByte());
                    activity.PutExtra("Data", bundle);
                    activity.PutExtra("NameFile", backingfile_coordonnées);
                    StartActivity(activity);
                    break;
            }
        }

        private void Toolbar_NavigationOnClick(object sender, EventArgs e)
        {
            if (course != null)
            {
                SaveHistorique();
            }
            this.Finish();
        }

        private void Buttonetape_Click(object sender, EventArgs e)
        {
            Log.Info("MainActivity", "Buttonetape_Click -> Appel Dialog_New_Coordonnée");
            Dialog_New_Coordonnée.ShowDialog(coordonnées);
        }

        /*************************************
         *     Navigation
         * 
        **************************************/
        #region navigation
        protected override void OnSaveInstanceState(Bundle outState)
        {
            Log.Info("MainActivity", "OnSaveInstanceState ");
            //outState.PutLong(DTSTART_KEY, dtstart.Ticks);
            outState.PutLong(TIMESTART_KEY, timestart.Ticks);

            outState.PutLong(DISTANCE_KEY, distanceparcourue);

            outState.PutBoolean(COURSESTART_KEY, isCourseStart);
            outState.PutBoolean(REQUESTING_KEY, isRequestingLocationUpdates);
            outState.PutBoolean(ALERTPOSITION_KEY, isAlertPosition);
            outState.PutBoolean(CTRLPOSITION_KEY, isCtrlPosition);

            if (!etapelocation.IsNullorZero())
            {
                outState.PutDoubleArray(ETAPELOCATION_KEY, new double[] { etapelocation.Latitude, etapelocation.Longitude });
            }
            if (startlocation != null)
            {
                outState.PutDoubleArray(STARTLOCATION_KEY, new double[] { startlocation.Latitude, startlocation.Longitude });
            }
            if (course != null)
            {
                outState.PutByteArray(TOURNEE_KEY, course.ToByte());
            }
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            Log.Info("MainActivity", $"OnRestoreInstanceState ");


            var n = savedInstanceState.GetLong(DTSTART_KEY);
            //dtstart = new DateTime(n);
            var t = savedInstanceState.GetLong(TIMESTART_KEY);
            timestart = new TimeSpan(t);

            isCourseStart = savedInstanceState.GetBoolean(COURSESTART_KEY);
            isRequestingLocationUpdates = savedInstanceState.GetBoolean(REQUESTING_KEY);
            isAlertPosition = savedInstanceState.GetBoolean(ALERTPOSITION_KEY);
            isCtrlPosition = savedInstanceState.GetBoolean(CTRLPOSITION_KEY);

            distanceparcourue = savedInstanceState.GetLong(DISTANCE_KEY);
            var d1 = savedInstanceState.GetDoubleArray(STARTLOCATION_KEY);
            startlocation.Longitude = d1[0];
            startlocation.Latitude = d1[1];

            var buffer = savedInstanceState.GetByteArray(TOURNEE_KEY);
           // Log.Info("MainActivity", $"OnRestoreInstanceState -> dtstart {dtstart}");
            course = new Course(buffer);
         
            base.OnRestoreInstanceState(savedInstanceState);

        }

        protected override void OnStart()
        {
            base.OnStart();
            Log.Info("MainActivity", $"On Start course IsNull {course.IsNull()}");

            if (isRequestingLocationUpdates == false)
            {
                if (isGooglePlayServicesInstalled)
                {
                    locationRequest = new LocationRequest()
                                      .SetPriority(LocationRequest.PriorityHighAccuracy)
                                      .SetInterval(LOCATIONINTERVAL)
                                      .SetFastestInterval(FASTESTINTERVAL);

                    locationCallback = new FusedLocationProviderCallback(this);
                    fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
                    InitialLocation();
                }
                else
                {
                    Snackbar.Make(rootLayout, Resource.String.missing_googleplayservices_terminating, Snackbar.LengthIndefinite)
                            .SetAction(Resource.String.ok, delegate { FinishAndRemoveTask(); })
                            .Show();
                }


                // mise a zero des variable de travail
                course = new Course();
                //dtstart = new DateTime();
                timestart = new TimeSpan();
                isCourseStart = false;
                isCtrlPosition = false;
                isCtrlPosition = false;
                lastcoordonnées = null;
                etapelocation = new Position();

#if TESTHISTORIQUE

                Initial();
#else
                if (backgroundLocation == null)
                {
                    InitializeBackgroundWorker();
                    backgroundLocation.RunWorkerAsync(numberToCompute);
                }
#endif
            }
        }

        /* derniére modif OnResume */
        /* controle coordonné de départ -> domicile */
        protected override void OnResume()
        {
            base.OnResume();
            bool ok = false;
            startandstop.Background = GetDrawable(Resource.Color.colorStop);

            try
            {
                if (Coordonnées_Data == null)
                {
                    Coordonnées_Data = new Coordonnées(backingfile_coordonnées);
                }
                if (course.IsNull())
                {
                    LoadCourse();
                    if (!course.IsNull())
                    {
                        if (course.Arrivée != new TimeSpan())
                        {
                            course = new Course();
                        }
                        else
                        {
                            ok = true;
                        }
                    }
                    else
                    {
                        CtrlDépartCourse();
                    }
                }
                else
                {
                    ok = true;
                }

                if (ok)
                {
                    startandstop.Background = GetDrawable(Resource.Color.colorRun);
                    int id = (int)course.IdDépart;
                    var co = Coordonnées_Data[id];
                    isCourseStart = true;
                    timestart = course.Départ;
                    startandstop.Text = "00:00";
                    startlocation = co.Location;
                    if (course.Etapes.Count > 0)
                    {
                        var k = course.Etapes.Last().IdEtape;
                        etapelocation = new Position(Coordonnées_Data[k].Location);
                    }
                    else
                    {
                        etapelocation = new Position(startlocation);
                    }
                    AfficheCourse(co);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Make(rootLayout, $"Erreur :{ex.Message}", Snackbar.LengthIndefinite)
                        .SetAction("ok", delegate { FinishAndRemoveTask(); })
                        .Show();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            SaveCourse();
            Log.Info("MainActivity", "On Pause");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
           /// ResetCourse();
            Log.Info("MainActivity", "On Destroy");
        }

#endregion

        /// <summary>
        /// Initialisation de la position du départ de la course
        /// (Domicile Etape 0)
        /// </summary>
        private async void InitialLocation()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                await StartRequestingLocationUpdates();
                isRequestingLocationUpdates = true;
            }
            else
            {
                RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
            }

            await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
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
        private Location Deplacement(int n, BackgroundWorker worker, DoWorkEventArgs e)
        {
            // DateTime DtDepart = DateTime.Now;
            TimeSpan delta = new TimeSpan(0, 0, 0);
            Location result = new Location("");
            while (!worker.CancellationPending & dtstop == new DateTime())
            {
                Thread.Sleep((int)FASTESTINTERVAL);
                _delta += FASTESTINTERVAL;
                int percentComplete = (int)((float)delta.Ticks / (float)_max * 100);
                worker.ReportProgress(percentComplete, delta);
            }
            return result;
        }

        private async void backgroundLocation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string ms = "";

            timecurent = DateTime.Now.TimeOfDay;
            BackgroundWorker ctl = (BackgroundWorker)sender;
            if (ctl.IsBusy)
            {
                textheure.Text = $"{timecurent.Hours}:{timecurent.Minutes}:{timecurent.Seconds}";

                if (lastcoordonnées != null)
                {
                    await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);
                    var q1 = (long)coordonnées.DistanceTo(lastcoordonnées);
                    textdistance.Text = distance.ToString();

                    if (distance == 0 & isCourseStart)    //Arret
                    {
                        ms = "Arret => ";
                        if (timearret == TimeSpan.Zero)
                        {
                            Log.Info("MainActivity.backgroundLocation_ProgressChanged", "Arret");
                            isAlertPosition = false;
                            isCtrlPosition = false;
                            timearret = DateTime.Now.TimeOfDay;
                            buttonetape.Enabled = true;
                            buttonetape.Background = GetDrawable(Resource.Color.colorRun);
                        }
                        else
                        {
                            timepause = DateTime.Now.TimeOfDay - timearret;
                        }
                        ms += $" {timepause.Minutes}:{timepause.Seconds}  ({timepause.Ticks})";

                        if (Coordonnées_Data.Count == 0)
                        {
                            textMessage.Text = "fichier coordonnée_etape vide !!";
                            Dialog_New_Coordonnée.ShowDialog(coordonnées);
                        }
                        else
                        {
                            if (etapelocation.IsNullorZero())
                            {
                                if (!isCtrlPosition)
                                {
                                    coordonnée_etape = Coordonnées_Data.GetCoordonnée(coordonnées);
                                    if (coordonnée_etape != null)
                                    {
                                        textMessage.Text = $"{coordonnée_etape.Nom} -> {coordonnée_etape.StrType}";
                                        CtrlEtape(coordonnée_etape);
                                        etapelocation.Set(coordonnées);
                                        buttonetape.Enabled = false;
                                        buttonetape.Background = GetDrawable(Resource.Color.colorStop);
                                    }
                                    isCtrlPosition = true;
                                }
                                else
                                {
                                    /* la position n'est pas repectoriée 
                                     */
                                    if (timepause.Ticks > TIMEPAUSEETAPE & !isAlertPosition)
                                    {
                                        isAlertPosition = true;
                                        isCtrlPosition = false;
                                        textMessage.Text = "nouvelle etape";
                                        Dialog_New_Coordonnée.ShowDialog(coordonnées);
                                    }
                                }
                            }
                        }
                        ms += $"Lat:{etapelocation.Latitude} Lon:{etapelocation.Longitude}";
                        textMessage.Text = ms;
                    }
                    else   // Déplacement
                    {
                        textMessage.Text = $"Déplacement {distance}";
                        if (timepause != TimeSpan.Zero)
                        {
                            Log.Info("MainActivity.backgroundLocation_ProgressChanged", "Déplacement");

                            /*
                             * enregister le temps de pause de l'etape ???
                             */
                            if(course != null)
                            {
                                if (course.Etapes.Count > 0)
                                {
                                    course.Etapes.Last().départ = timecurent;
                                }
                            }

                            Pause = 0;
                            etapelocation.Zero();
                            timepause = TimeSpan.Zero;
                            timearret = TimeSpan.Zero;
                            buttonetape.Enabled = false;
                            buttonetape.Background = GetDrawable(Resource.Color.colorStop);
                        }

                        if (distance > 50 & startlocation != null) // course en cour controle si 
                        {
                            coordonnée_etape = Coordonnées_Data.GetCoordonnée(coordonnées);
                            if (coordonnée_etape != null)
                            {
                                etapelocation.Set(coordonnées);
                                textMessage.Text = $"{coordonnée_etape.Nom} -> {coordonnée_etape.StrType}";
                                CtrlEtape(coordonnée_etape);
                            }
                        }
                    }

                    if (timestart != new TimeSpan())
                    {
                        timecourse = timecurent - timestart;
                        startandstop.Text = $"{timecourse.Hours}:{timecourse.Minutes}:{timecourse.Seconds}";
                    }
                }
                lastcoordonnées = coordonnées;
            }
        }

        private async void CtrlDépartCourse()
        {
            coordonnée_etape = null;

            await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, locationCallback);

            if (Coordonnées_Data.Count == 0)
            {
                textMessage.Text = "fichier coordonnée_etape vide !!";
                Dialog_New_Coordonnée.ShowDialog(coordonnées);
            }
            else
            {
                coordonnée_etape = Coordonnées_Data.GetCoordonnée(coordonnées);
                if (coordonnée_etape == null)
                {
                    textMessage.Text = "nouvelle etape";
                    Dialog_New_Coordonnée.ShowDialog(coordonnées);
                }
                else
                {
                    dtjour = DateTime.Now;
                    timecurent = dtjour.TimeOfDay;
                    timestart = timecurent;
                    CtrlEtape(coordonnée_etape);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="co"></param>
        private void CtrlEtape(Coordonnée co)
        {
            Log.Info("MainActivity.", $" CtrlEtape Input {co.Id}");
            switch ((Type_Etape)co.Type)
            {
                case Type_Etape.Null:
                    break;
                case Type_Etape.Dom_Chauffeur:
                    if (course.IsNull() )
                    {
                        timestart = timecurent;
                        startandstop.Text = "00:00";
                        startlocation = coordonnées;
                        InitialCourse(co);
                    }
                    else
                    {
                        if (!co.Equals(last_coordonnée_etape))
                        {
                            CloseCourse(co.Id,true);
                        }
                    }
                    break;
                case Type_Etape.Dom_Enfant:
                    if (course.Etapes.Count > 0)
                    {
                        var last = course.Etapes.Last();
                        var q1 = course.Etapes.Last().Info_Etape;
                        var s1 = course.Sens;
                        if (s1 == EnumSens.Aller & q1 == Type_Etape.Ets_Scolaire)
                        {
                            Coordonnée cx = Coordonnées_Data.GetById(last.IdEtape);
                            CloseCourse(last.IdEtape, false);                           // cloture de la course encour
                            InitialCourse(cx, s1);                                      // ouverture d'une nouvelle course
                        }
                    }
                    AjoutEtape(co);
                    break;
                case Type_Etape.Ets_Scolaire:

                    if (course.Etapes.Count > 0)
                    {
                        var q1 = course.Etapes.Last().Info_Etape;
                        var s1 = course.Sens;
                        if (s1 == EnumSens.Retour & q1 == Type_Etape.Dom_Enfant)
                        {
                            CloseCourse(co.Id, false);           // cloture de la course encour
                            InitialCourse(co, s1);            // ouverture d'une nouvelle course
                        }
                        else
                        {
                            AjoutEtape(co);
                        }
                    }
                    else
                    {
                        AjoutEtape(co);
                    }
                    break;
                case Type_Etape.Dépôt:
                    break;
                case Type_Etape.Parking:
                    break;
                default:
                    break;
            }
            last_coordonnée_etape = co;
        }

        /// <summary>
        /// creation nouvelle course
        /// </summary>
        /// <param name="date"></param>
        private void InitialCourse( Coordonnée co, EnumSens sens = EnumSens.Null)
        {
            course = new Course()
            {
                Jour = dtjour,
                Départ = timecurent,
                IdDépart = co.Id,
                Sens = sens
            };
            AfficheCourse(co);
        }

        private void AfficheCourse(Coordonnée co)
        {
            rootLayout.FindViewById<TextView>(Resource.Id.course_lieu_depart).Text = co.Nom.ToString();
            rootLayout.FindViewById<TextView>(Resource.Id.sens_cell).Text = course.Sens.ToString();
            rootLayout.FindViewById<TextView>(Resource.Id.depart_cell).Text = course.Départ.ToStringFormat();
            if(course.Etapes != null)
            {
                ListView_etapes.Adapter = new EtapeAdapter(this, course.Etapes);
            }
        }

        private void AjoutEtape(Coordonnée co)
        {
           // Etape LastEtape = null;
           // int indexetape = 0;

            if (co != null & course != null)
            {
                if (!course.Etapes.Exists(c => c.IdEtape == co.Id))
                {
                    var idtour = Coordonnées_Data.GetIdTourById(co.Id);
                    /*
                    indexetape = course.Etapes.Count - 1;
                    if (indexetape > -1)
                    {
                        LastEtape  = course.Etapes.Last();
                        LastEtape.départ = LastEtape.Heure + timepause;
                        course.Etapes[indexetape] = LastEtape;
                    }
                    */
                    Log.Info("MainActivity.", $" AjoutEtape Input {co.Id}");
                    Etape etape = new Etape() {
                        Heure = timecurent,
                        IdEtape = co.Id,
                        IdTour = idtour,
                        départ = new TimeSpan(),
                        Info_Etape = (Type_Etape)co.Type,    //// ???? --> modif Coordonné
                        NameTour = co.Tour
                    };
                    course.Etapes.Add(etape);

                    if (course.Etapes.Count == 1 & course.Sens == EnumSens.Null)
                    {
                        if((Type_Etape)co.Type == Type_Etape.Dom_Enfant)
                        {
                            course.Sens = EnumSens.Aller;
                        }
                        else if ((Type_Etape)co.Type == Type_Etape.Ets_Scolaire)
                        {
                            course.Sens = EnumSens.Retour;
                        }
                        rootLayout.FindViewById<TextView>(Resource.Id.sens_cell).Text = course.Sens.ToString();
                    }
                    ListView_etapes.Adapter = new EtapeAdapter(this, course.Etapes);
                }
            }
        }
/*
        private void CloseCourse(Coordonnée co, bool stop = false)
        {
            Log.Info("MainActivity", "CloseCourse");
            course.IdArrivée = co.Id;
            course.Arrivée = timecurent;
            SaveCourse();
            SaveHistorique();
            if (stop)
            {
                isCourseStart = false;
                dtstop = DateTime.Now;
                timestart = new TimeSpan();
                //EnvoiCourse();
                ResetCourse();
            }
            course = new Course();
        }
*/
        private void CloseCourse(int idetape, bool stop = false)
        {
            Log.Info("MainActivity", $"Cloture de la Course {idetape} a {timecurent}");
            course.IdArrivée = idetape;
            course.Arrivée = timecurent;
            SaveCourse();
            SaveHistorique();
            if (stop)
            {
                isCourseStart = false;
                dtstop = DateTime.Now;
                timecourse = new TimeSpan();
                //EnvoiCourse();
                ResetCourse();
            }
            course = new Course();
        }

        /// <summary>
        /// Inititialisation des taches de travail de fond 
        /// </summary>
        private void InitializeBackgroundWorker()
        {
            Log.Info("MainActivity", "InitializeBackgroundWorker");
            backgroundLocation = new BackgroundWorker();
            backgroundLocation.DoWork += new DoWorkEventHandler(backgroundLocation_DoWork);
            backgroundLocation.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundLocation_RunWorkerCompleted);
            backgroundLocation.ProgressChanged += new ProgressChangedEventHandler(backgroundLocation_ProgressChanged);
            backgroundLocation.WorkerSupportsCancellation = true;
        }

        private void backgroundLocation_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.Info("MainActivity", $"backgroundLocation_RunWorkerCompleted -> {dtstop} ");
            numberToCompute = 1;
            textMessage.Text = "fin de tache de fond Location";
           // SaveHistorique();
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

        async Task RemoveRequestingLocationUpdates()
        {
            await fusedLocationProviderClient.RemoveLocationUpdatesAsync(locationCallback);
        }

        /// <summary>
        /// ok
        /// Sauvegarde de l'historique
        /// </summary> 
        public void SaveHistorique()
        {
            if (backingFile_historique != "" & course != null)
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

        public void SaveCourse()
        {
            Log.Info("MainActivity.SaveCourse", "Ecriture du Fichier " + backingfile_course);

            if (backingfile_course != "" & course != null)
            {
                using (var filestream = new FileStream(backingfile_course, FileMode.Create))
                {
                    filestream.Write(course.ToByte());
                }
            }
        }

        
        
        public void LoadCourse()
        {
            Log.Info("MainActivity.LoadCourse", "lecture du fichier " + backingfile_course);
            int lenght = 0;
            byte[] bufferheader = new byte[4];
            byte[] buffer; // = new byte[0];

            if (backingfile_course != "")
            {
                if (File.Exists(backingfile_course))
                {

                    using (var filestream = new FileStream(backingfile_course, FileMode.Open))
                    {
                        
                        filestream.Read(bufferheader, 0, 4);
                        lenght = BitConverter.ToInt32(bufferheader);
                        buffer = new byte[lenght];
                        filestream.Read(buffer, 0, lenght);

                        course = new Course(buffer);
                        dtjour = course.Jour;
                        //dtstart = course.Départ;
                    }
                }
            }
        }
    
        /// <summary>
        /// Remise a zero des fichiers Dat
        /// </summary>
        public void ResetCourse()
        {
            Log.Info("MainActivity", "ResetCourse");

            if (backingfile_course != null)
            {
                if (File.Exists(backingfile_course))
                {
                    File.Delete(backingfile_course);
                }
            }
        }

        public void ResetFile(string namefile)
        {
            if (namefile != null)
            {
                if (File.Exists(namefile))
                {
                    File.Delete(namefile);
                }
            }
        }

        private void CtrlVersion(string namefile)
        {
            if (namefile != "")
            {
                if (File.Exists(namefile))
                {
                    var info = new FileInfo(namefile);
                    if (info.LastWriteTime < new DateTime(2021, 11, 1))
                    {
                        File.Delete(backingfile_course);
                    }
                }
            }
        }

#if TESTHISTORIQUE

        private void Initial()
        {

            List<Coordonnée> matin = new List<Coordonnée>()
            {
                Coordonnées_Data.GetById(0),    // domicile
                Coordonnées_Data.GetById(8),    //théo
                Coordonnées_Data.GetById(1),    //paul
                Coordonnées_Data.GetById(9),    //colége
                Coordonnées_Data.GetById(6),    //ECOLE
                Coordonnées_Data.GetById(0),    //domicile
            };

            List<Coordonnée> Aller1 = new List<Coordonnée>()
            {
                Coordonnées_Data.GetById(0),    // domicile
                Coordonnées_Data.GetById(8),    //théo
                Coordonnées_Data.GetById(9),    //colége
                Coordonnées_Data.GetById(1),    //paul
                Coordonnées_Data.GetById(6),    //ECOLE
                Coordonnées_Data.GetById(0),    //domicile
            };


            List<Coordonnée> collection = new List<Coordonnée>()
            {
                Coordonnées_Data.GetById(0),    //domicile
                Coordonnées_Data.GetById(6),    //ECOLE
                Coordonnées_Data.GetById(1),    //paul
                Coordonnées_Data.GetById(9),    //colége
                Coordonnées_Data.GetById(8),    //théo
                Coordonnées_Data.GetById(0),    //domicile
            };

            List<Coordonnée> Retour1 = new List<Coordonnée>()
            {
                Coordonnées_Data.GetById(0),    //domicile
                Coordonnées_Data.GetById(6),    //ECOLE
                Coordonnées_Data.GetById(9),    //colége
                Coordonnées_Data.GetById(1),    //paul
                Coordonnées_Data.GetById(8),    //théo
                Coordonnées_Data.GetById(0),    //domicile
            };

            ResetFile(backingfile_course);
            ResetFile(backingFile_historique);

            timepause = new TimeSpan(0, 5, 0);
            dtjour = new DateTime(2021, 11, 30);
            timecurent = new TimeSpan(6, 30, 00);
            foreach (var item in Aller1)
            {
                CtrlEtape(item);
                if (course.Etapes.Count > 0)
                {
                    course.Etapes.Last().départ = timecurent + timepause;
                }
                timecurent += new TimeSpan(0, 15, 0);
            }
            /*
            timecurent = new TimeSpan(15, 45, 00);
            foreach (var item in Retour1)
            {
                CtrlEtape(item);
                if (course.Etapes.Count > 0)
                {
                    course.Etapes.Last().départ = timecurent + timepause;
                }

                timecurent += new TimeSpan(0, 15, 0);

            }
            */
        }
#endif

        public void EnvoiCourse()
        {
            string BodyMessage = course.ToFiche();

            var email = new Intent(Android.Content.Intent.ActionSend);
            email.PutExtra(Android.Content.Intent.ExtraEmail, new string[] { "p_penchenat@orange.fr" });
            email.PutExtra(Android.Content.Intent.ExtraSubject, "Course");
            email.PutExtra(Android.Content.Intent.ExtraText, BodyMessage);
            email.SetType("message/rfc822");
            StartActivity(email);
        }
    }
}

