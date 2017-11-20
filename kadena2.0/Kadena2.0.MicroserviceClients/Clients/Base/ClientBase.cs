﻿using Kadena.Dto.General;
using Kadena2.MicroserviceClients.Helpers;
using Kadena2.MicroserviceClients.Contracts.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kadena2.MicroserviceClients.Clients.Base
{
    public abstract class ClientBase
    {
        private readonly ISuppliantDomainClient _suppliantDomain;

        private const bool SignRequest = true;

        // TODO consider using static or singleton, based on how we will store credentials
        private readonly IAwsV4Signer signer;

        public ClientBase()
        {
            this.signer = new DefaultAwsV4Signer();
        }

        protected ClientBase(ISuppliantDomainClient suppliantDomain) : this()
        {
            _suppliantDomain = suppliantDomain;
        }

        private const string _responseIncorrectMessage = "Response from microservice is not in correct format.";

        private static JsonSerializerSettings camelCaseSerializer = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        protected async Task<BaseResponseDto<TResult>> Get<TResult>(string url)
        {
            return await Send<TResult>(HttpMethod.Get, url).ConfigureAwait(false); ;
        }

        protected async Task<BaseResponseDto<TResult>> Post<TResult>(string url, object body)
        {
            return await Send<TResult>(HttpMethod.Post, url, body).ConfigureAwait(false); ;
        }

        protected async Task<BaseResponseDto<TResult>> Delete<TResult>(string url, object body = null)
        {
            return await Send<TResult>(HttpMethod.Delete, url, body).ConfigureAwait(false); ;
        }

        protected async Task<BaseResponseDto<TResult>> Patch<TResult>(string url, object body)
        {
            return await Send<TResult>(new HttpMethod("PATCH"), url, body).ConfigureAwait(false); ;
        }

        protected async Task<BaseResponseDto<TResult>> Put<TResult>(string url, object body)
        {
            return await Send<TResult>(HttpMethod.Put, url, body).ConfigureAwait(false); ;
        }

        protected static string SerializeRequestContent(object body)
        {
            return JsonConvert.SerializeObject(body, camelCaseSerializer);
        }


        private async Task<BaseResponseDto<TResult>> Send<TResult>(HttpMethod method, string url, object body = null)
        {
            using (var request = CreateRequest(method, url, body))
            {
                await SignRequestMessage(request).ConfigureAwait(false);
                return await SendRequest<TResult>(request).ConfigureAwait(false);
            }
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url, object body = null)
        {
            var request = new HttpRequestMessage(method, url);
            AddSuppliantDomain(request);

            if (body != null)
            {
                request.Content = new StringContent(SerializeRequestContent(body), Encoding.UTF8, "application/json");
            }
            return request;
        }

        protected static async Task<BaseResponseDto<TResult>> SendRequest<TResult>(HttpRequestMessage request)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                {
                    return await ReadResponseJson<TResult>(response).ConfigureAwait(false);
                }
            }
        }

        private void AddSuppliantDomain(HttpRequestMessage request)
        {
            var suppliantDomain = _suppliantDomain?.GetSuppliantDomain();
            if (!string.IsNullOrEmpty(suppliantDomain))
            {
                request.Headers.Add("suppliantDomain", suppliantDomain);
            }
        }

        protected async Task SignRequestMessage(HttpRequestMessage request)
        {
            if (SignRequest && signer != null)
            {
                await signer.SignRequest(request).ConfigureAwait(false);
            }
        }
        
        protected static async Task<BaseResponseDto<TResult>> ReadResponseJson<TResult>(HttpResponseMessage response)
        {
            BaseResponseDto<TResult> result = null;
            BaseErrorDto innerError = null;
            string responseContent = string.Empty;

            // In these cases, there may be JSON with standard structure and proper error message from microservice
            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.BadGateway ||
                response.StatusCode == HttpStatusCode.NotImplemented ||
                response.StatusCode == HttpStatusCode.InternalServerError)
            {
                responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    try
                    {
                        result = JsonConvert.DeserializeObject<BaseResponseDto<TResult>>(responseContent);
                    }
                    catch (Exception e)
                    {
                        innerError = new BaseErrorDto { Message = $"{e.Message} response content: '{responseContent}'" };
                    }
                }
            }
            else // some severe network error
            {
                result = new BaseResponseDto<TResult>
                {
                    Success = false,
                    Payload = default(TResult),
                    Error = new BaseErrorDto
                    {
                        Message = $"HttpClient received status {response.StatusCode}, reason {response.ReasonPhrase}"
                    }
                };
            }

            return result ?? new BaseResponseDto<TResult>
            {
                Success = false,
                Payload = default(TResult),
                Error = new BaseErrorDto
                {
                    Message = $"{_responseIncorrectMessage} response content: '{responseContent}'",
                    InnerError = innerError
                }
            };
        }
    }
}
