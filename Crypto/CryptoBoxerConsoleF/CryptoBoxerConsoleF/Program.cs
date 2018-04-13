using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CryptoBoxer;

namespace CryptoBoxerConsoleF
{
    class Program
    {
        static void Main(string[] args)
        {
            Boxer m_boxer = Boxer.createBoxer(null);
            m_boxer.MainLoop();

            while (true)
            {
                System.Threading.Thread.Sleep(1);
            }
        }
    }
}
