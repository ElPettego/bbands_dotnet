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
}