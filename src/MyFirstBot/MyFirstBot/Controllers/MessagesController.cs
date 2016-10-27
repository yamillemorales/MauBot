using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using MyFirstBot.Models;
using MyFirstBot.Enums;


namespace MyFirstBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        string defaultMessage = "You may have just short-circuited my system! I can't handle this!";
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {

            if (!(activity.Type == ActivityTypes.Message))
            {
                return await HandleSystemMessage(activity);
            }


            StateClient stateClient = activity.GetStateClient();

            //Default answer
            var answer = defaultMessage;

            //Get the input message
            var message = activity.Text.ToLower().TrimEnd();

            //Validate if you already started a conversation
            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

            //////////////////////////////////////////
            //Example of hardcoded end of conversation
            //This is commented and implemented as LUIS's EndConversation Intent
            /////////////////////////////////////////
            //End conversation if requested 
            //if (message.ToUpper().Contains("BYE"))
            //{
            //    stateClient.BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
            //    answer = "Talk to you later, alligator!";
            //    return await Response(activity, answer);
            //}

            //Use LUIS (http://luis.ai) to validate as the main means of validating the context of the conversation
            var context = await GetConversationContextFromLuis(message);


            switch (context)
            {
                case KnownIntent.SendGreeting:

                    //Get languange of conversation
                    var result = await GetGreetingLanguage(message);

                    //Validate if you already started a conversation
                    var alreadyGreeted = userData.GetProperty<bool>("SentGreeting");
                    if (alreadyGreeted)
                    {
                        if ((Language)result == Language.English)
                            answer = "We already started this conversation! Do you want to start over?";
                        else if ((Language)result == Language.Spanish)
                            answer = "Ya estabamos hablando? Quiere comenzar de nuevo? Algo que no te gusto?";
                        else
                            answer = "We already started this conversation! Do you want to start over?";
                        return await Response(activity, answer);
                    }
                    else
                    {
                        //Do greeting logic here if it is the first time
                        if ((Language)result == Language.English)
                            answer = "Hi there, how can I help you?";
                        else if ((Language)result == Language.Spanish)
                            answer = "Hola, como te puedo ayudar? Aunque se que estas hablando español te seguire hablando en ingles.";

                        //Register the greeting
                        userData.SetProperty<bool>("SentGreeting", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        break;
                    }
                case KnownIntent.EndConversation:
                    stateClient.BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                    answer = "Talk to you later... Just ping me if you want to start a new conversation!";
                    break;
                case KnownIntent.Registration:
                    answer = "Soon I'll be able to help you process a registration.";
                    break;              
                case KnownIntent.ChangeLanguage:
                    answer = "Let me know what language you wish to change to.";
                    break;
                case KnownIntent.SpeakEnglish:
                    answer = "Ok, lets speak english then.";
                    break;
                case KnownIntent.SpeakSpanish:
                    answer = "Me hablar muy poco de spanish. Let's hablar english for ahora.";
                    break;
                case KnownIntent.CreateAccount:
                    answer = "Ok, so I'll have to ask you for some information so we can create an account.";
                    break;
                case KnownIntent.None:
                    //Makes it easier to find text in spanish and languages with many characters
                    message = message.ToUpper();
                    //Analyze specific wording
                    switch (message)
                    {
                        case "ESTAS AHI?":
                            answer = "Si. un poco aburrido pero ahí vamos. Estamos listos?";
                            break;
                        case "WHAT IS YOUR NAME?":
                            answer = "Mauro, but you can call me Mau.";
                            break;
                        case "COMO TE LLAMAS?":
                            answer = "Mauro, but you can call me Mau.";
                            break;
                        case "COMENZAMOS":
                            answer = "Que importa, yo se que te pueden esperar un poquito mas. Sigamos hablando que estoy aburrido.";
                            break;
                        case "POR QUE?":
                            answer = "porque yo soy un bot! y tú les vas a enseñar como me creaste!";
                            break;                     
                        case "MUCHAS GRACIAS POR TU COLABORACION":
                            answer = "Gracias a ti Yamille y a le envío un abrazo a la todos los participantes de HackPR. Ojalá nos chateemos pronto! LEs deseo suerte en el día de hoy! Saludos...";
                            break;
                        case "TIENES INFORMACION SOBRE PROPIEDADES EN PUERTO RICO?":
                            answer = "Seguro que si. Estamos colaborando con el equipo de Popular y \"DeShow.com\"";
                            break;
                        default:
                            answer = "Aún no he sido entrenado para ayudarte con este especifico. ¿Hay algo adicional en lo que te pueda ayudar?.";
                            break;
                    }

                    break;

                default:
                    answer = "I got lost there for a momment. Can we try again?";
                    break;
            }

            return await Response(activity, answer);


            //De Show API Example
            if (message.ToLower().Contains("busca propiedades"))
            {
                message = message.Replace("buca propiedades", "");
                answer = await GetPropertyInfo(message); //TODO Extract city name

                return await Response(activity, answer);

            }


        }

        private async Task<string> AnalyzeGreeting(string message)
        {
            var answer = "";

            var result = await GetGreetingLanguage(message);

            if ((Language)result == Language.Spanish)
                answer = "I don't speak spanish. Do you mind if we switch to english? ";
            else if (result == Language.English)
                answer = "I'll continue speaking english for the remainder of the conversation.?";
            else if (result == Language.Catalan)
                answer = "Hi means \"there\" in Catalán. I don't speak that language.";
            else
                answer = "I still don't speak that lanaguage (" + result.ToString() + ").";
            return answer;
        }



        private async Task<string> GetSmartAnswer(string message)
        {
            string answer = string.Empty;

            string entity = string.Empty;
            var sTemperature = LuisAnalytics.AnalyzeTemperature(message, out entity);
            double temperature = 0d;
            if (double.TryParse(sTemperature, out temperature))
            {
                double fTemp = GetFTemperature(temperature);

                answer = $"The weather in {entity} is {fTemp} degrees farenheit.";
            }
            else
            {
                //Could improve this
                answer = await EvaluateSentimentalAnswer(message);
            }
            return answer;
        }


        private async Task<KnownIntent> GetConversationContextFromLuis(string message)
        {
            KnownIntent detectedIntent;
            var context = string.Empty;

            var response = LuisAnalytics.UnderstandContext(message, out context);

            Enum.TryParse<KnownIntent>(context, out detectedIntent);

            return detectedIntent;
        }


        private double GetFTemperature(double cTemp)
        {

            return Math.Round((cTemp * 1.8) + 32.0, 0);
        }


        private async Task<string> GetPropertyInfo(string message)
        {
            string answer = string.Empty;


            answer = DeShowService.GetPropInfo(message);

            return answer;
        }

        private async Task<Language> GetGreetingLanguage(string message)
        {

            var answer = await TextAnalyticsCommunicator.LanguageDetection(message);

            return answer;
        }

        private async Task<HttpResponseMessage> Response(Activity activity, string answer)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            var replyMessage = activity.CreateReply();
            replyMessage.Recipient = activity.From;
            replyMessage.Type = ActivityTypes.Message;
            replyMessage.Text = answer;
            await connector.Conversations.ReplyToActivityAsync(replyMessage);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        /// <summary>
        /// Hanldes messages that are not of type message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> HandleSystemMessage(Activity activity)
        {
            StateClient stateClient = activity.GetStateClient();

            if (activity.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion her
                // If we handle user deletion, return a real activity

                stateClient.BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                var answer = "We just deleted your data as requested! Bye for now.";
                return await Response(activity, answer);

            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                var answer = this.defaultMessage;
                return await Response(activity, answer);
            }
            else if (activity.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
                var answer = defaultMessage;
                return await Response(activity, answer);
            }
            else if (activity.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
                var answer = "I'm eager to know what your typing about. Come'n lets see what you've got!";
                return await Response(activity, answer);
            }
            else if (activity.Type == ActivityTypes.Ping)
            {

                var answer = "I'm still here. How can I help you?";
                return await Response(activity, answer);
            }

            return null;
        }



        #region Helpers


        [HttpPost]
        private async Task<string> EvaluateSentimentalAnswer(string message)
        {
            var answer = string.Empty;
            var sentimentScore = await TextAnalyticsCommunicator.SentimentScore(message);
            if (sentimentScore >= 0.8)
            {
                answer = "Excelente!";
            }
            else if (sentimentScore >= 0.6)
            {
                answer = "Estoy completamente de acuerdo contigo";
            }
            else if (sentimentScore < 0.3)
            {
                answer = "Wow, que mal te va. ¿Quieres que te refiera a un psicólogo?";
            }
            else
            {
                answer = "Hum, ya veo";
            }
            return answer;
        }


        #endregion
    }
}