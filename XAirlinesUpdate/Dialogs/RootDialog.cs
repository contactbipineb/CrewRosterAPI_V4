using Airlines.XAirlines.Common;
using Airlines.XAirlines.Helpers;
using Airlines.XAirlines.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Airlines.XAirlines.Helpers.WeatherHelper;
using Microsoft.Bot.Builder.Teams;
using XAirlinesUpdate.Helpers;
using Microsoft.Extensions.Configuration;

namespace Airlines.XAirlines.Dialogs
{
    [Serializable]
    public class RootDialog : ComponentDialog
    {
        private const string mentionPattern = "<at>([^]]*)</at>";
        private readonly GraphHelper graphHelper;

        public RootDialog(IConfiguration configuration)
        {
            this.graphHelper = new GraphHelper(configuration);
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            try
            {
                var activity = innerDc.Context.Activity;

                string message = string.Empty;
                var userDetails = await TeamsInfo.GetMemberAsync(innerDc.Context, innerDc.Context.Activity.From.Id);//await GetCurrentUserDetails(activity);
                if (userDetails == null)
                {
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("Failed to read user profile. Please try again."), cancellationToken);//await context.PostAsync("Failed to read user profile. Please try again.");
                }

                if (!string.IsNullOrEmpty(activity.Text))
                {
                    message = RemoveMention(activity.Text.ToLower());
                    Attachment card = null;
                    string crewid = string.Empty;
                    switch (message)
                    {
                        case Constants.NextMonthRoster:
                            crewid = await graphHelper.GetUserEmployeeIdAsync(userDetails.UserPrincipalName); // ${Debugging}
                            card = CardHelper.GetMonthlyRosterCard(crewid);
                            break;
                        case Constants.NextWeekRoster:
                            crewid = await graphHelper.GetUserEmployeeIdAsync(userDetails.UserPrincipalName);
                            card = await CardHelper.GetWeeklyRosterCard(crewid);
                            // card = await CardHelper.GetWeeklyRosterCard("10055"); // ${Debugging}
                            break;
                        case Constants.UpdateCard:
                            card = CardHelper.GetUpdateScreen();
                            break;
                        case Constants.ShowDetailedRoster:
                            card = await GetDetailedRoasterCard(activity, userDetails, this.graphHelper);
                            break;
                        default:
                            card = CardHelper.GetWelcomeScreen(userDetails.GivenName ?? userDetails.Name);
                            break;
                    }

                    await innerDc.Context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);

                }
                else if (activity.Value != null)
                {
                    await HandleActions(innerDc.Context, cancellationToken, activity, userDetails);
                }
            }
            catch (Exception e)
            {
                await innerDc.Context.SendActivityAsync(MessageFactory.Text(e.ToString()), cancellationToken);// await context.PostAsync(e.ToString()).ConfigureAwait(false);
            }
            return await innerDc.EndDialogAsync();
        }

        private async Task HandleActions(ITurnContext context, CancellationToken cancellationToken, Activity activity, TeamsChannelAccount userDetails)
        {
            var actionDetails = JsonConvert.DeserializeObject<ActionDetails>(activity.Value.ToString());
            // var userDetails = await TeamsInfo.GetMemberAsync(context, context.Activity.From.Id);//await GetCurrentUserDetails(activity);
            var type = actionDetails.ActionType;

            Attachment card = null;
            string crewid = string.Empty;

            switch (type)
            {
                case Constants.ShowDetailedRoster:
                    card = await GetDetailedRoasterCard(activity, userDetails, this.graphHelper);
                    break;
                case Constants.NextWeekRoster:
                    crewid = await graphHelper.GetUserEmployeeIdAsync(userDetails.UserPrincipalName);
                    card = await CardHelper.GetWeeklyRosterCard(crewid);
                    // card = await CardHelper.GetWeeklyRosterCard("10055"); // ${Debugging}
                    break;
                case Constants.NextMonthRoster:
                    crewid = await graphHelper.GetUserEmployeeIdAsync(userDetails.UserPrincipalName);
                    card = CardHelper.GetMonthlyRosterCard(crewid);
                    break;
                case Constants.WeatherCard:
                    card = await GetWeatherCard(activity);
                    break;
                case Constants.CurrencyCard:
                    card = await GetCurrencyCard(activity);
                    break;
            }

            await context.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
        }

        private static string RemoveMention (string message)
        {
             return System.Text.RegularExpressions.Regex.Replace(message, mentionPattern, String.Empty).Trim();
        }

        private static async Task<Attachment> GetDetailedRoasterCard(Activity activity, TeamsChannelAccount userDetails, GraphHelper graphHelper)
        {
            var details = JsonConvert.DeserializeObject<AirlineActionDetails>(activity.Value.ToString());
            // Crew crew = await CabinCrewPlansHelper.ReadJson(userDetails.UserPrincipalName);

            string crewid = await graphHelper.GetUserEmployeeIdAsync(userDetails.UserPrincipalName);
            Crew crew = await CabinCrewPlansHelper.ReadJson(crewid);
            // Crew crew = await CabinCrewPlansHelper.ReadJson("10055"); // ${Debugging}
            var datePlan = crew.plan.FirstOrDefault(c => c.flightDetails.flightStartDate.Date.ToString() == details.Id);
            return CardHelper.GetDetailedRoster(datePlan);
        }

        private static async Task<Attachment> GetCurrencyCard(Activity activity)
        {
            var desCurrency = JsonConvert.DeserializeObject<CurrencyActionDetails>(activity.Value.ToString());
            CurrencyInfo currencyinfo = CurrencyHelper.GetCurrencyInfo();

            return await CardHelper.GetCurrencyCard(currencyinfo, desCurrency.City, desCurrency.DestinationCurrencyCode);
        }

        private static async Task<Attachment> GetWeatherCard(Activity activity)
        {
            var desLocationInfo = JsonConvert.DeserializeObject<WeatherActionDetails>(activity.Value.ToString());
            WeatherInfo weatherinfo = WeatherHelper.GetWeatherInfo(desLocationInfo.City);
            
            return await CardHelper.GetWeatherCard(weatherinfo, desLocationInfo.Date);
          

        }

    }
}
