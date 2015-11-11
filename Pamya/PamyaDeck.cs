using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Pamya
{

    public sealed class PamyaDeck
    {
        private static readonly Lazy<PamyaDeck> lazy =
            new Lazy<PamyaDeck>(() => new PamyaDeck());

        public static PamyaDeck Instance { get { return lazy.Value; } }

        private PamyaDeck()
        {

        }

        public List<string> GameTypes = new List<string>()
        {
            "Typing Game", "Multiple Choice Game", "Flash Card Game"
        };

        public List<Delegate> GameChangeDelegates;

        private GameInterface current_game;
        public GameInterface CurrentGame
        {
            get
            {
                return current_game;
            }
            set
            {
                current_game = value;
            }
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

        private string deck_file;
        public string DeckFile
        {
            get
            {
                return deck_file;
            }
            set
            {
                deck_file = value;
            }
        }

        private string user_file;
        public string UserFile
        {
            get
            {
                return user_file;
            }
            set
            {
                user_file = value;
            }
        }

        private Deck current_deck;
        public Deck CurrentDeck
        {
            get
            {
                return current_deck;
            }
            set
            {
                current_deck = value;
            }
        }

        private Word current_word;
        public Word CurrentWord
        {
            get
            {
                return current_word;
            }
            set
            {
                current_word = value;
            }
        }
        private bool _review_only;
        public bool bReviewOnly
        {
            get
            {
                return _review_only;
            }
            set
            {
                _review_only = value;
            }
        }

        private string app_data_folder;
        public string AppDataFolder
        {
            get
            {
                return app_data_folder;
            }
            set
            {
                app_data_folder = value;
            }
        }


        public void ShowDeck()
        {
            //Ideally we want to control what goes on with the deck here, so we will pick a card from the deck, then pass it onto the game, then call the appropriate stuff
            CurrentWord = CurrentDeck.GetNextWord(bReviewOnly);
            CurrentGame.ShowDeck();
        }

        public void _OpenDialog(object sender, RoutedEventArgs e)
        {
            GameChangeDelegates[0].DynamicInvoke(); //This is the mainmenu opening delegate
        }

        public void _OpenDeck(string FileName, int GameType)
        {
            //this.Title = "Pamya - " + fname; //FIXME
            CurrentDeckFolder = DecksFolder + @"\" + FileName;
            DeckFile = CurrentDeckFolder + @"\deck.sqlite";

            UserFile = CurrentDeckFolder + @"\userdata.sqlite";


            //I feel like this is too much overhead
            SQLiteConnection deckdbcon;
            deckdbcon =
            new SQLiteConnection("Data Source=" + DeckFile + ";Version=3;");
            deckdbcon.Open();


            SQLiteConnection userdbcon;
            userdbcon =
            new SQLiteConnection("Data Source=" + UserFile + ";Version=3;");
            userdbcon.Open();

            string sql = "SELECT * FROM deck ORDER BY id ASC";
            SQLiteCommand command = new SQLiteCommand(sql, deckdbcon);
            SQLiteDataReader deck_reader = command.ExecuteReader();

            CurrentDeck = new Deck();


            while (deck_reader.Read())
            {
                var word = new Word(deck_reader["question"].ToString(), deck_reader["answer"].ToString());
                word.id = Convert.ToInt32(deck_reader["id"]);
                word.wav_file_loc = deck_reader["wavfileloc"].ToString();
                word.guid = deck_reader["guid"].ToString();
                word.example = deck_reader["example"].ToString();
                word.image_file_location = deck_reader["imagefileloc"].ToString();
                var user_sql = "SELECT * FROM deck WHERE guid='" + deck_reader["guid"].ToString() + "'";
                var user_command = new SQLiteCommand(user_sql, userdbcon);
                try
                {
                    var user_reader = user_command.ExecuteReader();
                    user_reader.Read();
                    word.EF = Convert.ToDouble(user_reader["ef"]);
                    word.I = Convert.ToDouble(user_reader["i"]);
                    word.n = Convert.ToInt32(user_reader["n"]);
                    word.studied = Convert.ToBoolean(user_reader["studied"]);
                    word.time_due = Convert.ToInt32(user_reader["timedue"]);
                    user_reader.Close();
                    CurrentDeck.AddWord(word);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            }

            deck_reader.Close();

            userdbcon.Close();
            deckdbcon.Close();

            //TODO
            //FIND OUT WHICH GAME IS PLAYED


            //OPEN THE GAME 

            GameChangeDelegates[GameType + 1].DynamicInvoke(); // +1 as 0 is the mainmenu page



        }



    }
}
