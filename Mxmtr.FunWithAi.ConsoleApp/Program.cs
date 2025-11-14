using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Mxmtr.FunWithAi.ConsoleApp
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine("You included arg information:");
                
                foreach (var arg in args)
                {
                    Console.WriteLine($" - {arg}");
                }
            }

            await RunAiTimeAssistantAsync();
        }

        private static async Task RunAiTimeAssistantAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                Console.Write("Please enter your name: ");
                var name = Console.ReadLine() ?? "1";

                if (Regex.IsMatch(name, @"^\p{L}+$"))
                {
                    Console.WriteLine($"Hello {name}! Because I am artificial intelligent, I can remember your name!");
                    break;
                }

                Console.WriteLine("I don't believe that's your name so try again.");
            }

            Console.WriteLine("Press any key to make me your time assistant...");
            await Task.Run(() => Console.ReadKey(true), cancellationToken);
            DrainInputBuffer(); // clear any extra buffered keys from fast/accidental presses

            ClearConsoleAndDisplayTime();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var exitOnKeyTask = Task.Run(() => Console.ReadKey(true), cts.Token); // completes on any key to exit

            while (true)
            {
                var exitRequested = await WaitUntilNextMinuteOrExitAsync(exitOnKeyTask, cts.Token);
                
                if (exitRequested)
                {
                    await GoodbyeSequence(cts);

                    return;
                }

                ClearConsoleAndDisplayTime();
            }
        }

        private static async Task GoodbyeSequence(CancellationTokenSource cts)
        {
            await cts.CancelAsync();
            DrainInputBuffer();
            Console.WriteLine("Goodbye friend!");
            Beep();
            Thread.Sleep(3000);
            Beep(5);
        }

        private static async Task<bool> WaitUntilNextMinuteOrExitAsync(Task exitOnKeyTask, CancellationToken token)
        {
            var now = DateTime.Now;
            var nextMinute = now.AddSeconds(60 - now.Second).AddMilliseconds(-now.Millisecond);
            var delay = nextMinute - now;

            var delayTask = Task.Delay(delay, token);
            var completed = await Task.WhenAny(delayTask, exitOnKeyTask).ConfigureAwait(false);

            // true >> key pressed; false >> minute boundary reached
            return completed == exitOnKeyTask;
        }

        private static void ClearConsoleAndDisplayTime()
        {
            Console.Clear();
            Console.WriteLine($" It is {DateTime.Now:t}.  You are a nice friend.");
            Console.WriteLine("When you don't need me anymore, just press a key to stop...");
            Beep();
        }

        private static void Beep(int times = 1)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var frequency = 800;
                    var decrement = frequency / times;

                    for (var i = 0; i < times; i++)
                    {
                        Console.Beep(frequency, 300);
                        frequency -= decrement;

                        if (frequency < 200)
                        {
                            frequency = 200;
                        }
                    }

                    return;
                }
                catch
                {
                    // ignored
                }
            }

            for (var i = 0; i < times; i++)
            {
                Console.Write("\a");
            }
        }

        private static void DrainInputBuffer()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }
    }
}