using api.sdk.Dependencies.RestClient;
using AutoFixture;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace api.sdk;

public class MyApiClient_test
{
	private Fixture F = new Fixture();
	
	[Fact]
	public async Task MakeRequestAsync_body_ShouldMakeRestRequestWithSpecifiedParameters() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse(F.Create<HttpStatusCode>()));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (endpoint, body) = (F.Create<string>(), new {foo = "bar", stuff = "thing"});
		await sut.MakeRequestAsync<int>(HttpMethod.Get, endpoint, body);

		// Verify
		var encodedParams = @"{""foo"":""bar"",""stuff"":""thing""}";
		mockClient.Verify(x => x.ExecuteAsync(
			It.Is<IRestRequest>(r => r.Method == HttpMethod.Get 
				&& r.Resource == endpoint
				&& r.GetBody() == encodedParams
			),
			It.IsAny<CancellationToken>()
		));
	}
		[Fact]
	public async Task MakeRequestAsync_body_ShouldMakeRestRequestWithProperContentLength() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse(F.Create<HttpStatusCode>()));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (endpoint, body) = (F.Create<string>(), new {foo = "bar", stuff = "thing"});
		await sut.MakeRequestAsync<int>(HttpMethod.Get, endpoint, body);

		// Verify
		var encodedParams = @"{""foo"":""bar"",""stuff"":""thing""}";
		mockClient.Verify(x => x.ExecuteAsync(
			It.Is<IRestRequest>(r => r.Method == HttpMethod.Get 
				&& r.Resource == endpoint
				&& r.GetBody() == encodedParams
				&& r.GetHeaders().Any(p => p.Key == "Content-Length" && p.Value == encodedParams.Length.ToString())
			),
			It.IsAny<CancellationToken>()
		));
	}
	
	[Fact]
	public async Task MakeRequestAsync_dict_ShouldMakeRestRequestWithSpecifiedParameters() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse(F.Create<HttpStatusCode>()));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (method, endpoint) = (F.Create<HttpMethod>(), F.Create<string>());
		var dict = new Dictionary<string, string> {
			["foo"] = "bar",
			["stuff"] = "thing"
		};
		await sut.MakeRequestAsync<int>(method, endpoint, dict);
		
		// Verify
		var encodedParams = @"{""foo"":""bar"",""stuff"":""thing""}";
		mockClient.Verify(x => x.ExecuteAsync(It.Is<RestRequest>(r => r.Method == method 
			&& r.Resource == endpoint
			&& r.GetBody() == encodedParams
		), It.IsAny<CancellationToken>()));
	}
	
	[Fact]
	public async Task MakeRequestAsync_ShouldReturnSuccessOn200ResponseWithSuccessFlagTrueFromApi() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse(
				HttpStatusCode.OK,
				null,
				$@"{{""success"": true, ""result"": {F.Create<int>()}}}"
			));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (method, endpoint, body) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<int>(method, endpoint, body);
		
		// Verify
		Assert.True(response.Success);
	}
	
	[Fact]
	public async Task MakeRequestAsync_ShouldReturnFailureOn200ResponseWithSuccessFlagFalseFromApi() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse (
				HttpStatusCode.OK,
				null,
				$@"{{""success"": false, ""result"": {F.Create<int>()}}}"
			));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (method, endpoint, body) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<int>(method, endpoint, body);
		
		// Verify
		Assert.False(response.Success);
		Assert.Equal(HttpStatusCode.OK, response.ResponseCode);
	}
	
	///  - Should return failure on 400/500 response, with message set appropriately
	[Fact]
	public async Task MakeRequestAsync_ShouldReturnFailureOn400ResponseWithMessageSet() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		var errorMsg = F.Create<string>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse (
				HttpStatusCode.NotFound,
				errorMsg
		));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (method, endpoint, body) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<int>(method, endpoint, body);
		
		// Verify
		Assert.False(response.Success);
		Assert.Equal(errorMsg, response.Message);
	}
	
	[Fact]
	public async Task MakeRequestAsync_ShouldHaveLastErrorSetOnFailedRequest() {
		// Setup
		var mocklClient = new Mock<IRestClient>();
		var errorMsg = F.Create<string>();
		mocklClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse (
				HttpStatusCode.InternalServerError,
				errorMsg
		));
		var sut = SetupSutWithMockRestClient(mocklClient);
		
		// Test
		var (m, e, b) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<int>(m, e, b);
		
		// Verify
		Assert.False(response.Success);
		Assert.Equal(errorMsg, sut.LastError);
	}

	[Fact]
	public async Task MakeRequestAsync_ShouldHaveLastErrorDefaultToErrorCodeStringWhenFailedButNoDescriptiveMessage() {
		// Setup
		var mocklClient = new Mock<IRestClient>();
		var errorMsg = F.Create<string>();
		mocklClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse (HttpStatusCode.InternalServerError));
		var sut = SetupSutWithMockRestClient(mocklClient);
		
		// Test
		var (m, e, b) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<int>(m, e, b);
		
		// Verify
		Assert.False(response.Success);
		Assert.Equal("Internal Server Error", sut.LastError);
	}
	
	[Fact]
	public async Task MakeRequestAsync_ShouldHaveLastErrorResetOnSuccessfulRequest() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse (
				HttpStatusCode.OK
			));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (m, e, b) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<int>(m, e, b);
	
		// Verify
		Assert.True(response.Success);
		Assert.Null(sut.LastError);
	}
	
	///  - On successful response should have deserialized object result
	[Fact]
	public async Task MakeRequestAsync_ShouldReturnDeserializedObjectResult() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		var testObj = F.Create<TestReturnValue>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse (
				HttpStatusCode.OK,
				null,
				JsonSerializer.Serialize(new {
					success = true,
					result = testObj
				})
			));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (method, endpoint, body) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<TestReturnValue>(method, endpoint, body);
		
		// Verify
		Assert.NotNull(response.Result);
		Assert.Equal(testObj.Foo, response.Result!.Foo);
		Assert.Equal(testObj.Bar, response.Result!.Bar);
		Assert.Equal(testObj.Baz, response.Result!.Baz);
	}

	[Fact]
	public async Task MakeRequestAsync_ShouldReturnNullResultIfEndpointDoesntSupplyIt() {
		// Setup
		var mockClient = new Mock<IRestClient>();
		var testObj = F.Create<TestReturnValue>();
		mockClient.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((RestRequest _, CancellationToken _) => new RestResponse (
				HttpStatusCode.OK,
				null,
				JsonSerializer.Serialize(new {
					success = true
				})
			));
		var sut = SetupSutWithMockRestClient(mockClient);
		
		// Test
		var (method, endpoint, body) = (F.Create<HttpMethod>(), F.Create<string>(), F.Create<string>());
		var response = await sut.MakeRequestAsync<TestReturnValue>(method, endpoint, body);
		
		// Verify
		Assert.Null(response.Result);
	}
	

	#region Helpers

	class TestReturnValue
	{
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		public string? Foo { get; set; }
		public int Bar { get; set; }
		public bool Baz { get; set; }
		// ReSharper restore UnusedAutoPropertyAccessor.Local
	}
	
	internal static MyApiClient SetupSutWithMockRestClient(Mock<IRestClient> mockClient) {
		var clientConfig = new MyApiClientConfiguration("https://test.local");
		clientConfig.RestClient = mockClient.Object;
		return clientConfig.CreateClient();
	}
	
	#endregion
}