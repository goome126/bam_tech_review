using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleHandler : IRequestHandler<GetPeople, GetPeopleResult>
    {
        public readonly StargateContext _context;
        public GetPeopleHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<GetPeopleResult> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            var result = new GetPeopleResult();

            var people = from person in _context.People
                        join astronautDetail in _context.AstronautDetails
                        on person.Id equals astronautDetail.PersonId into details
                        from detail in details.DefaultIfEmpty()
                        select new PersonAstronaut
                        {
                            PersonId = person.Id,
                            Name = person.Name,
                            CurrentRank = detail.CurrentRank,
                            CurrentDutyTitle = detail.CurrentDutyTitle,
                            CareerStartDate = detail.CareerStartDate,
                            CareerEndDate = detail.CareerEndDate
                        };

            result.People = await people.ToListAsync(cancellationToken);

            return result;
        }
    }

    public class GetPeopleResult : BaseResponse
    {
        public List<PersonAstronaut> People { get; set; } = new List<PersonAstronaut> { };

    }
}
