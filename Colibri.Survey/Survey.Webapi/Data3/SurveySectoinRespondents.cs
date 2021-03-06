﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Survey.Webapi.Data3
{
    [Table("SurveySectoin_Respondents")]
    public partial class SurveySectoinRespondents
    {
        public Guid Id { get; set; }
        public Guid RespondentId { get; set; }
        public Guid SurveySectionId { get; set; }

        [ForeignKey("RespondentId")]
        [InverseProperty("SurveySectoinRespondents")]
        public Respondents Respondent { get; set; }
        [ForeignKey("SurveySectionId")]
        [InverseProperty("SurveySectoinRespondents")]
        public SurveySections SurveySection { get; set; }
    }
}
