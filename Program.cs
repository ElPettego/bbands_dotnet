using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
using System.Diagnostics;
using DotNetEnv;

internal class Program
{
    private static int LB = 10;
    private static string? PythonPath;
    private static string TF = "15m";
    private static string LiveDataPath = @$"./data/btc_live_data_{TF}.csv";
    private static string LiveDataPathInds = @$"./data/btc_live_data_{TF}_inds.csv";
    private static Uri WSSURI = new Uri($"wss://stream.binance.com:9443/ws/btcusdt@kline_{TF}");
    private static TelegramApi tgapi = new TelegramApi("-1001642976010", "5843509254:AAFYSy1dX5GvQEzrm5PGv7aaNDCxUfa5p8k");
    private static Logger logger = new Logger("INFO");
    private static Requests r = new Requests("https://www.binance.com");
    private static Agent agent = new Agent();
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
    private static bool handle_mex(string mex)
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

        bool ret = false;
        // return a bool for new candle?
        if (lines[lines.Count - 1].Contains(tstamp)){
            lines[lines.Count - 1] = _deb;
        }
        else {
            lines.RemoveAt(1);
            lines.Add(_deb);
            ret = true;
        }

        File.WriteAllLines(LiveDataPath, lines);
        return ret;
    }
    private static async global::System.Threading.Tasks.Task Main(string[] args)
    {        
        logger.log_mex("WARNING", "Starting the bot bruv");

        // LOADING STUFF
        Env.Load();
        #pragma warning disable CS8601
        PythonPath = Environment.GetEnvironmentVariable("PYTHON_PATH"); 
        #pragma warning restore CS8601
        var StartInfo = new ProcessStartInfo
        {
            FileName = PythonPath,
            Arguments = "calculate_inds.py",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = false,
        };
        
        
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

                bool eval = handle_mex(msg);

                if (eval)
                {                    
                    var pr = Process.Start(StartInfo);
                    #pragma warning disable CS8602
                    pr.WaitForExit();
                    #pragma warning restore CS8602

                    decimal[] close = CsvParser.ParseCsvColumn(LiveDataPathInds, 11, 5,  LB);
                    decimal[] ema   = CsvParser.ParseCsvColumn(LiveDataPathInds, 11, 6,  LB);
                    decimal[] rsi   = CsvParser.ParseCsvColumn(LiveDataPathInds, 11, 7,  LB);
                    decimal[] bbu   = CsvParser.ParseCsvColumn(LiveDataPathInds, 11, 9,  LB);
                    decimal[] bbl   = CsvParser.ParseCsvColumn(LiveDataPathInds, 11, 10, LB);

                    var _close = close[LB-2];
                    var _bbu   = bbu[LB-2]; 
                    var _bbl   = bbl[LB-2]; 
                    var _ema   = ema[LB-2]; 
                    var _rsi   = rsi[LB-2];

                    Console.BackgroundColor = ConsoleColor.Magenta;
                    string time = DateTimeOffset.Now.ToString();
                    Console.Write($"{time}");
                    Console.ResetColor();
                    Console.Write(" - ");
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"INFO");
                    Console.ResetColor();
                    Console.Write($" - PRICE: {_close} - EMA: {_ema} - BBU: {_bbu} - BBL: {_bbl} - RSI: {_rsi}\r");

                    if (agent.current_trade == null || !agent.current_trade.open_trade)
                    {
                        if (_bbu < _close & _rsi > 70)
                        {
                            agent.open_position("LONG", (float)_close, DateTimeOffset.Now);
                            logger.log_mex("WARNING", $"OPENING TRADE -> SIDE: LONG - PRICE: {_close}");
                        }
                        if (_bbu > _close & _rsi < 30)
                        {
                            agent.open_position("SHORT", (float)_close, DateTimeOffset.Now);
                            logger.log_mex("WARNING", $"OPENING TRADE -> SIDE: SHORT - PRICE: {_close}");
                        }
                        continue;
                    }
                    if (agent.current_trade.open_trade)
                    {
                        float cur_pl = _utils.calculate_result(
                            agent.current_trade.long_short, 
                            agent.current_trade.open_price, 
                            (float)_close);

                        if(agent.current_trade.long_short.Equals("LONG"))
                        {
                            if (_close < _ema)
                            {
                                agent.close_position((float)_close, DateTimeOffset.Now);
                                logger.log_mex("WARNING", $"CLOSING TRADE -> SIDE: LONG - PRICE: {_close} - RESULT: {cur_pl}");
                            }
                        }
                        if(agent.current_trade.long_short.Equals("SHORT"))
                        {
                            if (_close > _ema)
                            {
                                agent.close_position((float)_close, DateTimeOffset.Now);
                                logger.log_mex("WARNING", $"CLOSING TRADE -> SIDE: SHORT - PRICE: {_close} - RESULT: {cur_pl}");
                            }
                        }
                    }
                }

                
                
                
                
            }
        }      
    }
}