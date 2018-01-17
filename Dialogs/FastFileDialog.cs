using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Bot.Connector;

using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FormFlow;
using FASTBOT.Models;
using FASTBOT.Dialogs;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Resource;

namespace FASTBOT.Dialogs
{
    
    [Serializable]
    public class FastFileDialog :  IDialog
    {
        string FileNumber = string.Empty;
        
        
        private static Chain.Continuation<object, string> AfterGreetingContinuation;
        
        List<FileParty> buyers = new List<FileParty>();
        FileInfo FileDetails = null;
        string msgtxt = string.Empty;
        private int attempts = 3;
        private const string BuyerOption = "buyers";
        private const string SellerOption = "sellers";
        private const string CancelOption = "cancel";
        private const string StatusOption = "status";

        List<EntityRecommendation> FileEntities = null;
 

        public FastFileDialog(string message, List<EntityRecommendation> entities)
        {
            this.msgtxt = message;
            this.FileEntities = entities;
        }
        
        public async Task StartAsync(IDialogContext context)
         
        {
            context.PrivateConversationData.SetValue("Help1", "File");
            try
            {
                if (msgtxt.Length == 5 && bFileNumberOnlyInMessage(msgtxt))
                {
                    FileNumber = msgtxt;
                    await context.SayAsync("You want me to get information for File no: " + FileNumber + "?", "You want me to get information for File number: " + FileNumber + "?");
                    context.Wait(ConfirmAndShowFileInformationAsync);
                    context.Done("Dine");


                }
                else if (msgtxt.Length < 5)
                {
                    await context.SayAsync("Please enter valid File No.", "Please Enter valid File Number");
                    context.Wait(ManageFileNumber);
                }

                else if (msgtxt.Length > 5)
                {
                    try
                    {
                        Regex FileNo = new Regex(@"\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d");
                        Match m = FileNo.Match(msgtxt);
                        if (m.Success)
                        {
                            string tempFileNo = m.Value.Trim();
                            if (tempFileNo.Length == 5)
                            {

                                FileNumber = tempFileNo.ToString();
                                await context.SayAsync("You want me to get information for File no: " + FileNumber + "?",
                                    "You want me to get information for File number: " + FileNumber + "?");

                                context.Wait(ConfirmAndShowFileInformationAsync);
                            }
                            else
                            {
                                await context.SayAsync("Couldn't get 5 digit file number. Please enter valid file number.", "Couldn't get 5 digit file number. Please enter valid file number.");
                                await context.SayAsync("Please enter File No.", "Please Enter File Number");
                                context.Wait(ManageFileNumber);
                            }

                        }
                        else
                        {
                            await context.SayAsync("Please enter File No.", "Please Enter File Number");
                            context.Wait(ManageFileNumber);
                        }
                    }

                    catch (Exception ex)
                    {
                        await context.PostAsync(ex.Message);
                    }

                }

            }
            catch(Exception ex)
            {
                context.SayAsync(ex.InnerException.ToString());

            }
         

        }

        private async Task ConfirmAndShowFileInformationAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var internalMessage = await message;
            string msgtxt = internalMessage.Text;
            if(msgtxt.ToLower().Contains("yes") || msgtxt.ToLower().Contains("yah") || msgtxt.ToUpper().Equals("Y")|| msgtxt.ToLower().Contains("correct"))
            {
                //context.Wait(ShowFileInformationAsync);
                ShowFileInformationAsync(context);
               if( FileEntities!=null && FileEntities.Count>2) // setting this condition where we believe that flow is over as we found specific entities to show info
                     context.Done ("complete");

            }
            else if(msgtxt.ToLower().Contains("no") || msgtxt.ToLower().Contains("nah") || msgtxt.ToUpper().Equals("N")|| msgtxt.ToLower().Contains("ignore"))
            {
                context.Done("Done");

            }
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.SayAsync("I'm sorry, I don't understand what you mean.","Sorry, I don't understand your reply");
                    await context.PostAsync("You want me to get information for File no: " + FileNumber + "?", "You want me to get information for File Number" );

