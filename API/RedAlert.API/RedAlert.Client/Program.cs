using System;
using System.IO.Ports;

namespace RedAlert.Client
{
    class Program
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            using (var mySerialPort = new SerialPort("COM6"))
            {
                mySerialPort.Open();

                while (true)
                {
                    var input = Console.ReadLine();

                    if (input == "q")
                    {
                        break;
                    }

                    mySerialPort.Write(input);
                }
            }
        }
    }
}
