using System.Net;

namespace api.sdk.Dependencies.RestClient;

public interface IRestClient
{
	Task<IRestResponse> ExecuteAsync(IRestRequest request, CancellationToken cancellationToken = default);
}

public interface IRestRequest
{
	public HttpMethod Method {get; set; }
	public string Resource { get; set; }
	
	public void AddHeader(string name, string value);
	public void AddPostParameter(string name, string value);
	public void AddJsonBody(object body);

	public string GetBody();
	public Dictionary<string, string?> GetHeaders();
}

public interface IRestResponse
{
	public HttpStatusCode StatusCode { get; }
	public string? ErrorMessage { get; }
	public string? Content { get; }
}
