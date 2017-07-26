using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Linq;
using System;

namespace XamUBot
{
	[BotAuthentication]
	[RoutePrefix("api/messages")]
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
		[HttpPost]
		[Route("")]
		public async Task<IHttpActionResult> HandleIncomingActivity([FromBody]Activity activity)
		{
			if(activity == null)
			{
				return BadRequest("No activity provided.");
			}

			// TODO: shows typing indicator even if the bot won't send an answer. The typing indicator should automagically appear if sending a reply takes longer.
            // show typing indicator - because bots type too :)
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity isTypingReply = activity.CreateReply();
                isTypingReply.Type = ActivityTypes.Typing;
                await connector.Conversations.ReplyToActivityAsync(isTypingReply);
            }

			/*
			if(activity.Type == ActivityTypes.ConversationUpdate)
			{
				IConversationUpdateActivity update = activity;
				
				// Remove bot from the members added
				update.MembersAdded = update.MembersAdded.Where(member => member.Id != update.Recipient.Id).ToList();

				if (update.MembersAdded.Count == 0)
				{
					return Ok();
				}
			}
			*/

			await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());

			return Ok();
		}
	}
}