﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace fIT.WebApi.Models
{
    /// <summary>
    /// Trainingsplan
    /// </summary>
    public class ScheduleModel : EntryModel<int>
    {
        public ScheduleModel()
        {
            this.Exercises = new List<EntryModel<int>>();
        }

        /// <summary>
        /// Nutzer, dem der Trainingsplan gehört
        /// </summary>
        [Required(ErrorMessageResourceName = "Error_Required", ErrorMessageResourceType = typeof(Resources))]
        public string UserId { get; set; }
        /// <summary>
        /// Zugehoerige Uebungen
        /// </summary>
        public IEnumerable<EntryModel<int>> Exercises { get; set; }

    }
}