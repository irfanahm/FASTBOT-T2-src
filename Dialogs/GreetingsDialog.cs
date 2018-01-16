using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Resource;
using Microsoft.Bot.Connector;
using System.Text;
using System.Xml;
using System.IO;
using Microsoft.Bot.Builder.Luis;

namespace FASTBOT.Dialogs
{

    [Serializable]
    public class GreetingsDialog : IDialog

    {
        private static string SelectRandomString(IList<string> options)
        { 
             string Entity_Device = BuiltIn.DateTime.DayPart.EV.ToString();
             

        var rand = new Random();
            var index = rand.Next(0, options.Count - 1);
            return options[index];
        }
        public async Task StartAsync(IDialogContext context)
        {

            var replyText = SelectRandomString(new string[] { "Hello I am FAST Bot. How can I help you today?",
                                                              "Welcome to FAST Bot. How can I help you today?",
                                                               "Greetings. Welcome to Fast Bot. I am here to help you"});
            //await context.SayAsync("Hi I am FAST Bot. How can I help you today?", "Hi I am FAST Bot. How can I help you today");
            //var message = context.MakeMessage();
            //message.Speak = SSMLHelper.Speak("Hi I am Fast Service Bot");
            //message.InputHint = Microsoft.Bot.Connector.InputHints.AcceptingInput;
            // await Respond(context);
            var reply = CreateResponse(
                            context,
                            context.MakeMessage(),
                            replyText,
                            replyText,
                            messageType: MessageType.StartLightningMode,
                            audioToPlay: "tv_gameshow_bell_01.wav",
                            inputHint: InputHints.IgnoringInput);
            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
            context.Done("Thank You");
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Microsoft.Bot.Connector.IMessageActivity> result)
        {

            var message = await result;
            var userName = string.Empty;
            var GetName = false;
            context.UserData.TryGetValue<string>("Name", out userName);
            context.UserData.TryGetValue<bool>("GetName", out GetName);
            if (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem"))
            {
                //await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else
            if (GetName)
            {
                userName = message.Text;
                context.UserData.SetValue<string>("Name", userName);
                context.UserData.SetValue<bool>("GetName", false);

            }

            await Respond(context);
           // context.Done(message);
            //var ticketNumber = new Random().Next(0, 20000);
            //await context.PostAsync($"Your message '{message.Text}' was registered. Once we resolve it; we will get back to you.");
            //context.Done(ticketNumber);
            //  await context.PostAsync($"Hello Sir.Thank You for using FAST Bot.");
            
            
            

        }

        public static async Task Respond(IDialogContext context)
        {
            var userName = "Irfan";
            //context.UserData.TryGetValue<string>("Name", out userName);
            if(string.IsNullOrEmpty(userName))
            {
                await context.PostAsync("May I have your Name Please? It will help during our conversation?");
                context.UserData.SetValue("Getname", true);

            }
            else
            {
                await context.PostAsync(string.Format("Hi {0}. How can I help you today?", userName));
            }
        }

        private static IMessageActivity CreateResponse
        (
            IDialogContext context,
            IMessageActivity message,
            string displayText,
            string speakText,
            MessageType messageType,
            string audioToPlay = null,
            IList<string> optionsToAdd = null,
            string inputHint = InputHints.AcceptingInput
        )
        {
            var activityToSend = context.MakeMessage();

            if (displayText != null)
            {
                activityToSend.Text = displayText;
            }

            var ssml = (string)null;
            if (speakText != null)
            {
                var escapedSpeakText = speakText.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
                ssml = SsmlWrapper.Wrap(escapedSpeakText);

                if (audioToPlay != null && message.ChannelId != "cortana")
                {
                    var assetPath = "http://" + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + "/Assets/";
                    var uri = new Uri(assetPath + audioToPlay);
                    ssml = CombineAudioAndTextForSSML(uri, ssml);
                }
            }

            activityToSend.Speak = ssml;
            activityToSend.InputHint = inputHint;

            var appEntities =
                new AppEntities
                {
                    MessageType = messageType,
                    TriviaAnswerOptions = optionsToAdd?.Count > 0 ? optionsToAdd : null
                };

            if (optionsToAdd != null)
            {
                List<CardAction> cardButtons = new List<CardAction>();

                bool numberOptions = optionsToAdd.Count > 2;
                for (int i = 0; i < optionsToAdd.Count; i++)
                {
                    var display = numberOptions ? $"{(i + 1).ToString()}: {optionsToAdd[i]}" : optionsToAdd[i];
                    cardButtons.Add(new CardAction() { Value = optionsToAdd[i], Type = "postBack", Title = display });
                }

                var plCard = new ThumbnailCard()
                {
                    // Title = "Pick an answer",
                    Buttons = cardButtons,
                };
                activityToSend.Attachments.Add(plCard.ToAttachment());
            }

            var entity = new Entity();
            entity.SetAs<AppEntities>(appEntities);
            activityToSend.Entities.Add(entity);

            return activityToSend;
        }

        public static string CombineAudioAndTextForSSML(Uri audioStream, string text)
        {
            StringBuilder sb = new StringBuilder();
            const string ssmlPrefix = @"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' " +
                                            "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                                            "xsi:schemaLocation='http://www.w3.org/2001/10/synthesis " +
                                            "http://www.w3.org/TR/speech-synthesis/synthesis.xsd' " +
                                            "xml:lang='en-us'>";
            const string ssmlSuffix = "</speak>";

            sb.Append(ssmlPrefix);
            sb.Append($"<audio src='{audioStream.AbsoluteUri}'/>");
            sb.Append(GetInnerSsmlContents(SsmlWrapper.Wrap(text)));
            sb.Append(ssmlSuffix);
            return sb.ToString();
        }
        public static string GetInnerSsmlContents(string ssml)
        {
            StringBuilder sb = new StringBuilder();
            XmlReader reader = null;
            reader = XmlReader.Create(new StringReader(ssml));
            string inner = "";
            if (reader.Read())
            {
                inner = reader.ReadInnerXml();
            }

            return inner;
        }
    }
}