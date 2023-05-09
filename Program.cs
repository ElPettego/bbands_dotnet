using System.Text.Json;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using TANet.Core;

internal class Program
{
    private static string TF = "15m";
    private static string LiveDataPath = @$"./data/btc_live_data_{TF}.csv";
    private static Uri WSSURI = new Uri($"wss://stream.binance.com:9443/ws/btcusdt@kline_{TF}");
    private static Logger logger = new Logger("DEBUG");
    private static Requests r = new Requests("https://www.binance.com");
    private static async global::System.Threading.Tasks.Task get_candles()
    {
        logger.log_mex("INFO", "Getting last 100 candles");
        DateTimeOffset now = DateTimeOffset.UtcNow;
        long unixTimeSeconds = now.ToUnixTimeMilliseconds();
        string url = $"/api/v3/uiKlines?symbol=BTCUSDT&interval={TF}&limit=100&endTime={unixTimeSeconds}";
        
        string response = await r.get_request(url);
        JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(response);

        // flushes the file
        using (FileStream stream = new FileStream(LiveDataPath, FileMode.Truncate)){}

        File.AppendAllText(LiveDataPath, "Date,Volume,Open,High,Low,Close\n");

        foreach (JsonElement element in jsonElement.EnumerateArray())
        {
            DateTimeOffset tstamp = DateTimeOffset.FromUnixTimeMilliseconds(Int64.Parse(element[0].ToString()));
            var volume = element[5].ToString().Substring(0, element[5].ToString().Length - 6);
            var open   = element[1].ToString().Substring(0, element[1].ToString().Length - 6);
            var high   = element[2].ToString().Substring(0, element[2].ToString().Length - 6);
            var low    = element[3].ToString().Substring(0, element[3].ToString().Length - 6);
            var close  = element[4].ToString().Substring(0, element[4].ToString().Length - 6);
            logger.log_mex("DEBUG", tstamp.ToString() + " " + volume + " " + close);
            string data = $"{tstamp.ToString()},{volume},{open},{high},{low},{close}\n";
            File.AppendAllText(LiveDataPath, data);
        }

    }
    private static void handle_mex(string mex)
    {   
        List<string> lines = File.ReadAllLines(LiveDataPath).ToList();

        JsonElement resj = JsonSerializer.Deserialize<JsonElement>(mex);
        logger.log_mex("DEBUG", resj.ToString());

        var date   = resj.GetProperty("k").GetProperty("t").ToString();
        var tstamp = DateTimeOffset.FromUnixTimeMilliseconds(Int64.Parse(date)).ToString();

        var volume = resj.GetProperty("k").GetProperty("v").ToString(); volume = volume.Substring(0, volume.Length -6);
        var open   = resj.GetProperty("k").GetProperty("o").ToString(); open   = open.Substring(0, open.Length -6);
        var high   = resj.GetProperty("k").GetProperty("h").ToString(); high   = high.Substring(0, high.Length -6);
        var low    = resj.GetProperty("k").GetProperty("l").ToString(); low    = low.Substring(0, low.Length -6);
        var close  = resj.GetProperty("k").GetProperty("c").ToString(); close  = close.Substring(0, close.Length -6);

        string _deb = $"{tstamp},{volume},{open},{high},{low},{close}";

        logger.log_mex("DEBUG", _deb);
        logger.log_mex("DEBUG", lines[lines.Count - 1]);

        if (lines[lines.Count - 1].Contains(tstamp)){
            lines[lines.Count - 1] = _deb;
        }
        else {
            lines.RemoveAt(1);
            lines.Add(_deb);
        }

        File.WriteAllLines(LiveDataPath, lines);
    }
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {        
        logger.log_mex("WARNING", "Starting the bot bruv");
        await get_candles();

        using (ClientWebSocket ws = new ClientWebSocket())
        {
            await ws.ConnectAsync(WSSURI, CancellationToken.None);
            logger.log_mex("INFO", $"Connected to {WSSURI}");
            byte[] buffer = new Byte[1024];
            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult res = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);                
                var msg = Encoding.UTF8.GetString(buffer, 0, res.Count);

                handle_mex(msg);
                
            }
        }


        

        
        // var bbands = Indicators.BollingerBands();
    }
}