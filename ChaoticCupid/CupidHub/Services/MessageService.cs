using System.Security.Cryptography;
using CupidHub.Models;
using CupidHub.Services;
using Microsoft.AspNetCore.SignalR;

namespace CupidHub.Hubs
{
    public class MessageService : Hub, IPersonService, ICupidService
    {
        private readonly static Dictionary<string, Person> _subscribers = new();

        public async Task InitSinglePerson(string username, string city, int age, string phone) 
        {
            if (!_subscribers.ContainsKey(username))
            {
                var person = new Person(username, city, age, phone);
                person.ConnectionId = Context.ConnectionId;
                _subscribers.Add(username, person);
                Console.WriteLine($"[SERVER] korisnik {username} je uspešno prijavljen da prima pisma!");
                await Clients.Caller.SendAsync("Success", $"{username} uspešno registrovan!");
            }
            else
            {
                Console.WriteLine($"[SERVER] korisnik {username} već postoji!");
                await Clients.Caller.SendAsync("Error", $"{username} već postoji!");
                return;
            }
        }

        public async Task SendLetters()
        {
            Console.WriteLine("[SERVER] Šaljem pisma...");

            foreach (var receiver in _subscribers.Values)
            {
                if (!receiver.ConfirmedLastLetter) continue;
                Person? bestMatch = null;
                int bestScore = -1;
                foreach (var sender in _subscribers.Values)
                {
                    if (sender.Username.Equals(receiver.Username)) continue;
                    if (receiver.BlockedUsers.Contains(sender.Username)) continue;
                    int score = CountScore(receiver, sender);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = sender;
                    }
                }

                if (bestMatch != null)
                {
                    receiver.ConfirmedLastLetter = false;
                    string[] messages =
                    {
                        "Radujem se našem susretu!",
                        "Želim da se upoznamo.",
                        "Nisam zainteresovan/a za upoznavanje."
                    };
                    var rng = RandomNumberGenerator.Create();
                    byte[] buffer = new byte[1];
                    rng.GetBytes(buffer);
                    string message = messages[buffer[0] % 3];
                    bool showPhoneNumber = !message.Equals("Nisam zainteresovan/a za upoznavanje.");

                    await Clients.Client(receiver.ConnectionId)
                    .SendAsync("ReceiveLetter",
                        bestMatch.Username,
                        bestMatch.City,
                        bestMatch.Age,
                        showPhoneNumber ? bestMatch.PhoneNumber : "",
                        message);
                }
            }
        }

        public Task ConfirmReceivedLetter()
        {
            var person = _subscribers.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (person != null) person.ConfirmedLastLetter = true;
            return Task.CompletedTask;
        }

        public Task BlockUser(string username)
        {
            var person = _subscribers.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (person != null && !person.BlockedUsers.Contains(username))
            {
                person.BlockedUsers.Add(username);
                Console.WriteLine(
                    $"[SERVER] {person.Username} je blokirao {username}");
            }
            return Task.CompletedTask;
        }

        private int CountScore(Person person1, Person person2)
        {
            int score = 0;
            if (person1.City.Equals(person2.City)) score += 30;
            if (IsAgeSimilar(person1.Age, person2.Age)) score += 20;
            var rng = RandomNumberGenerator.Create();
            byte[] buffer = new byte[1];
            rng.GetBytes(buffer);
            score += buffer[0] % 101;
            return score;
        }

        private bool IsAgeSimilar(int age1, int age2)
        {
            return Math.Abs(age1 - age2) <= 2;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var person = _subscribers.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (person != null)
            {
                _subscribers.Remove(person.Username);
                Console.WriteLine($"[SERVER] {person.Username} se odjavio.");
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
