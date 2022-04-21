namespace TgPics.WebApiWrapper;

using HttpTracer;
using RestSharp;
using TgPics.Core.Models;
using TgPics.Core.Models.Requests;
using TgPics.Core.Models.Responses;
using TgPics.WebApiWrapper.Helpers;

public class TgPicsClient
{
    public TgPicsClient(string host, bool secure = true)
    {
        var options = new RestClientOptions(host)
        {
            ConfigureMessageHandler = handler =>
                new HttpTracerHandler(
                    handler,
                    new DebugLoggerButBetter(),
                    HttpMessageParts.ResponseHeaders | HttpMessageParts.RequestHeaders |
                    HttpMessageParts.ResponseHeaders | HttpMessageParts.RequestCookies)
        };

        if (!secure)
            options.RemoteCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;

        restClient = new RestClient(options);
    }

    public TgPicsClient(
        string host, string token, bool secure = true) : this(host, secure)
    {
        this.token = token;
    }

    private string token = string.Empty;
    private readonly RestClient restClient;


    public async Task<UsersAuthResponse> AuthAsync(
        UsersAuthRequest parameters) => await AuthAndSaveTokenAsync(parameters);

    public async Task<PostModel> AddPostAsync(PostsAddRequest parameters) =>
        await Post<PostsAddRequest, PostModel>(parameters, "api/posts/add", token);

    public async Task RemovePostAsync(IdRequest parameters) =>
        await Post(parameters, "api/posts/remove", token);

    public async Task<PostsGetAllResponse> GetAllPostsAsync() =>
        await Get<PostsGetAllResponse>("api/posts/getall", token);

    public async Task RemoveAllPostsAsync(PostsRemoveAllRequest parameters) =>
        await Post(parameters, "api/posts/removeall", token);

    public async Task<ListResponse<MediaFileInfo>> UploadFilesAsync(List<string> filePaths)
    {
        var request = new RestRequest("api/files/upload")
            .AddHeader("Authorization", $"Bearer {token}")
            .AddHeader("Content-Type", "multipart/form-data");

        request.AlwaysMultipartFormData = true;

        foreach (var path in filePaths)
            request.AddFile($"files", path);
        //[{Path.GetFileName(path)}]

        var response = await restClient.PostAsync(request);
        return ProceedResponse<ListResponse<MediaFileInfo>>(response);
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

        var response = await restClient.PostAsync(request);
        ProceedResponse<T>(response);
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

        var response = await restClient.PostAsync(request);
        return ProceedResponse<T2>(response);
    }

    protected async Task<T> Get<T>(string route, string token = "")
        where T : class
    {
        var request = new RestRequest(route);

        if (token != "")
            request.AddHeader("Authorization", $"Bearer {token}");

        var response = await restClient.GetAsync(request);
        return ProceedResponse<T>(response);
    }

    protected T ProceedResponse<T>(RestResponse response)
    {
        var code = (int)response.StatusCode;

        switch (code)
        {
            case int s when (s >= 200 && s <= 200):
                var deserialized = restClient.Deserialize<T>(response).Data;

                if (deserialized != null)
                    return deserialized;
                else
                    throw new Exception("Не удалось десериализовать ответ.");

            case 400:
                var error = restClient.Deserialize<MessageResponse>(response).Data;
                if (error != null)
                    throw new Exception(error.Message);
                else
                    throw new Exception($"Сервер вернул ошибку: {response.StatusCode}\n" +
                        $"{response.Content}");

            default:
                throw new Exception($"Сервер вернул ошибку: {response.StatusCode}\n" +
                    $"{response.Content}");
        }
    }
}
