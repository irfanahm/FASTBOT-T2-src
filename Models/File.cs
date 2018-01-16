using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Web;

namespace FASTBOT.Models
{
    [Serializable]

    public class FileInfo
    {
        // File Number is the key
        public int FileNumber { get; set; }
        
        public string Status { get; set; }
        public List<Buyer> Buyers { get; set; }
        public List<Seller> Sellers { get; set; }
        public Account Account { get; set; }

        public static IForm<FileInfo> BuildForm()
        {
            return new FormBuilder<FileInfo>()
                .Message("Welcome to Fast Bot")
                .Build();

        }
    }
}
