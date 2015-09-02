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
using System.Xml.Linq;
using System.Reflection;
using System.Globalization;

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
        public static RoutedCommand DeleteCard = new RoutedCommand();
        public static RoutedCommand ImportFromZipDialog = new RoutedCommand();
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

    public sealed class PamyaDeck
    {
        private static readonly Lazy<PamyaDeck> lazy =
            new Lazy<PamyaDeck>(() => new PamyaDeck());

        public static PamyaDeck Instance { get { return lazy.Value; } }

        private PamyaDeck()
        {

        }

        private string current_deck_folder;
        public string CurrentDeckFolder
        {
            get
            {
                return current_deck_folder;
            }
            set
            {
                current_deck_folder = value;
            }
        }
        private string decks_folder;
        public string DecksFolder
        {
            get
            {
                return decks_folder;
            }
            set
            {
                decks_folder = value;
            }
        }
    }

    public partial class MainWindow : Window
    {
        public Deck current_deck;
        private Word current_word;
        private string filename;
        private string app_data_folder;
        private string ideckfolder;
        private string ideckfile;
        private string iuserfile;


        //private string espeak_binary_location;

        private Boolean _review_only;
        public MainWindow()
        {
            current_deck = new Deck();
            current_word = new Word("retard", "malfruiĝulo");

            app_data_folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Pamya";

            MakeAppDataFolder();

            PamyaDeck.Instance.DecksFolder = app_data_folder + @"\Decks";

            var settings_file_path = app_data_folder + @"\settings.xml";
            if (!File.Exists(settings_file_path))
            {
                Stream resource = GetType().Assembly.GetManifestResourceStream("Pamya.Resources.settings.xml");
                File.WriteAllText(settings_file_path, (new StreamReader(resource)).ReadToEnd());
            }
            PamyaSettings.Instance.SetSettingsFile(settings_file_path);
            PamyaSettings.Instance.GetSettings();

            ResourceDictionary dict = new ResourceDictionary();
            var lang = PamyaSettings.Instance.GetSetting("lang");
            switch(lang)
            {
                case "en":
                    dict.Source = new Uri("/Resources/Localise.xaml", UriKind.Relative);
                    break;
                case "ru":
                    dict.Source = new Uri("/Resources/Localise.ru-RU.xaml", UriKind.Relative);
                    break;
                case "eo":
                    dict.Source = new Uri("/Resources/Localise.eo-EO.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("/Resources/Localise.xaml", UriKind.Relative);
                    break;
            }
            
            this.Resources.MergedDictionaries.Add(dict);

            InitializeComponent();

            _review_only = false;
            
        }

        private void MakeAppDataFolder()
        {
            //userData = new FileStream(Application.UserAppDataPath + "\\appdata.txt", FileMode.OpenOrCreate);
            if (!Directory.Exists(app_data_folder))
            {
                Directory.CreateDirectory(app_data_folder);
            }
            if (!Directory.Exists(app_data_folder + @"\Decks"))
            {
                Directory.CreateDirectory(app_data_folder + @"\Decks");
            }

            if (!Directory.Exists(app_data_folder + @"\Links"))
            {
                Directory.CreateDirectory(app_data_folder + @"\Links");
            }
        }

        private void _ToggleReviewOnly(object sender, RoutedEventArgs e)
        {

            _review_only = !_review_only;
            UpdateStatusBar();
            ReviewOnlyMenuItem.IsChecked = _review_only;
        }


        private void _AppExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void _OpenDialog(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog open_file_dialog = new OpenFileDialog();
            var open_file_dialog = new OpenDeckWindow();
            //open_file_dialog.InitialDirectory = app_data_folder + @"\Links";
            if (open_file_dialog.ShowDialog() == true)
            {
                //var fname = System.IO.Path.GetFileNameWithoutExtension(open_file_dialog.FileName);
                var fname = open_file_dialog.FileName;
                this.Title = "Pamya - " + fname;
                ideckfolder = app_data_folder + @"\Decks\" + fname;
                PamyaDeck.Instance.CurrentDeckFolder = ideckfolder;
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

                string sql = "SELECT * FROM deck ORDER BY id ASC";
                SQLiteCommand command = new SQLiteCommand(sql, deckdbcon);
                SQLiteDataReader deck_reader = command.ExecuteReader();
                
                current_deck = new Deck();

                
                while (deck_reader.Read())
                {
                    var word = new Word(deck_reader["question"].ToString(),deck_reader["answer"].ToString());
                    word.id = Convert.ToInt32(deck_reader["id"]);
                    word.wav_file_loc = deck_reader["wavfileloc"].ToString();
                    word.guid = deck_reader["guid"].ToString();
                    word.example = deck_reader["example"].ToString();
                    word.image_file_location = deck_reader["imagefileloc"].ToString();
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
                    current_deck.AddWord(word);
                }

                ShowDeck();

                deck_reader.Close();

                userdbcon.Close();
                deckdbcon.Close();
            }
            
                
        }

        //DANGEROUS DO NOT USE
        private void _ImportDialog(object sender, RoutedEventArgs e)
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

                var deckfolder = app_data_folder + @"\Decks\" + System.IO.Path.GetFileNameWithoutExtension(filename);
                var deckfile = deckfolder + @"\deck.sqlite";

                //var userfolder = appdatafolder + @"\UserData\" + System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                //var userfile = userfolder + @".sqlite";
                var userfile = deckfolder + @"\userdata.sqlite";
                //MessageBox.Show(deckfile);

                Directory.CreateDirectory(deckfolder);

                File.Create(app_data_folder + @"\Links\" + System.IO.Path.GetFileNameWithoutExtension(filename) + ".deck");

                SQLiteConnection.CreateFile(deckfile);
                SQLiteConnection deckdbcon;
                deckdbcon =
                new SQLiteConnection("Data Source="+deckfile+";Version=3;");
                deckdbcon.Open();
                string sql = "create table deck (guid text,question text, answer text, wavfileloc text, id integer, example text, imagefileloc text)";
                SQLiteCommand command = new SQLiteCommand(sql, deckdbcon);
                command.ExecuteNonQuery();

                SQLiteConnection.CreateFile(userfile);
                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + userfile + ";Version=3;");
                userdbcon.Open();
                sql = "create table deck (guid text,EF text,I text, n text, studied text, timedue text, id integer)";
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
                    //if (!File.Exists(wavfilename))
                   // {

                   //     startInfo.Arguments = "-w \"" + wavfilename + "\" -v eo \"" + w.answer.Replace("!", "").Replace("-", "").Replace(".", "").Replace(",", "").Replace("ŝ", "sx").Replace("ĉ", "cx").Replace("ĝ", "gx").Replace("ĵ", "jx").Replace("ĥ", "hx").Replace("ŭ", "ux") + "\"";
                        //MessageBox.Show(wavfilename + " ~~~ " + startInfo.Arguments);
                   //     process.StartInfo = startInfo;
                    //    process.Start();
                   // }

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

                var deckfolder = app_data_folder + @"\Decks\" + System.IO.Path.GetFileNameWithoutExtension(filename);
                var deckfile = deckfolder + @"\deck.sqlite";

                var userfile = deckfolder + @"\userdata.sqlite";

                Directory.CreateDirectory(deckfolder);

                File.Create(app_data_folder + @"\Links\" + System.IO.Path.GetFileNameWithoutExtension(filename) + ".deck");

                //Extract the zip file
                ZipFile.ExtractToDirectory(filename, deckfolder);

                SQLiteConnection deckdbcon;
                deckdbcon =
                new SQLiteConnection("Data Source=" + deckfile + ";Version=3;");
                deckdbcon.Open();

                SQLiteConnection.CreateFile(userfile);
                SQLiteConnection userdbcon = new SQLiteConnection("Data Source=" + userfile + ";Version=3;");
                userdbcon.Open();
                var sql = "CREATE TABLE deck (guid text,EF text,I text, n text, studied text, timedue text, id integer)";
                var command = new SQLiteCommand(sql, userdbcon);
                command.ExecuteNonQuery();

                sql = "SELECT * FROM deck ORDER BY id ASC";
                command = new SQLiteCommand(sql, deckdbcon);
                SQLiteDataReader deckreader = command.ExecuteReader();
                Word w = new Word("Test", "Test"); //Word for default values
                while (deckreader.Read())
                {
                    
                    sql = "insert into deck values('" + deckreader["guid"] + "','" + w.EF.ToString() + "','" + w.I.ToString() + "','" + w.n.ToString() + "','" + w.studied.ToString() + "','" + w.time_due.ToString() + "',"+deckreader["id"]+");";
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
            current_word = current_deck.GetNextWord(_review_only);
            questionBlock.Text = current_word.question;
            UpdateStatusBar();
        }


        //I have to fix this next, maybe with some states or somethiong, b/c this is also a mess at the moment
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
                    TBox.Text = current_word.answer;
                    exampleBox.Text = current_word.example;
                    if (current_word.answered(ans))
                    {
                        TBox.Foreground = Brushes.Green;
                    } else
                    {
                        TBox.Foreground = Brushes.Red;
                    }

                    var img_file = PamyaDeck.Instance.CurrentDeckFolder + @"\" + current_word.image_file_location;
                    if (File.Exists(img_file))
                    {
                        image.Source = new BitmapImage(new Uri(img_file));
                        image.Visibility = Visibility.Visible;
                    }

                    SpeechPlayer.SpeakWord(current_word);

                    UpdateStatusBar();

                    //update the database
                    UpdateUserDB(current_word);

                }
                else
                {
                    TBox.Text = "";
                    TBox.IsEnabled = true;
                    TBox.Foreground = Brushes.Black;
                    TBox.Focus();
                    exampleBox.Text = "";
                    image.Visibility = Visibility.Hidden;
                    ShowDeck();
                }
            }
        }
        public void UpdateUserDB(Word w)
        {
            UpdateUserDB(new List<Word>() { w });
        }

        public void UpdateUserDB(List<Word> ws)
        {
            if (File.Exists(iuserfile))
            {
                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + iuserfile + ";Version=3;");
                userdbcon.Open();
                using (var usertrans = userdbcon.BeginTransaction())
                {
                    foreach (Word w in ws)
                    {
                        //first we have to check if the word even exists in the database
                        var check_sql_query = "SELECT count(*) FROM deck WHERE guid='" + w.guid + "';";
                        var check_sql_command = new SQLiteCommand(check_sql_query, userdbcon);
                        int count = Convert.ToInt32(check_sql_command.ExecuteScalar());
                        if (count == 0)
                        {
                            var insert_sql_query = "INSERT INTO deck (id, EF, I, n, studied, timedue, guid) VALUES "
                                + "(" + w.id + ",'" + w.EF + "','" + w.I + "','" + w.n + "','" + w.studied + "','" + w.time_due + "','" + w.guid + "');";
                            var insert_sql_command = new SQLiteCommand(insert_sql_query, userdbcon);
                            try { insert_sql_command.ExecuteNonQuery(); }
                            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                        }
                        else
                        {
                            string sql = "UPDATE deck SET id=" + w.id + ", EF='" + w.EF + "', I='" + w.I + "', n='" + w.n +
                            "', studied='" + w.studied + "', timedue='" + w.time_due + "' WHERE guid='" + w.guid + "';";

                            var command = new SQLiteCommand(sql, userdbcon);
                            try { command.ExecuteNonQuery(); }
                            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                        }
                    }
                    usertrans.Commit();
                }
                
                userdbcon.Close();
            }
        }
        public void UpdateDeckDB(Word w)
        {
            UpdateDeckDB(new List<Word>() { w });
        }
        public void UpdateDeckDB(List<Word> ws)
        {
            if (File.Exists(ideckfile))
            {
                SQLiteConnection deckdbcon;
                deckdbcon =
                new SQLiteConnection("Data Source=" + ideckfile + ";Version=3;");
                deckdbcon.Open();
                //first we have to check if the word even exists in the database
                using (var decktrans = deckdbcon.BeginTransaction())
                {
                    foreach (Word w in ws)
                    {
                        var check_sql_query = "SELECT count(*) FROM deck WHERE guid='" + w.guid + "';";
                        var check_sql_command = new SQLiteCommand(check_sql_query, deckdbcon);
                        int count = Convert.ToInt32(check_sql_command.ExecuteScalar());
                        if (count == 0)
                        {
                            var insert_sql_query = "INSERT INTO deck (id, question, answer, example, wavfileloc, guid, imagefileloc) VALUES "
                                + "(" + w.id
                                + ",'" + w.question.Replace("'", "''")
                                + "','" + w.answer.Replace("'", "''")
                                + "','" + w.example.Replace("'", "''")
                                + "','" + w.wav_file_loc.Replace("'", "''")
                                + "','" + w.guid
                                + "','" + w.image_file_location.Replace("'", "''") + "');";
                            var insert_sql_command = new SQLiteCommand(insert_sql_query, deckdbcon);
                            try { insert_sql_command.ExecuteNonQuery(); }
                            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                        }
                        else
                        {
                            string sql = "UPDATE deck SET question='" + w.question.Replace("'", "''")
                                + "', answer='" + w.answer.Replace("'", "''")
                                + "', example='" + w.example.Replace("'", "''")
                                + "', wavfileloc='" + w.wav_file_loc.Replace("'", "''")
                                + "', id=" + w.id
                                + ", imagefileloc='" + w.image_file_location.Replace("'", "''")
                                + "' WHERE guid='" + w.guid + "';";
                            //MessageBox.Show(sql);
                            var command = new SQLiteCommand(sql, deckdbcon);
                            try { command.ExecuteNonQuery(); }
                            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                        }
                    }
                    decktrans.Commit();
                }
                deckdbcon.Close();
            }
        }


        public void UpdateStatusBar()
        {
            var time_due = EpochTime.AddToEpoch(current_word.time_due);
            lblStatus.Text = "EF: " + current_word.EF.ToString() + "; n: " + current_word.n.ToString() + "; Time Due Next: " + time_due.ToLocalTime().ToLongDateString() + " " + time_due.ToLocalTime().ToLongTimeString() + "; RevOnly: " + _review_only.ToString();
        }

        private void _EditCardDialog(object sender, RoutedEventArgs e)
        {
            var edit_window = new EditCardWindow(current_word);
            edit_window.ShowDialog();
            questionBlock.Text = current_word.question;

            if (!TBox.IsEnabled)
            {
                TBox.Text = current_word.answer;
                exampleBox.Text = current_word.example;
            }
            UpdateDeckDB(current_word);
            UpdateStatusBar();
        }

        private void _EditDeckDialog(object sender, RoutedEventArgs e)
        {
            var edit_deck_window = new DeckView(current_deck);
            edit_deck_window.ShowDialog();

            if (edit_deck_window.save_changes)
            {
                SQLiteConnection deckdbcon;
                deckdbcon =
                new SQLiteConnection("Data Source=" + ideckfile + ";Version=3;");
                deckdbcon.Open();
                SQLiteConnection userdbcon;
                userdbcon =
                new SQLiteConnection("Data Source=" + iuserfile + ";Version=3;");
                userdbcon.Open();
                //Remove all words to remove
                using (var decktrans = deckdbcon.BeginTransaction())
                {
                    using (var usertrans = userdbcon.BeginTransaction())
                    {
                        foreach (Word w in edit_deck_window.removed_deck.dc)
                        {
                            var delete_sql_query = "DELETE FROM deck WHERE guid='" + w.guid + "';";
                            var delete_sql_command = new SQLiteCommand(delete_sql_query, deckdbcon);
                            delete_sql_command.ExecuteNonQuery();
                            delete_sql_command = new SQLiteCommand(delete_sql_query, userdbcon);
                            delete_sql_command.ExecuteNonQuery();
                        }
                        usertrans.Commit();
                    }
                    decktrans.Commit();
                }

                deckdbcon.Close();
                userdbcon.Close();

                //add and update all other words
                //foreach(Word w in edit_deck_window.deck.dc)
                //{
                //    UpdateDeckDB(w);
                //    UpdateUserDB(w);
                //}
                UpdateDeckDB(edit_deck_window.deck.dc);
                UpdateUserDB(edit_deck_window.deck.dc);

                current_deck = edit_deck_window.deck;
                UpdateStatusBar();
                ShowDeck();
            }
        }

        private void _MarkAsEasy(object sender, RoutedEventArgs e)
        {
            current_word.MarkAsEasy();
            UpdateUserDB(current_word);
            UpdateStatusBar();
        }
        private void _MarkAsHard(object sender, RoutedEventArgs e)
        {
            current_word.MarkAsHard();
            UpdateUserDB(current_word);
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
