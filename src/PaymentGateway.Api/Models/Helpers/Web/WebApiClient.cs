using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.AspNetCore.Mvc.Routing;

using Microsoft.AspNetCore.Mvc;

using static PaymentGateway.Api.Models.Helpers.Web.IWebApiClient;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using RestSharp;

namespace PaymentGateway.Api.Models.Helpers.Web;
public class WebApiClient : IWebApiClient, IDisposable
{
    public HttpClient HttpClient { get; private set; }

    protected internal JsonSerializerSettings SerializerSettings { get; private set; }

    public WebApiClient(TimeSpan? timeout = null)
    {
        HttpClient = new HttpClient();
        HttpClient.Timeout = timeout ?? TimeSpan.FromMinutes(5);
    }
    public void Dispose()
    {
        if (HttpClient != null)
        {
            HttpClient.Dispose();
            HttpClient = null;
        }
    }

    public async Task<T> Get<T>(string enpoint, string actionPath, object querystr = null, Func<Task<NameValueCollection>> getHeaders = null)
    {
        var url = UrlHelper.BuildUrl(enpoint, actionPath, querystr);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

        if (getHeaders != null)
        {
            var headers = await getHeaders();
            foreach (var header in headers.AllKeys)
            {
                request.Headers.Add(header, headers.GetValues(header).FirstOrDefault());
            }
        }

        HttpResponseMessage response = await HttpClient.SendAsync(request);
        var res = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<T>(res);
        }
        else
        {
            throw new ApplicationException($"Failed GET from {url}: {response.StatusCode}");
        }
    }


    public async Task<T> Post<T>(string enpoint, string actionPath, object payload)
    {
        var client = new RestClient(string.Format("{0}/{1}", enpoint, actionPath));
        var uri = new Uri(new Uri(enpoint), actionPath);
        UrlHelper.BuildUrl(enpoint, actionPath);
        var request = new RestRequest(uri, Method.Post);
        request.AddHeader("Content-Type", "application/json");
        var body = JsonConvert.SerializeObject(payload, SerializerSettings);
        request.AddParameter("application/json", body, ParameterType.RequestBody);
        var response = client.Execute(request);
        var res = response.Content;
        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<T>(res);
        }
        else//service is down for example
        { 
            throw new KeyNotFoundException ($"Failed POST from {enpoint}/{actionPath}: with status code: {response.StatusCode} {response.Content}");
        }

    }

}
