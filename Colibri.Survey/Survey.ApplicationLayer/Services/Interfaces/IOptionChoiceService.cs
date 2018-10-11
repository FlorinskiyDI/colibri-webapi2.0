﻿using Survey.ApplicationLayer.Dtos.Models;
using Survey.DomainModelLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Survey.ApplicationLayer.Services.Interfaces
{
    public interface IOptionChoiceService
    {
        Task<Guid> AddAsync(Guid optionGroupId, ItemModel item = null);
        //Task AddAsync(Guid optionGroupId, ItemModel item);
        void AddRange(Guid optionGroupId, List<ItemModel> items);
        Task<List<ItemModel>> GetListByOptionGroup(Guid? optionGroupId);
        Task<IEnumerable<OptionChoises>> GetListByOptionGroupId(Guid? optionGroupId);
        void UpdateOptionChoise(OptionChoises choise);
        void DeleteOptionChoise(OptionChoises choise);
    }
}
