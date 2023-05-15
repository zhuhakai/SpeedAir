using AirFreightService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirFreightService
{
    public class App
    {
        private string _scheduleInfoFileName = "Schedules.txt";
        private string _orderInfoFileName = "Orders.json";

        private readonly int _maxOrdersPerFlight;
        private readonly ServiceUtil _serviceUtil;

        public App(ServiceUtil serviceUtil)
        {
            _serviceUtil = serviceUtil;
            _maxOrdersPerFlight = 20;
        }

        public async Task RunAsync(string[] args)
        {
           
            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (arg.StartsWith("sche="))
                    {
                        _scheduleInfoFileName = arg.Substring(5);
                    }
                    else if (arg.StartsWith("order="))
                    {
                        _orderInfoFileName = arg.Substring(6);
                    }
                }
            }
            var scheduledFlightInfo = await _serviceUtil.GetScheduledFlightInfoAsync(_scheduleInfoFileName);
            if (scheduledFlightInfo == null)
            {
                return;
            }

            PrintScheduleInfo(scheduledFlightInfo);

            var orderInfo = _serviceUtil.GetOrderInfo(_orderInfoFileName);

            Console.WriteLine();
            Console.WriteLine("=============================================================");
            Console.WriteLine();
            if (orderInfo == null)
            {
                return;
            }
            PrintOrderInfo(scheduledFlightInfo, orderInfo);
        }

        private void PrintScheduleInfo(Dictionary<string, List<FlightInfo>> ScheduledFlightInfo)
        {

            var strList = new List<string>();
            foreach (var sfi in ScheduledFlightInfo)
            {
                foreach (var fi in sfi.Value)
                {
                    strList.Add($"Flight: {fi.FlightNo}, departure: YUL, arrival: {sfi.Key}, day: {fi.DayNum}");
                }
            }

            strList.OrderBy(str => str).ToList().ForEach(str => { Console.WriteLine(str); });
        }

        private void PrintOrderInfo(
            Dictionary<string, List<FlightInfo>> ScheduledFlightInfo,
            Dictionary<string, OrderInfo> orderInfo)
        {

            foreach (var order in orderInfo)
            {
                var isScheduled = false;
                var airportCode = order.Value.Destination.ToUpperInvariant();
                if (ScheduledFlightInfo.ContainsKey(airportCode))
                {
                    foreach (var flightInfo in ScheduledFlightInfo[airportCode])
                    {
                        if(flightInfo.OrderCount < 20)
                        {
                            isScheduled = true;
                            flightInfo.OrderCount++;
                            Console.WriteLine($"order: {order.Key}, flightNumber: {flightInfo.FlightNo}, departure: YUL, arrival: {airportCode}, day: {flightInfo.DayNum}");
                            break;
                        }

                        
                    }

                    if (!isScheduled)
                    {
                        Console.WriteLine($"order: {order.Key}, flightNumber: not scheduled, departure: YUL, arrival: {airportCode},");
                    }
                }
                else
                {
                    Console.WriteLine($"order: {order.Key}, flightNumber: not scheduled, departure: YUL, arrival: {airportCode},");
                }
            }
            
        }
    }
}

