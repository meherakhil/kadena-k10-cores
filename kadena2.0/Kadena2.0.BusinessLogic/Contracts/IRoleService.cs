﻿using Kadena.Models;
using System.Collections.Generic;

namespace Kadena.BusinessLogic.Contracts
{
    public interface IRoleService
    {
        void AssignSSORoles(User user, int siteId, IEnumerable<string> ssoRoles);
    }
}
