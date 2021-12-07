using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chronos_2.Adapters
{
    public class Course_Adaptater : BaseAdapter<Course>
    {

        private readonly Activity _context;
        private readonly List<Course> _courses;

        public Course_Adaptater(Activity activity, List<Course> courses)
        {
            _context = activity;
            _courses = courses;
        }
        public override Course this[int position] 
        {
            get { return _courses[position]; }
        }

        public override int Count 
        {
            get { return _courses.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            View view = convertView;
            if (view == null)
            {
                view = _context.LayoutInflater.Inflate(Resource.Layout.Course_Cell, parent, false);
            }

            Course  Course = this[position];

            var keypair = Course.GetNameTour();

            view.FindViewById<TextView>(Resource.Id.date_cell).Text = Course.Jour.ToShortDateString();
            view.FindViewById<TextView>(Resource.Id.sens_cell).Text = Course.Sens.ToString();
            view.FindViewById<TextView>(Resource.Id.tour_cell).Text = keypair.Key;
            view.FindViewById<TextView>(Resource.Id.depart_cell).Text = $"{Course.Départ.Hours,2}:{Course.Départ.Minutes,2}";
            view.FindViewById<TextView>(Resource.Id.arrivee_cell).Text = $"{Course.Arrivée.Hours,2}:{Course.Arrivée.Minutes,2}";
            view.FindViewById<TextView>(Resource.Id.effectif_cell).Text = keypair.Value.ToString();

            return view;
        }
    }
}