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

        public void addWord(Word w)
        {
            dc.Add(w);
        }

        public void fillDeckFromString(string s)
        {
            dc = new List<Word>();
            foreach (string l in s.Split('\n'))
            {
                string[] ws = l.Split('|');
                if (ws.Length == 2)
                {
                    addWord(new Word(ws[0].Trim(), ws[1].Trim()));
                }
                else if (ws.Length == 7)
                {
                    addWord(new Word(ws[0].Trim(), ws[1].Trim(), ws[2].Trim(), ws[3].Trim(), ws[4].Trim(), ws[5].Trim(), ws[6].Trim()));
                }
                else if (ws.Length == 9)
                {
                    addWord(new Word(ws[0].Trim(), ws[1].Trim(), ws[2].Trim(), ws[3].Trim(), ws[4].Trim(), ws[5].Trim(), ws[6].Trim(), ws[7].Trim(), ws[8].Trim()));
                }
                //
            }
        }

        public string getQ(int nnn)
        {
            return dc[nnn].question;
        }

        public Word getNextWord(Boolean _review_only)
        {
            if (studyList.Count == 0)
            {
                //n += 1;
                //studyOrder = dc.OrderBy(o => o.I).ToList();
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                int secondsSinceEpoch = (int)t.TotalSeconds;
                //get all cards already studied
                var studiedCards = dc.Where(x => x.studied == true).ToList();
                //out of those get all cards due
                //studyList = studyList.Where(x => x.timeDue <= (secondsSinceEpoch + 900)).ToList();

                //get all the cards past due and randomise them
                studyList = studiedCards.Where(x => x.timeDue <= (secondsSinceEpoch)).ToList();
                studyList.Shuffle();

                //append all cards that will be due in 10 minutes or less randomised
                var willBeDue = studiedCards.Where(x => (x.timeDue <= (secondsSinceEpoch + 600)) && (x.timeDue > (secondsSinceEpoch))).ToList();
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
