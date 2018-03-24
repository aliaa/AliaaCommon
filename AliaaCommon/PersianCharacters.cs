using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AliaaCommon
{
    public static class PersianCharacters
    {
        private const char ALEF_AKOLAD = '\u0622';
        private const char ALEF = '\u0627';
        private const char BE = '\u0628';
        private const char PE = '\u067e';
        private const char TE = '\u062a';
        private const char SE = '\u062b';
        private const char JIM = '\u062c';
        private const char CHE = '\u0686';
        private const char HE_JIMI = '\u062d';
        private const char KHE = '\u062e';
        private const char DAL = '\u062f';
        private const char ZAL = '\u0630';
        private const char RE = '\u0631';
        private const char ZE = '\u0632';
        private const char ZHE = '\u0698';
        private const char SIN = '\u0633';
        private const char SHIN = '\u0634';
        private const char SAD = '\u0635';
        private const char ZAD = '\u0636';
        private const char TA = '\u0637';
        private const char ZA = '\u0638';
        private const char EYN = '\u0639';
        private const char GHEYN = '\u063a';
        private const char FE = '\u0641';
        private const char QAF = '\u0642';
        private const char KAF = '\u06a9';
        private const char GAF = '\u06af';
        private const char LAM = '\u0644';
        private const char MIM = '\u0645';
        private const char NUN = '\u0646';
        private const char VAV = '\u0648';
        private const char HE = '\u0647';
        private const char YE = '\u06cc';
        private const char HAMZE = '\u0626';

        private const char N0 = '\u06f0';
        private const char N1 = '\u06f1';
        private const char N2 = '\u06f2';
        private const char N3 = '\u06f3';
        private const char N4 = '\u06f4';
        private const char N5 = '\u06f5';
        private const char N6 = '\u06f6';
        private const char N7 = '\u06f7';
        private const char N8 = '\u06f8';
        private const char N9 = '\u06f9';

        private const char SPACE = ' ';

        private static readonly List<char> PRIMARY_CHARS, IGNORED_CHARS;
        private static readonly Dictionary<char, char> CHAR_MAPPING;
        private static readonly Dictionary<char, char> NUMBERS_MAPPING;
        private static readonly Dictionary<char, char> NUMBERS_MAPPING_REVERSE;

        static PersianCharacters()
        {
            PRIMARY_CHARS = new List<char>();
            PRIMARY_CHARS.Add(ALEF_AKOLAD);
            PRIMARY_CHARS.Add(ALEF);
            PRIMARY_CHARS.Add(BE);
            PRIMARY_CHARS.Add(PE);
            PRIMARY_CHARS.Add(TE);
            PRIMARY_CHARS.Add(SE);
            PRIMARY_CHARS.Add(JIM);
            PRIMARY_CHARS.Add(CHE);
            PRIMARY_CHARS.Add(HE_JIMI);
            PRIMARY_CHARS.Add(KHE);
            PRIMARY_CHARS.Add(DAL);
            PRIMARY_CHARS.Add(ZAL);
            PRIMARY_CHARS.Add(RE);
            PRIMARY_CHARS.Add(ZE);
            PRIMARY_CHARS.Add(ZHE);
            PRIMARY_CHARS.Add(SIN);
            PRIMARY_CHARS.Add(SHIN);
            PRIMARY_CHARS.Add(SAD);
            PRIMARY_CHARS.Add(ZAD);
            PRIMARY_CHARS.Add(TA);
            PRIMARY_CHARS.Add(ZA);
            PRIMARY_CHARS.Add(EYN);
            PRIMARY_CHARS.Add(GHEYN);
            PRIMARY_CHARS.Add(FE);
            PRIMARY_CHARS.Add(QAF);
            PRIMARY_CHARS.Add(KAF);
            PRIMARY_CHARS.Add(GAF);
            PRIMARY_CHARS.Add(LAM);
            PRIMARY_CHARS.Add(MIM);
            PRIMARY_CHARS.Add(NUN);
            PRIMARY_CHARS.Add(VAV);
            PRIMARY_CHARS.Add(HE);
            PRIMARY_CHARS.Add(YE);
            PRIMARY_CHARS.Add(N0);
            PRIMARY_CHARS.Add(N1);
            PRIMARY_CHARS.Add(N2);
            PRIMARY_CHARS.Add(N3);
            PRIMARY_CHARS.Add(N4);
            PRIMARY_CHARS.Add(N5);
            PRIMARY_CHARS.Add(N6);
            PRIMARY_CHARS.Add(N7);
            PRIMARY_CHARS.Add(N8);
            PRIMARY_CHARS.Add(N9);
            PRIMARY_CHARS.Add(SPACE);


            IGNORED_CHARS = new List<char>();
            IGNORED_CHARS.Add('\u064b');
            IGNORED_CHARS.Add('\u064c');
            IGNORED_CHARS.Add('\u064d');
            IGNORED_CHARS.Add('\u064e');
            IGNORED_CHARS.Add('\u064f');
            IGNORED_CHARS.Add('\u0650');
            IGNORED_CHARS.Add('\u0651');
            IGNORED_CHARS.Add('\u0640');


            CHAR_MAPPING = new Dictionary<char, char>();

            CHAR_MAPPING.Add('\u0623', ALEF);
            CHAR_MAPPING.Add('\u0624', VAV);
            CHAR_MAPPING.Add('\u0625', ALEF);
            CHAR_MAPPING.Add('\u0626', HAMZE);
            CHAR_MAPPING.Add('\u0629', HE);
            CHAR_MAPPING.Add('\u0643', KAF);
            CHAR_MAPPING.Add('\u0649', YE);
            CHAR_MAPPING.Add('\u064a', YE);
            CHAR_MAPPING.Add('\u0660', N0);
            CHAR_MAPPING.Add('\u0661', N1);
            CHAR_MAPPING.Add('\u0662', N2);
            CHAR_MAPPING.Add('\u0663', N3);
            CHAR_MAPPING.Add('\u0664', N4);
            CHAR_MAPPING.Add('\u0665', N5);
            CHAR_MAPPING.Add('\u0666', N6);
            CHAR_MAPPING.Add('\u0667', N7);
            CHAR_MAPPING.Add('\u0668', N8);
            CHAR_MAPPING.Add('\u0669', N9);
            CHAR_MAPPING.Add('\u0671', ALEF);
            CHAR_MAPPING.Add('\u0672', ALEF);
            CHAR_MAPPING.Add('\u0673', ALEF);
            CHAR_MAPPING.Add('\u0675', ALEF);
            CHAR_MAPPING.Add('\u0676', VAV);
            CHAR_MAPPING.Add('\u0677', VAV);
            CHAR_MAPPING.Add('\u0678', YE);
            CHAR_MAPPING.Add('\u067b', BE);
            CHAR_MAPPING.Add('\u067c', TE);
            CHAR_MAPPING.Add('\u0680', BE);
            CHAR_MAPPING.Add('\u0681', HE_JIMI);
            CHAR_MAPPING.Add('\u0687', CHE);
            CHAR_MAPPING.Add('\u068a', DAL);
            CHAR_MAPPING.Add('\u0692', RE);
            CHAR_MAPPING.Add('\u0693', RE);
            CHAR_MAPPING.Add('\u0694', RE);
            CHAR_MAPPING.Add('\u0695', RE);
            CHAR_MAPPING.Add('\u06ab', KAF);
            CHAR_MAPPING.Add('\u06b9', NUN);
            CHAR_MAPPING.Add('\u06ba', NUN);
            CHAR_MAPPING.Add('\u06bb', NUN);
            CHAR_MAPPING.Add('\u06bc', NUN);
            CHAR_MAPPING.Add('\u06bd', NUN);
            CHAR_MAPPING.Add('\u06be', HE);
            CHAR_MAPPING.Add('\u06c1', HE);
            CHAR_MAPPING.Add('\u06c2', HE);
            CHAR_MAPPING.Add('\u06c3', HE);
            CHAR_MAPPING.Add('\u06c4', VAV);
            CHAR_MAPPING.Add('\u06c5', VAV);
            CHAR_MAPPING.Add('\u06c6', VAV);
            CHAR_MAPPING.Add('\u06c7', VAV);
            CHAR_MAPPING.Add('\u06c8', VAV);
            CHAR_MAPPING.Add('\u06c9', VAV);
            CHAR_MAPPING.Add('\u06ca', VAV);
            CHAR_MAPPING.Add('\u06cb', VAV);
            CHAR_MAPPING.Add('\u06cc', YE);
            CHAR_MAPPING.Add('\u06cd', YE);
            CHAR_MAPPING.Add('\u06ce', YE);
            CHAR_MAPPING.Add('\u06cf', VAV);
            CHAR_MAPPING.Add('\u06d0', YE);
            CHAR_MAPPING.Add('\u06d1', YE);
            CHAR_MAPPING.Add('\u06d2', YE);
            CHAR_MAPPING.Add('\u06d3', YE);
            CHAR_MAPPING.Add('\u06d5', HE);

            CHAR_MAPPING.Add('\ufb50', ALEF);
            CHAR_MAPPING.Add('\ufb51', ALEF);
            CHAR_MAPPING.Add('\ufb52', BE);
            CHAR_MAPPING.Add('\ufb53', BE);
            CHAR_MAPPING.Add('\ufb54', BE);
            CHAR_MAPPING.Add('\ufb55', BE);
            CHAR_MAPPING.Add('\ufb56', PE);
            CHAR_MAPPING.Add('\ufb57', PE);
            CHAR_MAPPING.Add('\ufb58', PE);
            CHAR_MAPPING.Add('\ufb59', PE);
            CHAR_MAPPING.Add('\ufb5a', BE);
            CHAR_MAPPING.Add('\ufb5b', BE);
            CHAR_MAPPING.Add('\ufb5c', BE);
            CHAR_MAPPING.Add('\ufb5d', BE);
            CHAR_MAPPING.Add('\ufb5e', TE);
            CHAR_MAPPING.Add('\ufb5f', TE);
            CHAR_MAPPING.Add('\ufb60', TE);
            CHAR_MAPPING.Add('\ufb61', TE);
            CHAR_MAPPING.Add('\ufb62', TE);
            CHAR_MAPPING.Add('\ufb63', TE);
            CHAR_MAPPING.Add('\ufb64', TE);
            CHAR_MAPPING.Add('\ufb65', TE);
            CHAR_MAPPING.Add('\ufb66', TE);
            CHAR_MAPPING.Add('\ufb67', TE);
            CHAR_MAPPING.Add('\ufb68', TE);
            CHAR_MAPPING.Add('\ufb69', TE);
            CHAR_MAPPING.Add('\ufb7a', CHE);
            CHAR_MAPPING.Add('\ufb7b', CHE);
            CHAR_MAPPING.Add('\ufb7c', CHE);
            CHAR_MAPPING.Add('\ufb7d', CHE);
            CHAR_MAPPING.Add('\ufb7e', CHE);
            CHAR_MAPPING.Add('\ufb7f', CHE);
            CHAR_MAPPING.Add('\ufb80', CHE);
            CHAR_MAPPING.Add('\ufb81', CHE);
            CHAR_MAPPING.Add('\ufb8a', ZHE);
            CHAR_MAPPING.Add('\ufb8b', ZHE);
            CHAR_MAPPING.Add('\ufb8e', KAF);
            CHAR_MAPPING.Add('\ufb8f', KAF);
            CHAR_MAPPING.Add('\ufb90', KAF);
            CHAR_MAPPING.Add('\ufb91', KAF);
            CHAR_MAPPING.Add('\ufb92', GAF);
            CHAR_MAPPING.Add('\ufb93', GAF);
            CHAR_MAPPING.Add('\ufb94', GAF);
            CHAR_MAPPING.Add('\ufb95', GAF);
            CHAR_MAPPING.Add('\ufba4', YE);
            CHAR_MAPPING.Add('\ufba5', YE);
            CHAR_MAPPING.Add('\ufbd7', VAV);
            CHAR_MAPPING.Add('\ufbd8', VAV);
            CHAR_MAPPING.Add('\ufbd9', VAV);
            CHAR_MAPPING.Add('\ufbda', VAV);
            CHAR_MAPPING.Add('\ufbdb', VAV);
            CHAR_MAPPING.Add('\ufbdc', VAV);
            CHAR_MAPPING.Add('\ufbdd', VAV);
            CHAR_MAPPING.Add('\ufbde', VAV);
            CHAR_MAPPING.Add('\ufbdf', VAV);
            CHAR_MAPPING.Add('\ufbe0', VAV);
            CHAR_MAPPING.Add('\ufbe1', VAV);
            CHAR_MAPPING.Add('\ufbe2', VAV);
            CHAR_MAPPING.Add('\ufbe3', VAV);
            CHAR_MAPPING.Add('\ufbe4', YE);
            CHAR_MAPPING.Add('\ufbe5', YE);
            CHAR_MAPPING.Add('\ufbe6', YE);
            CHAR_MAPPING.Add('\ufbe7', YE);
            CHAR_MAPPING.Add('\ufbfc', YE);
            CHAR_MAPPING.Add('\ufbfd', YE);
            CHAR_MAPPING.Add('\ufbfe', YE);
            CHAR_MAPPING.Add('\ufbff', YE);

            CHAR_MAPPING.Add('\ufe81', ALEF_AKOLAD);
            CHAR_MAPPING.Add('\ufe82', ALEF_AKOLAD);
            CHAR_MAPPING.Add('\ufe83', ALEF);
            CHAR_MAPPING.Add('\ufe84', ALEF);
            CHAR_MAPPING.Add('\ufe85', VAV);
            CHAR_MAPPING.Add('\ufe86', VAV);
            CHAR_MAPPING.Add('\ufe87', ALEF);
            CHAR_MAPPING.Add('\ufe88', ALEF);
            CHAR_MAPPING.Add('\ufe89', YE);
            CHAR_MAPPING.Add('\ufe8a', YE);
            CHAR_MAPPING.Add('\ufe8d', ALEF);
            CHAR_MAPPING.Add('\ufe8e', ALEF);
            CHAR_MAPPING.Add('\ufe8f', BE);
            CHAR_MAPPING.Add('\ufe90', BE);
            CHAR_MAPPING.Add('\ufe91', BE);
            CHAR_MAPPING.Add('\ufe92', BE);
            CHAR_MAPPING.Add('\ufe95', TE);
            CHAR_MAPPING.Add('\ufe96', TE);
            CHAR_MAPPING.Add('\ufe97', TE);
            CHAR_MAPPING.Add('\ufe98', TE);
            CHAR_MAPPING.Add('\ufe99', SE);
            CHAR_MAPPING.Add('\ufe9a', SE);
            CHAR_MAPPING.Add('\ufe9b', SE);
            CHAR_MAPPING.Add('\ufe9c', SE);
            CHAR_MAPPING.Add('\ufe9d', JIM);
            CHAR_MAPPING.Add('\ufe9e', JIM);
            CHAR_MAPPING.Add('\ufe9f', JIM);
            CHAR_MAPPING.Add('\ufea0', JIM);
            CHAR_MAPPING.Add('\ufea1', HE_JIMI);
            CHAR_MAPPING.Add('\ufea2', HE_JIMI);
            CHAR_MAPPING.Add('\ufea3', HE_JIMI);
            CHAR_MAPPING.Add('\ufea4', HE_JIMI);
            CHAR_MAPPING.Add('\ufea5', KHE);
            CHAR_MAPPING.Add('\ufea6', KHE);
            CHAR_MAPPING.Add('\ufea7', KHE);
            CHAR_MAPPING.Add('\ufea8', KHE);
            CHAR_MAPPING.Add('\ufea9', DAL);
            CHAR_MAPPING.Add('\ufeaa', DAL);
            CHAR_MAPPING.Add('\ufeab', ZAL);
            CHAR_MAPPING.Add('\ufeac', ZAL);
            CHAR_MAPPING.Add('\ufead', RE);
            CHAR_MAPPING.Add('\ufeae', RE);
            CHAR_MAPPING.Add('\ufeaf', ZE);
            CHAR_MAPPING.Add('\ufeb0', ZE);
            CHAR_MAPPING.Add('\ufeb1', SIN);
            CHAR_MAPPING.Add('\ufeb2', SIN);
            CHAR_MAPPING.Add('\ufeb3', SIN);
            CHAR_MAPPING.Add('\ufeb4', SIN);
            CHAR_MAPPING.Add('\ufeb5', SHIN);
            CHAR_MAPPING.Add('\ufeb6', SHIN);
            CHAR_MAPPING.Add('\ufeb7', SHIN);
            CHAR_MAPPING.Add('\ufeb8', SHIN);
            CHAR_MAPPING.Add('\ufeb9', SAD);
            CHAR_MAPPING.Add('\ufeba', SAD);
            CHAR_MAPPING.Add('\ufebb', SAD);
            CHAR_MAPPING.Add('\ufebc', SAD);
            CHAR_MAPPING.Add('\ufebd', ZAD);
            CHAR_MAPPING.Add('\ufebe', ZAD);
            CHAR_MAPPING.Add('\ufebf', ZAD);
            CHAR_MAPPING.Add('\ufec0', ZAD);
            CHAR_MAPPING.Add('\ufec1', TA);
            CHAR_MAPPING.Add('\ufec2', TA);
            CHAR_MAPPING.Add('\ufec3', TA);
            CHAR_MAPPING.Add('\ufec4', TA);
            CHAR_MAPPING.Add('\ufec5', ZA);
            CHAR_MAPPING.Add('\ufec6', ZA);
            CHAR_MAPPING.Add('\ufec7', ZA);
            CHAR_MAPPING.Add('\ufec8', ZA);
            CHAR_MAPPING.Add('\ufec9', EYN);
            CHAR_MAPPING.Add('\ufeca', EYN);
            CHAR_MAPPING.Add('\ufecb', EYN);
            CHAR_MAPPING.Add('\ufecc', EYN);
            CHAR_MAPPING.Add('\ufecd', GHEYN);
            CHAR_MAPPING.Add('\ufece', GHEYN);
            CHAR_MAPPING.Add('\ufecf', GHEYN);
            CHAR_MAPPING.Add('\ufed0', GHEYN);
            CHAR_MAPPING.Add('\ufed1', FE);
            CHAR_MAPPING.Add('\ufed2', FE);
            CHAR_MAPPING.Add('\ufed3', FE);
            CHAR_MAPPING.Add('\ufed4', FE);
            CHAR_MAPPING.Add('\ufed5', QAF);
            CHAR_MAPPING.Add('\ufed6', QAF);
            CHAR_MAPPING.Add('\ufed7', QAF);
            CHAR_MAPPING.Add('\ufed8', QAF);
            CHAR_MAPPING.Add('\ufed9', KAF);
            CHAR_MAPPING.Add('\ufeda', KAF);
            CHAR_MAPPING.Add('\ufedb', KAF);
            CHAR_MAPPING.Add('\ufedc', KAF);
            CHAR_MAPPING.Add('\ufedd', LAM);
            CHAR_MAPPING.Add('\ufede', LAM);
            CHAR_MAPPING.Add('\ufedf', LAM);
            CHAR_MAPPING.Add('\ufee0', LAM);
            CHAR_MAPPING.Add('\ufee1', MIM);
            CHAR_MAPPING.Add('\ufee2', MIM);
            CHAR_MAPPING.Add('\ufee3', MIM);
            CHAR_MAPPING.Add('\ufee4', MIM);
            CHAR_MAPPING.Add('\ufee5', NUN);
            CHAR_MAPPING.Add('\ufee6', NUN);
            CHAR_MAPPING.Add('\ufee7', NUN);
            CHAR_MAPPING.Add('\ufee8', NUN);
            CHAR_MAPPING.Add('\ufee9', HE);
            CHAR_MAPPING.Add('\ufeea', HE);
            CHAR_MAPPING.Add('\ufeeb', HE);
            CHAR_MAPPING.Add('\ufeec', HE);
            CHAR_MAPPING.Add('\ufeed', VAV);
            CHAR_MAPPING.Add('\ufeee', VAV);
            CHAR_MAPPING.Add('\ufeef', YE);
            CHAR_MAPPING.Add('\ufef0', YE);
            CHAR_MAPPING.Add('\ufef1', YE);
            CHAR_MAPPING.Add('\ufef2', YE);
            CHAR_MAPPING.Add('\ufef3', YE);
            CHAR_MAPPING.Add('\ufef4', YE);

            CHAR_MAPPING.Add('\uc2a0', SPACE);

            NUMBERS_MAPPING = new Dictionary<char, char>();

            NUMBERS_MAPPING.Add('\u0030', N0);
            NUMBERS_MAPPING.Add('\u0031', N1);
            NUMBERS_MAPPING.Add('\u0032', N2);
            NUMBERS_MAPPING.Add('\u0033', N3);
            NUMBERS_MAPPING.Add('\u0034', N4);
            NUMBERS_MAPPING.Add('\u0035', N5);
            NUMBERS_MAPPING.Add('\u0036', N6);
            NUMBERS_MAPPING.Add('\u0037', N7);
            NUMBERS_MAPPING.Add('\u0038', N8);
            NUMBERS_MAPPING.Add('\u0039', N9);

            NUMBERS_MAPPING_REVERSE = new Dictionary<char, char>();

            NUMBERS_MAPPING_REVERSE.Add(N0, '\u0030');
            NUMBERS_MAPPING_REVERSE.Add(N1, '\u0031');
            NUMBERS_MAPPING_REVERSE.Add(N2, '\u0032');
            NUMBERS_MAPPING_REVERSE.Add(N3, '\u0033');
            NUMBERS_MAPPING_REVERSE.Add(N4, '\u0034');
            NUMBERS_MAPPING_REVERSE.Add(N5, '\u0035');
            NUMBERS_MAPPING_REVERSE.Add(N6, '\u0036');
            NUMBERS_MAPPING_REVERSE.Add(N7, '\u0037');
            NUMBERS_MAPPING_REVERSE.Add(N8, '\u0038');
            NUMBERS_MAPPING_REVERSE.Add(N9, '\u0039');
        }

        public static char UnifyCharacter(char ch, bool farsiNumbers)
        {
            if (CHAR_MAPPING.ContainsKey(ch))
                return CHAR_MAPPING[ch];
            if (farsiNumbers)
            {
                if(NUMBERS_MAPPING.ContainsKey(ch))
                    return NUMBERS_MAPPING[ch];
            }
            else
            {
                if(NUMBERS_MAPPING_REVERSE.ContainsKey(ch))
                    return NUMBERS_MAPPING_REVERSE[ch];
            }
            return ch;
        }

        public static string UnifyString(string str, bool farsiNumbers)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in str)
            {
                if (!IGNORED_CHARS.Contains(ch))
                    sb.Append(UnifyCharacter(ch, farsiNumbers));
            }
            return sb.ToString();
        }

        public static char ConvertPersianDigitToEnglish(char ch)
        {
            if (NUMBERS_MAPPING_REVERSE.ContainsKey(ch))
                return NUMBERS_MAPPING_REVERSE[ch];
            return ch;
        }

        public static string ConvertPersianDigitToEnglish(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            StringBuilder sb = new StringBuilder();
            foreach (char ch in str)
            {
                sb.Append(ConvertPersianDigitToEnglish(ch));
            }
            return sb.ToString();
        }
    }
}