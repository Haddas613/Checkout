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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security;
namespace PaymentGateway.Api.Models.Helpers;
public static class SecureMethods
{
    public static string GetSecuredString(SecureString sString)
    {
        if (sString == null)
            return String.Empty;

        IntPtr bstr = Marshal.SecureStringToBSTR(sString);
        try
        {
            return Marshal.PtrToStringBSTR(bstr);
        }
        catch
        {
            return String.Empty;
        }
        finally
        {
            Marshal.ZeroFreeBSTR(bstr);
        }
    }

    public static void SetSecuredString(this SecureString secureString, string sString)
    {
        if (String.IsNullOrEmpty(sString) || secureString == null)
        {
            return;
        }

        for (int i = 0; i < sString.Length; i++)
            secureString.AppendChar(sString[i]);

        secureString.MakeReadOnly();
    }
}
