public class Requests {
    private HttpClient client;
    
    public Requests(string BaseAddress){
        this.client = new HttpClient();
        this.client.BaseAddress = new Uri(BaseAddress);

    }
    public async Task<string> get_request(string url){
        var response = await this.client.GetStringAsync(url);
        return response;

    }

    public async Task<string> post_request(string url, Dictionary<string, string> content){
        var _content = new FormUrlEncodedContent(content);
        var response = await this.client.PostAsync(url, _content);
        var _str_res = await response.Content.ReadAsStringAsync();
        return _str_res;

    }
}