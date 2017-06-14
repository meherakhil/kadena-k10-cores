﻿using DryIoc.WebApi;
using DryIoc;
using Kadena.WebAPI.Contracts;
using Kadena.WebAPI.Services;
using System.Web.Http;
using Kadena.WebAPI.Infrastructure.Filters;
using AutoMapper;
using CMS.Ecommerce;
using Kadena.WebAPI.Models;
using System.Linq;
using Kadena.Dto.Checkout;
using Kadena.WebAPI.Models.SubmitOrder;
using PaymentMethod = Kadena.WebAPI.Models.PaymentMethod;
using Kadena.Dto.SubmitOrder;
using Kadena.WebAPI.Models.CustomerData;
using Kadena.Dto.CustomerData;
using Kadena2.MicroserviceClients.Contracts;
using Kadena2.MicroserviceClients.Clients;
using Kadena2.MicroserviceClients.Responses;
using Kadena.Dto.SubmitOrder.Requests;
using Kadena.Dto.SubmitOrder.Responses;

namespace Kadena.WebAPI
{
    public static class WebApiConfig
    {
        /// <summary>
        /// Run api configuration.
        /// </summary>
        public static void Configure(HttpConfiguration apiConfig)
        {
            RegisterApiRoutes(apiConfig);
            ConfigureFilters(apiConfig);
            ConfigureMapper();
            ConfigureContainer(apiConfig);
            apiConfig.EnsureInitialized();
        }

        private static void ConfigureFilters(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.Filters.Add(new ExceptionFilter());
            GlobalConfiguration.Configuration.Filters.Add(new ValidateModelStateAttribute());
        }

        private static void ConfigureMapper()
        {
            Mapper.Initialize(config =>
            {
                config.CreateMap<AddressInfo, DeliveryAddress>().ProjectUsing(ai => new DeliveryAddress()
                {
                    Id = ai.AddressID,
                    Checked = false,
                    City = ai.AddressCity,
                    State = ai.GetStateCode(),
                    Country = ai.GetCountryTwoLetterCode(),
                    StateId = ai.AddressStateID,
                    CountryId = ai.AddressCountryID,
                    Street = new[] { ai.AddressLine1 }.ToList(),
                    Zip = ai.AddressZip
                });

                config.CreateMap<CarrierInfo, DeliveryCarrier>().ProjectUsing(ci => new DeliveryCarrier()
                {
                    Id = ci.CarrierID,
                    Opened = false,
                    Title = ci.CarrierDisplayName
                });

                config.CreateMap<ShippingOptionInfo, DeliveryOption>().ProjectUsing(s => new DeliveryOption()
                {
                    Id = s.ShippingOptionID,
                    CarrierId = s.ShippingOptionCarrierID,
                    Title = s.ShippingOptionDisplayName,
                    Service = s.ShippingOptionCarrierServiceName,
                });

                config.CreateMap<PaymentOptionInfo, PaymentMethod>().ProjectUsing(p => new PaymentMethod()
                {
                    Id = p.PaymentOptionID,
                    Checked = false,
                    Disabled = false,
                    Icon = "",
                    Title = p.PaymentOptionDisplayName,
                    ClassName = p.PaymentOptionClassName
                });

                config.CreateMap<OrderItem, OrderItemDTO>().ProjectUsing(p => new OrderItemDTO(p.OrderItemType)
                {
                    DesignFilePath = p.DesignFilePath,
                    LineNumber = p.LineNumber,
                    MailingList = new MailingListDTO()
                    {
                        MailingListID = p.MailingListId
                    },
                    SKU = new SKUDTO()
                    {
                        KenticoSKUID = p.KenticoSKUId,
                        Name = p.SKUName,
                        SKUNumber = p.SKUNumber
                    },
                    TotalPrice = p.TotalPrice,
                    TotalTax = p.TotalTax,
                    UnitCount = p.UnitCount,
                    UnitOfMeasure = p.UnitOfMeasure,
                    UnitPrice = p.UnitPrice
                });

                config.CreateMap<CustomerData, CustomerDataDTO>();
                config.CreateMap<CustomerAddress, CustomerAddressDTO>();
                config.CreateMap<CartItems, CartItemsDTO>();
                config.CreateMap<CartItem, CartItemDTO>()
                    .AfterMap((src, dest) => dest.Price = string.Format("{0:#,0.00}", src.Price))
                    .AfterMap((src, dest) => dest.MailingList = src.MailingListName);
                config.CreateMap<PaymentMethod, PaymentMethodDTO>();
                config.CreateMap<PaymentMethods, PaymentMethodsDTO>();
                config.CreateMap<Total, TotalDTO>();
                config.CreateMap<Totals, TotalsDTO>();
                config.CreateMap<DeliveryOption, DeliveryServiceDTO>();
                config.CreateMap<DeliveryCarriers, DeliveryMethodsDTO>();
                config.CreateMap<DeliveryCarrier, DeliveryMethodDTO>();
                config.CreateMap<DeliveryAddresses, DeliveryAddressesDTO>();
                config.CreateMap<DeliveryAddress, DeliveryAddressDTO>();
                config.CreateMap<CheckoutPage, CheckoutPageDTO>();
                config.CreateMap<SubmitRequestDto, SubmitOrderRequest>();
                config.CreateMap<SubmitOrderResult, SubmitOrderResponseDto>();
                config.CreateMap<SubmitOrderServiceResponseDto, SubmitOrderResult>();
                config.CreateMap<SubmitOrderErrorDto, SubmitOrderError>();
                config.CreateMap<PaymentMethodDto, Models.SubmitOrder.PaymentMethod>();
            });
        }

        private static void ConfigureContainer(HttpConfiguration apiConfig)
        {
            var container = new Container();
            container.Register<IShoppingCartService,ShoppingCartService>();
            container.Register<IKenticoProviderService, KenticoProviderService>();
            container.Register<IKenticoResourceService, KenticoResourceService>();
            container.Register<IOrderServiceClient, OrderServiceClient>();
            container.Register<IKenticoLogger, KenticoLogger>();
            container.Register<ICustomerDataService, CustomerDataService>();
            container.Register<ITaxEstimationService, TaxEstimationServiceClient>();
            container.RegisterInstance(typeof(IMapper), Mapper.Instance);
            container.WithWebApi(apiConfig);
        }

        /// <summary>
        /// Register api routes.
        /// </summary>
        /// <param name="config">The configuration holder object.</param>
        private static void RegisterApiRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { action = RouteParameter.Optional }
            );
        }
    }
}
