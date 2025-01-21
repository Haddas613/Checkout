using System;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net;

namespace PaymentGateway.Api.Models.Helpers.Web;
public interface IWebApiClient
{
    Task<T> Post<T>(string enpoint, string actionPath, object payload);
}