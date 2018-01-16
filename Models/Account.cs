using System;
namespace FASTBOT.Models
{
    [Serializable]
    public class Account
    {
        public double CurrentBalance { get; set; }
        public double FundDue { get; set; }
    }
}