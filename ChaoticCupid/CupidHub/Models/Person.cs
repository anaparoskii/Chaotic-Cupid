namespace CupidHub.Models
{
    public class Person
    {
        public string Username { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int Age { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool ConfirmedLastLetter { get; set; } = true;
        public List<string> BlockedUsers { get; set; } = new();
        public string ConnectionId { get; set; } = string.Empty;

        public Person(string username, string city, int age, string phoneNumber)
        {
            Username = username;
            City = city;
            Age = age;
            PhoneNumber = phoneNumber;
        }
    }
}
