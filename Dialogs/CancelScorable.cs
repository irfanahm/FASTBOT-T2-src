namespace FASTBOT.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Builder.Scorables.Internals;
    using Microsoft.Bot.Builder.Dialogs;

#pragma warning disable 1998

    public class CancelScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogTask task;

        public CancelScorable(IDialogTask task)
        {
            SetField.NotNull(out this.task, nameof(task), task);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.Text.Equals("cancel", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }
                if (message.Text.Equals("help", StringComparison.InvariantCultureIgnoreCase) || message.Text.Equals("support", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }
                if (message.Text.Equals("exit", StringComparison.InvariantCultureIgnoreCase) ||
                    message.Text.Equals("abort", StringComparison.InvariantCultureIgnoreCase) || 
                    message.Text.Equals("start again", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }

                if (message.Text.Equals("Thank You", StringComparison.InvariantCultureIgnoreCase) ||
                    message.Text.Equals("Thanks", StringComparison.InvariantCultureIgnoreCase) ||
                    message.Text.Equals("Thanks Again", StringComparison.InvariantCultureIgnoreCase))
                {
                    return message.Text;
                }


            }

            return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            // this.task.Reset();
            var message = item as IMessageActivity;

            if (message != null)
            {
                var incomingMessage = message.Text.ToLowerInvariant();
                var messageToSend = string.Empty;

                //if (incomingMessage == "hello")
                //    messageToSend = "Hi! I am a bot";

                if (incomingMessage.ToLower().Contains("help")|| incomingMessage.ToLower().Contains("support"))
                {
                    messageToSend = "help";
                    var helpDialog = new HelpDialog();
                    var interruption = helpDialog.Void<object, IMessageActivity>();
                    this.task.Call(interruption, null);
                    await this.task.PollAsync(token);
                }

                if (incomingMessage.ToLower().Contains("cancel"))
                {
                    messageToSend = "cancel";

                    var canDialog = new CancelDialog();
                    var interruption = canDialog.Void<object, IMessageActivity>();
                    this.task.Call(interruption, null);
                    await this.task.PollAsync(token);
                }
                if (incomingMessage.ToLower().Contains("exit") ||
                        incomingMessage.ToLower().Contains("abort") ||
                        incomingMessage.ToLower().Contains("start again"))
                {
                    messageToSend = "cancel";

                    var exitDialog = new ExitDialog();
                    var interruption = exitDialog.Void<object, IMessageActivity>();
                    this.task.Call(interruption, null);
                    await this.task.PollAsync(token);
                }

                if (incomingMessage.ToLower().Contains("thanks") ||
                        incomingMessage.ToLower().Contains("thank you") ||
                        incomingMessage.ToLower().Contains("thanks once again"))
                {
                    messageToSend = "Thanks";

                    var happyDialog = new HappyUserDialog();
                    var interruption = happyDialog.Void<object, IMessageActivity>();
                    this.task.Call(interruption, null);
                    await this.task.PollAsync(token);
                }
            }
        }
        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}