using api.sdk.Dependencies.RestClient;
using api.sdk.Extensions;
using System.Text.Json;
using System.Threading.Tasks;

namespace api.sdk;

public class MyApiClient : IMyApiClient
{
	private readonly MyApiClientConfiguration _config;
	private readonly IRestClient _client;
	internal MyApiClient(MyApiClientConfiguration config) {

		_config = config;
		
		_client = config.RestClient!;
	}

	public string? LastError { get; internal set; }
	internal string? Token { get; set; }

	internal async Task<RestResult<T>> MakeRequestAsync<T>(HttpMethod method, string endpoint, object? body = null) {
		var request = GetRequest(method, endpoint);
		if (body != null) {
			request.AddJsonBody(body);
			 request.AddHeader("Content-Length", request.GetBody().Length.ToString());
		}

		if (!string.IsNullOrEmpty(Token)) {
			request.AddHeader("Authorization", $"Bearer {Token}");
		}

		return await ExecuteRequestAsync<T>(request);
	}

	private async Task<RestResult<T>> ExecuteRequestAsync<T>(IRestRequest request) {
		IRestResponse response = await _client.ExecuteAsync(request);
		var ret = new RestResult<T> {
			Success = (int)response.StatusCode >= 200 && (int)response.StatusCode <= 299,
			ResponseCode = response.StatusCode,
			Message = response.ErrorMessage
		};
		LastError = !ret.Success && string.IsNullOrEmpty(response.ErrorMessage) 
			? ret.DisplayResponseCode() 
			: response.ErrorMessage;
		
		if (ret.Success && !string.IsNullOrEmpty(response.Content)) {
			var doc = JsonDocument.Parse(response.Content);
			ret.Success &= doc.RootElement.GetProperty("success").GetBoolean();
			if(doc.RootElement.TryGetProperty("message", out var msgElement))
				ret.Message = msgElement.GetString();
			if(doc.RootElement.TryGetProperty("result", out var resultEl))
				ret.Result = resultEl.Deserialize<T>(_config.JsonOptions);
		}
		

		return ret;
	}

	internal RestRequest GetRequest(HttpMethod method, string endpoint) {
		var ret = new RestRequest(endpoint, method, _config.JsonOptions);
		ret.AddHeader("cache-control", "no-cache");

		return ret;
	}
}