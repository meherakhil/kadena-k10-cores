﻿using Kadena.Dto.General;
using Kadena.KOrder.PaymentService.Infrastucture.Helpers;
using Kadena2.MicroserviceClients.Clients.Base;
using Kadena2.MicroserviceClients.Contracts;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kadena2.MicroserviceClients.Clients
{
    public class ExportClient : ClientBase, IExportClient
    {
        //public ExportClient() : base()
        //{

        //}

        //public ExportClient(IAwsV4Signer signer) : base(signer)
        //{

        //}

        public async Task<BaseResponseDto<Stream>> ExportMailingList(string endpoint, Guid containerId, string siteName)
        {
            var url = $"{endpoint}/api/MailingListExport/GetFileReport?ContainerId={containerId}&SiteName={siteName}&ReportType=processedMails&OutputType=csv";
            using (var client = new HttpClient())
            {
                using (var message = await client.GetAsync(url).ConfigureAwait(false))
                {
                    if (message.IsSuccessStatusCode)
                    {
                        var contentStream = await message.Content.ReadAsStreamAsync();
                        var resultStream = new MemoryStream();
                        await contentStream.CopyToAsync(resultStream).ConfigureAwait(false);
                        return new BaseResponseDto<Stream>
                        {
                            Success = true,
                            Payload = resultStream
                        };
                    }
                    else
                    {
                        return await ReadResponseJson<Stream>(message).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
