using System.Collections.Generic;
using System.Web.Hosting;

namespace XamUBot.Utterances
{
    /// <summary>
    /// Keywords for the bot.
    /// </summary>
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


        public static bool IsHelpKeyword(string keyword) => keyword?.ToLowerInvariant() == Help;// || keyword?.ToLowerInvariant() == Support;

        public static bool IsExitKeyword(string keyword) => keyword?.ToLowerInvariant() == Exit || keyword?.ToLowerInvariant() == Leave;

        public static bool IsSwearWord(string keyword) => _censor.HasCensoredWord(keyword);
    }
}