﻿using storagecore.EntityFrameworkCore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.DomainModelLayer.Entities
{
    public partial class SurveySections : BaseEntity<Guid>
    {
        public SurveySections()
        {
            Pages = new HashSet<Pages>();
        }

        //public Guid Id { get; set; }
        [Required]
        [StringLength(500)]
        public string Name { get; set; }
        public string Description { get; set; }

        [InverseProperty("Survey")]
        public ICollection<Pages> Pages { get; set; }
    }
}