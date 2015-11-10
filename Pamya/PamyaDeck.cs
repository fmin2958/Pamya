using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamya
{
    public sealed class PamyaDeck
    {
        private static readonly Lazy<PamyaDeck> lazy =
            new Lazy<PamyaDeck>(() => new PamyaDeck());

        public static PamyaDeck Instance { get { return lazy.Value; } }

        private PamyaDeck()
        {

        }

        private string current_deck_folder;
        public string CurrentDeckFolder
        {
            get
            {
                return current_deck_folder;
            }
            set
            {
                current_deck_folder = value;
            }
        }
        private string decks_folder;
        public string DecksFolder
        {
            get
            {
                return decks_folder;
            }
            set
            {
                decks_folder = value;
            }
        }

        private Deck current_deck;
        public Deck CurrentDeck
        {
            get
            {
                return current_deck;
            }
            set
            {
                current_deck = value;
            }
        }

        private Word current_word;
        public Word CurrentWord
        {
            get
            {
                return current_word;
            }
            set
            {
                current_word = value;
            }
        }
        private bool _review_only;
        public bool bReviewOnly
        {
            get
            {
                return _review_only;
            }
            set
            {
                _review_only = value;
            }
        }

    }
}
