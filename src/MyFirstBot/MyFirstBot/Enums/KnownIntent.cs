

namespace MyFirstBot.Enums
{
    /// <summary>
    /// Place here the name of the Intents that LUIS has been trained to detect
    /// </summary>
    public enum KnownIntent
    {
        //Trained Intents
        Unknown,
        SendGreeting,
        EndConversation,
        PlaceOrder,
        GetWeather,
        Registration,
        ChangeLanguage,
        None,
        

        //Secondary Intents
        //Used to combine Intents and Entities detected by LUIS
        CreateAccount,
        OrderFood,
        CreateServiceRequest,
        SpeakSpanish,
        SpeakEnglish,
        OrderSandwich
            

    }
}