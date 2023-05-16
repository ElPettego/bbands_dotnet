using DotNetEnv;

public class _utils 
{
    public static float calculate_result(string long_short, float open_price, float close_price)
        {
            if (long_short.Equals("LONG"))
            {
                return (close_price - open_price)/open_price * 100;
            }
            else
            {
                return (open_price - close_price)/open_price * 100;
            }
        }
}
public class Trade
{
    public DateTimeOffset open_date, close_date;
    public float open_price, close_price;
    public string long_short;
    public bool open_trade = false;
    public float result;

    public Trade(string long_short, float open_price, DateTimeOffset open_date)
    {
        this.long_short = long_short;
        this.open_trade = true;
        this.open_price = open_price;
        this.open_date = open_date;
    }    

    public void close_trade(float close_price, DateTimeOffset close_date)
    {
        this.close_price = close_price;
        this.close_date  = close_date;
        this.open_trade  = false;
        this.result = (float) Math.Round(_utils.calculate_result(this.long_short, this.open_price, close_price), 2);
    }

    public override string ToString()
    {
        return $"{this.open_price},{this.open_date},{this.long_short},{this.result},{this.close_price},{this.close_date}";
    }
}

public class Agent 
{
    public Trade? current_trade;
    private TelegramApi telegramApi;

    private static string TradesPath = @"./data/trades.csv";
    
    #pragma warning disable CS8604
    public Agent()
    {
        Env.Load();
        this.telegramApi = new TelegramApi(
            Environment.GetEnvironmentVariable("CHAT_ID"),
            Environment.GetEnvironmentVariable("BOT_TOKEN")
        );
    }
    #pragma warning restore CS8604

    public void open_position(string long_short, float open_price, DateTimeOffset open_date)
    {
        this.current_trade = new Trade(long_short, open_price, open_date);
        string mex;
        if (long_short.Equals("LONG"))
        {
            mex = $"🚨 NEW TRADE 🚨\n\n🟩 LONG POSITION\n📄 ASSETT => BTCUSDT\n🕰️ OPEN DATE => {open_date}\n💰 OPEN PRICE => {open_price}";
        }
        else
        {
            mex = $"🚨 NEW TRADE 🚨\n\n🟥 SHORT POSITION\n📄 ASSETT => BTCUSDT\n🕰️ OPEN DATE => {open_date}\n💰 OPEN PRICE => {open_price}";
        }        
        this.telegramApi.emit(mex);
    }

    public void close_position(float close_price, DateTimeOffset close_date)
    {
        #pragma warning disable CS8602 
        this.current_trade.close_trade(close_price, close_date);
        #pragma warning restore CS8602
        string trade_result;
        string emoji;
        if (this.current_trade.result > 0)
        {
            trade_result = "WIN"; emoji = "🟩";
        } 
        else 
        {
            trade_result = "LOSS"; emoji = "🟥";
        }
        var mex = $"🚨 CLOSE POSITION 🚨\n\n📄 ASSETT => BTCUSDT\n{emoji} {trade_result}\n🕰️ OPEN DATE => {this.current_trade.open_date}\n🕰️ CLOSE DATE => {close_date}\n💰 OPEN PRICE => {this.current_trade.open_price}\n💰 CLOSE PRICE => {close_price}\n\n{emoji} RESULT => {this.current_trade.result}% 💵";
                 
        File.AppendAllText(TradesPath, $"{this.current_trade.ToString()}\n");
        this.telegramApi.emit(mex);
    }
}
