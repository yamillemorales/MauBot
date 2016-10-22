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


namespace MyFirstBot
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
            //if (activity.Type == ActivityTypes.Message)
            //{
            //    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            //    // calculate something for us to return
            //    int length = (activity.Text ?? string.Empty).Length;

            //    // return our reply to the user
            //    Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");
            //    await connector.Conversations.ReplyToActivityAsync(reply);
            //}
            //else
            //{
            //    HandleSystemMessage(activity);
            //}
            //var response = Request.CreateResponse(HttpStatusCode.OK);
            //return response;

            var message = activity.Text.ToLower().TrimEnd();
            var answer = "Hum, ya veo...";


            //De Show
            if (message.ToLower().Contains("busca propiedades"))
            {
                message = message.Replace("buca propiedades", "");
                answer = await GetPropertyInfo(message); //TODO Extract city name

                return await Response(activity, answer);

            }


            switch (message)
            {
                case "como te llamas?":
                    answer = "Mauro, pero me puedes decir Mau.";
                    break;
                case "hola":
                    answer = await AnalyzeGreeting(message);
                    break;
                case "hello":
                    answer = await AnalyzeGreeting(message);
                    break;
                case "hi":
                    answer = await AnalyzeGreeting(message);
                    break;
                case "como estas?":
                    answer = "Bien por dicha. ¿Y tu como estas?";
                    break;

                case "relleno":
                    answer = "El mundo de tecnologías para desarrolladores de Microsoft es cada vez más abierto. Hoy en día hasta hemos publicado toda una versión de nuestro framework que es totalmente open source. Si además quieres un editor de código moderno, OSS y además amigable con .NET Core y ASP.NET Core y que encima de todo pueda correr en Windows, Mac y Linux, te recomiendo Visual Studio Code";
                    break;
                case "bueno, vamos a comenzar":
                    answer = "Que importa, yo se que te pueden esperar un poquito mas. Sigamos hablando que estoy aburrido.";
                    break;
                case "no! eso sería una falta de respeto!":
                    answer = "pffff... no pasa nada, de seguro les va a gustar lo que vamos a presentar!";
                    break;
                case "por qué?":
                    answer = "porque yo soy un bot! y tú les vas a enseñar como me creaste!";
                    break;
                case "muy bien! comencemos en serio":
                    answer = "Claro, primero un saludo a todos los participantes del Include-a-thon!";
                    break;
                case "ok":
                    answer = "Pregúntame del clima en las ciudades del mundo!";
                    break;
                case "bueno mau, muchas gracias por tu colaboración":
                    answer = "Gracias a ti Yamille y a le envío un abrazo a la todos los participantes de HackPR. Ojalá nos chateemos pronto! LEs deseo suerte en el día de hoy! Saludos...";
                    break;

                case "tienes información sobre propiedades en puerto rico?":
                    answer = "Seguro que si. Estamos colaborando con el equipo de Popular y \"DeShow.com\"";
                    break;


                default:
                    //Evaluar el clima o el sentimiento
                    answer = await GetSmartAnswer(message);
                    break;
            }
            return await Response(activity, answer);

        }

        private async Task<string> AnalyzeGreeting(string message)
        {
            var answer = ""; 
        
            var result = await GetGreetingLanguage(message);

            if ((Language)result == Language.Spanish)
                answer = "Te hablaré español para seguir la conversación.";
            else if (result == Language.English)
                answer = "I don't speak english. Do you mind if we switch to spanish?";
            else if (result == Language.Catalan)
                answer = "Hi queire decir \"alli\" en Catalán. Aún no hablo en ese idioma. Si de casualidad estas hablando en inglés, tampoco lo hablo.";
            else
                answer = "Aún no hablo en ese idioma (" + result.ToString() + ").";
         
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

                answer = $"La temperatura en {entity} es {fTemp} grados farenheit.";
            }
            else
            {
                //Could improve this
                answer = await EvaluateSentimentalAnswer(message);
            }
            return answer;
        }


        private double GetFTemperature(double cTemp)
        {

            return Math.Round((cTemp * 1.8) + 32.0,0); 
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