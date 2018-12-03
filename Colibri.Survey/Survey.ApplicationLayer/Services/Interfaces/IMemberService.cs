﻿using Survey.ApplicationLayer.Dtos.Models.IdentityServer;
using Survey.ApplicationLayer.Dtos.Search;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Survey.ApplicationLayer.Services.Interfaces
{
    public interface IMemberService
    {
        Task<SearchResultDto<MemberDto>> GetMembersAsync(string groupId, SearchQueryDto pageSearchEntry);
    }
}