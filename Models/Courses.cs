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

namespace Chronos_2
{
    public static class Courses
    {
        public static List<Course> courses { get; set; } = new List<Course>();

        private static int _offset = 0;

        public static byte[] ToByte()
        {
            List<byte> buffeur = new List<byte>();

            byte[] tempo;
            foreach (var item in courses)
            {
                tempo = item.ToByte();
                buffeur.AddRange(tempo);
            }

            return buffeur.ToArray();
        }

        public static List<Course> SetCourses(byte[] buffeur)
        {
            byte[] tempo;
            int lenght = BitConverter.ToInt32(buffeur, _offset);
            tempo = new byte[lenght];
            buffeur.CopyTo(tempo, _offset + 4);
            courses.Add(new Course(tempo));
            return courses;
        }
    }
}