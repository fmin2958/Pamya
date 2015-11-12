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

namespace Pamya
{
    /// <summary>
    /// Interaction logic for TypingGame.xaml
    /// So the Typing Game is supposed to be a game that, on any given word forced the user to type a response.
    /// That response is taken an processed for correctness. 
    /// Closeness to the response determines how the word is handled.
    /// </summary>
    public partial class TypingGame : Page, GameInterface
    {
        public TypingGame()
        {
            InitializeComponent();
        }


        public void ShowDeck()
        {
            questionBlock.Text = PamyaDeck.Instance.CurrentWord.question;
        }

        private void ResetAnswerTextBox()
        {
            TBox.Text = "";
            TBox.IsEnabled = true;
            TBox.Foreground = Brushes.Black;
            TBox.Focus();
        }

        public void _KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //do something if 'enter' key is pressed.
                if (TBox.IsEnabled == true)
                {
                    TBox.IsEnabled = false;
                    string ans = TBox.Text;
                    TBox.Text = PamyaDeck.Instance.CurrentWord.answer;
                    exampleBox.Text = PamyaDeck.Instance.CurrentWord.example;
                    if (PamyaDeck.Instance.CurrentWord.answered(ans))
                    {
                        TBox.Foreground = Brushes.Green;
                    }
                    else
                    {
                        TBox.Foreground = Brushes.Red;
                    }

                    var img_file = PamyaDeck.Instance.CurrentDeckFolder + @"\" + PamyaDeck.Instance.CurrentWord.image_file_location;
                    if (File.Exists(img_file))
                    {
                        image.Source = new BitmapImage(new Uri(img_file));
                        image.Visibility = Visibility.Visible;
                    }

                    SpeechPlayer.SpeakWord(PamyaDeck.Instance.CurrentWord);

                }
                else
                {   
                    ResetAnswerTextBox();

                    exampleBox.Text = "";
                    image.Visibility = Visibility.Hidden;

                    PamyaDeck.Instance.ShowDeck();
                }
            }
        }

        public void _EditCard()
        {
            questionBlock.Text = PamyaDeck.Instance.CurrentWord.question;

            if (!TBox.IsEnabled)
            {
                TBox.Text = PamyaDeck.Instance.CurrentWord.answer;
                exampleBox.Text = PamyaDeck.Instance.CurrentWord.example;
            }
        }
    }
}
