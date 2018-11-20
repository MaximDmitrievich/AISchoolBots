using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using WeatherBot.Dialogs;

namespace WeatherBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => Conversation.Container.ResolveNamed<IDialog<object>>(nameof(RootDialog)));
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity updated = message as IConversationUpdateActivity;
                if (updated != null)
                {
                    foreach (var member in updated.MembersAdded ?? Array.Empty<ChannelAccount>())
                    {
                        if (member.Id == updated.Recipient.Id)
                        {
                            HeroCard card = new HeroCard
                            {
                                Title = "Погодный бот",
                                Subtitle = "Подскажу погоду в любой точке мира",
                                Text = "Просто напишите мне любой город, в котором вы хотите узнать погоду",
                            };
                            Activity responseMessage = message.CreateReply();
                            responseMessage.Attachments.Add(card.ToAttachment());
                            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                            await connector.Conversations.ReplyToActivityAsync(responseMessage);
                        }
                    }
                }
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
        }
    }
}