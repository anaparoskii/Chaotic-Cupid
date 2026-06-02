using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Dobrodošli na haotičnog kupidona!");
Console.WriteLine("Kucajte /block <username> da blokirate osobu.");

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5108/messageService")
    .Build();

bool registrationSuccess = false;
bool waitingForConfirmation = false;

connection.On<string>("Error", (msg) =>
{
    Console.WriteLine($"[ERROR] {msg}");
    registrationSuccess = false;
});

connection.On<string>("Success", (msg) =>
{
    Console.WriteLine($"[SUCCESS] {msg}");
    registrationSuccess = true;
});

connection.On<string, string, int, string, string>("ReceiveLetter",
    (senderUsername, senderCity, senderAge, senderPhone, message) =>
    {
        Console.WriteLine("\nDobili ste pismo!");
        Console.WriteLine($"   Od:      {senderUsername}");
        Console.WriteLine($"   Grad:    {senderCity}");
        Console.WriteLine($"   Godine:  {senderAge}");
        if (!senderPhone.Equals("")) Console.WriteLine($"   Telefon: {senderPhone}");
        Console.WriteLine(message);
        Console.WriteLine("\nPritisnite ENTER da potvrdite prijem...");
        waitingForConfirmation = true;
    });

await connection.StartAsync();

while (!registrationSuccess)
{
    Console.Write("Korisničko ime: ");
    string username = Console.ReadLine() ?? "";
    while (string.IsNullOrWhiteSpace(username))
    {
        Console.WriteLine("Korisničko ime ne sme biti prazno!");
        Console.Write("Korisničko ime: ");
        username = Console.ReadLine() ?? "";
    }

    Console.Write("Grad: ");
    string city = Console.ReadLine() ?? "";
    while (string.IsNullOrWhiteSpace(city))
    {
        Console.WriteLine("Grad ne sme biti prazan!");
        Console.Write("Grad: ");
        city = Console.ReadLine() ?? "";
    }

    int age = 0;
    while (true)
    {
        Console.Write("Godine: ");
        string input = Console.ReadLine() ?? "";
        if (!int.TryParse(input, out age) || age <= 0)
            Console.WriteLine("Godine moraju biti broj veći od 0!");
        else break;
    }

    Console.Write("Broj telefona: ");
    string phone = Console.ReadLine() ?? "";
    while (string.IsNullOrWhiteSpace(phone))
    {
        Console.WriteLine("Broj telefona ne sme biti prazan!");
        Console.Write("Broj telefona: ");
        phone = Console.ReadLine() ?? "";
    }

    await connection.InvokeAsync("InitSinglePerson", username, city, age, phone);

    await Task.Delay(500);
}

while (true)
{
    string? command = Console.ReadLine();

    if (command == null) continue;

    else if (command == "" && waitingForConfirmation)
    {
        await connection.InvokeAsync("ConfirmReceivedLetter");
        waitingForConfirmation = false;
        Console.WriteLine("[SUBSCRIBER] Pismo potvrđeno.");
    }
    else if (command.StartsWith("/block "))
    {
        string target = command.Substring(7).Trim();
        await connection.InvokeAsync("BlockUser", target);
        Console.WriteLine($"[SUBSCRIBER] Blokirali ste {target}.");
    }
}