using System;
using System.Collections.Generic;
using System.IO;
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

namespace Pamya
{
    /// <summary>
    /// Interaction logic for FlashCardGame.xaml
    /// </summary>
    public partial class FlashCardGame : Page, GameInterface
    {
        public Delegate _PostI;
        public FlashCardGame(Delegate _PostI)
        {
            InitializeComponent();
            this._PostI = _PostI;
        }

        public void ShowDeck()
        {
            questionBlock.Text = PamyaDeck.Instance.CurrentWord.question;
        }


        public void _EditCard()
        {
            questionBlock.Text = PamyaDeck.Instance.CurrentWord.question;

            if (!TBox.IsEnabled)
            {
                TBox.Text = PamyaDeck.Instance.CurrentWord.answer;
                answerBox.Text = PamyaDeck.Instance.CurrentWord.answer;
                exampleBox.Text = PamyaDeck.Instance.CurrentWord.example;
            }
        }

        public void _KeyPress(object sender, KeyEventArgs e)
        {
            
        }

        private void ShowThreeButtons()
        {
            ShowButton.Visibility = System.Windows.Visibility.Collapsed;
            HardButton.Visibility = System.Windows.Visibility.Visible;
            GoodButton.Visibility = System.Windows.Visibility.Visible;
            EasyButton.Visibility = System.Windows.Visibility.Visible;

            answerBox.Text = PamyaDeck.Instance.CurrentWord.answer;
            exampleBox.Text = PamyaDeck.Instance.CurrentWord.example;

            var img_file = PamyaDeck.Instance.CurrentDeckFolder + @"\" + PamyaDeck.Instance.CurrentWord.image_file_location;
            if (File.Exists(img_file))
            {
                image.Source = new BitmapImage(new Uri(img_file));
                image.Visibility = Visibility.Visible;
            }

            SpeechPlayer.SpeakWord(PamyaDeck.Instance.CurrentWord);
        }

        private void HideThreeButtons()
        {
            ShowButton.Visibility = System.Windows.Visibility.Visible;
            HardButton.Visibility = System.Windows.Visibility.Collapsed;
            GoodButton.Visibility = System.Windows.Visibility.Collapsed;
            EasyButton.Visibility = System.Windows.Visibility.Collapsed;

            answerBox.Text = "";

            exampleBox.Text = "";
            image.Visibility = Visibility.Hidden;
            PamyaDeck.Instance.ShowDeck();
            _PostI.DynamicInvoke();
        }

        public void _FlashEasyPress(object sender, RoutedEventArgs e)
        {
            PamyaDeck.Instance.CurrentWord.MarkAsEasy();
            PamyaDeck.Instance.CurrentWord.answered(PamyaDeck.Instance.CurrentWord.answer);
            HideThreeButtons();
        }
        public void _FlashGoodPress(object sender, RoutedEventArgs e)
        {
            PamyaDeck.Instance.CurrentWord.answered(PamyaDeck.Instance.CurrentWord.answer);
            HideThreeButtons();
        }
        public void _FlashHardPress(object sender, RoutedEventArgs e)
        {
            PamyaDeck.Instance.CurrentWord.answered("");
            HideThreeButtons();
        }
        public void _FlashShowPress(object sender, RoutedEventArgs e)
        {
            ShowThreeButtons();

        }
    }
}
