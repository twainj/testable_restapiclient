using System.Net;

namespace api.sdk.Dependencies.RestClient;

public class RestResponse : IRestResponse
{
	internal RestResponse(HttpStatusCode statusCode, string? errMsg = null, string? content = null) {
		StatusCode = statusCode;
		ErrorMessage = errMsg;
		Content = content;
	}
	
	public HttpStatusCode StatusCode { get; }
	public string? ErrorMessage { get; }
	public string? Content { get; }
}