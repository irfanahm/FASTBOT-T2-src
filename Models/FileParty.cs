using System;
namespace FASTBOT.Models
{
    [Serializable]
    public class FileParty
    {
        // there is no specific key, typically identified by name

        // Id is just a database identifier, and this is hidden from users
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{LastName}, {FirstName}";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PostalAddress { get; set; }
    }
}