                    context.Wait(this.ConfirmAndShowFileInformationAsync);
                }
                else
                {
                    await context.PostAsync("I'm sorry, I don't understand what you mean. I have to close this conversation. Please try later.");
                    context.EndConversation("NO Proper Reply from user");
                }
                
            }
        }

        public async Task ManageFileNumber(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var internalMessage = await message;
            string msgtext = internalMessage.Text;
            string result = string.Empty;

            if (msgtext.Length == 5 && bFileNumberOnlyInMessage(msgtext))
            {
                FileNumber = msgtext;
                await context.SayAsync("You want me to get information for File no: " + msgtext + "?", "You want me to get information for File no: " + msgtext + " ? ");
                 //context.Wait(ShowFileInformationAsync);
                 context.Wait(ConfirmAndShowFileInformationAsync);
            }
            else if (msgtext.Length > 5)
            {

                Regex FileNo = new Regex(@"\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d");
                Match m = FileNo.Match(msgtext);
                if (m.Success)
                {
                    string tempFileNo = m.Value.Trim();
                    if (tempFileNo.Length == 5)
                    {
                        FileNumber = tempFileNo.ToString();
                       // context.Wait(ShowFileInformationAsync);
                        ShowFileInformationAsync(context);
                         
                    }
                    else
                        await context.SayAsync("File Number is more than 5 digit. Please enter valid File number.", "Please enter valid File number.");
                }
                else
                {
                    await context.SayAsync("Please enter File No.", "Please Enter File Number");
                }

                
            }
            else
            {
                await context.SayAsync("Please enter File No.", "Please Enter File Number");
            }

                //HttpClient client = new HttpClient();
                //client.BaseAddress = new Uri("http://localhost:56851/");

                //// Add an Accept header for JSON format.
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));

                //HttpResponseMessage response = client.GetAsync("api/User/" + FileNumber).Result;

                //if (response.IsSuccessStatusCode)
                //{
                //    var responseContent = await response.Content.ReadAsStringAsync();
                //    var strJson = Newtonsoft.Json.Linq.JToken.Parse(responseContent).ToString();
                //    // FileInfo listMessageCode = JsonConvert.DeserializeObject<FileInfo>(strJson);

                //    FileDetails = JsonConvert.DeserializeObject<FileInfo>(strJson);
                //    var message1 = context.MakeMessage();

                //    string strMsg = string.Empty;

                //    if (FileDetails != null)
                //    {
                //        strMsg += "\n\n\n\n";
                //        strMsg = "Staus: " + FileDetails.Status.ToString() + "\n\n";

                //        if (FileDetails.Buyers != null)
                //        {
                //            strMsg += "Buyers Count: " + FileDetails.Buyers.Count.ToString() + "\n\n";
                //        }
                //        if (FileDetails.Sellers != null)
                //        {
                //            strMsg += "Buyers Count: " + FileDetails.Sellers.Count.ToString() + "\n\n";
                //        }
                //        else
                //            strMsg += "Seller Count: " + "0" + "\n\n";
                //        if (FileDetails.Account != null)
                //        {
                //            strMsg += "Available Balance: " + FileDetails.Account.CurrentBalance.ToString() + "\n\n";
                //            strMsg += "Due: " + FileDetails.Account.FundDue.ToString() + Environment.NewLine;

                //        }
                //    }
                //    else
                //        strMsg = "Sorry, failed to retrieve File information for you. Please try again.";

                //    var heroCard = new HeroCard
                //    {
                //        Title = "Summary for File No: " + FileNumber,
                //        //Subtitle = FileDetails.Status,
                //        Text = strMsg,
                //        //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                //        Buttons = new List<CardAction> { new CardAction(ActionTypes.PostBack, "Buyers Info", value: "https://docs.microsoft.com/bot-framework"),
                //         new CardAction(ActionTypes.OpenUrl, "Seller Info", value: "https://docs.microsoft.com/bot-framework")
                //         }

                //        // SellerButton = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Sellers Info", value: "https://docs.microsoft.com/bot-framework") }
                //    };
                //    message1.Attachments.Add(heroCard.ToAttachment());
                //    await context.PostAsync(message1);
                //    await context.PostAsync("Do you want to have details of buyers or sellers");
                //    context.Wait(BuyerorSellerDetails);


                //}

             
        }

        //        private async Task ShowFileInformationAsync(IDialogContext context, IAwaitable<IMessageActivity> message)
        private  async Task ShowFileInformationAsync(IDialogContext context)
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
                            foreach(var entity in FileEntities)
                            {
                                if (string.Compare(entity.Type,BuyerOption,true)==0)
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
                                        await context.PostAsync("Failed to get seller information or there is no seller for mentioned file number.");
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
                        if(!bEntityFound)
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
                            this.ShowOptions(context);

                        } //else of bEntityFound
                        else
                        {
                            //context.Done("Done");
                        }


                    }//FileDetails !=Null
                     
                }
                else
                {
                    context.SayAsync("File Number not found. Please try again.", "File Number not found. Please try again.");
                    context.Done("Done");
                }

            }
            catch (Exception ex)
            {
                 context.PostAsync(ex.Message);
            }
            finally
            {
                 
            }
        }

        private bool bFileNumberOnlyInMessage(string msgtext)
        {            
                foreach (char c in msgtext)
                {
                    if (c < '0' || c > '9')
                        return false;
                }

                return true;
            
        }

        public async Task BuyerorSellerDetails(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var internalMessage = await message;
            string strType = internalMessage.Text;
            if (strType.ToLower().Contains("buyer")|| strType.ToLower().Contains("buyers"))
            {
                attempts = 3;
                strType = "Buyer";
                if (FileDetails.Buyers != null)
                {
                    foreach (var buyer in FileDetails.Buyers)
                    {
                        var message1 = context.MakeMessage();
                        var heroCard = new HeroCard
                        {
                            Title = "Buyer Details for file : " + FileNumber,
                            //Subtitle = FileDetails.Status,
                            Text = "First Name: " + buyer.FirstName + "\n\n" + "Last Name: " + buyer.LastName + "\n\n" + "Email Id: " + buyer.Email + "\n\n" + "Phone Number: " + buyer.PhoneNumber + "\n\n" + "Address: " + buyer.PostalAddress + "\n\n",
                            //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                        };
                        message1.Attachments.Add(heroCard.ToAttachment());
                        await context.PostAsync(message1);
                    }

                }
                else
                {
                    await context.PostAsync("Failed to get Buyer information");
                }
            }
             
            else if (strType.ToLower().Contains("seller") || strType.ToLower().Contains("sellers"))
                {
                attempts = 3;
                strType = "sellers";
                    if (FileDetails.Sellers != null )
                    {
                        foreach (var seller in FileDetails.Sellers)
                        {
                            var message1 = context.MakeMessage();
                            var heroCard = new HeroCard
                            {
                                Title = "Buyer Details for file : " + FileNumber,
                                //Subtitle = FileDetails.Status,
                                Text = "First Name: " + seller.FirstName + "\n\n" + "Last Name: " + seller.LastName + "\n\n" + "Email Id: " + seller.Email + "\n\n" + "Phone Number: " + seller.PhoneNumber + "\n\n" + "Address: " + seller.PostalAddress + "\n\n",
                                //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                            };
                            message1.Attachments.Add(heroCard.ToAttachment());
                            await context.PostAsync(message1);
                        }

                    }
                    else
                    {
                        await context.PostAsync("Failed to get seller information or there is no seller for mentioned file number.");
                    }
                }
            else
            {
                --attempts;
                if (attempts > 0)
                {
                    await context.PostAsync("I'm sorry, I don't understand your reply.");
                    context.Wait(this.BuyerorSellerDetails);
                }
                else
                {
                    await context.PostAsync("I'm sorry, I don't understand your reply. I have to close this conversation. Please try later.");
                    context.EndConversation("NO Proper Reply from user");
                }
            }
             
           // context.Done("Thank you");
        }
        
        private static Attachment GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "File Summary Card",
                Subtitle = "Your bots — wherever your users are talking",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are, from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") }
            };

            return heroCard.ToAttachment();
        }

        private Task Respond(IDialogContext context)
        {
            throw new NotImplementedException();
        }

        private void ShowOptions(IDialogContext context)
        {
            var descriptions = new List<string>() { "Buyer", "Seller", "Cancel" };
            var choices = new Dictionary<string, IReadOnlyList<string>>()
             {
                { "buyers", new List<string> { "Buyers", "Buyer", "only buyer", "buyer" } },
                { "sellers", new List<string> { "Sellers", "Seller", "only celler", "seller" } },
                { "cancel", new List<string> { "cancel", "don't want it", "Cancel","None","None of them","No" }
                } 
            };
            var promptOptions = new PromptOptionsWithSynonyms<string>
                (
                   "Please select one of the Option",
                   choices: choices,
                   descriptions: descriptions,
                    speak: SsmlWrapper.Wrap("Would you like to have more information about Buyer or Seller")
                   
                   
                );
            //PromptStyle style = PromptStyle.Keyboard;
            // PromptDialog.Choice(context, this.OnOptionSelected, new List<string>() { BuyerOption, SellerOption,CancelOption }, "Would you like to have more information about Buyer or Seller?", "Not a valid option", 3);
            PromptDialog.Choice(context, this.OnOptionSelected, promptOptions);
            
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case BuyerOption:
                        if (FileDetails.Buyers != null)
                        {
                            foreach (var buyer in FileDetails.Buyers)
                            {
                                var message1 = context.MakeMessage();
                                var heroCard = new HeroCard
                                {
                                    Title = "Buyer Details for file : " + FileNumber,
                                    //Subtitle = FileDetails.Status,
                                    Text = "First Name: " + buyer.FirstName + "\n\n" + "Last Name: " + buyer.LastName + "\n\n" + "Email Id: " + buyer.Email + "\n\n" + "Phone Number: " + buyer.PhoneNumber + "\n\n" + "Address: " + buyer.PostalAddress + "\n\n",
                                    //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                                };
                                message1.Attachments.Add(heroCard.ToAttachment());
                                await context.PostAsync(message1);
                            }

                        }
                        else
                        {
                            await context.PostAsync("Failed to get buyer information or there is no seller for mentioned file number.");
                        }
                        break;

                    case SellerOption:
                        if(FileDetails.Sellers != null)
                        {
                                foreach (var seller in FileDetails.Sellers)
                                {
                                    var message1 = context.MakeMessage();
                                    var heroCard = new HeroCard
                                    {
                                        Title = "Buyer Details for file : " + FileNumber,
                                        //Subtitle = FileDetails.Status,
                                        Text = "First Name: " + seller.FirstName + "\n\n" + "Last Name: " + seller.LastName + "\n\n" + "Email Id: " + seller.Email + "\n\n" + "Phone Number: " + seller.PhoneNumber + "\n\n" + "Address: " + seller.PostalAddress + "\n\n",
                                        //Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                                    };
                                    message1.Attachments.Add(heroCard.ToAttachment());
                                    await context.PostAsync(message1);
                                }

                            }
                            else
                            {
                                await context.PostAsync("Failed to get seller information or there is no seller for mentioned file number.");
                            }
                        break;
                    default:
                        await context.PostAsync("Failed to get information. Please try again.");
                        break;
                }
                attempts = 3;
                context.Wait(BuyerorSellerDetails);
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, you can start over again!");
                context.Done("Help Done");
                //context.Wait(this.MessageReceived);
            }
        }
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Microsoft.Bot.Connector.IMessageActivity> result)
        {

            var message = await result;
            var FileNo = string.Empty;
            var FileInfo = true;
            context.UserData.TryGetValue<string>("FileNumber", out FileNo);
            context.UserData.TryGetValue<bool>("FileInformation", out FileInfo);
            if (message.Text.ToLower().Contains("help") || message.Text.ToLower().Contains("support") || message.Text.ToLower().Contains("problem"))
            {
                //await context.Forward(new FastFileDialog(), this.ResumeAfterFileDialog, message, CancellationToken.None);
            }
            else
            if (FileInfo)
            {
                FileNo = message.Text;
                context.UserData.SetValue<string>("FileNumber", FileNo);
                context.UserData.SetValue<bool>("FileInformation", false);

            }
            context.Done(message);
        }

        private async Task ResumeAfterFileDialog(IDialogContext context, IAwaitable<object> result)
        {
            var  Number = await result;

           
            context.Wait(this.MessageReceivedAsync);
        }
        [LuisIntent("Greeting")]
        public async Task SeemsToBeAngry(IDialogContext context, LuisResult result)
        {
            context.Call(new AngryUserDialog(), ResumeAfterFileDialog);
        }

    }
}