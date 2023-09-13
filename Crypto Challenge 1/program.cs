using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AnnealDecrypt
{ 
    public class NgramScore
    {
        // Dictionary to hold the ngram frequencies
        private Dictionary<string, double> ngrams = new Dictionary<string, double>();

        // Length of the ngrams being used (e.g., trigrams will have a length of 3)
        private int ngramLength;

        // Total number of ngrams in the reference file
        private double totalNgrams;

        // A floor value used for ngrams not found in the reference file
        private double floorValue;

        // Constructor that initializes the NgramScore with a reference ngram frequency file
        public NgramScore(string filePath)
        {
            // Parse the reference file and populate the ngram dictionary
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(' ');
                ngrams[parts[0]] = double.Parse(parts[1]);
                ngramLength = parts[0].Length;
            }

            // Calculate the total number of ngrams
            totalNgrams = ngrams.Values.Sum();

            // Convert raw frequencies to logarithmic probabilities
            foreach (var key in ngrams.Keys.ToList())
            {
                ngrams[key] = Math.Log10(ngrams[key] / totalNgrams);
            }

            // Set the floor value for ngrams not in the reference file
            floorValue = Math.Log10(0.01 / totalNgrams);
        }

        // Method to compute the score of a given text based on ngram probabilities
        public double Score(string text)
        {
            double score = 0;

            // Iterate over the text and compute the ngram score
            for (int i = 0; i <= text.Length - ngramLength; i++)
            {
                var ngram = text.Substring(i, ngramLength).ToUpper();
                score += ngrams.ContainsKey(ngram) ? ngrams[ngram] : floorValue;
            }

            return score;
        }
    }

    public class SubstitutionCipher
    {
        private static string cryptedText1 = "DFRJX MJ MB YXIJHMTVN IHJOXI H YFIMRFB GIRSFYJMRTYRLLH DFRJX BHMS\n" +
                                         "ORVLXBSRJ DFRJX HJ PMIBJ BMZOJ MJ ERFVS HGGXHI JR QX BRLX YOMVSMBO\n" +
                                         "GIHTUSRJ MJ YRTBMBJB RP H TFLQXI RP HQBFIS VMJJVX PMZFIXB SHTYMTZ\n" +
                                         "HYIRBB JOX GHGXI FGRT EOMYO JOXN HIX SIHETSRJ EON BORFVS NRF HJJIMQFJX\n" +
                                         "HTN MLGRIJHTYX JR BR ZIRJXBDFX HT RQKXYJ DFXBJMRTLHIU DFRJX SHBOSHBO\n" +
                                         "BOXIVRYU ORVLXB MT HSAXTJFIX RP JOX SHTYMTZ LXTYRTZIHJFVHJMRTBYRLLH\n" +
                                         "NRF OHAX BRVAXS JOX PMIBJ YINGJR YOHVVXTZXSRJ JOX TXWJ YOHVVXTZX MB\n" +
                                         "H YORBXTSHBOGVHMTJXWJ HJJHYUSRJ NRF PMTS HVV JOX YVFXB NRF TXXS PRI\n" +
                                         "BRVAMTZ YINGJR YOHVVXTZX JER RT JOX PRVVREMTZ HSSIXBBYRVRT\n" +
                                         "OJJGBYRVRTBVHBOBVHBOVFTSZIXTSRJGOSBVHBOMJBXAXTPMAXPRFIHBVHBOJERCXIRJERJOIXXBVHBOXBVHBOBMWBXAXTBMWJOIXXSRJCMG";

        // Split the encrypted text into lines for preserving the structure during decryption.
        private static List<string> lines = cryptedText1.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        // Extract unique characters from the encrypted text to form the key.
        private static List<char> uniqueChars = new HashSet<char>(cryptedText1.Where(c => !char.IsWhiteSpace(c))).ToList();

        // Initial key, this will be shuffled during the annealing process.
        private static string key = "abcdefghijklmnopqrstuwvxyz";

        // Ngram scorer to evaluate the quality of decrypted text.
        //n-gram list: https://raw.githubusercontent.com/ggerganov/kbd-audio/master/data/english_trigrams.txt
        private static NgramScore scorer = new NgramScore("english_trigrams.txt");

        // Evaluate the quality of decrypted text with a given key.
        private static double GetScore(string potentialKey)
        {
            double score = 0;

            // Decrypt each line separately and calculate its score.
            foreach (var line in lines)
            {
                var decrypted = string.Concat(line.Select(c =>
                {
                    int index = uniqueChars.IndexOf(c);
                    return index >= 0 ? potentialKey[index] : c; // Preserve spaces and new lines
                }));
                score += scorer.Score(decrypted);
            }

            return score;
        }

        // Shuffle the key to produce a new potential solution.
        private static string ShuffleKey(string currentKey)
        {
            var chars = currentKey.ToCharArray();
            Random rand = new Random();

            int index1 = rand.Next(chars.Length);
            int index2 = rand.Next(chars.Length);

            // Swap two random characters in the key.
            var temp = chars[index1];
            chars[index1] = chars[index2];
            chars[index2] = temp;

            return new string(chars);
        }

        public static void ToString()
        {
            double currentScore = -1000000;
            string result = "";

            //best result for Crypto Challenge 1
            double idealScore = -5231;

            // Simulated annealing loop.
            while (true)
            {
                var newKey = ShuffleKey(key);
                double newScore = GetScore(newKey);

                // If the new key provides a better score, update the current key and score.
                if (newScore > currentScore)
                {
                    key = newKey;
                    currentScore = newScore;

                    // Decrypt each line separately to preserve the structure.
                    var decryptedLines = lines.Select(line => string.Concat(line.Select(c =>
                    {
                        int index = uniqueChars.IndexOf(c);
                        return index >= 0 ? key[index] : c;// Preserve spaces and new lines
                    })))
                    .ToList();

                    result = string.Join(Environment.NewLine, decryptedLines).ToUpper();

                    Console.WriteLine($"Test: {result}");
                    Console.WriteLine($"Score: {currentScore}");
                    Console.WriteLine($"Key: {key}");

                    if (currentScore > idealScore)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine($"");
            Console.WriteLine($"Decrpted Text:");
            Console.WriteLine($"{result}");

            Console.WriteLine($"");
            Console.WriteLine($"Clear Text:");
            Console.WriteLine($"{ConvertText(result)}");

            Console.ReadLine();
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

            return result;
        }
    }

    public static void Main(string[] args)
    {
        SubstitutionCipher.ToString(); 
    } 
}
