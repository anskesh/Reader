using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reader
{
    internal class Program
    {
        private static Reader reader;

        private static CancellationTokenSource _cancellation;

        public static void Main(string[] args)
        {
            _cancellation = new CancellationTokenSource();
            reader = new Reader("text", _cancellation.Token);

            var read = new Thread(ReadLoop);

            var write = new Thread(() => WriteLoop(() => _cancellation.Cancel(false)));

            read.Start();
            write.Start();

            while (!_cancellation.IsCancellationRequested)
            {
                Task.Yield();
            }
        }

        ~Program()
        {
            _cancellation?.Cancel(false);
        }

        private static async void WriteLoop(Action done)
        {
            await reader.WriteLoopDelay();
            done?.Invoke();
        }

        private static async void ReadLoop()
        {
            await reader.ReadLoopDelay(2000);
        }
    }
}