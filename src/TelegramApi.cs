public class TelegramApi
{
    private string chat_id;
    private string bot_token;
    private static Requests r = new Requests("https://api.telegram.org");
    public TelegramApi(string chat_id, string bot_token)
    {
        this.chat_id = chat_id;
        this.bot_token = bot_token;
    }

    public async void emit(string mex)
    {
        string url = $"/bot{this.bot_token}/sendMessage?";
        var values = new Dictionary<string, string> 
        {
            { "chat_id", this.chat_id },
            { "text", mex}
        };
        var res = await r.post_request(url, values);
    }
}