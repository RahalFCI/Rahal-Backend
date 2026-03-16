using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Users.Domain.Entities._Common;
using Users.Domain.Enums;

namespace Users.Domain.Entities
{
    public class Explorer : User
    {
        public string Bio { get; set; } = string.Empty;
        public required string CountryCode { get; set; } //TODO: validate on the country code
        public required GenderEnum Gender { get; set; }
        public int AvailableXp { get; set; } = 0;
        public int CumaltiveXp { get; set; } = 0;
        public int Level { get; set; } = 1;
        public bool IsPublic { get; set; } = true;
        public bool IsPremium { get; set; } = false;
        public int PlanTierId { get; set; }
        //TODO: AddPlan Tier Navigation Property


        public required DateOnly BirthDate { get; set; }

        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;

                //Adjust if birthday hasn't occurred yet this year
                if (BirthDate.DayOfYear < today.DayOfYear)
                    age--;

                return age;
            }
        }
    }
}
