﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Media;
using System.Net;
using System.Data.SQLite;
using System.IO.Compression;

//THIS CODE IS A MESS


namespace Pamya
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    static class CustomCommands
    {
        public static RoutedCommand EditCard = new RoutedCommand();
        public static RoutedCommand EditDeck = new RoutedCommand();
        public static RoutedCommand MarkAsEasy = new RoutedCommand();
        public static RoutedCommand MarkAsHard = new RoutedCommand();
        public static RoutedCommand ToggleReviewOnly = new RoutedCommand();
        public static RoutedCommand InsertCardAfter = new RoutedCommand();
    }

    static class EpochTime
    {
        public static DateTime GetCurrent()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
        public static int GetAsInt()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }
        public static DateTime AddToEpoch(int seconds)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(seconds);
        }
    }

    public partial class MainWindow : Window
    {
        public Deck mc;
        private Word currentWord;
        private string filename;
        private string appdatafolder;
        private string ideckfolder;
        private string ideckfile;
        private string iuserfile;

        private Boolean _review_only;
        public MainWindow()
        {
            mc = new Deck();
            currentWord = new Word("retard", "malfruiĝulo");

            //Console.WriteLine(sky.EditDistance("Saturday", "Sunday"));
            InitializeComponent();



            appdatafolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Pamya";

            make_app_data_folder();

            _review_only = false;
            
        }

        private void make_app_data_folder()
        {
            //userData = new FileStream(Application.UserAppDataPath + "\\appdata.txt", FileMode.OpenOrCreate);
            if (!Directory.Exists(appdatafolder))
            {
                Directory.CreateDirectory(appdatafolder);
            }
            if (!Directory.Exists(appdatafolder + @"\Decks"))
            {
                Directory.CreateDirectory(appdatafolder + @"\Decks");
            }

            if (!Directory.Exists(appdatafolder + @"\Links"))
            {
                Directory.CreateDirectory(appdatafolder + @"\Links");
            }
        }

        private void _ToggleReviewOnly(object sender, RoutedEventArgs e)
        {

            _review_only = !_review_only;
            UpdateStatusBar();
            ReviewOnlyMenuItem.IsChecked = _review_only;
        }


        private void _app_exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void _OpenDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = appdatafolder + @"\Links";
            if (openFileDialog.ShowDialog() == true)
            {
                var fname = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                this.Title = "Pamya - " + fname;
                ideckfolder = appdatafolder + @"\Decks\" + fname;
                ideckfile = ideckfolder + @"\deck.sqlite";
                iuserfile = ideckfolder + @"\userdata.sqlite";

                SQLiteConnection deckdbcon;
                deckdbcon =
                new SQLiteConnection("Data Source=" + ideckfile + ";Version=3;");
                deckdbcon.Open();


                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + iuserfile + ";Version=3;");
                userdbcon.Open();

                List<string> deckstuff = new List<string>();

                string sql = "select * from deck order by id asc";
                SQLiteCommand command = new SQLiteCommand(sql, deckdbcon);
                SQLiteDataReader deck_reader = command.ExecuteReader();
                
                mc = new Deck();

                
                while (deck_reader.Read())
                {
                    var word = new Word(deck_reader["question"].ToString(),deck_reader["answer"].ToString());
                    word.id = Convert.ToInt32(deck_reader["id"]);
                    word.wavfileloc = deck_reader["wavfileloc"].ToString();
                    word.guid = deck_reader["guid"].ToString();
                    word.example = deck_reader["example"].ToString();
                    var user_sql = "SELECT * FROM deck WHERE guid='" + deck_reader["guid"].ToString() + "'";
                    var user_command = new SQLiteCommand(user_sql, userdbcon);
                    var user_reader = user_command.ExecuteReader();
                    user_reader.Read();
                    word.EF = Convert.ToDouble(user_reader["ef"]);
                    word.I = Convert.ToDouble(user_reader["i"]);
                    word.n = Convert.ToInt32(user_reader["n"]);
                    word.studied = Convert.ToBoolean(user_reader["studied"]);
                    word.time_due = Convert.ToInt32(user_reader["timedue"]);
                    user_reader.Close();
                    mc.AddWord(word);
                }

                ShowDeck();

                deck_reader.Close();

                userdbcon.Close();
                deckdbcon.Close();
            }
            
                
        }

        //DANGEROUS DO NOT USE
        private void _import_dialog(object sender, RoutedEventArgs e)
        {
            //ето просто хуйня 
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string stuff = File.ReadAllText(openFileDialog.FileName);
                filename = openFileDialog.FileName;
                var importDeck = new Deck();
                importDeck.fillDeckFromString(stuff);
                //show_deck();

                var deckfolder = appdatafolder + @"\Decks\" + System.IO.Path.GetFileNameWithoutExtension(filename);
                var deckfile = deckfolder + @"\deck.sqlite";

                //var userfolder = appdatafolder + @"\UserData\" + System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                //var userfile = userfolder + @".sqlite";
                var userfile = deckfolder + @"\userdata.sqlite";
                //MessageBox.Show(deckfile);

                Directory.CreateDirectory(deckfolder);

                File.Create(appdatafolder + @"\Links\" + System.IO.Path.GetFileNameWithoutExtension(filename) + ".deck");

                SQLiteConnection.CreateFile(deckfile);
                SQLiteConnection deckdbcon;
                deckdbcon =
                new SQLiteConnection("Data Source="+deckfile+";Version=3;");
                deckdbcon.Open();
                string sql = "create table deck (guid text,question text, answer text, wavfileloc text)";
                SQLiteCommand command = new SQLiteCommand(sql, deckdbcon);
                command.ExecuteNonQuery();

                SQLiteConnection.CreateFile(userfile);
                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + userfile + ";Version=3;");
                userdbcon.Open();
                sql = "create table deck (guid text,EF text,I text, n text, studied text, timedue text)";
                command = new SQLiteCommand(sql, userdbcon);
                command.ExecuteNonQuery();


                int id = 0;

                foreach (Word w in importDeck.dc)
                {

                    id++;

                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = @"C:\Program Files (x86)\eSpeak\command_line\espeak.exe";

                    //if (!File.Exists(wavfile))
                    //{
                    var wavfilename = deckfolder + @"\" + w.answer.Replace("!","_").Replace("-", "_").Replace(" ", "_").Replace(".", "_").Replace(",", "_").Replace("ŝ", "sx").Replace("ĉ", "cx").Replace("ĝ", "gx").Replace("ĵ", "jx").Replace("ĥ", "hx").Replace("ŭ", "ux") + ".wav";
                    if (!File.Exists(wavfilename))
                    {

                        startInfo.Arguments = "-w \"" + wavfilename + "\" -v eo \"" + w.answer.Replace("!", "").Replace("-", "").Replace(".", "").Replace(",", "").Replace("ŝ", "sx").Replace("ĉ", "cx").Replace("ĝ", "gx").Replace("ĵ", "jx").Replace("ĥ", "hx").Replace("ŭ", "ux") + "\"";
                        //MessageBox.Show(wavfilename + " ~~~ " + startInfo.Arguments);
                        process.StartInfo = startInfo;
                        process.Start();
                    }

                    //}


                    sql = "insert into deck values('" + id + "','" + w.question.Replace("'", "''") + "','" + w.answer.Replace("'", "''") + "','" + wavfilename + "');";
                    //MessageBox.Show(sql);
                    command = new SQLiteCommand(sql, deckdbcon);
                    try {
                        command.ExecuteNonQuery();
                         } catch
                    {
                        //MessageBox.Show(ex.ToString());
                        MessageBox.Show(sql);
                    }

                    sql = "insert into deck values('" + id + "','" + w.EF.ToString() + "','" + w.I.ToString() + "','" + w.n.ToString() + "','" + w.studied.ToString() + "','" + w.time_due.ToString() + "');";
                    //MessageBox.Show(sql);
                    command = new SQLiteCommand(sql, userdbcon);
                    command.ExecuteNonQuery();
                }


                userdbcon.Close();
                deckdbcon.Close();
            }

        }

        private void _ImportFromZipDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;

                var deckfolder = appdatafolder + @"\Decks\" + System.IO.Path.GetFileNameWithoutExtension(filename);
                var deckfile = deckfolder + @"\deck.sqlite";

                var userfile = deckfolder + @"\userdata.sqlite";

                Directory.CreateDirectory(deckfolder);

                File.Create(appdatafolder + @"\Links\" + System.IO.Path.GetFileNameWithoutExtension(filename) + ".deck");

                //Extract the zip file
                ZipFile.ExtractToDirectory(filename, deckfolder);

                SQLiteConnection deckdbcon;
                deckdbcon =
                new SQLiteConnection("Data Source=" + deckfile + ";Version=3;");
                deckdbcon.Open();

                SQLiteConnection.CreateFile(userfile);
                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + userfile + ";Version=3;");
                userdbcon.Open();
                var sql = "create table deck (guid text,EF text,I text, n text, studied text, timedue text, id integer)";
                var command = new SQLiteCommand(sql, userdbcon);
                command.ExecuteNonQuery();

                sql = "select * from deck order by id asc";
                command = new SQLiteCommand(sql, deckdbcon);
                SQLiteDataReader deckreader = command.ExecuteReader();
                Word w = new Word("Test", "Test"); //Word for default values
                while (deckreader.Read())
                {
                    
                    sql = "insert into deck values('" + deckreader["id"] + "','" + w.EF.ToString() + "','" + w.I.ToString() + "','" + w.n.ToString() + "','" + w.studied.ToString() + "','" + w.time_due.ToString() + "','"+deckreader["guid"]+"');";
                    //MessageBox.Show(sql);
                    command = new SQLiteCommand(sql, userdbcon);
                    command.ExecuteNonQuery();
                }
                deckreader.Close();



                deckdbcon.Close();
                userdbcon.Close();
            }

        }


        private void ShowDeck()
        {
            currentWord = mc.GetNextWord(_review_only);
            questionBlock.Text = currentWord.question;
            UpdateStatusBar();
        }
        private void _key_press(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //do something if 'enter' key is pressed.
                //MessageBox.Show("Boom");
                if (TBox.IsEnabled == true)
                {
                    TBox.IsEnabled = false;
                    string ans = TBox.Text;
                    TBox.Text = currentWord.answer;
                    exampleBox.Text = currentWord.example;
                    if (currentWord.answered(ans))
                    {
                        TBox.Foreground = Brushes.Green;
                    } else
                    {
                        TBox.Foreground = Brushes.Red;
                    }



                    //ttsEO(currentWord.answer.Replace("ŝ", "sx").Replace("ĉ", "cx").Replace("ĝ", "gx").Replace("ĵ", "jx").Replace("ĥ", "hx").Replace("ŭ", "ux"));


                    if (File.Exists(currentWord.wavfileloc))
                    {
                        SoundPlayer my_wave_file = new SoundPlayer(currentWord.wavfileloc);
                        my_wave_file.Play();
                    }

                    //

                    UpdateStatusBar();

                    //update the database
                    UpdateUserDB(currentWord);

                }
                else
                {
                    TBox.Text = "";
                    TBox.IsEnabled = true;
                    TBox.Foreground = Brushes.Black;
                    TBox.Focus();
                    exampleBox.Text = "";
                    ShowDeck();
                }
            }
        }

        public void UpdateUserDB(Word w)
        {
            if (File.Exists(iuserfile))
            {
                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + iuserfile + ";Version=3;");
                userdbcon.Open();

                //first we have to check if the word even exists in the database
                var check_sql_query = "SELECT count(*) FROM deck WHERE guid='" + w.guid + "';";
                var check_sql_command = new SQLiteCommand(check_sql_query, userdbcon);
                int count = Convert.ToInt32(check_sql_command.ExecuteScalar());
                if (count == 0)
                {
                    var insert_sql_query = "INSERT INTO deck (id, EF, I, n, studied, timedue, guid) VALUES "
                        + "("+w.id+",'" + w.EF +"','" + w.I + "','" + w.n + "','" + w.studied + "','" + w.time_due + "','" + w.guid + "');";
                } 
                else
                {
                    string sql = "update deck set id=" + w.id + ", EF='" + w.EF + "', I='" + w.I + "', n='" + w.n +
                    "', studied='" + w.studied + "', timedue='" + w.time_due + "' where guid='" + w.guid + "';";

                    var command = new SQLiteCommand(sql, userdbcon);
                    try { command.ExecuteNonQuery(); }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                }
                
                userdbcon.Close();
            }
        }
        public void UpdateDeckDB(Word w)
        {
            if (File.Exists(iuserfile))
            {
                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + ideckfile + ";Version=3;");
                userdbcon.Open();
                string sql = "update deck set question='" + w.question.Replace("'", "''")
                    + "', answer='" + w.answer.Replace("'", "''")
                    + "', example='" + w.example.Replace("'", "''")
                    + "', id=" + w.id
                    + " where guid='" + w.guid + "';";
                //MessageBox.Show(sql);
                var command = new SQLiteCommand(sql, userdbcon);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                userdbcon.Close();
            }
        }

        public void ttsEO(string w)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Program Files (x86)\eSpeak\command_line\espeak.exe"; //FIXME
            startInfo.Arguments = "-v eo \""+w+"\"";
            process.StartInfo = startInfo;
            process.Start();

            /*var wavfile = @"C:\Users\apopov\Desktop\study\esperanto101\" + w + ".wav";
            if (! File.Exists(wavfile))
            {
                startInfo.Arguments = "-w " + wavfile + " -v eo \"" + w + "\"" ;
                process.StartInfo = startInfo;
                process.Start();
            }*/
        }
        public void UpdateStatusBar()
        {
            //need to cleanup the time usage, I copy paste code which is terrible
            //var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //var timeD = epoch.AddSeconds(currentWord.timeDue);
            var timeD = EpochTime.AddToEpoch(currentWord.time_due);
            lblStatus.Text = "EF: " + currentWord.EF.ToString() + "; n: " + currentWord.n.ToString() + "; Time Due Next: " + timeD.ToLocalTime().ToLongDateString() + " " + timeD.ToLocalTime().ToLongTimeString() + "; RevOnly: " + _review_only.ToString();
        }

        private void _EditCardDialog(object sender, RoutedEventArgs e)
        {
            var edit_window = new EditCardWindow(currentWord);
            edit_window.ShowDialog();
            questionBlock.Text = currentWord.question;
            exampleBox.Text = currentWord.example;
            if (!TBox.IsEnabled)
                TBox.Text = currentWord.answer;
            UpdateDeckDB(currentWord);
            UpdateStatusBar();
        }

        private void _EditDeckDialog(object sender, RoutedEventArgs e)
        {
            var edit_deck_window = new DeckView(mc);
            edit_deck_window.ShowDialog();

            //questionBlock.Text = currentWord.question;
            //UpdateDeckDB();
            //UpdateStatusBar();
        }

        private void _MarkAsEasy(object sender, RoutedEventArgs e)
        {
            currentWord.MarkAsEasy();
            UpdateUserDB(currentWord);
            UpdateStatusBar();
        }
        private void _MarkAsHard(object sender, RoutedEventArgs e)
        {
            currentWord.MarkAsHard();
            UpdateUserDB(currentWord);
            UpdateStatusBar();
        }
    }


    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }


}
