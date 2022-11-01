using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Reader
{
    public class Reader
    {
        private static Semaphore sem = new Semaphore(1, 1);
        private string _path;
        
        public Reader(string path)
        {
            _path = path;
        }
        
        private async Task Read()
        {
            sem.WaitOne();
            using (StreamReader reader = new StreamReader(_path))
            {
                string text = await reader.ReadToEndAsync();
                Console.WriteLine(text);
            }
            sem.Release();
        }

        public async void ReadLoopDelay(int miliseconds)
        {
            while (true)
            {
                await Read();
                await Task.Delay(miliseconds);
            }
        }
        
        public async void WriteLoopDelay(int miliseconds)
        {
            string line = Console.ReadLine();
            while (line != "0")
            {
                line = Console.ReadLine();
                await Write(line);
            }
        }

        private async Task Write(string line)
        {
            sem.WaitOne();
            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                await writer.WriteLineAsync(line);
            }

            sem.Release();
        }
    }
}