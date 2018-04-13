using System;

using CryptoBoxer;

namespace CryptoBoxerConsoleCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Boxer m_boxer = Boxer.createBoxer(null);
            m_boxer.MainLoop();

            while (true)
            {
                System.Threading.Thread.Sleep(0);
            }
            //Console.WriteLine("Hello World!");
        }
    }
}
