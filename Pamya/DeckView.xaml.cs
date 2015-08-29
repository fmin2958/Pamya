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
using System.Windows.Shapes;

namespace Pamya
{
    /// <summary>
    /// Interaction logic for DeckView.xaml
    /// </summary>
    public partial class DeckView : Window
    {
        public Deck deck;
        public bool save_changes;
        public DeckView(Deck deck)
        {
            this.deck = deck.Clone();
            this.save_changes = false;
            InitializeComponent();
            ShowDeck();
        }

        private void _EditCardDialog(object sender, RoutedEventArgs e)
        {
            if (lvCards.SelectedIndex > -1)
            {
                int index = lvCards.SelectedIndex;
                var edit_window = new EditCardWindow(deck.dc[index]);
                edit_window.ShowDialog();
            }
            ShowDeck();
        }

        private void _InsertCardAfter(object sender, RoutedEventArgs e)
        {
            if (lvCards.SelectedIndex > -1)
            {
                int index = lvCards.SelectedIndex;
                int id = index + 1;
                foreach (Word w in deck.dc.Where(x => x.id > id))
                {
                    w.id++;
                }
                Word new_word = new Word("New", "Word");
                new_word.id = id + 1;

                deck.dc.Insert(index + 1, new_word);
            }
            ShowDeck();
        }

        private void ShowDeck()
        {
            List<CardView> cards = new List<CardView>();
            foreach (Word w in deck.dc)
            {
                cards.Add(new CardView() { ID = w.id, Question = w.question, Answer = w.answer, Example = w.example, Studied = w.studied });
            }
            lvCards.ItemsSource = cards;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            this.save_changes = true;
            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
    public class CardView
    {

        public int ID { get; set; }

        public string Answer { get; set; }

        public string Question { get; set; }

        public string Example { get; set; }

        public bool Studied { get; set; }
    }
}
