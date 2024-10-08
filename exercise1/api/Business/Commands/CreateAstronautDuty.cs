﻿using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime BackingDutyStartDate { get; set; }
        public DateTime DutyStartDate
        {
            get => BackingDutyStartDate;
            set
            {
                if (value.Kind == DateTimeKind.Unspecified)
                {
                    BackingDutyStartDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }
                else
                {
                    BackingDutyStartDate = value.ToUniversalTime();
                }
            }
        }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = _context.People.AsNoTracking().Include(p => p.AstronautDuties).FirstOrDefault(z => z.Name == request.Name);

            if (person is null) throw new BadHttpRequestException("Bad Request");

            var verifyNoPreviousDuty = _context.AstronautDuties.FirstOrDefault(z => z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate);

            if (verifyNoPreviousDuty is not null) throw new BadHttpRequestException("Bad Request");

            if (person.AstronautDuties is not null)
            {
                if(person.AstronautDuties.Count > 0)
                {
                    if (request.DutyStartDate.Date <= person.AstronautDuties.Where(ad => ad.DutyEndDate == null).First().DutyStartDate.Date)
                    {
                        throw new BadHttpRequestException("Bad Request");
                    }
                }
            }

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            #region Code for adding/creating someone's astronaut record.

            // Why using raw queries when ef?
            // Theoretically we'll never get to this point because of the preprocessor, but it's good practice to not have code where it could occur.
            // Even if a step higher in the pipeline will cover it.
            // This is why I'm not a fan of CQRS.
            var person = await _context.People.AsNoTracking().Include(p => p.AstronautDetail).Include(p => p.AstronautDuties).FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken) ?? throw new BadHttpRequestException("Bad Request");
            var astronautDetail = person.AstronautDetail;

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                if (request.DutyTitle == "RETIRED")
                {
                    // Curious that they have the correct rule applied in the else below
                    // but not in this case.
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail);

            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            // To be honest we should be saving the duty first and then updating the astronaut's detail
            // but that's just my opinion.

            #endregion

            var astronautDuty = person.AstronautDuties.Where(ad => ad.DutyEndDate == null).FirstOrDefault();  
            //Should probably check if astronautDuty returns more than one value and if so correct it.
            if (astronautDuty != null)
            {
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty,cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
