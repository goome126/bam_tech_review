using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var result = new GetPersonByNameResult();

            var personToSearch = from person in _context.People
                         join astronautDetail in _context.AstronautDetails
                         on person.Id equals astronautDetail.PersonId into details
                         from detail in details.DefaultIfEmpty()
                         where person.Name == request.Name
                         select new PersonAstronaut
                         {
                             PersonId = person.Id,
                             Name = person.Name,
                             CurrentRank = detail.CurrentRank,
                             CurrentDutyTitle = detail.CurrentDutyTitle,
                             CareerStartDate = detail.CareerStartDate,
                             CareerEndDate = detail.CareerEndDate
                         };

            result.Person = personToSearch.FirstOrDefault();

            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
