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
    /// Interaction logic for EditCardWindow.xaml
    /// </summary>
    public partial class EditCardWindow : Window
    {
        Word current_word;
        public EditCardWindow(Word current_word)
        {
            this.current_word = current_word;
            InitializeComponent();

            QuestionBox.Text = current_word.question;
            AnswerBox.Text = current_word.answer;
            ExampleBox.Text = current_word.example;
            StudiedBox.IsChecked = current_word.studied;
            WavFileBox.Text = current_word.wav_file_loc;
            ImageFileBox.Text = current_word.image_file_location;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            current_word.question = QuestionBox.Text;
            current_word.answer = AnswerBox.Text;
            current_word.example = ExampleBox.Text;
            current_word.studied = (bool)StudiedBox.IsChecked;
            current_word.wav_file_loc = WavFileBox.Text;
            current_word.image_file_location = ImageFileBox.Text;

            if ((bool)ResetWordBox.IsChecked)
            {
                Word w = new Word("", "");
                current_word.I = w.I;
                current_word.EF = w.EF;
                current_word.n = w.n;
                current_word.studied = w.studied;
                current_word.time_due = w.time_due;
            }


            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Play_TTS_Click(object sender, RoutedEventArgs e)
        {
            SpeechPlayer.SpeakWord(current_word);
        }
    }
}
