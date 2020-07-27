using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using NDesk.Options;

namespace IncotexDiscovery
{
    class Program
    {
        private static SerialPort ComPort = new SerialPort();

        private static string _portname;   //номер порта
        private static int _baudrate = 9600;        //скорость порта по-умолчанию
        private static int _serialtimeout = 100;          //таймаут порта

        private static bool _verbose = false;   //Вывод подробностей в ходе работы программы (полученных и отправленных фреймов, ошибок проверки CRC).

        private static int _timeout = 50;   //Timeout между запросами к счетчикам, мс

        private static int _found = 0;

        static void ConsoleMsg(string message, bool newline = false, string type = "info", bool time = true)
        {
            DateTime utcDate = DateTime.UtcNow;

            switch (type)
            {
                case "fail":
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case "success":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            if (newline)
            {
                Console.WriteLine(message);
            }
            else
            {
                if (time) Console.Write("[" + utcDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + " UTC] ");
                Console.Write(message);
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        static void waitExit()
        {
            Console.WriteLine("");

            ConsoleMsg("Press any key to exit.", true, "_", false);
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            OptionSet options = new OptionSet() {
                {"verbose|_verbose", v =>_verbose = v !=null},
                {"serial|_portname=", (string v) =>_portname = v},
                {"baudrate|_baudrate=", (int v) =>_baudrate = v},
                {"serialtimeout|_serialtimeout=", (int v) =>_serialtimeout = v},
                {"timeout|_timeout=", (int v) =>_timeout = v}
        };
            options.Parse(args);

            stopwatch.Start();
            try
            {
                ComPort.PortName = _portname;
                ComPort.BaudRate = _baudrate;
                ComPort.Parity = Parity.None;
                ComPort.DataBits = 8;
                ComPort.StopBits = StopBits.One;
                ComPort.ReadTimeout = _serialtimeout;
                ComPort.Open();
            }
            catch (Exception ex)
            {
                ConsoleMsg("FATAL ERROR: ", false, "fail", false);
                ConsoleMsg(ex.Message, true);

                waitExit();
            }

            ConsoleMsg("Discovering..", true);
            Console.WriteLine();

            MercuryPowerMeter pmeter = new MercuryPowerMeter(ComPort, _verbose, _timeout);

            for (int i = 1; i <= 240; i++)
            {
                pmeter.setAddress(i);

                string serial = "undefined";
                try
                {
                    if (!pmeter.Check()) continue;    //если счетчика нет в сети - переходим к следующему счетчику

                    if(!_verbose) ConsoleMsg("Found! Net address is " + i +".", false, "success", false);

                    if (!pmeter.Open())
                    {
                        ConsoleMsg(serial, true, "fail");
                        continue;     //если не удалось подключиться, то переходим к следующему счетчику
                    }

                    serial = pmeter.getSerialNumber();

                    if (!_verbose) ConsoleMsg(" Serial number is: " + serial+".", true, "success");

                    _found++;

                    //ConsoleMsg("Found power meter " + serial + " with net address is " + i, true, "success");

                    pmeter.Close();
                } catch (Exception e)
                {
                    ConsoleMsg("FATAL ERROR: ", false, "fail");
                    ConsoleMsg(e.Message, true);

                    waitExit();
                }
            }

            stopwatch.Stop();

            Console.WriteLine("");
            ConsoleMsg("Total found " + _found + " electricity power meters", true);
            Console.WriteLine("Time taken : {0}", stopwatch.Elapsed);
            waitExit();
        }
    }
}
