﻿using System;
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
public static class UrlHelper
{
    public static string BuildUrl(string enpoint, string actionPath, object querystr = null)
    {
        Uri baseUri = new Uri(enpoint);
        Uri uri = new Uri(baseUri, actionPath);

        var builder = new UriBuilder(uri);

        if (querystr != null)
        {
            var keyValueContent = querystr.ToKeyValue();

            return Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString(builder.ToString(), keyValueContent);
        }

        return builder.ToString();
    }

    public static IDictionary<string, string> ToKeyValue(this object metaToken)
    {
        if (metaToken == null)
        {
            return null;
        }

        if (!(metaToken is JToken token))
        {
            return ToKeyValue(JObject.FromObject(metaToken));
        }

        if (token.HasValues)
        {
            var contentData = new Dictionary<string, string>();
            foreach (var child in token.Children().ToList())
            {
                var childContent = child.ToKeyValue();
                if (childContent != null)
                {
                    contentData = contentData.Concat(childContent)
                        .ToDictionary(k => k.Key, v => v.Value);
                }
            }

            return contentData;
        }

        var jValue = token as JValue;
        if (jValue?.Value == null)
        {
            return null;
        }

        var value = jValue?.Type == JTokenType.Date ?
                        jValue?.ToString("o", CultureInfo.InvariantCulture) :
                        jValue?.ToString(CultureInfo.InvariantCulture);

        return new Dictionary<string, string> { { token.Path, value } };
    }
}
