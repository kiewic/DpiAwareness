using System.Threading;

namespace DpiAwarenessApp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                DpiUtils.CheckOnInterval();
                Thread.Sleep(1000);
            }
        }
    }
}
