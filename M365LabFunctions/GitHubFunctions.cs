using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class GitHubFunctions
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubFunctions> _logger;

    public GitHubFunctions(HttpClient httpClient, ILogger<GitHubFunctions> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [Function("GetRepositories")]
    public async Task<HttpResponseData> GetRepositories(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "repositories")] HttpRequestData req)
    {
        var githubUsername = Environment.GetEnvironmentVariable("GitHubUsername");
        var githubToken = Environment.GetEnvironmentVariable("GitHubToken");
        var url = $"https://api.github.com/users/{githubUsername}/repos";

        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YourAppName", "1.0"));

        if (!string.IsNullOrEmpty(githubToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", githubToken);
        }

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return req.CreateResponse(response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync();
        var repositories = JsonSerializer.Deserialize<List<GitHubRepo>>(json);

        var repoList = repositories?.Select(repo => new
        {
            Id = repo.HtmlUrl.Split('/').Last(),
            Name = repo.Name,
            Url = repo.HtmlUrl
        }).ToList();

        var responseData = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await responseData.WriteAsJsonAsync(repoList);
        return responseData;
    }

    [Function("GetLabContent")]
    public async Task<HttpResponseData> GetLabContent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "repositories/{repositoryId}/content")] HttpRequestData req,
        string repositoryId)
    {
        var githubUsername = Environment.GetEnvironmentVariable("GitHubUsername");
        var githubToken = Environment.GetEnvironmentVariable("GitHubToken");
        var branch = "master";

        var _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YourAppName", "1.0"));

        if (!string.IsNullOrEmpty(githubToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", githubToken);
        }

        var introUrl = $"https://raw.githubusercontent.com/{githubUsername}/{repositoryId}/{branch}/intro.md"; 

        var response = await _httpClient.GetAsync(introUrl);
        if (!response.IsSuccessStatusCode)
        {
            introUrl = $"https://raw.githubusercontent.com/{githubUsername}/{repositoryId}/{branch}/intro.txt"; 
            response = await _httpClient.GetAsync(introUrl);

            if (!response.IsSuccessStatusCode)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.NotFound);
            }
        }

        var introContent = await response.Content.ReadAsStringAsync();
        var httpResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await httpResponse.WriteStringAsync(introContent);
        return httpResponse;
    }

    [Function("GetLabFiles")]
    public async Task<HttpResponseData> GetLabFiles(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "repositories/{repositoryId}/labfiles")] HttpRequestData req,
        string repositoryId)
    {
        var githubUsername = Environment.GetEnvironmentVariable("GitHubUsername");
        var githubToken = Environment.GetEnvironmentVariable("GitHubToken");
        var branch = "master";

        if (repositoryId == "MS-4010-Extend-Microsoft365-Copilot-declarative-agents-VS-Code")
        {
            branch = "main";
        }

        var labFilesUrl = $"https://api.github.com/repos/{githubUsername}/{repositoryId}/contents/Instructions/Labs?ref={branch}"; 

        var _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YourAppName", "1.0"));

        if (!string.IsNullOrEmpty(githubToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", githubToken);
        }

        var response = await _httpClient.GetAsync(labFilesUrl);
        if (!response.IsSuccessStatusCode)
        {
            return req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
        }

        var json = await response.Content.ReadAsStringAsync();
        var labFiles = JsonSerializer.Deserialize<List<LabFile>>(json);

        var httpResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await httpResponse.WriteAsJsonAsync(labFiles);
        return httpResponse;
    }

    public class GitHubRepo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }
    }

    public class LabFile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("sha")]
        public string Sha { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("git_url")]
        public string GitUrl { get; set; }

        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

    public class Links
    {
        [JsonPropertyName("self")]
        public string Self { get; set; }

        [JsonPropertyName("git")]
        public string Git { get; set; }

        [JsonPropertyName("html")]
        public string Html { get; set; }
    }
}
