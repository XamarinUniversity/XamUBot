using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Cognitive.LUIS;

namespace XamUBot.Dialogs
{
	public static class DialogExtensions
	{
		/// <summary>
		/// Helper to get the actual value of an awaitable.
		/// Handles the TooManyAttemptsException used by picker dialogs.
		/// The method never fails and catches all exceptions. If the awaitable cannot be cast
		/// to T, the return value is "default(T)".
		/// </summary>
		/// <typeparam name="T">expected type</typeparam>
		/// <param name="awaitable">the awaitable</param>
		/// <returns>the value or null if value cannot be retrieved</returns>
		public async static Task<T> GetValueAsync<T>(this IAwaitable<object> awaitable)
		{
			try
			{
				var result = await awaitable;
				return (T) result;
			}
			catch (TooManyAttemptsException)
			{
				// Expected exception if user entered invalid answer too often.
			}
			catch(Exception ex)
			{
				Debug.WriteLine($"nameof(GetValueAsync) failed: {ex}.");
			}

			return default(T);
		}

		/// <summary>
		/// Attaches a hero-card to a reply.
		/// </summary>
		/// <param name="activity"></param>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="imageUrl"></param>
		/// <param name="subTitle"></param>
		/// <param name="buttons"></param>
		/// <returns></returns>
		public static HeroCard AttachHeroCard(this IMessageActivity activity, string title, string text, string imageUrl, string subTitle = null, params Tuple<string, string>[] buttons)
		{
			Debug.Assert(activity != null, "Activity is NULL - cannot attach hero card.");

			var heroCard = new HeroCard
			{
				Title = title,
				Subtitle = subTitle,
				Text = text
			};

			if(!string.IsNullOrWhiteSpace(imageUrl))
			{
				heroCard.Images = new[] { new CardImage(imageUrl) };
			}

			if(buttons?.Length > 0)
			{
				heroCard.Buttons = buttons.Select(b => new CardAction(ActionTypes.OpenUrl, b.Item1, b.Item2)).ToArray();
			}

			activity.Attachments.Add(heroCard.ToAttachment());
			
			return heroCard;
		}

		/// <summary>
		/// Extracts the best matching entity from a result.
		/// </summary>
		/// <param name="result"></param>
		/// <param name="entityName"></param>
		/// <returns></returns>
		public static Microsoft.Cognitive.LUIS.Entity GetBestMatchingEntity(this LuisResult result, string entityName)
		{
			if(result?.Entities == null)
			{
				return null;
			}

			var key = result.Entities.Keys.FirstOrDefault(k => k.ToLowerInvariant() == entityName.ToLowerInvariant());

			if(key == null)
			{
				return null;
			}

			IList<Microsoft.Cognitive.LUIS.Entity> entities = result.Entities[key];
			
			if(entities == null)
			{
				return null;
			}

			var bestEntity = entities
				.OrderByDescending(e => e.Score)
				.FirstOrDefault();

			if(bestEntity != null)
			{
				// All entities are returned as lowercase. To get the actual casing we must extract it from the original query.
				// See: https://github.com/Microsoft/BotBuilder/issues/963
				bestEntity.Name = result.OriginalQuery.Substring(bestEntity.StartIndex, bestEntity.EndIndex - bestEntity.StartIndex + 1);
			}

			return bestEntity;
		}
	}
}