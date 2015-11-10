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
        public MCGame(Delegate _PostI)
        {
            InitializeComponent();
            correct_button = 1;
            this._PostI = _PostI;
        }
        public void ShowDeck()
        {
            PamyaDeck.Instance.CurrentWord = PamyaDeck.Instance.CurrentDeck.GetNextWord(PamyaDeck.Instance.bReviewOnly);


            questionBlock.Text = PamyaDeck.Instance.CurrentWord.question;

            var filler_words = PamyaDeck.Instance.CurrentDeck.RandomWords(7, PamyaDeck.Instance.CurrentWord);
            var real_button = (new Random()).Next(1, 8);

            correct_button = real_button;

            Button1.Content = filler_words.Dequeue().answer;
            Button2.Content = filler_words.Dequeue().answer;
            Button3.Content = filler_words.Dequeue().answer;
            Button4.Content = filler_words.Dequeue().answer;
            Button5.Content = filler_words.Dequeue().answer;
            Button6.Content = filler_words.Dequeue().answer;
            Button7.Content = filler_words.Dequeue().answer;
            Button8.Content = filler_words.Dequeue().answer;

            switch (real_button)
            {
                case 1:
                    Button1.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                case 2:
                    Button2.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                case 3:
                    Button3.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                case 4:
                    Button4.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                case 5:
                    Button5.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                case 6:
                    Button6.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                case 7:
                    Button7.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                case 8:
                    Button8.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;
                default:
                    Button1.Content = PamyaDeck.Instance.CurrentWord.answer;
                    break;

            }
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
                Button1.IsEnabled = false;
                Button2.IsEnabled = false;
                Button3.IsEnabled = false;
                Button4.IsEnabled = false;
                Button5.IsEnabled = false;
                Button6.IsEnabled = false;
                Button7.IsEnabled = false;
                Button8.IsEnabled = false;

                button.IsEnabled = true;

                switch (correct_button)
                {
                    case 1:
                        Button1.Foreground = Brushes.Green;
                        Button1.IsEnabled = true;
                        break;
                    case 2:
                        Button2.Foreground = Brushes.Green;
                        Button2.IsEnabled = true;
                        break;
                    case 3:
                        Button3.Foreground = Brushes.Green;
                        Button3.IsEnabled = true;
                        break;
                    case 4:
                        Button4.Foreground = Brushes.Green;
                        Button4.IsEnabled = true;
                        break;
                    case 5:
                        Button5.Foreground = Brushes.Green;
                        Button5.IsEnabled = true;
                        break;
                    case 6:
                        Button6.Foreground = Brushes.Green;
                        Button6.IsEnabled = true;
                        break;
                    case 7:
                        Button7.Foreground = Brushes.Green;
                        Button7.IsEnabled = true;
                        break;
                    case 8:
                        Button8.Foreground = Brushes.Green;
                        Button8.IsEnabled = true;
                        break;
                    default:
                        Button1.Foreground = Brushes.Green;
                        Button1.IsEnabled = true;
                        break;
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
                TBox.Text = "";
                TBox.IsEnabled = true;

                Button1.Foreground = Brushes.Black;
                Button2.Foreground = Brushes.Black;
                Button3.Foreground = Brushes.Black;
                Button4.Foreground = Brushes.Black;
                Button5.Foreground = Brushes.Black;
                Button6.Foreground = Brushes.Black;
                Button7.Foreground = Brushes.Black;
                Button8.Foreground = Brushes.Black;

                Button1.IsEnabled = true;
                Button2.IsEnabled = true;
                Button3.IsEnabled = true;
                Button4.IsEnabled = true;
                Button5.IsEnabled = true;
                Button6.IsEnabled = true;
                Button7.IsEnabled = true;
                Button8.IsEnabled = true;

                //TBox.Focus();
                exampleBox.Text = "";
                image.Visibility = Visibility.Hidden;
                ShowDeck();
            }
            _PostI.DynamicInvoke();
        }

        public bool _KeyPress(object sender, KeyEventArgs e)
        {
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
