using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5108/messageService")
    .Build();

await connection.StartAsync();
Console.WriteLine("[PUBLISHER] Povezan!");

while (true)
{
    await Task.Delay(60000); // wait 60 second to send the next letter
    await connection.InvokeAsync("SendLetters");
    Console.WriteLine($"[PUBLISHER] Pisma poslata u {DateTime.Now}");
}