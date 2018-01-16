using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis.Models;

namespace FASTBOT.Dialogs
{
    public class HappyUserDialog : IDialog
    {


        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Your Welcome!!!. Happy To Help." + "\n\n" + "Can I close current conversation. I will still be available to help you with your new queries.");
            context.Wait(this.MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                if (message.Text.ToLower().Contains("yes") || message.Text.ToLower().Contains("yah") || message.Text.ToLower().Contains("y"))
                {
                    await context.PostAsync("Sure. Current conversation ended. I am still available to help you.");
                    context.EndConversation("Thank you");

                }
            }
            else
            {
                context.Fail(new Exception("Message was not a string or was an empty string."));
            }
        }
    }
}