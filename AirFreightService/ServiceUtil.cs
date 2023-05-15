using AirFreightService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AirFreightService
{

    public enum ActionResult
    {
        Success,
        Failed,
        FileNotExists,
        NotInitialized
    }
    public class ServiceUtil
    {
        public const string DayPrefix = "Day";
        public const string FlightPrefix = "Flight";

        //public const string FlightInfoFileName = "FlightInfo.json";

        private readonly ILogger<ServiceUtil> _logger;
        
        public ServiceUtil(ILogger<ServiceUtil> logger)
        {
            _logger = logger;
            
        }



        public async Task<Dictionary<string, List<FlightInfo>>?> GetScheduledFlightInfoAsync(string fileName)
        {
            try {
                var flightInfo = new Dictionary<string, List<FlightInfo>>();

                using (var sr = new StreamReader(fileName))
                {
                    string? line;
                    string dayNum = string.Empty;

                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            if (line.StartsWith(DayPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!TryGetIntVal(line, ref dayNum))
                                {
                                    _logger.LogError($"There is no day number, line=\"{line}\"");
                                    return null;
                                }


                            }
                            else if (line.StartsWith(FlightPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                var strTokens = line.Split(':');

                                if (strTokens.Length != 2)
                                {
                                    _logger.LogError($"There is no colon separator for the flight string, line=\"{line}\"");
                                    return null;
                                }

                                string flightNum = string.Empty;
                                if (!TryGetIntVal(strTokens[0], ref flightNum))
                                {
                                    _logger.LogError($"There is no flight number, line=\"{line}\"");
                                    return null;
                                }

                                //var match = Regex.Match(strTokens[1], @"\((.*?)\)");

                                var startIndex = strTokens[1].LastIndexOf('(');
                                var endIndex = strTokens[1].LastIndexOf(")");

                                if (startIndex == -1 || endIndex == -1)
                                {
                                    _logger.LogError($"Can not find the destination airport code, line=\"{line}\"");
                                    return null;
                                }

                                var airportCode = strTokens[1].Substring(startIndex + 1, endIndex - startIndex - 1).ToUpperInvariant();

                                if (flightInfo.ContainsKey(airportCode))
                                {
                                    flightInfo[airportCode].Add(new FlightInfo()
                                    {
                                        FlightNo = flightNum,
                                        DayNum = dayNum,
                                        OrderCount = 0
                                    });
                                }
                                else
                                {

                                    flightInfo.Add(airportCode, new List<FlightInfo>() { new FlightInfo() { 
                                        FlightNo = flightNum,
                                        DayNum = dayNum,
                                        OrderCount = 0
                                    } });
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Unrecognized info, line=\"{line}\"");
                            }
                        }
                    }


                }


                return flightInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetScheduledFlightInfoAsync)} failed");
                return null;
            }
        }

        public Dictionary<string, OrderInfo>? GetOrderInfo(string fileName)
        {
            try
            {
                var opts = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true
                };

                var ordersDict = JsonSerializer.Deserialize<Dictionary<string, OrderInfo>>(
                        File.ReadAllText(fileName), opts
                    );

                return ordersDict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(GetOrderInfo)} failed");
                return null;
            }
        }


        private bool TryGetIntVal(string strToken, ref string dayNum)
        {
            var match = Regex.Match(strToken, @"\d+");
            if (match.Success)
            {
                dayNum = match.Value;
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
