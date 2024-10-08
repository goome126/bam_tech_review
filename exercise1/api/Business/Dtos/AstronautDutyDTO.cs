﻿using StargateAPI.Business.Data;

namespace StargateAPI.Business.Dtos
{
    public class AstronautDutyDTO
    {
        public AstronautDutyDTO(AstronautDuty astronautDuty)
        {
            Id = astronautDuty.Id;
            Rank = astronautDuty.Rank;
            DutyTitle = astronautDuty.DutyTitle;
            DutyStartDate = astronautDuty.DutyStartDate;
            DutyEndDate = astronautDuty.DutyEndDate;
        }
        public int Id { get; set; }

        public string Rank { get; set; } = string.Empty;

        public string DutyTitle { get; set; } = string.Empty;
        public DateTime DutyStartDate { get; set; }
        public DateTime? DutyEndDate { get; set; }
    }
}
