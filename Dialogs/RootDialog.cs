using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace FASTBOT.Dialogs
{
    [Serializable]
    public class RootDialog : LUISDialog
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var activity = await result as Activity;
            var message = await result;
            
            // calculate something for us to return
            //if (message.Text.Contains("Hi") || message.Text.Contains("Hello") || message.Text.Contains("Hey"))
            //{
            //    await context.Forward(new GreetingsDialog(), this.ResumeAfterSupportDialog, message, System.Threading.CancellationToken.None);
            //}
            // return our reply to the user
        //    await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            context.Wait(MessageReceivedAsync);
        }

        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<object> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Thanks for contacting");
            context.Wait(this.MessageReceivedAsync);
        }

    }
}