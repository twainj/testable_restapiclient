using RestSharp;
using RestSharp.Serializers.Json;
using System.Text.Json;
using RestSharpClient = RestSharp.RestClient;

namespace api.sdk.Dependencies.RestClient;

public class RestClient : IRestClient
{
	private readonly RestSharpClient _restClient;

	private readonly JsonSerializerOptions _jsonOptions;
	
	public RestClient(string baseUrl, JsonSerializerOptions jsonOptions) {
		_restClient = new RestSharpClient(new RestClientOptions {
			BaseUrl = new Uri(baseUrl),
			// TODO: I think this may only be valid for localhost/devcert runs. Can I isolate those cases somehow?
			RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
		});

		_jsonOptions = jsonOptions;
		_restClient.UseSystemTextJson(_jsonOptions);
	}

	public async Task<IRestResponse> ExecuteAsync(IRestRequest request, CancellationToken cancellationToken = default) {
		var response = await _restClient.ExecuteAsync(TranslateRequest(request), cancellationToken);

		return new RestResponse(response.StatusCode, response.ErrorMessage, response.Content);
	}

	private RestSharp.RestRequest TranslateRequest(IRestRequest request) {
		if (request is RestRequest ret) {
			return ret.Request;
		}

		throw new NotImplementedException("Mix-and-matching RestClient implementations is probably not a good idea. If you really want it, *you* implement this.");
	}
}