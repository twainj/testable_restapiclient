using api.sdk.Dependencies.RestClient;
using System.Text.Json;

namespace api.sdk;

public class MyApiClientConfiguration
{
	public MyApiClientConfiguration(string apiBaseUrl) {
		BaseUrl = apiBaseUrl;
	}

	public string BaseUrl { get; private set; }
	
	internal readonly JsonSerializerOptions JsonOptions = Serialization.Settings;

	internal IRestClient? RestClient { get; set; }

	public MyApiClient CreateClient() {
		RestClient = RestClient ?? new api.sdk.Dependencies.RestClient.RestClient(BaseUrl, JsonOptions);
		
		return new MyApiClient(this);
	}
}