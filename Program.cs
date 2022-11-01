using System.Threading;

namespace Reader
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var reader = new Reader("text");
            var read = new Thread(delegate(object o) { reader.ReadLoopDelay(2000); });
            var write = new Thread(delegate(object o) { reader.WriteLoopDelay(20000); });
            
            read.Start();
            write.Start();
        }
    }
}