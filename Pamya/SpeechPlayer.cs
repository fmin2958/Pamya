using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.IO;
using System.Windows.Forms;


namespace Pamya
{
    static class SpeechPlayer
    {
        public static bool EspeakTTS(Word w, string lang, bool _generate_wav)
        {
            string espeak_binary_location = PamyaSettings.Instance.GetSetting("espeakbin");
            Dictionary<string, string> eo_string_replace_dict = new Dictionary<string, string>();
            eo_string_replace_dict.Add("Ĝ", "Gx");
            eo_string_replace_dict.Add("ĝ", "gx");
            eo_string_replace_dict.Add("Ĥ", "Hx");
            eo_string_replace_dict.Add("ĥ", "hx");
            eo_string_replace_dict.Add("Ĵ", "Jx");
            eo_string_replace_dict.Add("ĵ", "jx");
            eo_string_replace_dict.Add("Ŝ", "Sx");
            eo_string_replace_dict.Add("ŝ", "sx");
            eo_string_replace_dict.Add("Ĉ", "Cx");
            eo_string_replace_dict.Add("ĉ", "cx");
            eo_string_replace_dict.Add("Ŭ", "Ux");
            eo_string_replace_dict.Add("ŭ", "ux");
            eo_string_replace_dict.Add("-", "_");
            eo_string_replace_dict.Add(" ", "_");
            eo_string_replace_dict.Add("!", "_");
            eo_string_replace_dict.Add(",", "_");
            var text = eo_string_replace_dict.Aggregate(w.answer, (current, value) =>
                current.Replace(value.Key, value.Value));
            if (File.Exists(espeak_binary_location) && (! _generate_wav))
            {
                //ugly stuff for now
                


                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = espeak_binary_location;
                startInfo.Arguments = "-v " + lang + " \"" + text + "\"";
                process.StartInfo = startInfo;
                process.Start();
                return true;
            }
            else if (File.Exists(espeak_binary_location) && _generate_wav)
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = espeak_binary_location;
                startInfo.Arguments = "-w " + text + ".wav " + "-v " + lang + " \"" + text + "\"";
                startInfo.WorkingDirectory = PamyaDeck.Instance.CurrentDeckFolder;
                process.StartInfo = startInfo;
                process.Start();
                w.wav_file_loc = text + ".wav ";
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool WavFilePlayer(Word w)
        {
            var wav_file_loc = PamyaDeck.Instance.CurrentDeckFolder + @"\" + w.wav_file_loc;
            if (File.Exists(wav_file_loc))
            {
                SoundPlayer my_wave_file = new SoundPlayer(wav_file_loc);
                my_wave_file.Play();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SpeakWord(Word w)
        {
            if (!WavFilePlayer(w))
            {
                EspeakTTS(w, "eo", false);
            }
        }
    }
}
