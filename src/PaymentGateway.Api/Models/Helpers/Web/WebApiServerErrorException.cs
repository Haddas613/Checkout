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
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace PaymentGateway.Api.Models.Helpers.Web;

public class WebApiServerErrorException : Exception
{
    public string Response { get; }

    public HttpStatusCode StatusCode { get; }

    public WebApiServerErrorException(string message, HttpStatusCode statusCode, string response)
        : base(message)
    {
        Response = response;

        StatusCode = statusCode;
    }
}
