﻿using AutoMapper;
using dataaccesscore.Abstractions.Uow;
using dataaccesscore.EFCore.Paging;
using dataaccesscore.EFCore.Query;
using IdentityServer.Webapi.Data;
using IdentityServer.Webapi.Dtos;
using IdentityServer.Webapi.Dtos.Search;
using IdentityServer.Webapi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Webapi.Services
{
    public class GroupService : IGroupService
    {
        private readonly IMemberService _memberService;
        private readonly IGroupNodeService _groupNodeService;
        private readonly IDataPager<Groups> _pager;
        private readonly IUowProvider _uowProvider;
        private readonly IMapper _mapper;
        private readonly ApplicationUserManager _userManager;
        public GroupService(
            IUowProvider uowProvider,
            IDataPager<Groups> pager,
            IGroupNodeService groupNodeService,
            IMemberService memberService,
            ApplicationUserManager userManager,
            IMapper mapper
        )
        {
            _uowProvider = uowProvider;
            _pager = pager;
            _mapper = mapper;
            _groupNodeService = groupNodeService;
            _memberService = memberService;
            _userManager = userManager;
        }


        public async Task<GroupDto> UpdateGroup(GroupDto model, string userId)
        {
            var entity = new Groups();
            try



            {
                using (var uow = _uowProvider.CreateUnitOfWork())
                {
                    var repository = uow.GetRepository<Groups>();
                    entity = await repository.GetAsync<Guid>(model.Id);
                    entity = _mapper.Map<GroupDto, Groups>(model, entity);
                    repository.Update(entity);
                    await uow.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _mapper.Map<Groups, GroupDto>(entity);
        }

        public async Task<SearchResult<GroupDto>> GetRootAsync(string userId, SearchQuery searchEntry)
        {
            // generate sort expression
            var sort = searchEntry?.OrderStatement == null
                ? new OrderBy<Groups>(c => c.OrderBy(d => d.Name))
                : new OrderBy<Groups>(searchEntry.OrderStatement.ColumName, searchEntry.OrderStatement.Reverse);
            // generate filter expression
            var filters = new Filter<Groups>(c => c.MemberGroups.Any(d => d.UserId == new Guid(userId)));

            var page = new SearchResult<GroupDto>();
            try
            {
                using (var uow = _uowProvider.CreateUnitOfWork())
                {
                    var repository = uow.GetRepository<Groups>();

                    // get data
                    if (searchEntry?.SearchQueryPage == null)
                    {
                        var data = await repository.QueryAsync(filters.Expression, sort.Expression);
                        page = new SearchResult<GroupDto>()
                        {
                            ItemList = data.Select(c => new GroupDto
                            {
                                Id = c.Id,
                                ParentId = c.ParentId,
                                Name = c.Name,
                                CountChildren = c.InverseParent.Count
                            }).ToList(),
                        };
                    }
                    // get page data
                    else
                    {
                        var startRow = searchEntry.SearchQueryPage.PageNumber;
                        var data = await repository.QueryPageAsync(filters.Expression, sort.Expression, null, startRow, searchEntry.SearchQueryPage.PageLength);
                        var totalCount = await repository.CountAsync(filters.Expression);

                        page = new SearchResult<GroupDto>()
                        {
                            ItemList = data.Select(c => new GroupDto
                            {
                                Id = c.Id,
                                ParentId = c.ParentId,
                                Name = c.Name,
                                CountChildren = c.InverseParent.Count,
                                GroupID = c.GroupID,
                                Description = c.Description
                            }).ToList(),
                            SearchResultPage = new SearchResultPage()
                            {
                                TotalItemCount = totalCount,
                                PageLength = searchEntry.SearchQueryPage.PageLength,
                                PageNumber = totalCount / searchEntry.SearchQueryPage.PageLength
                            }
                        };
                    }
                }
            }
            catch (Exception ex) { throw ex; }

            return page;
        }

        public async Task<SearchResult<GroupDto>> GetAllAsync(string userId, SearchQuery searchEntry)
        {
            // generate sort expression
            var sort = searchEntry?.OrderStatement == null
                ? new OrderBy<Groups>(c => c.OrderBy(d => d.Name))
                : new OrderBy<Groups>(searchEntry.OrderStatement.ColumName, searchEntry.OrderStatement.Reverse);
            // generate includes expression
            var includes = new Includes<Groups>(c => c.Include(v => v.InverseParent).Include(v => v.Parent));

            var page = new SearchResult<GroupDto>();
            try
            {
                using (var uow = _uowProvider.CreateUnitOfWork())
                {
                    var repository = uow.GetRepository<Groups>();
                    var userRolerepository = uow.GetRepository<ApplicationUserRole>();

                    // get root groups fo user
                    var items = repository.Query(c => c.MemberGroups.Any(d => d.UserId == new Guid(userId))).ToList();
                    Filter<Groups> filters = new Filter<Groups>(c => c.Ancestors.Any(d => items.Contains(d.Ancestor)));  // TODO: List "itemsParentIds" can store a large list that will be passed in the request. It may cause an error in the future!!!
                    //Filter<Groups> filters = new Filter<Groups>(c => items.Contains(c.ParentId.Value) || c.ParentId == null);  // TODO: List "itemsParentIds" can store a large list that will be passed in the request. It may cause an error in the future!!!

                    // get data
                    if (searchEntry?.SearchQueryPage == null)
                    {
                        var data = await repository.QueryAsync(filters.Expression, sort.Expression);
                        page = new SearchResult<GroupDto>()
                        {
                            ItemList = data.Select(c => new GroupDto
                            {
                                Id = c.Id,
                                ParentId = c.ParentId,
                                Name = c.Name,
                                CountChildren = c.InverseParent.Count
                            }).ToList(),
                        };

                    }
                    else // get page data
                    {
                        var startRow = searchEntry.SearchQueryPage.PageNumber;
                        var data = await repository.QueryPageAsync(filters.Expression, sort.Expression, null, startRow, searchEntry.SearchQueryPage.PageLength);
                        var totalCount = await repository.CountAsync(filters.Expression);

                        page = new SearchResult<GroupDto>()
                        {
                            ItemList = data.Select(c => new GroupDto
                            {
                                Id = c.Id,
                                ParentId = c.ParentId,
                                Name = c.Name,
                                CountChildren = c.InverseParent.Count,
                                GroupID = c.GroupID,
                                Description = c.Description
                            }).ToList(),
                            SearchResultPage = new SearchResultPage()
                            {
                                TotalItemCount = totalCount,
                                PageLength = searchEntry.SearchQueryPage.PageLength,
                                PageNumber = totalCount / searchEntry.SearchQueryPage.PageLength
                            }
                        };
                    }

                }
            }
            catch (Exception ex) { throw ex; }

            return page;
        }

        public async Task<IEnumerable<GroupDto>> GetByParentIdAsync(string userId, SearchQuery searchEntry, string parentId)
        {
            // generate sort expression
            var sort = searchEntry?.OrderStatement == null
                ? new OrderBy<Groups>(c => c.OrderBy(d => d.Name))
                : new OrderBy<Groups>(searchEntry.OrderStatement.ColumName, searchEntry.OrderStatement.Reverse);
            // generate filter expression
            var filters = new Filter<Groups>(c => c.ParentId == new Guid(parentId));
            // generate includes expression
            var includes = new Includes<Groups>(c => c.Include(v => v.InverseParent).Include(v => v.Parent));

            IEnumerable<GroupDto> data;
            try
            {
                using (var uow = _uowProvider.CreateUnitOfWork())
                {
                    var repository = uow.GetRepository<Groups>();

                    // get data
                    var items = await repository.QueryAsync(filters.Expression, sort.Expression, includes.Expression);
                    data = items.Select(c => new GroupDto
                    {
                        Id = c.Id,
                        ParentId = c.ParentId,
                        Name = c.Name,
                        CountChildren = c.InverseParent.Count,
                        GroupID = c.GroupID,
                        Description = c.Description
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return data;
        }

        public async Task<GroupDto> CreateGroup(GroupDto model, string userId)
        {
            var entity = _mapper.Map<GroupDto, Groups>(model);
            try
            {
                // add new group
                Groups group = null;
                using (var uow = _uowProvider.CreateUnitOfWork())
                {
                    var repository = uow.GetRepository<Groups>();
                    group = await repository.AddAsync(entity);
                    await uow.SaveChangesAsync();
                }
                
                if (model.ParentId == null)
                {
                    // add member to group
                    await _memberService.AddUserToGroup(new MemberGroups { GroupId = entity.Id, UserId = new Guid(userId) });
                    // add role to member user
                    var user = await _userManager.FindByIdAsync(userId);
                    await _userManager.AddToRoleAsync(user, "GroupAdmin", group.Id);

                }
                // add paths between new descendant and exist ancestors
                await _groupNodeService.AddPathsBetweenDescendantAndAncestors(entity.Id, model.ParentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return _mapper.Map<Groups, GroupDto>(entity);
        }

        public async Task DeleteGroup(string groupId, string userId)
        {
            IEnumerable<GroupNode> ancestors;
            IEnumerable<GroupNode> offspring;
            IEnumerable<Groups> inverseParent;

            try
            {
                using (var uow = _uowProvider.CreateUnitOfWork())
                {
                    var repository = uow.GetRepository<Groups>();
                    var result = await repository.GetAsync<Guid>(new Guid(groupId), c => c.Include(d => d.Ancestors).Include(d => d.Offspring));
                    ancestors = result.Ancestors;
                    offspring = result.Offspring;
                }
                // delete all paths to group
                await _groupNodeService.DeletePathsForAncestorsByDescendants(ancestors, offspring);
                await _memberService.DeletePathsWhereGroup(groupId);
                using (var uow = _uowProvider.CreateUnitOfWork())
                {
                    var repository = uow.GetRepository<Groups>();
                    var result = await repository.GetAsync<Guid>(new Guid(groupId), c => c.Include(d => d.InverseParent));
                    inverseParent = result.InverseParent;
                    repository.Remove(result);
                    uow.SaveChanges();
                }

                foreach (var item in inverseParent)
                {
                    await _memberService.AddUserToGroup(new MemberGroups { GroupId = item.Id, UserId = new Guid(userId) });
                }


                // add new group
                //using (var uow = _uowProvider.CreateUnitOfWork())
                //{
                //    var repository = uow.GetRepository<Groups>();
                //    var result = await repository.AddAsync(entity);
                //    await uow.SaveChangesAsync();
                //}
                //// add user to group
                //if (model.ParentId == null)
                //{
                //    await _memberService.AddUserToGroup(new ApplicationUserGroups { GroupId = entity.Id, UserId = userId });
                //}
                //// add paths between new descendant and exist ancestors
                //await _groupNodeService.AddPathsBetweenDescendantAndAncestors(entity.Id, model.ParentId);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return;
        }


        //public async Task<DataPage<Groups, Guid>> GetRootAsync(PageSearchEntry searchEntry, string userId)
        //{
        //    var pageData = new PageData<Groups>();
        //    // sort
        //    var sort = new OrderBy<Groups>(c => c.OrderBy(d => d.Name)); // default order
        //    if (searchEntry.OrderStatement != null)
        //    {
        //        sort = new OrderBy<Groups>(searchEntry.OrderStatement.ColumName, searchEntry.OrderStatement.Reverse);
        //    }
        //    // init filter
        //    var filters = new Filter<Groups>(c => c.ApplicationUserGroups.Any(d =>d.UserId == userId));
        //    if (searchEntry.FilterStatements.Count() > 0)
        //    { }
        //    try
        //    {
        //        var results = await _pager.QueryAsync(
        //            searchEntry.PageNumber, // PageNumber should be more than 0!!!
        //            searchEntry.PageLength,
        //            filters,
        //            sort
        //            ); 
        //        return results;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //public async void SubscribeToGroupAsync(string userId, Guid groupId)
        //{
        //    var userGroup = await _appUserGroupRepository.GetAppUserGroupAsync(userId, groupId);
        //    if (userGroup == null)
        //    {
        //        await _appUserGroupRepository.CreateAppUserGroupAsync(new ApplicationUserGroups()
        //        {
        //            GroupId = groupId,
        //            UserId = userId
        //        });
        //    }
        //}

        //public async Task UnsubscribeToGroup(string userId, Guid groupId)
        //{
        //    var userGroup = await _appUserGroupRepository.GetAppUserGroupAsync(userId, groupId);
        //    if (userGroup == null)
        //    {
        //        throw new ArgumentException("not found userGroup");
        //    }
        //    _appUserGroupRepository.DeleteAppUserGroupAsync(userGroup);

        //    return;
        //}
        //}
    }
}
