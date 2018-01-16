namespace FASTBOT.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using System.Collections.Generic;

    public class HelpDialog : IDialog<object>
    {
        private const string YesOption = "Yes";

        private const string NoOption = "No";
        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync("Do you need help?");
            this.ShowOptions(context);
           // context.Wait(this.MessageReceived);
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if ((message.Text != null) && (message.Text.Trim().Length > 0))
            {
                //if (message.Text.ToLower().Contains("yes") || message.Text.ToLower().Contains("yah") || message.Text.ToLower().Contains("y"))
                //{
                //    var Helpmessage = context.MakeMessage();

                //    var Videoattachment = GetVideoCard();
                //    var Audioattachment = GetAudioCard();
                //    Helpmessage.Attachments.Add(Videoattachment);
                //    Helpmessage.Attachments.Add(Audioattachment);
                //    await context.PostAsync(Helpmessage);
                //    context.Done("Help Done");

                //}
                //else
                //{
                //    await context.PostAsync("OK. I am available to help you further.");
                //    context.Done("Help Done");
                //}
                //this.ShowOptions(context);
            }
            else
            {
                context.Fail(new Exception("Message was not a string or was an empty string."));
            }
        }

        private static Attachment GetVideoCard()
        {
            var videoCard = new VideoCard
            {
                Title = "Big Buck Bunny",
                Subtitle = "by the Blender Institute",
                Text = "Big Buck Bunny (code-named Peach) is a short computer-animated comedy film by the Blender Institute, part of the Blender Foundation. Like the foundation's previous film Elephants Dream, the film was made using Blender, a free software application for animation made by the same foundation. It was released as an open-source film under Creative Commons License Attribution 3.0.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Big_buck_bunny_poster_big.jpg/220px-Big_buck_bunny_poster_big.jpg"
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4"
                    }
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Learn More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://peach.blender.org/"
                    }
                }
            };

            return videoCard.ToAttachment();
        }

        private static Attachment GetAudioCard()
        {
            var audioCard = new AudioCard
            {
                Title = "I am your father",
                Subtitle = "Star Wars: Episode V - The Empire Strikes Back",
                Text = "The Empire Strikes Back (also known as Star Wars: Episode V – The Empire Strikes Back) is a 1980 American epic space opera film directed by Irvin Kershner. Leigh Brackett and Lawrence Kasdan wrote the screenplay, with George Lucas writing the film's story and serving as executive producer. The second installment in the original Star Wars trilogy, it was produced by Gary Kurtz for Lucasfilm Ltd. and stars Mark Hamill, Harrison Ford, Carrie Fisher, Billy Dee Williams, Anthony Daniels, David Prowse, Kenny Baker, Peter Mayhew and Frank Oz.",
                Image = new ThumbnailUrl
                {
                    Url = "https://upload.wikimedia.org/wikipedia/en/3/3c/SW_-_Empire_Strikes_Back.jpg"
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = "http://www.wavlist.com/movies/004/father.wav"
                    }
                },
                Buttons = new List<CardAction>
                {
                    new CardAction()
                    {
                        Title = "Read More",
                        Type = ActionTypes.OpenUrl,
                        Value = "https://en.wikipedia.org/wiki/The_Empire_Strikes_Back"
                    }
                }
            };

            return audioCard.ToAttachment();
        }

        private void ShowOptions(IDialogContext context)
        {
            PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { YesOption, NoOption }, "Are you looking for help?", "Not a valid option", 3);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case YesOption:
                        var str = context.PrivateConversationData.GetValue<string>("Help1");
                        var Helpmessage = context.MakeMessage();

                        var Videoattachment = GetVideoCard();
                        var Audioattachment = GetAudioCard();
                        Helpmessage.Attachments.Add(Videoattachment);
                        Helpmessage.Attachments.Add(Audioattachment);
                        await context.PostAsync(Helpmessage);
                       
                        context.Done("Help Done");
                        break;

                    case NoOption:
                        await context.PostAsync("OK. I will resume our conversation.");
                       context.Done("Help Done");
                        break;
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");
                context.Done("Help Done");
                //context.Wait(this.MessageReceived);
            }
        }
    }
}