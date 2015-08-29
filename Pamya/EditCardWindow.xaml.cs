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
        Word currentword;
        public EditCardWindow(Word curcard)
        {
            currentword = curcard;
            InitializeComponent();

            QuestionBox.Text = currentword.question;
            AnswerBox.Text = currentword.answer;
            StudiedBox.IsChecked = currentword.studied;
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            currentword.question = QuestionBox.Text;
            currentword.answer = AnswerBox.Text;
            currentword.example = ExampleBox.Text;
            currentword.studied = (bool)StudiedBox.IsChecked;
            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
