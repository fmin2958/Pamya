using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Pamya
{
    /// <summary>
    /// Interaction logic for MCGame.xaml
    /// </summary>
    public partial class MCGame : Page, GameInterface
    {
        public int correct_button;
        public Delegate _PostI;
        public List<Button> Buttons;
        public MCGame(Delegate _PostI)
        {
            InitializeComponent();
            correct_button = 1;
            this._PostI = _PostI;
            Buttons = new List<Button>()
            {
                Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8
            };
        }
        public void ShowDeck()
        {
            questionBlock.Text = PamyaDeck.Instance.CurrentWord.question;

            var filler_words = PamyaDeck.Instance.CurrentDeck.RandomWords(7, PamyaDeck.Instance.CurrentWord);
            correct_button = (new Random()).Next(1, 8);

            for (int i = 0; i < Buttons.Count; i++ )
            {
                Buttons[i].Content = (i+1).ToString() + ". " + filler_words.Dequeue().answer;
            }

            Buttons[correct_button - 1].Content = correct_button.ToString() + ". " + PamyaDeck.Instance.CurrentWord.answer;
        }

        public void _ButtonPress(object sender, RoutedEventArgs e)
        {
            Button button = ((Button)sender);
            var button_number = Int32.Parse(Regex.Match(button.Name, @"\d+").Value);

            if (TBox.IsEnabled == true)
            {
                TBox.IsEnabled = false;
                string ans = TBox.Text;
                TBox.Text = PamyaDeck.Instance.CurrentWord.answer;
                exampleBox.Text = PamyaDeck.Instance.CurrentWord.example;
                if (button_number == correct_button)
                {
                    PamyaDeck.Instance.CurrentWord.answered(PamyaDeck.Instance.CurrentWord.answer);
                    button.Foreground = Brushes.Green;
                }
                else
                {
                    PamyaDeck.Instance.CurrentWord.answered("");
                    button.Foreground = Brushes.Red;
                }

                //Disable all buttons except for this one
                foreach (Button b in Buttons)
                {
                    b.IsEnabled = false;
                }

                button.IsEnabled = true;


                Buttons[correct_button - 1].Foreground = Brushes.Green;
                Buttons[correct_button - 1].IsEnabled = true;


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
                TBox.Text = "";
                TBox.IsEnabled = true;

                foreach(Button b in Buttons)
                {
                    b.Foreground = Brushes.Black;
                    b.IsEnabled = true;
                }

                //TBox.Focus();
                exampleBox.Text = "";
                image.Visibility = Visibility.Hidden;
                ShowDeck();
            }
            _PostI.DynamicInvoke();
        }

        public bool _KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1: _ButtonPress(Button1, e); break;
                case Key.D2: _ButtonPress(Button2, e); break;
                case Key.D3: _ButtonPress(Button3, e); break;
                case Key.D4: _ButtonPress(Button4, e); break;
                case Key.D5: _ButtonPress(Button5, e); break;
                case Key.D6: _ButtonPress(Button6, e); break;
                case Key.D7: _ButtonPress(Button7, e); break;
                case Key.D8: _ButtonPress(Button8, e); break;
                default: _ButtonPress(Button1, e); break;
            }

            return true;
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
