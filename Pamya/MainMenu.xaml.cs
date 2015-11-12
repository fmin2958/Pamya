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
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page, GameInterface
    {
        private string file_name;
        private List<DeckDisplay> dd;
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
        public MainMenu()
        {
            InitializeComponent();

            dd = new List<DeckDisplay>();

            foreach (var d in Directory.GetDirectories(PamyaDeck.Instance.DecksFolder))
            {
                dd.Add(new DeckDisplay { Name = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(d.ToString() + @"\")) });
            }

            ListBox.ItemsSource = dd;

            GameType.ItemsSource = PamyaDeck.Instance.GameTypes;
            GameType.SelectedIndex = 0;
        }

        public void _KeyPress(object sender, KeyEventArgs e)
        {

        }

        public void _EditCard()
        {

        }

        public void ShowDeck()
        {

        }

        private void _OpenButtonClick(object sender, RoutedEventArgs e)
        {
            DeckDisplay dec = ((Button)sender).Tag as DeckDisplay;
            FileName = dec.Name;
            //MessageBox.Show(FileName);
            if (Directory.Exists(PamyaDeck.Instance.DecksFolder + @"\" + FileName))
            {
                //MessageBox.Show(GameType.SelectionBoxItem);
                PamyaDeck.Instance._OpenDeck(FileName, GameType.SelectedIndex);
            }
            //FileName = ListBox.SelectedItem as DeckDisplay;
            /*if (ListBox.SelectedIndex > -1)
            {
                FileName = dd[ListBox.SelectedIndex].Name;
                if (Directory.Exists(PamyaDeck.Instance.DecksFolder + @"\" + FileName))
                {
                    
                }
            }*/
        }
    }
}
