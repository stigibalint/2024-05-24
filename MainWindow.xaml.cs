using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;
using System.IO;

namespace sqlwpf
{
    public partial class MainWindow : Window
    {
        private List<Dal> dalok;
        private string connectionString = "Data Source=eurovizio.db";

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists("eurovizio.db"))
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    var commandText = File.ReadAllText("adatbazis.sql");
                    using (var cmd = new SqliteCommand(commandText, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void btnBetolt_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            dalok = new List<Dal>();

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, ev, eloado, cim, helyezes, pontszam FROM dal";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dalok.Add(new Dal
                            {
                                Id = reader.GetInt32(0),
                                Ev = reader.GetInt32(1),
                                Eloado = reader.GetString(2),
                                Cim = reader.GetString(3),
                                Helyezes = reader.GetInt32(4),
                                Pontszam = reader.GetInt32(5),
                                Orszag = reader.GetString(6)
                            });
                        }
                    }
                }
            }

            dataGrid.ItemsSource = dalok;
            if (dalok.Count > 0)
            {
                dataGrid.SelectedIndex = 0;
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem is Dal selectedDal)
            {
                lblSzervezoOrszag.Content = $"Szervező ország: {GetSzervezoOrszag(selectedDal.Ev)}";
            }
        }

        private string GetSzervezoOrszag(int ev)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT orszag FROM verseny WHERE ev = @ev";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ev", ev);
                    return (string)cmd.ExecuteScalar();
                }
            }
        }

        private void btnVersenyDatum_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is Dal selectedDal)
            {
                MessageBox.Show($"Verseny dátuma: {GetVersenyDatum(selectedDal.Ev)}", "Verseny Dátum");
            }
        }

        private DateTime GetVersenyDatum(int ev)
        {
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT datum FROM verseny WHERE ev = @ev";
                using (var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ev", ev);
                    return (DateTime)cmd.ExecuteScalar();
                }
            }
        }

        private void btnMagyarVersenyzok_Click(object sender, RoutedEventArgs e)
        {
            int magyarVersenyzok = dalok.Count(d => d.Orszag == "Magyarország");
            lblMagyarVersenyzok.Content = $"Magyar versenyzők: {magyarVersenyzok}";
        }

        private void btnTorles_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem is Dal selectedDal)
            {
                using (var conn = new SqliteConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM dal WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedDal.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadData();
            }
        }


   
    }

    public class Dal
    {
        public int Id { get; set; }
        public int Ev { get; set; }
        public string Orszag { get; set; }
        public string Eloado { get; set; }
        public string Cim { get; set; }
        public int Helyezes { get; set; }
        public int Pontszam { get; set; }
    }
}
