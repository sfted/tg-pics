namespace TgPics.WebApiWrapper;

using RestSharp;
using TgPics.WebApi.Models;

public class TgPicsClient
{
    public TgPicsClient(string host)
    {
        restClient = new RestClient(host);
    }

    public TgPicsClient(
        string host, string token) : this(host)
    {
        this.token = token;
    }

    private string token;
    private readonly RestClient restClient;


    public async Task<AuthenticateResponse> AuthAsync(
        AuthenticateRequest parameters) => await AuthAndSaveTokenAsync(parameters);

    public async Task AddPostAsync(AddPostRequest parameters) =>
        await Post(parameters, "api/posts/add", token);

    public async Task RemovePostAsync(RemovePostRequest parameters) =>
        await Post(parameters, "api/posts/remove", token);

    public async Task<GetAllPostsResponse> GetAllPostsAsync() =>
        await Get<GetAllPostsResponse>("api/posts/getall", token);

    public async Task RemoveAllPostsAsync(RemoveAllPostsRequest parameters) =>
        await Post(parameters, "api/posts/removeall", token);
    

    private async Task<AuthenticateResponse> AuthAndSaveTokenAsync(
       AuthenticateRequest parameters)
    {
        var response = await Post<AuthenticateRequest, AuthenticateResponse>(
            parameters, "api/auth");

        token = response.Token;
        return response;
    }


    // TODO: убрать повторяющийся код (token)
    protected async Task Post<T>(
        T parameters, string route, string token = "")
        where T : class
    {
        var request = new RestRequest(route)
            .AddJsonBody(parameters);

        if (token != "")
            request.AddHeader("Authorization", $"Bearer {token}");

        await restClient.PostAsync(request);
    }

    protected async Task<T2> Post<T1, T2>(
        T1 parameters, string route, string token = "")
        where T1 : class
        where T2 : class
    {
        var request = new RestRequest(route)
            .AddJsonBody(parameters);

        if (token != "")
            request.AddHeader("Authorization", $"Bearer {token}");

        return await restClient.PostAsync<T2>(request);
    }

    protected async Task<T> Get<T>(string route, string token = "")
        where T : class
    {
        var request = new RestRequest(route);

        if (token != "")
            request.AddHeader("Authorization", $"Bearer {token}");

        return await restClient.GetAsync<T>(request);
    }
}
