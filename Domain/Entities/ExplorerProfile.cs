using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities._Common;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{


    public class ExplorerProfile : BaseAuditableEntity
    {

        public required Guid UserId { get; set; }


        public required User User { get; set; }


        public required GenderEnum Gender { get; set; }

        public required DateOnly BirthDate { get; set; }


        public string Bio { get; set; } = string.Empty;


        public required string CountryCode { get; set; }


        public int AvailableXp { get; set; } = 0;

 
        public int CumulativeXp { get; set; } = 0;

        public int Level { get; set; } = 1;


        public bool IsPublic { get; set; } = true;


        public bool IsPremium { get; set; } = false;

        public int? PlanTierId { get; set; }


        // public PlanTier? PlanTier { get; set; } TODO: uncomment

        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.DayOfYear > today.DayOfYear)
                    age--;
                return age;
            }
        }
    }
}

