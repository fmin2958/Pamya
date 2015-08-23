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
        private string deckdbfilename;
        private string userdbfilename;
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
                ideckfolder = appdatafolder + @"\Decks\" + System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
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
        private void my_text_KeyUp(object sender, KeyEventArgs e)
        {
            //handler codes go here as needed.
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
                    SQLiteConnection userdbcon;
                    userdbcon =
                    new SQLiteConnection("Data Source=" + iuserfile + ";Version=3;");
                    userdbcon.Open();
                    string sql = "update deck set EF='" + currentWord.EF + "', I='" + currentWord.I + "', n='" + currentWord.n +
                        "', studied='" + currentWord.studied + "', timedue='" + currentWord.timeDue + "' where guid='" + currentWord.id + "';";
                    //MessageBox.Show(sql);
                    var command = new SQLiteCommand(sql, userdbcon);
                    try
                    {
                        command.ExecuteNonQuery();
                    } catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }

                    userdbcon.Close();

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
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeD = epoch.AddSeconds(currentWord.timeDue);
            lblStatus.Text = "EF: " + currentWord.EF.ToString() + "; n: " + currentWord.n.ToString() + "; Time Due Next: " + timeD.ToLocalTime().ToLongDateString() + " " + timeD.ToLocalTime().ToLongTimeString();
        }
    }

    public class Deck
    {
        public List<Word> dc;
        private List<Word> studyList;
        public Deck()
        {
            dc = new List<Word>();
            studyList = new List<Word>();
        } 

        public void addWord(Word w)
        {
            dc.Add(w);
        }

        public void fillDeckFromString(string s)
        {
            dc = new List<Word>();
            foreach (string l in s.Split( '\n' ))
            {
                string[] ws = l.Split('|');
                Console.WriteLine(ws[0]);
                if (ws.Length == 2)
                {
                    addWord(new Word(ws[0].Trim(), ws[1].Trim()));
                } else if (ws.Length == 7)
                {
                    addWord(new Word(ws[0].Trim(), ws[1].Trim(), ws[2].Trim(), ws[3].Trim(), ws[4].Trim(), ws[5].Trim(), ws[6].Trim()));
                }
                else if (ws.Length == 9)
                {
                    addWord(new Word(ws[0].Trim(), ws[1].Trim(), ws[2].Trim(), ws[3].Trim(), ws[4].Trim(), ws[5].Trim(), ws[6].Trim(), ws[7].Trim(), ws[8].Trim()));
                }
                //
            }
        }

        public string getQ(int nnn)
        {
            return dc[nnn].question;
        }

        public Word getNextWord(Boolean _review_only)
        {
            if (studyList.Count == 0)
            {
                //n += 1;
                //studyOrder = dc.OrderBy(o => o.I).ToList();
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;
                //get all cards already studied
                var studiedCards = dc.Where(x => x.studied == true).ToList();
                //out of those get all cards due
                //studyList = studyList.Where(x => x.timeDue <= (secondsSinceEpoch + 900)).ToList();

                //get all the cards past due and randomise them
                studyList = studiedCards.Where(x => x.timeDue <= (secondsSinceEpoch)).ToList();
                studyList.Shuffle();

                //append all cards that will be due in 10 minutes or less randomised
                var willBeDue = studiedCards.Where(x => (x.timeDue <= (secondsSinceEpoch + 600)) && (x.timeDue > (secondsSinceEpoch))).ToList();
                willBeDue.Shuffle();
                studyList.AddRange(willBeDue);
                //studyList.Shuffle();

                //Add new cards too at the start
                if (studyList.Count < 5 && (! _review_only))
                {
                    studyList.Reverse();
                    studyList.AddRange(dc.Where(x => x.studied == false).ToList().Take(1));
                    studyList.Reverse();
                }
                
            }
            
                if (!(studyList.Count > 0))
                    return new Word("YOU ARE DONE FOR NOW", "GG");
                var nextCard = studyList[0];
                studyList.RemoveAt(0);
                return nextCard;

            
        }



        public override string ToString()
        {
            string s = "";
            foreach (Word w in dc)
            {
                s += w.ToString() + "\n";
            }
            return s;
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

    public class Word
    {
        public string question;
        public string answer;
        public double EF;
        public double I;
        public int n;
        public bool studied;
        public int timeDue;

        public string id;
        public string wavfileloc;
        public Word(string q,string a)
        {
            question = q;
            answer = a;
            EF = 2.5;
            I = 1;
            n = 1;
            studied = false;
            timeDue = 0;

            id = "";
            wavfileloc = "";

        }
        public Word(string q, string a, string e, string i, string nnn, string s, string t)
        {
            question = q;
            answer = a;
            EF = Convert.ToDouble(e);
            I = Convert.ToDouble(i);
            n = Convert.ToInt32(nnn);
            studied = Convert.ToBoolean(s);
            timeDue = Convert.ToInt32(t);

            id = "";
            wavfileloc = "";
        }
        public Word(string idd,string q, string a, string e, string i, string nnn, string s, string t, string wav)
        {
            question = q;
            answer = a;
            EF = Convert.ToDouble(e);
            I = Convert.ToDouble(i);
            n = Convert.ToInt32(nnn);
            studied = Convert.ToBoolean(s);
            timeDue = Convert.ToInt32(t);

            id = idd;
            wavfileloc = wav;
        }
        private uint editDistance(string s,string t)
        {
            s = s.ToLower();
            t = t.ToLower();
            int m = s.Length;
            int n = t.Length;
            uint[,] d = new uint[m+1, n+1];
            for (int i = 0; i <= m; i++)
            {
                d[i,0] = (uint)i;
            }
            for (int j = 0; j <= n; j++)
            {
                d[0, j] = (uint)j;
            }

            for (int j = 1; j <= n; j++)
            {
                for (int i = 1; i <= m; i++)
                {
                    if (s[i-1] == t[j-1])
                    {
                        d[i, j] = d[i - 1, j - 1];
                    }
                    else
                    {
                        d[i, j] = Math.Min(d[i - 1, j] + 1, Math.Min(d[i, j - 1] + 1, d[i - 1, j - 1] + 1));
                   } 
                }
            }
            return d[m,n];
        }

        private void EFchange(double q)
        {
            EF = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.2));
            if (EF < 1.3) { EF = 1.3; }
        }

        private double Ifactor(int nnn)
        {
            if (nnn <= 3)
            {
                return 1d;
            }
            else if (nnn == 4)
            {
                return 4d;
            }
            else
            {
                return Ifactor(nnn - 1) * EF;
            }
        }

        public bool answered(string attempt)
        {
            uint dist = editDistance(answer, attempt);
            int q = 5 - ((int)dist);
            if (attempt.Length == 0)
            {
                q = 0;
            }
            if (q < 0) { q = 0; }

            if (q < 4)
            {
                EFchange(3);
                I = 1d;
                n = 1;
            } 
            else if (q == 5)
            {
                if (n > 3)
                    EFchange(q);

                n += 1;
                I = Ifactor(n);
            }
            else if (q == 4)
            {
                if (n > 3)
                    EFchange(q);

                n -= 1;
                if (n < 1) { n = 1; }
                I = Ifactor(n);
            }
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            timeDue = secondsSinceEpoch + Convert.ToInt32(I * 60 * 2.5);
            studied = true;

            if (q == 5) { return true; } else { return false; }
        }

        public override string ToString()
        {
            return question + "|" + answer + "|" + EF.ToString() + "|" + I.ToString() + "|" + n.ToString() + "|" + studied.ToString() + "|" + timeDue.ToString();
        }
    }
}
