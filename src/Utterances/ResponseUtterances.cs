using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace XamUBot
{
    public static class ResponseUtterances
    {
        public enum ReplyTypes
        {
            Welcome,
            Hello,
            Swear,
            Repeat,
            TeamWelcome,
            RootPrompt,
			SupportWelcome,
			RepetitiveAnswer,
			NotUnderstood,
		}

        private static Dictionary<string, List<string>> utterances = new Dictionary<string, List<string>>();
        private static Random rnd = new Random();
        static ResponseUtterances()
        {
            var censoredWords = new List<string>();
            var fileContents = System.IO.File.ReadAllLines(HostingEnvironment.MapPath(@"~/App_Data/ReplyUtterances.txt"));

            foreach (var item in fileContents)
            {
                var values = item.Split('|');
                var key = values[0];
                var value = values[1];

                if (!utterances.ContainsKey(key))
                    utterances.Add(key, new List<string>());

                utterances[key].Add(value);
            }
        }

        public static string GetResponse(ReplyTypes responseType)
        {
            return GetResponse(responseType.ToString());
        }

        public static string GetResponse(string responseType)
        {
            int index = rnd.Next(utterances[responseType].Count);
            string randomString = utterances[responseType][index];
            return randomString;
        }
    }
}