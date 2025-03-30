using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace GitHubRepoFetcher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoriesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RepositoriesController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetRepositories()
        {
            string githubUsername = _configuration["GitHubUsername"];
            string url = $"http://api.github.com/users/{githubUsername}/repos";
            string githubToken = _configuration["GitHubToken"];

            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YourAppName", "1.0"));

            if (!string.IsNullOrEmpty(githubToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", githubToken);
            }

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch repositories.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var repositories = JsonSerializer.Deserialize<List<GitHubRepo>>(json);

            if (repositories == null || repositories.Count == 0)
            {
                return NoContent();
            }

            var repoList = repositories.ConvertAll(repo => new
            {
                Id = repo.HtmlUrl.Split('/').Last(),
                Name = repo.Name,
                Url = repo.HtmlUrl
            });

            return Ok(repoList);
        }

        [HttpGet("{repositoryId}/content")]
        public async Task<IActionResult> GetLabContent(string repositoryId)
        {
            try
            {
                string githubUsername = _configuration["GitHubUsername"];
                string githubToken = _configuration["GitHubToken"];
                string branch = "master";

                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("YourAppName", "1.0"));

                if (!string.IsNullOrEmpty(githubToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", githubToken);
                }

                string introUrl = $"http://raw.githubusercontent.com/{githubUsername}/{repositoryId}/{branch}/intro.md";

                var response = await _httpClient.GetAsync(introUrl);
                if (!response.IsSuccessStatusCode)
                {
                    introUrl = $"http://raw.githubusercontent.com/{githubUsername}/{repositoryId}/{branch}/intro.txt";
                    response = await _httpClient.GetAsync(introUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        introUrl = $"http://raw.githubusercontent.com/{githubUsername}/{repositoryId}/{branch}/intro.md";
                        response = await _httpClient.GetAsync(introUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            return NotFound($"intro niet gevonden voor repository: {repositoryId}");
                        }
                    }
                }

                var introContent = await response.Content.ReadAsStringAsync();
                return Ok(introContent);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound($"intro niet gevonden voor repository: {repositoryId}");
                }
                else
                {
                    return StatusCode(500, $"Interne serverfout: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Interne serverfout: {ex.Message}");
            }
        }

        [HttpGet("{repositoryId}/labfiles")]
        public async Task<IActionResult> GetLabFiles(string repositoryId)
        {
            try
            {
                string githubUsername = _configuration["GitHubUsername"];
                string githubToken = _configuration["GitHubToken"];
                string branch = "master";

                //Bepaal de branch
                if (repositoryId == "MS-4010-Extend-Microsoft365-Copilot-declarative-agents-VS-Code")
                {
                    branch = "main";
                }

                string labFilesUrl = $"http://api.github.com/repos/{githubUsername}/{repositoryId}/contents/Instructions/Labs?ref={branch}";

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
                    return StatusCode((int)response.StatusCode, $"Failed to fetch lab files for repository: {repositoryId}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var labFiles = JsonSerializer.Deserialize<List<LabFile>>(json);

                return Ok(labFiles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Interne serverfout: {ex.Message}");
            }
        }

        public class GitHubRepo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("html_url")]
            public string HtmlUrl { get;set; }
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
}