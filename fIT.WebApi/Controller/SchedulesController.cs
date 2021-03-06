﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.OData;
using fIT.WebApi.Models;
using Microsoft.AspNet.Identity;
using Swashbuckle.Swagger.Annotations;

namespace fIT.WebApi.Controller
{
    /// <summary>
    /// Grants access to schedule data
    /// </summary>
    [SwaggerResponse(HttpStatusCode.Unauthorized, "You are not allowed to receive this resource")]
    [SwaggerResponse(HttpStatusCode.InternalServerError, "An internal Server error has occured")]
    [Authorize]
    [RoutePrefix("api/schedule")]
    public class SchedulesController : BaseApiController
    {
        /// <summary>
        /// Gets all schedules of a user or all, if logged in as an admin
        /// </summary>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<ScheduleModel>))]
        [Route("")]
        [HttpGet]
        [EnableQuery]
        public async Task<IQueryable<ScheduleModel>> GetSchedules()
        {
            var all = await this.AppRepository.Schedules.GetAllAsync();
            return all.Where(x => IsValidSchedule(x.UserID)).Select(this.TheModelFactory.CreateViewModel).AsQueryable();
        }

        /// <summary>
        /// Get one Schedule
        /// </summary>
        /// <param name="id">Schedule id</param>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ScheduleModel))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpGet]
        [Route("{id:int}", Name = "GetScheduleById")]
        public async Task<IHttpActionResult> GetSchedule(int id)
        {
            var schedule = await this.AppRepository.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            if (!IsValidSchedule(schedule.UserID))
            {
                ModelState.AddModelError("UserId", "You can only delete your own schedules");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(this.TheModelFactory.CreateViewModel(schedule));
        }

        /// <summary>
        /// Updates an existing schedule
        /// </summary>
        /// <param name="schedule">New schedule Data</param>
        /// <param name="id">Schedule id</param>
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("{id:int}")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateSchedule([FromUri]int id, [FromBody]ScheduleModel schedule)
        {
            if (id != schedule.Id)
            {
                ModelState.AddModelError("id", "The given id have to be the same as in the model");
            }
            else if(!IsValidSchedule(schedule.UserId))
            {
                ModelState.AddModelError("id", "You can only update your own schedules");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await this.AppRepository.Schedules.ExistsAsync(id);

            try
            {
                var orig = await this.AppRepository.Schedules.FindAsync(id);
                orig = this.TheModelFactory.CreateModel(schedule, orig);
                await this.AppRepository.Schedules.UpdateAsync(orig);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!exists)
                {
                    return NotFound();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Create new Schedule for the logged in user
        /// </summary>
        /// <param name="schedule">Schedule data to create</param>
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(ScheduleModel))]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [Route("")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateSchedule(ScheduleModel schedule)
        {
            if (ModelState.IsValid && !schedule.UserId.Equals(this.CurrentUserId))
            {
                ModelState.AddModelError("UserId", Resources.Error_CreateScheduleOnlyForYourself);
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var datamodel = this.TheModelFactory.CreateModel(schedule);
            await this.AppRepository.Schedules.AddAsync(datamodel);
            var result = this.TheModelFactory.CreateViewModel(datamodel);
            return CreatedAtRoute("GetScheduleById", new { id = schedule.Id }, result);
        }

        /// <summary>
        /// Removes a schedule
        /// </summary>
        /// <param name="id">Id of the schedule to delete</param>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("{id:int}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSchedule(int id)
        {
            var schedule = await this.AppRepository.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            if (!IsValidSchedule(schedule.UserID))
            {
                ModelState.AddModelError("UserId", "You can only delete your own schedules");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await this.AppRepository.Schedules.RemoveAsync(schedule);
            return Ok();
        }

        /// <summary>
        /// Adds an existing exercise to a schedule
        /// </summary>
        /// <param name="scheduleId">Id of a schedule of the logged in user</param>
        /// <param name="exerciseId">Id of an exercise</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("{scheduleId:int}/Exercise/{exerciseId:int}")]
        [HttpPut]
        public async Task<IHttpActionResult> AddExerciseToSchedule(int scheduleId, int exerciseId)
        {
            var schedule = await this.AppRepository.Schedules.FindAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound();
            }
            if (!IsValidSchedule(schedule.UserID))
            {
                ModelState.AddModelError("UserId", "You can only delete your own schedules");
            }
            if (schedule.Exercises!= null && schedule.Exercises.Any(x=>x.Id==exerciseId))
            {
                ModelState.AddModelError("", "Exercise already exists within the given schedule");
            }

            var exercise = await this.AppRepository.Exercise.FindAsync(exerciseId);
            if (exercise == null)
            {
                return NotFound();
            }
            
            bool adding = false;
            if (ModelState.IsValid)
            {
                adding = await this.AppRepository.Schedules.AddExerciseAsync(schedule.Id, exerciseId);
            }
            if (!adding)
            {
                ModelState.AddModelError("", "Error on adding the exercise");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok();
        }

        /// <summary>
        /// Removes an existing exercise from a schedule
        /// </summary>
        /// <param name="scheduleId">Id of a schedule of the logged in user</param>
        /// <param name="exerciseId">Id of an exercise</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.BadRequest)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        [Route("{scheduleId:int}/Exercise/{exerciseId:int}")]
        public async Task<IHttpActionResult> RemoveExerciseFromSchedule(int scheduleId, int exerciseId)
        {
            var schedule = await this.AppRepository.Schedules.FindAsync(scheduleId);
            if (schedule == null)
            {
                return NotFound();
            }
            if (!IsValidSchedule(schedule.UserID))
            {
                ModelState.AddModelError("UserId", "You can only delete your own schedules");
            }
            if (schedule.Exercises != null && schedule.Exercises.All(x => x.Id != exerciseId))
            {
                return NotFound();
            }

            var exercise = await this.AppRepository.Exercise.FindAsync(exerciseId);
            if (exercise == null)
            {
                return NotFound();
            }

            bool removing = false;
            if (ModelState.IsValid)
            {
                removing = await this.AppRepository.Schedules.RemoveExerciseAsync(schedule.Id, exerciseId);
            }
            if (!removing)
            {
                ModelState.AddModelError("", "Error on removing the exercise");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok();
        }

        private bool IsValidSchedule(string schedulesUserId)
        {
            return schedulesUserId.Equals(User.Identity.GetUserId());
        }
    }
}
