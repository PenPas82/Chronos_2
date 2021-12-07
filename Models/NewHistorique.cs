using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chronos_2
{
    public class NewHistorique : List<NewCourse>
    {
        private const int LENGHT = 20;

        private static string NameFile { get; set; } = "";

        private NewCourse _course;

        public NewHistorique() { }

        public NewHistorique(string name)
        {
            NameFile = name;
            if (NameFile == "") return;

            if (!File.Exists(NameFile))
            {
                File.Create(NameFile);
            }
            else
            {
                int lenght = 0;
                byte[] bufferheader = new byte[4];

                byte[] buffer; // = new byte[0];
                int start = 0;
                using (var filestream = new FileStream(NameFile, FileMode.Open))
                {
                    while (start < filestream.Length)
                    {
                        filestream.Position = start;
                        filestream.Read(bufferheader, 0, 4);
                        lenght = BitConverter.ToInt32(bufferheader);
                        buffer = new byte[lenght];
                        filestream.Read(buffer, 0,lenght);
                        _course = new NewCourse(buffer);
                        this.Add(_course);
                        start += lenght+4;
                    }
                    filestream.Close();
                }
            }
        }

        public byte[] ToByte()
        {
            List<byte> buffeur = new List<byte>();
            foreach (var item in this)
            {
                buffeur.AddRange(item.ToByte());
            }

            return buffeur.ToArray();
        }

        public override string ToString()
        {
            string texte = "";
            foreach (var item in this)
            {
                texte = texte + item.ToEdit();
            }
            return texte;
        }
    }
}