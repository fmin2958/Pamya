using System;
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

//THIS CODE IS A MESS


namespace Pamya
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            currentWord = new Word("fuck", "you");

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

        private void _toggle_review_only(object sender, RoutedEventArgs e)
        {

            _review_only = !_review_only;
        }


        private void _app_exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void _open_dialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = appdatafolder + @"\Links";
            /*if (openFileDialog.ShowDialog() == true)
            {
                string stuff = File.ReadAllText(openFileDialog.FileName);
                filename = openFileDialog.FileName;
                mc.fillDeckFromString(stuff);
                show_deck();
            }*/
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

                string stuff = "";

                List<string> deckstuff = new List<string>();

                string sql = "select * from deck order by guid asc";
                SQLiteCommand command = new SQLiteCommand(sql, deckdbcon);
                SQLiteDataReader deckreader = command.ExecuteReader();
                SQLiteCommand usercommand = new SQLiteCommand(sql, userdbcon);
                SQLiteDataReader userreader = usercommand.ExecuteReader();
                while (deckreader.Read() && userreader.Read())
                {
                    deckstuff.Add(deckreader["guid"] + "|" + deckreader["question"] + "|" + deckreader["answer"] + "|" +
                        userreader["ef"] + "|" + userreader["i"] + "|" + userreader["n"] + "|" + userreader["studied"] + "|" + userreader["timedue"] + "|" + deckreader["wavfileloc"]);
                }

                stuff = String.Join("\n", deckstuff);
                //Console.Write(stuff);
                //for 
                mc.fillDeckFromString(stuff);
                show_deck();

                deckreader.Close();
                userreader.Close();

                userdbcon.Close();
                deckdbcon.Close();
            }
            
                
        }
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


                int guid = 0;

                foreach (Word w in importDeck.dc)
                {

                    guid++;

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


                    sql = "insert into deck values('" + guid + "','" + w.question.Replace("'", "''") + "','" + w.answer.Replace("'", "''") + "','" + wavfilename + "');";
                    //MessageBox.Show(sql);
                    command = new SQLiteCommand(sql, deckdbcon);
                    try {
                        command.ExecuteNonQuery();
                         } catch (Exception ex)
                    {
                        //MessageBox.Show(ex.ToString());
                        MessageBox.Show(sql);
                    }

                    sql = "insert into deck values('" + guid + "','" + w.EF.ToString() + "','" + w.I.ToString() + "','" + w.n.ToString() + "','" + w.studied.ToString() + "','" + w.timeDue.ToString() + "');";
                    //MessageBox.Show(sql);
                    command = new SQLiteCommand(sql, userdbcon);
                    command.ExecuteNonQuery();
                }


                userdbcon.Close();
                deckdbcon.Close();
            }

        }
        private void show_deck()
        {
            currentWord = mc.getNextWord(_review_only);
            questionBlock.Text = currentWord.question;
            changeStatusBar();
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

                    changeStatusBar();

                    //if (File.Exists(filename))
                     //   System.IO.File.WriteAllText(filename, mc.ToString());
                    //

                    //update the database
                    if (File.Exists(iuserfile))
                    {
                        SQLiteConnection userdbcon;
                        userdbcon =
                        new SQLiteConnection("Data Source=" + iuserfile + ";Version=3;");
                        userdbcon.Open();
                        string sql = "update deck set EF='" + currentWord.EF + "', I='" + currentWord.I + "', n='" + currentWord.n +
                            "', studied='" + currentWord.studied + "', timedue='" + currentWord.timeDue + "' where rowid=" + currentWord.id + ";";
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
                else
                {
                    TBox.Text = "";
                    TBox.IsEnabled = true;
                    TBox.Foreground = Brushes.Black;
                    TBox.Focus();
                    show_deck();
                }
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
        public void changeStatusBar()
        {
            //need to cleanup the time usage, I copy paste code which is terrible
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeD = epoch.AddSeconds(currentWord.timeDue);
            lblStatus.Text = "EF: " + currentWord.EF.ToString() + "; n: " + currentWord.n.ToString() + "; Time Due Next: " + timeD.ToLocalTime().ToLongDateString() + " " + timeD.ToLocalTime().ToLongTimeString();
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
