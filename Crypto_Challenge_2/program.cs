using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class VigenereDecryptorWithNgrams
{
    public class NgramScore
    {
        private Dictionary<string, double> ngrams = new Dictionary<string, double>();
        private int ngramLength;
        private double floorValue;
        public Dictionary<string, double> Ngrams => ngrams;

        public NgramScore(string filePath)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(' ');
                ngrams[parts[0]] = double.Parse(parts[1]);
                ngramLength = parts[0].Length;
            }

            double totalNgrams = ngrams.Values.Sum();
            foreach (var key in ngrams.Keys.ToList())
            {
                ngrams[key] = Math.Log10(ngrams[key] / totalNgrams);
            }
            floorValue = Math.Log10(0.01 / totalNgrams);
        }

        public double Score(string text)
        {
            double score = 0;
            for (int i = 0; i <= text.Length - ngramLength; i++)
            {
                var ngram = text.Substring(i, ngramLength).ToUpper();
                score += ngrams.ContainsKey(ngram) ? ngrams[ngram] : floorValue;
            }
            return score;
        }
    }

    public static string DecryptWithKey(string text, string key)
    {
        return new string(text.Select((c, index) => (char)(((c - key[index % key.Length] + 26) % 26) + 'A')).ToArray());
    }

    private static NgramScore scorer = new NgramScore("english_trigrams.txt");

    public static string DetermineKey(string text, int keyLength)
    {
        string key = "";
        for (int i = 0; i < keyLength; i++)
        {
            string slice = new string(text.Where((c, index) => index % keyLength == i).ToArray());
            double maxScore = double.MinValue;
            char bestGuess = 'A';

            for (char guess = 'A'; guess <= 'Z'; guess++)
            {
                string decryptedSlice = new string(slice.Select(c => (char)(((c - guess + 26) % 26) + 'A')).ToArray());
                double currentScore = scorer.Score(decryptedSlice);
                if (currentScore > maxScore)
                {
                    maxScore = currentScore;
                    bestGuess = guess;
                }
            }

            key += bestGuess;
        }
        return key;
    }

    public static string ConvertText(string input)
    {
        string result = input;

        result = result.Replace("COMMA", ",");
        result = result.Replace("QUOTE", "\"");
        result = result.Replace("DOT", ".");
        result = result.Replace("QUESTIONMARK", "?");
        result = result.Replace("DASH", "-");
        result = result.Replace("COLON", ":");
        result = result.Replace("SLASH", "/");
        result = result.Replace("ONE", "1");
        result = result.Replace("TWO", "2");
        result = result.Replace("THREE", "3");
        result = result.Replace("FOUR", "4");
        result = result.Replace("FIVE", "5");
        result = result.Replace("SIX", "6");
        result = result.Replace("SEVEN", "7");
        result = result.Replace("EIGHT", "8");
        result = result.Replace("NINE", "9");
        result = result.Replace("TEN", "10");
        result = result.Replace("ZERO", "0");
        result = result.Replace("XX", "\n");

        return result;
    }


    public static void Main(string[] args)
    {
        string encryptedText = "RUPUEESVASFETXFRJOGZLFVJDITMUYPGATESEBBDPEDEOUTUEMODBTOSODPUTNILIOEOLTMADFWNISEUIEXINIHITHIBMPOSZISCPNMGSSSPNEQMODPGWKMSDNJRXSSDPUTNITEBSEPYTTFYASTMETDORSOIUDOAPEAMTOHITONFTNMOGNVCNFFTUFRJSUPSFPGVFTPFNZISTIFSIESYEPOXHPTRVOZIYXEBSNHBSIGUZYSANBITEIEBEITXIEQPLRWYXFYCKPMEOUWUVLDPUIZWFENTTNIWIHFNKVFOSBCRINIHITTSUPSPVOHFGPPDYIDUSJTEEGTFSARPEOUUOWYPTFDHGVMETCAHFBGFDOSQBTIFFOVTTLOOCRUOFWEXFSEBLTNIWIHFNKVFCJQHKVDOMPNURFOGUHKQPSUTITKVLBSCNESADUEXMTTJDSUJUHFBRZSGDFDIVLFRJOGOWUHFTTXSOGDPNBMDTJPNVSTSFTSKHCYFWEXCQESTOTGPMNBEBIOMPEEXEUEMZAIUVAJOTKHXIUIIZGPMNBTNEUHFJSGFMEUPCURTTSVCZEDIQIEXAIIDINUFPDZFLYIDAOEEIMQHFSDUXYXZPUNEWEQSOBIOTPCEGJPRNJDGFMEBODYLSEXECUHFBSFAQISIOEEKHEOUUHKRFXUDHGPMEOHEOWBKOPWTHBSIQLGMOTFYTGXUADLDUXZOVGITHBLMUHKGMUFTYUYOEFEFUVTOMWITKDRZQTUGIAMMETKFTISEKSOTIFFUPMOXJNMEEDSFSYGPLPOXDLUTQTCUPPNTMAYLTLBTHRYODHSETHPTQIDYPBSIJTYIWEOGIBIGOVSAYPBSIUWUDFRPUWUXIRFFSRETHUTLGWINJOETMOEFJGNXUHSFEJSUZJQ";  // Your encrypted text here
        
        int likelyKeyLength = 7;
        string key = DetermineKey(encryptedText, likelyKeyLength);
        string decryptedText = DecryptWithKey(encryptedText, key);

        Console.WriteLine($"Most likely key: {key}");
        Console.WriteLine($"{ConvertText(decryptedText)}");

        /*likelyKeyLength = 8;
        key = DetermineKey(encryptedText, likelyKeyLength);
        decryptedText = DecryptWithKey(encryptedText, key);

        Console.WriteLine($"Most likely key: {key}");
        Console.WriteLine($"{ConvertText(decryptedText)}");

        likelyKeyLength = 9;
        key = DetermineKey(encryptedText, likelyKeyLength);
        decryptedText = DecryptWithKey(encryptedText, key);

        Console.WriteLine($"Most likely key: {key}");
        Console.WriteLine($"{ConvertText(decryptedText)}");

        likelyKeyLength = 10;
        key = DetermineKey(encryptedText, likelyKeyLength);
        decryptedText = DecryptWithKey(encryptedText, key);

        Console.WriteLine($"Most likely key: {key}");
        Console.WriteLine($"{ConvertText(decryptedText)}");*/

    }
}
