﻿using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kadena.Dto.MailingList.MicroserviceResponses;
using Kadena.WebAPI;
using Kadena.WebAPI.Services;
using AutoMapper;
using Kadena2.MicroserviceClients.Contracts;
using Kadena.WebAPI.KenticoProviders.Contracts;
using Kadena.Models;
using Kadena.Dto.General;

namespace Kadena.Tests.WebApi
{
    public class KListServiceTests
    {
        private readonly Guid _containerId;
        private readonly List<MailingAddressDto> _addresses;

        public KListServiceTests()
        {
            _containerId = Guid.NewGuid();
            var count = new Random().Next(100);
            _addresses = new List<MailingAddressDto>();
            for (int i = 0; i < count; i++)
            {
                _addresses.Add(new MailingAddressDto
                {
                    Id = Guid.NewGuid(),
                    firstName = $"Name{i}",
                    ContainerId = _containerId,
                    address1 = $"Address 1, {i}",
                    address2 = $"Address 2, {i}",
                    city = $"City {i}",
                    state = $"State {i}",
                    zip = $"Zip {i}",
                    Error = i % 2 == 0 ? $"Some error {i}" : null
                });
            }
        }

        private KListService Create()
        {
            MapperBuilder.InitializeAll();
            var mapper = Mapper.Instance;

            var mailingClient = new Mock<IMailingListClient>();
            mailingClient.Setup(c => c.GetAddresses(null, _containerId)).Returns(Task.FromResult(GetAddresses()));
            mailingClient.Setup(c => c.Validate(null, null, _containerId)).Returns(Task.FromResult(Validate()));
            var kenticoClient = new Mock<IKenticoResourceService>();
            kenticoClient.Setup(p => p.GetKenticoSite())
                .Returns(new KenticoSite());
            
            return new KListService(mailingClient.Object, kenticoClient.Object, mapper);
        }

        private BaseResponseDto<IEnumerable<MailingAddressDto>> GetAddresses()
        {
            return new BaseResponseDto<IEnumerable<MailingAddressDto>>
            {
                Success = true,
                Payload = _addresses.Where(a => a.ContainerId == _containerId)
            };
        }
        
        private BaseResponseDto<string> Validate()
        {
            return new BaseResponseDto<string>
            {
                Success = true,
                Payload = string.Empty
            };
        }

        [Fact]
        public async Task UseOnlyCorrectTest()
        {
            var srvs = Create();
            var result = await srvs.UseOnlyCorrectAddresses(_containerId);

            Assert.True(result);
        }
    }
}
