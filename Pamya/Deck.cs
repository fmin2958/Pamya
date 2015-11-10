using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamya
{
    public class Deck
    {
        public List<Word> dc;
        private List<Word> studyList;
        public Deck()
        {
            dc = new List<Word>();
            studyList = new List<Word>();
        }

        public Deck Clone()
        {
            var clone_deck = new Deck();
            foreach (Word w in dc)
            {
                clone_deck.AddWord(w.Clone());
            }
            return clone_deck;
        }

        public void AddWord(Word w)
        {
            dc.Add(w);
        }

        public void fillDeckFromString(string s)
        {
            dc = new List<Word>();
            int id = 0;
            foreach (string l in s.Split('\n'))
            {
                string[] ws = l.Split('|');
                id++;
                string guid = new Guid().ToString();
                Word word = new Word(ws[0].Trim(), ws[1].Trim());
                word.id = id;
                word.guid = guid;
                dc.Add(word);
            }
        }

        public Word GetNextWord(Boolean _review_only)
        {
            //FIXME make this rely on config file
            const int _TIME_TO_LOOK_AHEAD = 3300; //55 minutes FIXME: make modifiable
            if (studyList.Count == 0)
            {

                int secondsSinceEpoch = EpochTime.GetAsInt();

                //get all cards already studied
                var studiedCards = dc.Where(x => x.studied == true).ToList();

                //get all the cards past due and randomise them 
                studyList = studiedCards.Where(x => x.time_due <= (secondsSinceEpoch)).ToList();

                studyList.Shuffle();


                //append all cards that will be due in _TIME_TO_LOOK_AHEAD seconds or less randomised
                var willBeDue = studiedCards.Where(x => (x.time_due <= (secondsSinceEpoch + _TIME_TO_LOOK_AHEAD)) && (x.time_due > (secondsSinceEpoch))).ToList();
                willBeDue.Shuffle();
                studyList.AddRange(willBeDue);




                //Add new cards too at the start only if _review_only is false
                if (studyList.Count < 5 && (!_review_only))
                {
                    studyList.Reverse();
                    var nextCards = dc.Where(x => x.studied == false).ToList();
                    nextCards = nextCards.OrderBy(o => o.id).ToList();
                    studyList.AddRange(nextCards.Take(1));
                    studyList.Reverse();
                }



            }

            if (!(studyList.Count > 0))
                return new Word("YOU ARE DONE FOR NOW, SELECT ANOTHER DECK OR TURN OFF REVIEW ONLY IN THE OPTIONS MENU", "GG");
            var nextCard = studyList[0];
            studyList.RemoveAt(0);
            return nextCard;


        }


        public Queue<Word> RandomWords(int amount, Word exclude)
        {
            Queue<Word> ws = new Queue<Word>();
            dc.Shuffle();
            int offset = 0;
            bool bExclude = dc.Count > 1;
            if (dc.Count == 0)
            {
                for (int i = 0; i < amount + 1; i++)
                {
                    ws.Enqueue(new Word("TOO","TOO SHORT"));
                }
            }
            else
            {
                for (int i = 0; i < amount + 1; )
                {
                    if (i + offset == dc.Count)
                    {
                        i = 0;
                        offset = 0;
                    }
                    if (bExclude && dc[i + offset].question == exclude.question)
                    {
                        offset++;
                    }
                    else
                    {
                        ws.Enqueue(dc[i + offset]);

                        i++;
                    }
                }
            }
            return ws;
        }



        public override string ToString()
        {
            string s = "";
            foreach (Word w in dc)
            {
                s += w.ToString() + "\n";
            }
            return s;
        }
    }
}
