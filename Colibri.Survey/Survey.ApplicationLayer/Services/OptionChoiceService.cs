﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using dataaccesscore.Abstractions.Uow;
using Survey.ApplicationLayer.Dtos.Entities;
using Survey.ApplicationLayer.Dtos.Models;
using Survey.ApplicationLayer.Services.Interfaces;
using Survey.DomainModelLayer.Entities;

namespace Survey.ApplicationLayer.Services
{
    public class OptionChoiceService : IOptionChoiceService
    {

        protected readonly IUowProvider UowProvider;
        protected readonly IMapper Mapper;

        public OptionChoiceService(
            IUowProvider uowProvider,
            IMapper mapper
        )
        {
            this.UowProvider = uowProvider;
            this.Mapper = mapper;
        }


        public async Task<List<OptionChoises>> GetListByOptionGroupId(Guid? optionGroupId, bool includAdditionalChoice = false)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repositoryOptionChoice = uow.GetRepository<OptionChoises, Guid>();
                if (!includAdditionalChoice)
                {
                    return repositoryOptionChoice.QueryAsync(item => item.OptionGroupId == optionGroupId).Result.Where(x => x.IsAdditionalChoise == includAdditionalChoice).ToList();
                }
                else
                {
                    return repositoryOptionChoice.QueryAsync(item => item.OptionGroupId == optionGroupId).Result.ToList();
                }
            }
        }


        public void UpdateOptionChoise(OptionChoises choise)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repositoryChoice = uow.GetRepository<OptionChoises, Guid>();
                repositoryChoice.Update(choise);
                uow.SaveChanges();
            }
        }


        public void DeleteOptionChoise(OptionChoises choise)
        {
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repositoryChoice = uow.GetRepository<OptionChoises, Guid>();
                repositoryChoice.Remove(choise);
                uow.SaveChanges();
            }
        }


        public async Task<List<ItemModel>> GetListByOptionGroup(Guid? optionGroupId, bool includAdditionalChoice = false)
        {
            List<ItemModel> items = new List<ItemModel>();
            IEnumerable<OptionChoises> choises;
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                var repositoryOptionChoice = uow.GetRepository<OptionChoises, Guid>();
                choises = await repositoryOptionChoice.QueryAsync(item => item.OptionGroupId == optionGroupId);
                await uow.SaveChangesAsync();
                IEnumerable<OptionChoisesDto> optionChoisesDto = Mapper.Map<IEnumerable<OptionChoises>, IEnumerable<OptionChoisesDto>>(choises);

                foreach (var item in optionChoisesDto)
                {
                    if (!item.IsAdditionalChoise)
                    {
                        ItemModel page = new ItemModel()
                        {
                            Id = item.Id.ToString(),
                            Value = item.Name,
                            Order = 0, // strub
                            Label = "",
                            IsAdditionalChoise = item.IsAdditionalChoise
                        };
                        items.Add(page);
                    }
                    else
                    {
                        if (includAdditionalChoice)
                        {
                            ItemModel page = new ItemModel()
                            {
                                Id = item.Id.ToString(),
                                Value = item.Name,
                                Order = 0, // strub
                                Label = "",
                                IsAdditionalChoise = item.IsAdditionalChoise

                            };
                            items.Add(page);
                        }
                    }
                }
                return items;
            }
        }



        public async Task<Guid> AddAsync(Guid optionGroupId, ItemModel item = null, bool isAdditionalChoice = false)
        {
            OptionChoisesDto optionChoisesDto = new OptionChoisesDto()
            {
                Name = item != null ? item.Value : "",
                OptionGroupId = optionGroupId,
                IsAdditionalChoise = isAdditionalChoice,
                OrderNo = 1 // stub
            };

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                OptionChoises optionChoisesEntity = Mapper.Map<OptionChoisesDto, OptionChoises>(optionChoisesDto);
                var repositoryOptionChoise = uow.GetRepository<OptionChoises, Guid>();
                await repositoryOptionChoise.AddAsync(optionChoisesEntity);
                await uow.SaveChangesAsync();
                return optionChoisesDto.Id;
            }
        }



        public void AddRange(Guid optionGroupId, List<ItemModel> items)
        {
            List<OptionChoisesDto> listchoiseDto = new List<OptionChoisesDto>();
            foreach (var item in items)
            {
                OptionChoisesDto optionChoisesDto = new OptionChoisesDto()
                {
                    IsAdditionalChoise = item.IsAdditionalChoise,
                    Name = item != null ? item.Value : "",
                    OptionGroupId = optionGroupId,
                    OrderNo = 1 // stub
                };
                listchoiseDto.Add(optionChoisesDto);
            }
            using (var uow = UowProvider.CreateUnitOfWork())
            {
                List<OptionChoises> optionChoisesEntity = Mapper.Map<List<OptionChoisesDto>, List<OptionChoises>>(listchoiseDto);
                var repositoryOptionChoise = uow.GetRepository<OptionChoises, Guid>();

                repositoryOptionChoise.AddRange(optionChoisesEntity.ToArray());
                uow.SaveChanges();
            }
        }
    }
}
