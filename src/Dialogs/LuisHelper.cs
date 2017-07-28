using Microsoft.Cognitive.LUIS;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace XamUBot.Dialogs
{
	public static class LuisHelper
	{
		static LuisClient _luisClient;

        /// <summary>
        /// Static constructor to initialize our LUIS client.
        /// LuisClient is using HttpClient and that is meant to be used as a single instance, 
        /// so let's keep this static.
        /// </summary>
        static LuisHelper()
        {
            var appId = ConfigurationManager.AppSettings["LuisClientAppId"];
            var appKey = ConfigurationManager.AppSettings["LuisClientAppKey"];

            _luisClient = new LuisClient(appId, appKey);
        }

		/// <summary>
		/// Performs a call to LUIS to get back the best matching intent.
		/// See also: https://github.com/Microsoft/Cognitive-LUIS-Windows
		/// </summary>
		/// <param name="message">the message to pass to LUIS</param>
		/// <param name="intentPrefix">use to filter the intents</param>
		/// <param name="minimumScore">require the found itent to have a certain minimum score. If below threshold, NULL will be returned for the top scoring intent</param>
		/// <returns></returns>
		public static async Task<LuisResult> PredictLuisAsync(string message, string intentPrefix = null, float minimumScore = 0.6f)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return null;
			}

			LuisResult luisResult = null;

			try
			{
				luisResult = await _luisClient.Predict(message);
			}
			catch (Exception)
			{
				return null;
			}

			if (intentPrefix != null)
			{
				// Remove all intents not matching the prefix.
				luisResult.Intents = luisResult.Intents
					.Where(i => i.Name.StartsWith(intentPrefix, StringComparison.OrdinalIgnoreCase))
					.OrderByDescending(i => i.Score)
					.ToArray();
			}
			else
			{
				luisResult.Intents = luisResult.Intents
					.OrderByDescending(i => i.Score)
					.ToArray();
			}

			// Update the top scoring intent because we have potentially removed the original intent.
			// Also the top intent could be below our required minimum score.
			luisResult.TopScoringIntent = luisResult.Intents.FirstOrDefault(i => i.Score >= minimumScore);

			return luisResult;
		}
	}
}