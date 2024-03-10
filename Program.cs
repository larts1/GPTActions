using Assistants;

var chat = new ChatRunner();
await chat.SyncAssistant();

while (true)
{
    var question = Console.ReadLine();
    var response = await chat.Ask(question);
    Console.WriteLine(response);
}