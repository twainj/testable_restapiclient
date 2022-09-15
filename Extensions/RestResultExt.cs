using api.sdk.Dependencies.RestClient;
using System.Net;
using System.Text.RegularExpressions;

namespace api.sdk.Extensions;

// ReSharper disable InconsistentNaming
public static class RestResultExt
{
	public static bool Is2xxCode(this RestResult response) {
		return (int)response.ResponseCode >= 200 && (int)response.ResponseCode <= 299;
	}

	public static bool Is3xxCode(this RestResult response) {
		return (int)response.ResponseCode >= 300 && (int)response.ResponseCode <= 399;
	}

	public static bool Is4xxCode(this RestResult response) {
		return (int)response.ResponseCode >= 400 && (int)response.ResponseCode <= 499;
	}

	public static bool Is5xxCode(this RestResult response) {
		return (int)response.ResponseCode >= 500 && (int)response.ResponseCode <= 599;
	}

	public static string DisplayResponseCode(this RestResult response) {
		return string.Join(' ', Regex.Split(response.ResponseCode.ToString(), @"(?=[ABCDEFGHIJKLMNOPQRSTUVWXYZ])").Skip(1));
	}
}