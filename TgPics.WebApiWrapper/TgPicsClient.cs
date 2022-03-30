using HttpTracer;
using RestSharp;
using TgPics.Core.Models;
using TgPics.Core.Models.Requests;
using TgPics.Core.Models.Responses;
using TgPics.WebApiWrapper.Helpers;

namespace TgPics.WebApiWrapper;

public class TgPicsClient
{
    public TgPicsClient(string host)
    {
        var options = new RestClientOptions(host)
        {
            ConfigureMessageHandler = handler =>
                new HttpTracerHandler(
                    handler,
                    new FuckingWorkingDebugLogger(),
                    HttpMessageParts.All)
        };

        restClient = new RestClient(options);
    }

    public TgPicsClient(
        string host, string token) : this(host)
    {
        this.token = token;
    }

    private string token;
    private readonly RestClient restClient;


    public async Task<UsersAuthResponse> AuthAsync(
        UsersAuthRequest parameters) => await AuthAndSaveTokenAsync(parameters);

    public async Task AddPostAsync(PostsAddRequest parameters) =>
        await Post(parameters, "api/posts/add", token);

    public async Task RemovePostAsync(IdRequest parameters) =>
        await Post(parameters, "api/posts/remove", token);

    public async Task<PostsGetAllResponse> GetAllPostsAsync() =>
        await Get<PostsGetAllResponse>("api/posts/getall", token);

    public async Task RemoveAllPostsAsync(PostsRemoveAllRequest parameters) =>
        await Post(parameters, "api/posts/removeall", token);

    public async Task<List<MediaFileInfo>> UploadFilesAsync(List<string> filePaths)
    {
        var request = new RestRequest("api/files/upload")
            .AddHeader("Authorization", $"Bearer {token}")
            .AddHeader("Content-Type", "multipart/form-data");

        request.AlwaysMultipartFormData = true;

        foreach (var path in filePaths)
            request.AddFile($"files", path);
        //[{Path.GetFileName(path)}]

        return await restClient.PostAsync<List<MediaFileInfo>>(request);
    }

    private async Task<UsersAuthResponse> AuthAndSaveTokenAsync(
       UsersAuthRequest parameters)
    {
        var response = await Post<UsersAuthRequest, UsersAuthResponse>(
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
