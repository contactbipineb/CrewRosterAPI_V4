// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.10.3

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using Airlines.XAirlines.Helpers;
using Microsoft.Bot.Builder.Teams;

namespace XAirlinesUpdate.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        protected readonly Dialog dialog;
        protected readonly BotState conversationState;

        public DialogBot(ConversationState conversationState, T dialog)
        {
            this.conversationState = conversationState;
            this.dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            // await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
       {

            await this.SendTypingIndicatorAsync(turnContext);
            await dialog.RunAsync(turnContext, conversationState.CreateProperty<DialogState>("DialogState"),cancellationToken);
            // await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

        }

        protected override Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return base.OnInvokeActivityAsync(turnContext, cancellationToken);
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            // ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            var channelData = activity.GetChannelData<TeamsChannelData>();
            // Treat 1:1 add/remove events as if they were add/remove of a team member
            if (channelData.EventType == null)
            {
                if (activity.MembersAdded != null)
                    channelData.EventType = "teamMemberAdded";
                if (activity.MembersRemoved != null)
                    channelData.EventType = "teamMemberRemoved";
            }
            switch (channelData.EventType)
            {
                case "teamMemberAdded":
                    // Team member was added (user or bot)
                    if (activity.MembersAdded.Any(m => m.Id.Contains(activity.Recipient.Id)))
                    {
                        // Bot was added to a team: send welcome message
                        // activity.Text = "hi";
                        // await Conversation.SendAsync(activity, () => new RootDialog());
                        await this.SendTypingIndicatorAsync(turnContext);
                        await dialog.RunAsync(turnContext, conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                        var userDetails = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id);
                        var card = CardHelper.GetWelcomeScreen(userDetails.GivenName ?? userDetails.Name);
                    }
                    break;
                case "teamMemberRemoved":
                    // Add team & channel details 
                    if (activity.MembersRemoved.Any(m => m.Id.Contains(activity.Recipient.Id)))
                    {
                        // Bot was removed from a team: remove entry for the team in the database
                    }
                    else
                    {
                        // Member was removed from a team: update the team member  count
                    }
                    break;
                // Update the team and channel info in the database when the team is rename or when channel are added/removed/renamed
                case "teamRenamed":
                    // Rename team & channel details 
                    break;
                case "channelCreated":
                    break;
                case "channelRenamed":
                    break;
                case "channelDeleted":
                    break;
                default:
                    break;
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        private Task SendTypingIndicatorAsync(ITurnContext turnContext)
        {
            var typingActivity = turnContext.Activity.CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            return turnContext.SendActivityAsync(typingActivity);
        }
    }
}
