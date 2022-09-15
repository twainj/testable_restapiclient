using RestSharp;
using System.Text.Json;

namespace api.sdk.Dependencies.RestClient;

public class RestRequest : IRestRequest
{
	internal readonly RestSharp.RestRequest Request;

	private readonly JsonSerializerOptions _jsonOptions;

	internal RestRequest(string endpoint, HttpMethod method, JsonSerializerOptions jsonOptions) {
		Request = new RestSharp.RestRequest(endpoint, (Method)method);
		_jsonOptions = jsonOptions;
	}
	
	public HttpMethod Method {
		get => (HttpMethod)Request.Method;
		set => Request.Method = (RestSharp.Method)value;
	}

	public void AddHeader(string name, string value) {
		Request.AddHeader(name, value);
	}

	public void AddPostParameter(string name, string value) {
		Request.AddParameter(name, value, ParameterType.RequestBody);
	}

	public void AddJsonBody(object body) {
		Request.AddJsonBody(body);
	}

	public string Resource {
		get => Request.Resource;
		set => Request.Resource = value;
	}

	public string GetBody() {
		var p = Request.Parameters.GetParameters<JsonParameter>().SingleOrDefault();
		if (p?.Value != null) {
			return JsonSerializer.Serialize(p.Value, _jsonOptions);
		}

		return "";

	}

	public Dictionary<string, string?> GetHeaders() {
		Dictionary<string, string?> headers = Request.Parameters.GetParameters<HeaderParameter>()
			.ToDictionary(h => h.Name!, h => h.Value?.ToString());

		return headers;
	}
}