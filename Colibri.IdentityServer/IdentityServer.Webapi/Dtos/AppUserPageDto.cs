﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Webapi.Dtos
{

    public class AppUserPageDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public double? EmailConfirmTokenLifespan { get; set; }
        public DateTimeOffset? EmailConfirmInvitationDate { get; set; }
    }

}
