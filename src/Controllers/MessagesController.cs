using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace XamUBot
{
	[BotAuthentication]
	public class MessagesController : ApiController
	{
		// Information below is stored in web.config.
		// Name: XamUBot
		// App ID: 9b28fa4b-173d-43b7-9987-c69081d618cb
		// Password: mRjMmFfbH7Hc8ynQeZQ37u9

		// Azure: http://xamubot.azurewebsites.net
		// Webchat preview: https://webchat.botframework.com/embed/xamubot?s=tya7B3F_yuE.cwA.tvs.A-sDDf4THvHgmlVZRZh1oBuBVkAvCDcobHRD5eEEZvw


		/// <summary>
		/// POST: api/Messages
		/// Receive a message from a user and reply to it
		/// </summary>
		public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
		{
			if (activity.Type == ActivityTypes.Message)
			{
				await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
			}
			else
			{
				HandleSystemMessage(activity);
			}
			var response = Request.CreateResponse(HttpStatusCode.OK);
			return response;
		}

		private Activity HandleSystemMessage(Activity message)
		{
			if (message.Type == ActivityTypes.DeleteUserData)
			{
				// Implement user deletion here
				// If we handle user deletion, return a real message
			}
			else if (message.Type == ActivityTypes.ConversationUpdate)
			{
				// Handle conversation state changes, like members being added and removed
				// Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
				// Not available in all channels
			}
			else if (message.Type == ActivityTypes.ContactRelationUpdate)
			{
				// Handle add/remove from contact lists
				// Activity.From + Activity.Action represent what happened
			}
			else if (message.Type == ActivityTypes.Typing)
			{
				// Handle knowing tha the user is typing
			}
			else if (message.Type == ActivityTypes.Ping)
			{
			}

			return null;
		}
	}
}