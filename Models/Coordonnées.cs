using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Chronos_2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chronos_2
{
    public class Coordonnées : List<Coordonnée>
    {
        private const int LENGHT = 60;
        private const float PRESITION = 50;  //*** 
        private static string NameFile { get; set; } = "";
        private Coordonnée _coordonnée;

        //  private List<Tournée> tournées = new List<Tournée>();
        public Dictionary<int, string> tournées = new Dictionary<int, string>();
        private Dictionary<string, List<int>> KeyValues_Tournée = new Dictionary<string, List<int>>();
        private Dictionary<int, int> _Index_Buffer = new Dictionary<int, int>();
        /// <summary>
        /// 
        /// </summary>
        public Coordonnées()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Nom du fichier</param>
        public Coordonnées(string name)
        {
            NameFile = name;
            if (NameFile == "") return;

            if (File.Exists(NameFile))
            {
                byte[] buffer = new byte[LENGHT];
                int start = 0;
                using (var filestream = new FileStream(NameFile, FileMode.Open))
                {
                    while (start < filestream.Length)
                    {
                        buffer = new byte[LENGHT];
                        filestream.Position = start;
                        Location loc = new Location("");
                        filestream.Read(buffer, 0, LENGHT);
                        _coordonnée = new Coordonnée(buffer);
                        _coordonnée.OnValueChanged += _coordonnée_OnValueChanged;
                        this.Add(_coordonnée);
                        _Index_Buffer.Add(_coordonnée.Id, start);
                        start += LENGHT;
                    }
                    filestream.Close();
                }
            }

            if (this.Count > 0)
            {
                var q1 = this.GroupBy(x => x.Tour, (a, b) => new
                {
                    Key = a,
                    Value = b.Select(y => y.Id).ToList()
                });
                int k = 0;
                tournées.Clear();
                KeyValues_Tournée.Clear();
                foreach (var item in q1)
                {
                    tournées.Add(k,item.Key);
                    KeyValues_Tournée.Add(item.Key, item.Value);
                    k += 1;
                }
            }
        }

        private void _coordonnée_OnValueChanged(object sender, SampleEventArgs e)
        {
            Log.Info("_coordonnée_OnValueChanged", e.ToString());

           // throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffeur"></param>
        ///
        public Coordonnées(byte[] buffeur)
        {
            int start = 0;
            this.Clear();

            byte[] tempo = new byte[LENGHT];
            while (start < buffeur.Length)
            {
                this.Add(new Coordonnée(buffeur, start));
                start += LENGHT;
            }
        }

        public Dictionary<string, List<int>> GetTournée()
        {
            var q1 = this.GroupBy(x => x.Tour, (a, b) => new
            {
                Key = a,
                Value = b.Select(y => y.Id).ToList()
            });

            KeyValues_Tournée.Clear();
            foreach (var item in q1)
            {
                KeyValues_Tournée.Add(item.Key, item.Value);
            }
            return KeyValues_Tournée;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public int GetIdCoordonnée(Location location)
        {
            var q1 = this.Where(r => r.DistanceTo(location) < PRESITION);
            if (q1.Any())
            {
                return q1.First().Id;
            }
            else
            {
                return -1;
            }
        }

        public Coordonnée GetCoordonnée(Location location)
        {
            var q1 = this.Where(r => r.DistanceTo(location) < PRESITION);
            if (q1.Any())
            {
                return q1.First();
            }
            else
            {
                return null;
            }
        }

        public Coordonnée GetById(int id)
        {
            return  this.Find(x => x.Id == id);
        }

        public string GetNameTourById(int id)
        {
            string r = "";
            var q1 = this.Find(x => x.Id == id);
            if (q1 != null)
            {
                r = q1.Tour;
            }
            return r;
        }

        public int GetIdTourById(int id)
        {
            int r = -1;
            var q1 = this.Find(x => x.Id == id);
            if (q1 != null)
            {
                 r = KeyValues_Tournée.Keys.ToList().FindIndex(x => x == q1.Tour);
            }
            return r;
        }

        public int GetTypeCoordonnée(Location location)
        {
            var q1 = this.Where(r => r.DistanceTo(location) < PRESITION);
            if (q1.Any())
            {
                return q1.First().Id;
            }
            else
            {
                return -1;
            }
        }

        public int SetCoordonnée(Location location)
        {
            int id = this.Count();
            List<byte> buffeur = new List<byte>();

            _coordonnée = new Coordonnée()
            {
                Id = id,
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            this.Add(_coordonnée);
            using (var filestream = new FileStream(NameFile, FileMode.Append))
            {
                filestream.Write(_coordonnée.ToByte());
                filestream.Close();
            }
            return id;
        }
        
        public int SetCoordonnée(Coordonnée coordonnée)
        {
            int id = this.Count();
            List<byte> buffeur = new List<byte>();
            this.Add(coordonnée);

            if (File.Exists(NameFile))
            {
                using (var filestream = new FileStream(NameFile, FileMode.Append))
                {
                    filestream.Write(coordonnée.ToByte());
                    filestream.Close();
                }
            }
            else
            {
                using (var filestream = new FileStream(NameFile, FileMode.Create))
                {
                    filestream.Write(coordonnée.ToByte());
                    filestream.Close();
                }
            }
            return id;
        }

        // ajoute une nouvelle coordonnée si n'existe pas
        public int GetInsert(Location location)
        {
            int r = GetIdCoordonnée(location);

            if (r == -1)
            {
                r = SetCoordonnée(location);
            }
            return r;
        }
        
        public int GetInsert(Coordonnée coordonnée)
        {
            Location _location = new Location("");
            _location.Latitude = coordonnée.Latitude;
            _location.Longitude = coordonnée.Longitude;

            int r = GetIdCoordonnée(_location);
            if (r == -1)
            {
                coordonnée.Id = this.Count;
                r = SetCoordonnée(coordonnée);
            }
            return r;
        }

        public void Update(Coordonnée coordonnée)
        {
            var r = coordonnée.Id;
            if (_Index_Buffer.ContainsKey(r))
            {
                var p = _Index_Buffer[r];

                using (var filestream = new FileStream(NameFile, FileMode.Open))
                {
                    //filestream.Flush();
                    var b = coordonnée.ToByte();
                    filestream.Position = p;
                    filestream.Write(b, 0, b.Length);
                    filestream.Close();
                }
            }
        }

        public Byte[] ToByte()
        {
            List<byte> resultat = new List<byte>();
            foreach (var item in this)
            {
                resultat.AddRange(item.ToByte());
            }
            return resultat.ToArray();
        }
    }
}