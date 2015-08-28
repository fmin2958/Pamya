using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pamya
{
    public class Word
    {
        public string question;
        public string answer;
        public double EF;
        public double I;
        public int n;
        public bool studied;
        public int timeDue;

        public int id;
        public string wavfileloc;
        public Word(string q, string a)
        {
            question = q;
            answer = a;
            EF = 2.5;
            I = 1;
            n = 1;
            studied = false;
            timeDue = 0;

            id = 0;
            wavfileloc = "";

        }
        public Word(string q, string a, string e, string i, string nnn, string s, string t)
        {
            question = q;
            answer = a;
            EF = Convert.ToDouble(e);
            I = Convert.ToDouble(i);
            n = Convert.ToInt32(nnn);
            studied = Convert.ToBoolean(s);
            timeDue = Convert.ToInt32(t);

            id = 0;
            wavfileloc = "";
        }
        public Word(string idd, string q, string a, string e, string i, string nnn, string s, string t, string wav)
        {
            question = q;
            answer = a;
            EF = Convert.ToDouble(e);
            I = Convert.ToDouble(i);
            n = Convert.ToInt32(nnn);
            studied = Convert.ToBoolean(s);
            timeDue = Convert.ToInt32(t);

            id = Convert.ToInt32(idd);
            wavfileloc = wav;
        }

        public Word(int id, string question, string answer, double EF, double I, int n, bool studied, int time_due, string wav_file_location)
        {
            this.question = question;
            this.answer = answer;
            this.EF = EF;
            this.I = I;
            this.n = n;
            this.studied = studied;
            this.timeDue = time_due;

            this.id = id;
            this.wavfileloc = wav_file_location;
        }

        private uint editDistance(string s, string t)
        {
            s = s.ToLower();
            t = t.ToLower();
            int m = s.Length;
            int n = t.Length;
            uint[,] d = new uint[m + 1, n + 1];
            for (int i = 0; i <= m; i++)
            {
                d[i, 0] = (uint)i;
            }
            for (int j = 0; j <= n; j++)
            {
                d[0, j] = (uint)j;
            }

            for (int j = 1; j <= n; j++)
            {
                for (int i = 1; i <= m; i++)
                {
                    if (s[i - 1] == t[j - 1])
                    {
                        d[i, j] = d[i - 1, j - 1];
                    }
                    else
                    {
                        d[i, j] = Math.Min(d[i - 1, j] + 1, Math.Min(d[i, j - 1] + 1, d[i - 1, j - 1] + 1));
                    }
                }
            }
            return d[m, n];
        }

        private void EFchange(double q)
        {
            EF = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.2));
            if (EF < 1.3) { EF = 1.3; }
        }

        private double Ifactor(int nnn)
        {
            if (nnn <= 3)
            {
                return 1d;
            }
            else if (nnn == 4)
            {
                return 6d;
            }
            else
            {
                return Ifactor(nnn - 1) * EF;
            }
        }

        public bool answered(string attempt)
        {
            uint dist = editDistance(answer, attempt);
            int q = 5 - ((int)dist);
            if (attempt.Length == 0)
            {
                q = 0;
            }
            if (q < 0) { q = 0; }

            if (q < 4)
            {
                EFchange(q);
                I = 1d;
                n = 1;
            }
            else if (q == 5)
            {
                if (n > 3)
                    EFchange(q);

                n += 1;
                I = Ifactor(n);
            }
            else if (q == 4)
            {
                if (n > 3)
                    EFchange(q);

                n -= 3;
                if (n < 1) { n = 1; }
                I = Ifactor(n);
            }

            SetNextDueDate();
            studied = true;

            if (q == 5) { return true; } else { return false; }
        }

        public void SetNextDueDate()
        {
            int secondsSinceEpoch = EpochTime.GetAsInt();
            timeDue = secondsSinceEpoch + Convert.ToInt32(I * 60 * 4.5);
        }

        public void MarkAsEasy()
        {
            EF = (EF > 3.0) ? EF : 3.0;
            n = (n > 8) ? n : 8;
            I = Ifactor(n);
            SetNextDueDate();
        }

        public void MarkAsHard()
        {
            EF = 1.3;
            n = 1;
            I = Ifactor(n);
            SetNextDueDate();
        }

        public override string ToString()
        {
            return question + "|" + answer + "|" + EF.ToString() + "|" + I.ToString() + "|" + n.ToString() + "|" + studied.ToString() + "|" + timeDue.ToString();
        }
    }
}
