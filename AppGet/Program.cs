using Newtonsoft.Json;

namespace AppGet
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            string filePath = @"C:\Users\Renat\Documents\AppGet\AppGet\IPs.txt";
            List<string> ipList = new List<string>();

            GetIPsFromFile(filePath, ipList);

            await GetIpInfoAsync(ipList[0]);
        }

        public static async Task<IpData> GetIpInfoAsync(string ip)
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.Timeout = TimeSpan.FromSeconds(10);

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


