﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Survey.DomainModelLayer.Models.IdentityServer
{
    public class MemberModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset DateOfSubscribe { get; set; }
    }
}
