using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using XamUApi;
using Microsoft.Cognitive.LUIS;

namespace XamUBot.Dialogs
{
	public static class DialogExtensions
	{
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

			IList<Microsoft.Cognitive.LUIS.Entity> entities = null;
			result.Entities.TryGetValue(entityName.ToLowerInvariant(), out entities);

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