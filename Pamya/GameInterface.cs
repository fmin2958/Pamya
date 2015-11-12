using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;

namespace Pamya
{
    public interface GameInterface
    {
        //void setPostInteractionFunc(Delegate _PostI);
        void ShowDeck();
        void _KeyPress(object sender, KeyEventArgs e);
        void _EditCard();
    }
}
