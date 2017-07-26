using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace XamUBot.Utternaces
{

    public static class Keywords
    {
        public const string Exit = "exit";
        public const string Leave = "leave";
        public const string Help = "help";
        public const string Support = "support";
        public const string Swear = "swear";

        static Censored.Censor _censor;

        static Keywords()
        {
            // load the list of censored words
            // obtained via: http://www.bannedwordlist.com/
            // I think most Australians could come up with a better list
            // off the top of their heads
            _censor = new Censored.Censor(GetNaughtyWordList());
        }

        private static List<string> GetNaughtyWordList()
        {
            var censoredWords = new List<string>();
            var fileContents = System.IO.File.ReadAllLines(HostingEnvironment.MapPath(@"~/App_Data/Naughty.txt"));
            censoredWords.AddRange(fileContents);
            return censoredWords;
        }


        public static bool IsHelpKeyword(string keyword)
        {
            return (keyword.Contains(Help) || keyword.Contains(Support));
        }

        public static bool IsExitKeyword(string keyword)
        {
            return (keyword.Contains(Exit) || keyword.Contains(Leave));
        }

        public static bool IsSwearWord(string keyword)
        {
            return (_censor.HasCensoredWord(keyword));
        }
    }
}