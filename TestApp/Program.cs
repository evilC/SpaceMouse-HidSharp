using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceMouse_HidSharp;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var sm = new SpaceMouse();
            sm.WatchDevice(0x046d, 0xc62b);
        }
    }
}
