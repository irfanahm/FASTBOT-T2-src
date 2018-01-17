using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
 

namespace FASTBOT.Dialogs
{
   // [LuisModel ("503a7603-77fd-4508-a927-f3dfa8f7bcb2", "7f8d0b67289d449ea3efe38d40341f88")] //First one
   // [LuisModel ("90c90725-0c3d-4bbe-87b1-50f517a95604", "7f8d0b67289d449ea3efe38d40341f88")] // quota exausted
      [LuisModel("90c90725-0c3d-4bbe-87b1-50f517a95604", "f4d49002878340c6888d4d8edb8e0c5f",domain: "westus2.api.cognitive.microsoft.com", Staging = true)]
    

    [Serializable]
    public class LUISDialog : LuisDialog<object>
    {
        [Serializable]
        public class PartialMessage
        {
            public string Text { set; get; }
        }

        private PartialMessage LUISmessage;

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            try
            {
                var msg = await item;
                this.LUISmessage = new PartialMessage { Text = msg.Text };
                await base.MessageReceived(context, item);
            }
            catch(Exception ex)
            {
                await context.PostAsync(ex.Message);
            }
        }

        [LuisIntent("")]
        public async Task None (IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I am sorry, I don't know what you mean.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            var message = context.MakeMessage();
            message.Text = "test";
            context.Call(new GreetingsDialog(), callback);
            //await context.Forward(new LUISFastFileDialog(), callback,message);
        }

        [LuisIntent("FileInfo")]
        public async Task FileInformation(IDialogContext context, LuisResult result)
        {
            string temp = LUISmessage.Text;
            List<EntityRecommendation> entities = result.Entities.ToList();
            // context.Call(new FastFileDialog(temp,entities),callback);
            var message = context.MakeMessage();
            message.Text = temp;
            await context.Forward(new LUISFastFileDialog(temp,entities), callback, message);
             
             

        }

        
        [LuisIntent("Help")]
        public async Task ShowHelp(IDialogContext context, LuisResult result)
        {
            context.Call(new HelpDialog(), callback);
        }

        [LuisIntent("Happy")]
        public async Task SeemsToBeHappy(IDialogContext context, LuisResult result)
        {
            context.Call(new HappyUserDialog(), callback);
        }
        private async Task callback(IDialogContext context, IAwaitable<object> result)
        {
            
            context.Wait(MessageReceived);
        }

        public async Task DoneDialog(IDialogContext context, IAwaitable<object> activity)
        {
            await context.PostAsync("I am still available for your help.");
            context.Done("Thank you for choosing FAST BOT Services.");
        }
    }
}