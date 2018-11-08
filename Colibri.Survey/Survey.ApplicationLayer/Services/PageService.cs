﻿using AutoMapper;
using Survey.ApplicationLayer.Dtos.Entities;
using Survey.ApplicationLayer.Dtos.Models;
using Survey.ApplicationLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Survey.DomainModelLayer.Entities;
using dataaccesscore.Abstractions.Uow;

namespace Survey.ApplicationLayer.Services
{
    public class PageService : IPageService
    {

        protected readonly IUowProvider UowProvider;
        protected readonly IMapper Mapper;

        public PageService(
            IUowProvider uowProvider,
            IMapper mapper
        )
        {
            this.UowProvider = uowProvider;
            this.Mapper = mapper;
        }



        public Pages GetPageById(Guid id)
        {
            try
            {
                Pages item;
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repositoryPage = uow.GetRepository<Pages, Guid>();
                    item = repositoryPage.Get(id);
                    //await uow.SaveChangesAsync();
                    return item;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public async Task<List<PageModel>> GetListBySurvey(Guid surveyId)
        {
            try
            {
                List<PageModel> pages = new List<PageModel>();
                IEnumerable<Pages> items;
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repositoryPage = uow.GetRepository<Pages, Guid>();
                    items = await repositoryPage.QueryAsync(item => item.SurveyId == surveyId);
                    await uow.SaveChangesAsync();
                    IEnumerable<PagesDto> surveyDto = Mapper.Map<IEnumerable<Pages>, IEnumerable<PagesDto>>(items);

                    foreach (var item in surveyDto)
                    {
                        PageModel page = new PageModel()
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            Order = item.OrderNo,
                            SurveyId = item.SurveyId
                        };
                        pages.Add(page);
                    }
                    return pages;

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeletePageById(Guid pageId)
        {
            try
            {
                using (var uow = UowProvider.CreateUnitOfWork())
                {
                    var repositoryPage = uow.GetRepository<Pages, Guid>();
                    repositoryPage.Remove(pageId);
                    uow.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }



        public async Task<Guid> AddAsync(PageModel survey, Guid surveyId)
        {
            string surveystring = surveyId.ToString();
            PagesDto pageDto = new PagesDto()
            {
                Description = survey.Description,
                Name = survey.Name,
                OrderNo = survey.Order,
                SurveyId = Guid.Parse(surveyId.ToString()) 
            };

            using (var uow = UowProvider.CreateUnitOfWork())
            {
                try
                {
                    Pages pageEntity = Mapper.Map<PagesDto, Pages>(pageDto);
                    var repositoryPage = uow.GetRepository<Pages, Guid>();
                    await repositoryPage.AddAsync(pageEntity);
                    await uow.SaveChangesAsync();

                    return pageEntity.Id;
                }
                catch (Exception e)
                {
                    Console.Write(e);
                    throw;
                }
            }       
        }
    }
}
