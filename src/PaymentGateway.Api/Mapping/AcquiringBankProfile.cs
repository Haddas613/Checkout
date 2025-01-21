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
using AutoMapper;
using PaymentGateway.Api.Integrations.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Mapping;
public class AcquiringBankProfile : Profile
{
    public AcquiringBankProfile() => RegisterMappings();

    internal void RegisterMappings()
    {
        RegisterTransactionMappings();
    }

    private void RegisterTransactionMappings()
    {
        CreateMap<PostPaymentRequest, AcquiringBankCreateTransactionRequest>()
            .ForMember(dest => dest.card_number, s => s.MapFrom(src => src.CardNumber))
         .ForMember(dest => dest.expiry_date, opt => opt.MapFrom(src => $"{src.ExpiryMonth:D2}/{src.ExpiryYear}")
        );

        CreateMap<AcquiringBankCreateTransactionResponse, PostPaymentResponse>()
    .ConvertUsing(src => new PostPaymentResponse
    {
        Status =  src.authorized.HasValue && src.authorized.Value ? PaymentStatus.Authorized : ( src.authorized.HasValue  && !src.authorized.Value ? PaymentStatus.Declined: PaymentStatus.Rejected),
        Id = src.authorization_code.HasValue ? src.authorization_code : null
    });

        CreateMap<PostPaymentRequest, PostPaymentResponse>()
            .ForMember(dest => dest.CardNumberLastFour, opt => opt.Ignore()
            );

    }
}
