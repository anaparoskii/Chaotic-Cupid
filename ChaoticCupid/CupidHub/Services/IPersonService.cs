namespace CupidHub.Services
{
    public interface IPersonService
    {
        Task InitSinglePerson(string username, string city, int age, string phone);
        Task ConfirmReceivedLetter();
        Task BlockUser(string username);
    }
}
