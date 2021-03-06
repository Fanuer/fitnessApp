﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.OData;
using System.Xml;
using fIT.WebApi.Entities;
using fIT.WebApi.Models;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace fIT.WebApi.Controller
{
    /// <summary>
    /// Handles User-based Actions
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Accounts")]
    [SwaggerResponse(HttpStatusCode.InternalServerError, "An internal Server error has occured")]
    public class AccountsController : BaseApiController
    {
        /// <summary>
        /// Method to prove the Serversa availability
        /// </summary>
        [ResponseType(typeof(void))]
        [Route("Ping")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult Ping()
        {
            var result = new { timestamp = DateTime.Now };
            return this.Ok(result);
        }

        #region Users
        /// <summary>
        /// Gets all application Users
        /// </summary>
        [Route("User")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<UserModel>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [Authorize(Roles = "Admin")]
        [EnableQuery]
        public IQueryable<UserModel> GetUsers()
        {
            return this.AppUserManager.Users.ToList().Select(u => this.TheModelFactory.CreateViewModel(u)).AsQueryable();
        }

        /// <summary>
        /// Get a user by its guid
        /// </summary>
        /// <param name="id">User's guid</param>
        [Route("User/{id:guid}", Name = "GetUserById")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetUser(string id)
        {
            var user = await this.AppUserManager.FindByIdAsync(id);

            if (user != null)
            {
                return Ok(this.TheModelFactory.CreateViewModel(user));
            }

            return NotFound();

        }

        /// <summary>
        /// Get User by Username
        /// </summary>
        /// <param name="username">username to search for</param>
        [Route("User/{username}")]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetUserByName(string username)
        {
            var user = await this.AppUserManager.FindByNameAsync(username);
            if (user != null)
            {
                return Ok(this.TheModelFactory.CreateViewModel(user));
            }
            return NotFound();
        }
        /// <summary>
        /// Returns the current users Information
        /// </summary>
        [Route("CurrentUser")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(UserModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetCurrentUser()
        {
            var currentUserId = User.Identity.GetUserId();
            var user = await this.AppUserManager.FindByIdAsync(currentUserId);

            if (user != null)
            {
                return Ok(this.TheModelFactory.CreateViewModel(user));
            }
            return NotFound();
        }

        /// <summary>
        /// Updates user data
        /// </summary>
        /// <param name="model">user data</param>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("CurrentUser")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        public async Task<IHttpActionResult> UpdateCurrentUser(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.Identity.GetUserId();
            var user = await this.AppUserManager.FindByIdAsync(currentUserId);
            user = this.TheModelFactory.CreateModel(model, user);
            var result = await this.AppUserManager.UpdateAsync(user);
            return !result.Succeeded ? GetErrorResult(result) : StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// User can register to the Application
        /// </summary>
        /// <param name="createUserModel">new User</param>
        [SwaggerResponse(HttpStatusCode.NoContent, Type = typeof(UserModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Register(CreateUserModel createUserModel)
        {
            // validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ApplicationUser user = null;
            try
            {
                user = new ApplicationUser()
                {
                    UserName = createUserModel.Username,
                    Email = createUserModel.Email,
                    Gender = createUserModel.Gender,
                    Fitness = createUserModel.Fitness,
                    Job = createUserModel.Job,
                    DateOfBirth = createUserModel.DateOfBirth

                };
            }
            catch (Exception e)
            {
                
                throw e;
            }
            

            IdentityResult addUserResult = await this.AppUserManager.CreateAsync(user, createUserModel.Password);

            if (!addUserResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));

            return Created(locationHeader, TheModelFactory.CreateViewModel(user));
        }

        /// <summary>
        /// User can change its password
        /// </summary>
        /// <param name="model">Data to change a password</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [Route("ChangePassword")]
        [HttpPut]
        [ResponseType(typeof(ChangePasswordModel))]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await this.AppUserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        /// <summary>
        /// Admin can delete User
        /// </summary>
        /// <param name="id">Id of the user to delete</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [Route("User/{id:guid}")]
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {
            //Only SuperAdmin or Admin can delete users (Later when implement roles)
            var appUser = await this.AppUserManager.FindByIdAsync(id);
            if (appUser != null)
            {
                var result = await this.AppUserManager.DeleteAsync(appUser);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
                return Ok();
            }
            return NotFound();
        }

        #endregion

        #region Roles
        /// <summary>
        /// Assigned the user to the given roles
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="rolesToAssign">Roles to assign to the user</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [Authorize(Roles = "Admin")]
        [Route("User/{id:guid}/Roles")]
        [HttpPut]
        public async Task<IHttpActionResult> AssignRolesToUser([FromUri] string id, [FromBody] string[] rolesToAssign)
        {
            var appUser = await this.AppUserManager.FindByIdAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }

            var currentRoles = await this.AppUserManager.GetRolesAsync(appUser.Id);
            var rolesNotExists = rolesToAssign.Except(this.AppRoleManager.Roles.Select(x => x.Name)).ToArray();

            if (rolesNotExists.Any())
            {
                ModelState.AddModelError("", string.Format("Roles '{0}' does not exixts in the system", string.Join(",", rolesNotExists)));
                return BadRequest(ModelState);
            }

            IdentityResult removeResult = await this.AppUserManager.RemoveFromRolesAsync(appUser.Id, currentRoles.ToArray());
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return BadRequest(ModelState);
            }

            IdentityResult addResult = await this.AppUserManager.AddToRolesAsync(appUser.Id, rolesToAssign);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                return BadRequest(ModelState);
            }

            return Ok();
        }
        #endregion
    }
}
