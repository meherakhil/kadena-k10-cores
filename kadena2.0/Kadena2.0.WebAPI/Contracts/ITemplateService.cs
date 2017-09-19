﻿using Kadena.Models.TemplatedProduct;
using System;
using System.Threading.Tasks;

namespace Kadena.WebAPI.Contracts
{
    public interface ITemplateService
    {
        Task<bool> SetName(Guid templateId, string name);
        Task<ProductTemplates> GetTemplatesByProduct(int documentId);
    }
}