using Newtonsoft.Json;

namespace AppGet
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // need write path to IPs.txt file
            string filePath = @"C:\Users\Renat\Documents\AppGet\AppGet\IPs.txt";
            List<string> ipList = new List<string>();
            Dictionary<string, IpData> countryDictionary = new Dictionary<string, IpData>();

            GetIPsFromFile(filePath, ipList);

            List<Task<IpData>> tasks = new List<Task<IpData>>();
            foreach (string ip in ipList)
            {
                tasks.Add(GetIpInfoAsync(ip));
            }

            IpData[] results = await Task.WhenAll(tasks);

            Dictionary<string, List<IpData>> groupedByCountry =
            GroupByCountry(results);

            foreach (var pair in groupedByCountry)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value.Count}");
            }

            string countryWithMostCities = null;
            int maxCityCount = 0;
            HashSet<string> citiesOfTopCountry = null;

            foreach (var countryGroup in groupedByCountry)
            {
                HashSet<string> uniqueCities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (IpData ip in countryGroup.Value)
                {
                    if (!string.IsNullOrEmpty(ip.City))
                    {
                        uniqueCities.Add(ip.City);
                    }
                }

                int cityCount = uniqueCities.Count;

                if (cityCount > maxCityCount)
                {
                    maxCityCount = cityCount;
                    countryWithMostCities = countryGroup.Key;
                    citiesOfTopCountry = uniqueCities;
                }
            }

            if (!string.IsNullOrEmpty(countryWithMostCities) && citiesOfTopCountry != null)
            {
                Console.WriteLine($"\n{countryWithMostCities}:");

                List<string> sortedCities = new List<string>(citiesOfTopCountry);
                sortedCities.Sort(StringComparer.OrdinalIgnoreCase);

                foreach (string city in sortedCities)
                {
                    Console.WriteLine($"- {city}");
                }
            }
            else
            {
                Console.WriteLine("Unable to find data about cities.");
            }
        }

        private static Dictionary<string, List<IpData>> GroupByCountry(IpData[] results)
        {
            Dictionary<string, List<IpData>> grouped =
                new Dictionary<string, List<IpData>>();

            foreach (IpData ipData in results)
            {
                if (ipData == null)
                    continue;

                if (string.IsNullOrEmpty(ipData.Country))
                    continue;

                if (!grouped.ContainsKey(ipData.Country))
                {
                    grouped[ipData.Country] = new List<IpData>();
                }

                grouped[ipData.Country].Add(ipData);
            }

            return grouped;
        }

        public static async Task<IpData> GetIpInfoAsync(string ip)
        {
            using HttpClient client = new HttpClient();

            string url = $"https://ipinfo.io/{ip}/json";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    IpData ipData = JsonConvert.DeserializeObject<IpData>(json);
                    return ipData;
                }
                else
                {
                    Console.WriteLine($"Error HTTP: {(int)response.StatusCode} {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return null;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Request timeout");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error JSON: {ex.Message}");
                return null;
            }
        }

        private static void GetIPsFromFile(string filePath, List<string> ipList)
        {
            if (File.Exists(filePath))
            {
                string[] allLines = File.ReadAllLines(filePath);

                foreach (string line in allLines)
                {
                    string cleanLine = line.Trim();
                    if (!string.IsNullOrEmpty(cleanLine))
                    {
                        ipList.Add(cleanLine);
                    }
                }
            }
            else
            {
                Console.WriteLine("The file was not found.: " + filePath);
            }
        }
    }
}


