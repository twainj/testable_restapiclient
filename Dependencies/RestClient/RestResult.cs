using System.Net;

namespace api.sdk.Dependencies.RestClient;

public class RestResult
{
    public bool Success { get; set; }
    public HttpStatusCode ResponseCode { get; set; }
    public string? Message { get; set; }
}

public class RestResult<T> : RestResult
{
    public T? Result { get; set; }
}