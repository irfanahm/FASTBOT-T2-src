using FASTBOT.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FASTBOT.Dialogs
{
    [LuisModel("90c90725-0c3d-4bbe-87b1-50f517a95604", "f4d49002878340c6888d4d8edb8e0c5f", domain: "westus2.api.cognitive.microsoft.com", Staging = true)]

    [Serializable]
    public class LUISFastFileDialog : LuisDialog<Object> 
    {
        List<EntityRecommendation> FileEntities = null;
        FileInfo FileDetails = null;
        string FileNumber = string.Empty;
        string msgtxt = string.Empty;
        
        private const string BuyerOption = "buyers";
        private const string SellerOption = "sellers";
        private const string CancelOption = "cancel";
        private const string StatusOption = "status";

        public LUISFastFileDialog(string message, List<EntityRecommendation> entities)
        {
            
            ///  this.msgtxt = message;
            //this.FileEntities = entities;
        }
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
                msgtxt = msg.Text;
                
                await base.MessageReceived(context, item);
            }
            catch (Exception ex)
            {
                await context.PostAsync(ex.Message);
            }
        }

        [LuisIntent("Buyer")]
        public async Task getBuyerInfo(IDialogContext context, LuisResult result)
        {
            context.PrivateConversationData.SetValue<string>("HelpContext", "Buyer");
            if (FileDetails.Buyers != null)
            {
                foreach (var buyer in FileDetails.Buyers)
                {
                    var BuyerMessage = context.MakeMessage();
                    var heroCard = new HeroCard
                    {
                        Title = "Buyer Details for file : " + FileNumber,
                        //Subtitle = FileDetails.Status,
                        Text = "First Name: " + buyer.FirstName + "\n\n" + "Last Name: " + buyer.LastName + "\n\n" + "Email Id: " + buyer.Email + "\n\n" + "Phone Number: " + buyer.PhoneNumber + "\n\n" + "Address: " + buyer.PostalAddress + "\n\n",
                        //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                    };

                    BuyerMessage.Attachments.Add(heroCard.ToAttachment());
                    await context.PostAsync(BuyerMessage);
                }
            }
            else
            {
                 context.SayAsync("Failed to get Buyer information OR there is no Buyer for mentioned file number.", "Failed to get Buyer information OR there is no Buyer for mentioned file number.");
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("Seller")]
        public async Task GetSellerInfo(IDialogContext context, LuisResult result)
        {
            context.PrivateConversationData.SetValue<string>("HelpContext", "Seller");
            if (FileDetails.Sellers != null)
            {
                foreach (var seller in FileDetails.Sellers)
                {
                    var SellerMessage = context.MakeMessage();
                    var heroCard = new HeroCard
                    {
                        Title = "Seller Details for file : " + FileNumber,
                        //Subtitle = FileDetails.Status,
                        Text = "First Name: " + seller.FirstName + "\n\n" + "Last Name: " + seller.LastName + "\n\n" + "Email Id: " + seller.Email + "\n\n" + "Phone Number: " + seller.PhoneNumber + "\n\n" + "Address: " + seller.PostalAddress + "\n\n",
                        //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                    };

                    SellerMessage.Attachments.Add(heroCard.ToAttachment());
                    
                    await context.PostAsync(SellerMessage);

                }
            }
            else
            {
                context.PostAsync("Failed to get seller information or there is no seller for mentioned file number.");
            }

            context.Wait(MessageReceived);
        }
        [LuisIntent("FileInfo")]
        public async Task GetFileInformation(IDialogContext context, LuisResult result)
        {
            context.PrivateConversationData.SetValue<string>("HelpContext", "File");
            this.FileEntities = result.Entities.ToList();
            try
            {
                Regex FileNo = new Regex(@"\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d");
                Match m = FileNo.Match(msgtxt);
                if (m.Success)
                {
                    string tempFileNo = m.Value.Trim();
                    FileNumber = tempFileNo.ToString();
                    //await context.SayAsync("You want me to get information for File no: " + FileNumber + "?",
                    //    "You want me to get information for File number: " + FileNumber + "?");
                     ShowFileInformationAsync(context);
                }
                else
                {
                    context.SayAsync("Please enter valid File Number", "Please provide a valid File Number.");
                }
            }
            catch (Exception ex)
            {
                context.SayAsync(ex.Message.ToString());
            }

            context.Wait(MessageReceived);
        }

       [ LuisIntent("Help")]
        private async Task ShowHelp(IDialogContext context,LuisResult result)
        {
            string strhelpcontext = context.PrivateConversationData.GetValueOrDefault<string>("HelpContext");

            switch (strhelpcontext)
            {
                case "Buyer":
                    var Helpmessage = context.MakeMessage();

                    var Videoattachment = GetVideoCard();
                    var Audioattachment = GetAudioCard();
                    Helpmessage.Attachments.Add(Videoattachment);
                    Helpmessage.Attachments.Add(Audioattachment);
                    await context.PostAsync(Helpmessage);

                    break;
                case "Seller":
                    context.SayAsync("Showing you Sellers Help.", "Showing you sellers help.");
                    break;
                case "File":
                    context.SayAsync("Showing you File Help.", "Showing you File help.");
                    break;

                default:
                    context.SayAsync("Showing you general Help.", "Showing you general help.");
                    break;
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
        private async Task ShowFileInformationAsync(IDialogContext context)
        {
            try
            {

                HttpClient client = new HttpClient();
                // client.BaseAddress = new Uri("http://localhost:56851/");
                client.BaseAddress = new Uri("https://webapidemo-t2.azurewebsites.net/");
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("api/User/" + FileNumber).Result;

                if (response.IsSuccessStatusCode)
                {

                    Task<string> responseContent = response.Content.ReadAsStringAsync();

                    var strJson = Newtonsoft.Json.Linq.JToken.Parse(responseContent.Result);
                    // FileInfo listMessageCode = JsonConvert.DeserializeObject<FileInfo>(strJson);

                    FileDetails = JsonConvert.DeserializeObject<FileInfo>(strJson.ToString());
                    var message1 = context.MakeMessage();


                    string strMsg = string.Empty;
                    bool bEntityFound = false;

                    if (FileDetails != null)
                    {
                        if (FileEntities != null)
                        {
                            foreach (var entity in FileEntities)
                            {
                                if (string.Compare(entity.Type, BuyerOption, true) == 0)
                                {
                                    if (FileDetails.Buyers != null)
                                    {
                                        foreach (var buyer in FileDetails.Buyers)
                                        {
                                            var BuyerMessage = context.MakeMessage();
                                            var heroCard = new HeroCard
                                            {
                                                Title = "Buyer Details for file : " + FileNumber,
                                                //Subtitle = FileDetails.Status,
                                                Text = "First Name: " + buyer.FirstName + "\n\n" + "Last Name: " + buyer.LastName + "\n\n" + "Email Id: " + buyer.Email + "\n\n" + "Phone Number: " + buyer.PhoneNumber + "\n\n" + "Address: " + buyer.PostalAddress + "\n\n",
                                                //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                                            };

                                            BuyerMessage.Attachments.Add(heroCard.ToAttachment());
                                            bEntityFound = true;
                                            await context.PostAsync(BuyerMessage);
                                        }
                                    }


                                    else
                                    {
                                        await context.PostAsync("Failed to get seller information or there is no seller for mentioned file number.");
                                    }
                                }
                                if (string.Compare(entity.Type, SellerOption, true) == 0)
                                {
                                    if (FileDetails.Sellers != null)
                                    {
                                        foreach (var seller in FileDetails.Sellers)
                                        {
                                            var SellerMessage = context.MakeMessage();
                                            var heroCard = new HeroCard
                                            {
                                                Title = "Seller Details for file : " + FileNumber,
                                                //Subtitle = FileDetails.Status,
                                                Text = "First Name: " + seller.FirstName + "\n\n" + "Last Name: " + seller.LastName + "\n\n" + "Email Id: " + seller.Email + "\n\n" + "Phone Number: " + seller.PhoneNumber + "\n\n" + "Address: " + seller.PostalAddress + "\n\n",
                                                //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                                            };

                                            SellerMessage.Attachments.Add(heroCard.ToAttachment());
                                            bEntityFound = true;
                                            await context.PostAsync(SellerMessage);

                                        }
                                    }
                                    else
                                    {
                                        context.PostAsync("Failed to get seller information or there is no seller for mentioned file number.");
                                    }
                                }
                                if (string.Compare(entity.Type, StatusOption, true) == 0)
                                {
                                    string strStatus;
                                    strStatus = " File Number " + FileNumber + " is currently " + FileDetails.Status.ToString() + "\n\n";
                                    bEntityFound = true;
                                    await context.PostAsync(strStatus);

                                }
                            }
                        }
                        if (!bEntityFound)
                        {
                            strMsg += "\n\n\n\n";
                            strMsg = "Staus: " + FileDetails.Status.ToString() + "\n\n";

                            if (FileDetails.Buyers != null)
                            {
                                strMsg += "Number of Buyers: " + FileDetails.Buyers.Count.ToString() + "\n\n";
                            }
                            else
                            {
                                strMsg += "Number of Buyers: " + "0" + "\n\n";
                            }
                            if (FileDetails.Sellers != null)
                            {
                                strMsg += "Number of Sellers: " + FileDetails.Sellers.Count.ToString() + "\n\n";
                            }
                            else
                                strMsg += "Number of Sellers: " + "0" + "\n\n";

                            if (FileDetails.Account != null)
                            {
                                strMsg += "Available Balance: " + FileDetails.Account.CurrentBalance.ToString() + "\n\n";
                                strMsg += "Due: " + FileDetails.Account.FundDue.ToString() + Environment.NewLine;

                            }
                            var heroCard = new HeroCard
                            {
                                Title = "Summary for File No: " + FileNumber,
                                //Subtitle = FileDetails.Status,
                                Text = strMsg,
                                Images = new List<CardImage> { new CardImage("https://fastbott29961.blob.core.windows.net/images/MyFAI.png") },
                                //Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Buyers Info", value: "https://docs.microsoft.com/bot-framework"),
                                // new CardAction(ActionTypes.OpenUrl, "Seller Info", value: "https://docs.microsoft.com/bot-framework")
                                //}

                                // SellerButton = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Sellers Info", value: "https://docs.microsoft.com/bot-framework") }
                            };
                            message1.Attachments.Add(heroCard.ToAttachment());
                            context.PostAsync(message1);
                            Thread.Sleep(400);
                            context.SayAsync("You can type Buyer or Seller for more information","Please say Buyer or Seller for more information" );
                            
                           // this.ShowOptions(context);

                        } //else of bEntityFound
                        else
                        {
                            //context.Done("Done");
                        }


                    }//FileDetails !=Null

                }
                else
                {
                    await context.SayAsync("File Number not found. Please provide valid file number again.", "File Number not found. Please provide valid file number again.");
                   
                }

            }
            catch (Exception ex)
            {
                await context.PostAsync(ex.Message);
                 
            }
            finally
            {
                 
            }
        }

    }
}