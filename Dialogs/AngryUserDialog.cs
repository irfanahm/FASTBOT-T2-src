using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis.Models;

namespace FASTBOT.Dialogs
{
    public class AngryUserDialog : IDialog
    {


        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Sorry!!!. I couldn't help you much this time. You can try to start it over again and check...");
            context.EndConversation("Get Lost.");

            //context.Wait(this.MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                if (message.Text.ToLower().Contains("yes") || message.Text.ToLower().Contains("yah") || message.Text.ToLower().Contains("y"))
                {
                    await context.PostAsync("Sure. Current conversation ended. I am still available to help you.");

                    context.EndConversation("Cancel");

                }
            }
            else
            {
                context.Fail(new Exception("Message was not a string or was an empty string."));
            }
        }
    }
}