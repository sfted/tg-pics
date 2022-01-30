namespace TgPics.WebApiWrapper;

using RestSharp;
using TgPics.WebApi.Models;

public class TgPicsClient
{
    public TgPicsClient(string host)
    {
        this.host = host;
        restClient = new RestClient(host);
    }

    public TgPicsClient(
        string host, string token) : this(host)
    {
        this.token = token;
    }

    private readonly string host;
    private string token;
    private RestClient restClient;

    public async Task<AuthenticateResponse> AuthAsync(
        AuthenticateRequest parameters)
    {
        var request = new RestRequest("api/auth")
            .AddJsonBody(parameters);

        var response = await restClient
            .PostAsync<AuthenticateResponse>(request);

        token = response.Token;
        return response;
    }

    public async Task AddPostAsync(AddPostRequest parameters) =>
        await Post(parameters, "api/posts/add");

    public async Task RemovePostAsync(RemovePostRequest parameters) =>
        await Post(parameters, "api/posts/remove");

    public async Task<GetAllPostsResponse> GetAllPostsAsync() =>
        await Get<GetAllPostsResponse>("api/posts/getall");

    public async Task RemoveAllPostsAsync(RemoveAllPostsRequest parameters) =>
        await Post(parameters, "api/posts/removeall");


    protected async Task Post<T>(T parameters, string route)
        where T : class
    {
        var request = new RestRequest(route)
            .AddJsonBody(parameters);

        await restClient.PostAsync(request);
    }

    protected async Task<T> Get<T>(string route)
        where T : class
    {
        var request = new RestRequest(route);

        return await restClient.GetAsync<T>(request);
    }
}
