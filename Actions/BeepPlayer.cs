namespace Assistants;

public class Test
{
    [Function(
        name: "play_beep_sound",
        description: "Sounds default console beep")
    ]
    public static string PlayBeepSound(
        [Parameter("The frequency of the beep")] string frequency,
        [Parameter("The duration of the beep")] string duration,
        [Parameter("The number of times to beep")] string times)
    {
        for (int i = 0; i < int.Parse(times); i++)
        {
            Console.Beep(int.Parse(frequency), int.Parse(duration));
        }
        return "done";
    }
}