using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Reader
{
    public class Reader
    {
        public class InterlockedConsole
        {
            private SemaphoreSlim _semaphore;
            private int _left;
            private int _top;

            public InterlockedConsole()
            {
                _semaphore = new SemaphoreSlim(1, 1);
            }

            public void WriteLine(string value)
            {
                Console.WriteLine(value);
            }

            public async Task<ConsoleKeyInfo> ReadKey(bool intercept)
            {
                await _semaphore.WaitAsync();
                var key = Console.ReadKey(intercept);
                _semaphore.Release();
                return key;
            }

            public async Task<string> ReadLine()
            {
                await _semaphore.WaitAsync();
                var line = Console.ReadLine();

                _semaphore.Release();
                return line;
            }

            public void ResetCursorAfterClear()
            {
                if (_top == 0) return;
                Console.SetCursorPosition(_left, _top);
            }

            public void Clear()
            {
                _left = Console.CursorLeft;
                _top = Console.CursorTop;
                for (int i = 0; i < _top; i++)
                {
                    Console.SetCursorPosition(0, i);
                    Console.WriteLine(new string(' ', Console.WindowWidth));
                }
                Console.SetCursorPosition(0, 0);
            }
        }

        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1, 1);
        private readonly CancellationToken _cancellationToken;
        private readonly InterlockedConsole _console = new InterlockedConsole();
        private readonly string _path;

        public Reader(string path, CancellationToken cancellationToken)
        {
            _path = path;
            _cancellationToken = cancellationToken;
        }

        private async Task Read()
        {
            await _sem.WaitAsync(_cancellationToken);
            using (var reader = new StreamReader(_path))
            {
                var text = await reader.ReadToEndAsync();
                _console.Clear();
                _console.WriteLine(text);
                _console.ResetCursorAfterClear();
            }

            _sem.Release();
        }

        public async Task ReadLoopDelay(int milliseconds)
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await Read();
                await Task.Delay(milliseconds, _cancellationToken);
            }
        }

        public async Task WriteLoopDelay()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var line = await _console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    return;
                }

                await Write(line);
                await Task.Yield();
            }
        }

        private async Task Write(string line)
        {
            await _sem.WaitAsync(_cancellationToken);
            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                await writer.WriteLineAsync(line);
            }

            _sem.Release();
        }
    }
}