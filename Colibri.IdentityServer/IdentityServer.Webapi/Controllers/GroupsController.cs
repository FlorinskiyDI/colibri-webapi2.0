﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using dataaccesscore.EFCore.Paging;
using dataaccesscore.EFCore.Query;
using ExpressionBuilder.Operations;
using IdentityServer.Webapi.Data;
using IdentityServer.Webapi.Dtos.Pager;
using IdentityServer.Webapi.Repositories.Interfaces;
using IdentityServer.Webapi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace IdentityServer.Webapi.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = "user")]
    [Produces("application/json")]
    [Route("api/groups")]
    public class GroupsController : Controller
    {

        // GET: api/groups
        // POST: api/groups/root
        // GET: api/groups/{id}/subgroups
        // POST: api/groups
        // PUT: api/groups
        // DELETE: api/groups/{id}

        protected readonly IGroupServices _groupServices;
        private readonly IGroupRepository _groupRepository;
        private readonly IAppUserGroupRepository _appUserGroupRepository;

        public GroupsController(
            IGroupServices groupServices,
            IGroupRepository groupRepository,
            IAppUserGroupRepository appUserGroupRepository
        )
        {
            _groupRepository = groupRepository;
            _groupRepository = groupRepository;
            _appUserGroupRepository = appUserGroupRepository;
            _groupServices = groupServices;
        }

        // GET: api/groups/
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var claims = this.HttpContext.User.Claims;
            var userId = claims.First(c => c.Type == "sub").Value;
            var list = await _groupRepository.GetRootWithInverseAsync(userId);

            var resultList = list.ToList();
            foreach (var item in list)
            {
                recursiveTreeSearch(item, resultList);
            }

            return Ok(resultList);
        }

        private List<Groups> recursiveTreeSearch(Groups group, List<Groups> siblings)
        {
            if (group.InverseParent.Count() > 0)
            {
                var cc = group.InverseParent;
                siblings.AddRange(cc);
                foreach (var item in group.InverseParent)
                {
                    recursiveTreeSearch(item, siblings);
                }
            }
            return siblings;
        }

        // POST: api/groups/root
        [HttpPost("root")]
        public async Task<IActionResult> GetRootGroups([FromBody] PageSearchEntry searchEntry)
        {
            var claims = this.HttpContext.User.Claims;
            var userId = claims.First(c => c.Type == "sub").Value;
            //
            var result = await _groupServices.GetRootAsync(searchEntry, userId);
            return Ok(result);
        }

        //public static T GetTfromString<T>(string mystring)
        //{
        //    var foo = TypeDescriptor.GetConverter(typeof(T));
        //    return (T)(foo.ConvertFromInvariantString(mystring));
        //}

        // GET: api/groups/{id}/subgroups
        [HttpGet("{id}/subgroups")]
        public async Task<IActionResult> GetSubGroups(Guid id)
        {

            #region test filter
            //var columnNames = new List<string>(new string[] { "Id", "Name" });
            //var pageNumber = 1;
            //var pageLength = 10;

            //var parameter = Expression.Parameter(typeof(Groups), "x");
            //var member = Expression.Property(parameter, "Name"); //x.Name
            //var constant = Expression.Constant("mytest1");
            //var body = Expression.Equal(member, constant); //x.Name = "mytest1"
            //var finalExpression = Expression.Lambda<Func<Groups, bool>>(body, parameter); //x => x.Name >= "mytest1"
            //try
            //{
            //    Type myType = typeof(Groups).GetProperty("Name").PropertyType;
            //    MethodInfo method = typeof(GroupsController).GetMethod("GetTfromString");
            //    MethodInfo generic = method.MakeGenericMethod(myType);
            //    var ccc = generic.Invoke(this, new[] { "03.03.1993" });

            //    var filtertest = new ExpressionBuilder.Generics.Filter<Groups>();
            //    filtertest.By("id", Operation.NotEqualTo, new Guid("5d35f7d0-4e5c-e811-9c5c-d017c2aa438d"));

            //    var filter = new Filter<Groups>(null);
            //    filter.AddExpression(filtertest);


            //    var results = await _pager.QueryAsync(pageNumber, pageLength, filter);
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
            #endregion



            var list = await _groupRepository.GetSubGroupsAsync(id);
            return Ok(list);
        }

        // GET: api/groups/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(Guid id)
        {
            Groups entity;
            try
            {
                entity = await _groupRepository.GetAsync(id);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(entity);
        }

        // DELETE: api/groups/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteGroup(Guid id)
        {
            try
            {
                _appUserGroupRepository.DeleteAppUserGroupByGroupAsync(id);
                _groupRepository.DeleteGroup(id);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
        }

        // POST: api/groups
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] Groups model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var claims = this.HttpContext.User.Claims;
            var userId = claims.First(c => c.Type == "sub").Value;
            Groups group;
            try
            {
                group = await _groupRepository.CreateGroupAsync(model);
                // if the parent has not been set, specify the user as an owner
                if (model.ParentId == null)
                {
                    await _appUserGroupRepository.CreateAppUserGroupAsync(new ApplicationUserGroups()
                    {
                        GroupId = group.Id,
                        UserId = userId
                    });
                }

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(group);
        }

        // PUT: api/groups
        [HttpPut]
        public IActionResult UpdateGroup([FromBody] Groups model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var claims = this.HttpContext.User.Claims;
            var userId = claims.First(c => c.Type == "sub").Value;
            Groups group;
            try
            {
                group = _groupRepository.UpdateGroup(model);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok(group);
        }

    }

    //public class Test
    //{
    //    public int MyInt{ get; set; }
    //    public string MyString { get; set; }
    //    public DateTime MyDate { get; set; }
    //}
}