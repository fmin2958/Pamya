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
    /// Interaction logic for OpenDeckWindow.xaml
    /// </summary>
    public partial class OpenDeckWindow : Window
    {
        private string file_name;
        public string FileName
        {
            get
            {
                return file_name;
            }
            set
            {
                file_name = value;
            }
        }
        public OpenDeckWindow()
        {
            InitializeComponent();
            //List<DeckDisplay> notes = new List<DeckDisplay>();
            List<string> notes = new List<string>();

            //MessageBox.Show(PamyaDeck.Instance.DecksFolder);

            foreach (var d in Directory.GetDirectories(PamyaDeck.Instance.DecksFolder))
            {
                //MessageBox.Show(d.ToString());
                //notes.Add(new DeckDisplay { Name = System.IO.Path.GetDirectoryName(d.ToString()) });
                notes.Add(System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(d.ToString() + @"\")));
            }
            //notes.Add(new DeckDisplay { Item = "a", Value = 1 });

            ListBox.ItemsSource = notes;
            //ListBox.DisplayMember = "Item";
        }

        private void _OpenButtonClick(object sender, RoutedEventArgs e)
        {
            FileName = ListBox.SelectedItem as string;
            if (Directory.Exists(PamyaDeck.Instance.DecksFolder + @"\" + FileName))
            {
                this.DialogResult = true;
                this.Close();
            }
        }
        private void _CancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }


    public class DeckDisplay
    {
        public string Name { get; set; }
    }
}
