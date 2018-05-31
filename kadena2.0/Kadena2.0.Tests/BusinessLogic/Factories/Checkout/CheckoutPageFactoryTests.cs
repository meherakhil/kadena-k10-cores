﻿using System;
using Xunit;
using Moq;
using Kadena.BusinessLogic.Factories.Checkout;
using Kadena.WebAPI.KenticoProviders.Contracts;
using System.Collections.Generic;
using Kadena.Models;
using Kadena.BusinessLogic.Contracts;
using System.Linq;

namespace Kadena.Tests.BusinessLogic.Factories.Checkout
{
    public class CheckoutPageFactoryTests : KadenaUnitTest<CheckoutPageFactory>
    {
        public static IEnumerable<object[]> GetDependencies()
        {
            var dependencies = new object[] {
                new Mock<IKenticoResourceService>().Object,
                new Mock<IKenticoDocumentProvider>().Object,
                new Mock<IKenticoLocalizationProvider>().Object,
                new Mock<IDialogService>().Object,
            };

            foreach (var dep in dependencies)
            {
                yield return dependencies
                    .Select(d =>
                    {
                        if (d.Equals(dep))
                        {
                            return null;
                        }
                        return d;
                    })
                    .ToArray();
            }
        }

        [Theory(DisplayName = "CheckoutPageFactory()")]
        [MemberData(nameof(GetDependencies))]
        public void CheckoutPageFactory(IKenticoResourceService resources, IKenticoDocumentProvider documents,
            IKenticoLocalizationProvider kenticoLocalization, IDialogService dialogService)
        {
            Assert.Throws<ArgumentNullException>(() => new CheckoutPageFactory(resources, documents, kenticoLocalization, dialogService));
        }

        public static IEnumerable<object[]> GetCreateDeliveryAddressesParameters()
        {
            yield return new object[] { null, null, false };
            yield return new object[] { null, null, true };
            yield return new object[] { null, string.Empty, false };
            yield return new object[] { null, string.Empty, true };
            yield return new object[] { new List<DeliveryAddress>(), null, false };
            yield return new object[] { new List<DeliveryAddress>(), null, true };
            yield return new object[] { new List<DeliveryAddress>(), string.Empty, false };
            yield return new object[] { new List<DeliveryAddress>(), string.Empty, true };
        }

        [Theory(DisplayName = "CheckoutPageFactory.CreateDeliveryAddresses()")]
        [MemberData(nameof(GetCreateDeliveryAddressesParameters))]
        public void CreateDeliveryAddresses(List<DeliveryAddress> addresses, string userNotification, bool otherAddressAvailable)
        {
            var actualResult = Sut.CreateDeliveryAddresses(addresses, userNotification, otherAddressAvailable);

            Assert.NotNull(actualResult);
        }
    }
}
