using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {

            var result = new GetAstronautDutiesByNameResult();

            var requestPerson = await _context.People.Where(p => p.Name == request.Name).Include(ad => ad.AstronautDuties).Include(ad => ad.AstronautDetail).FirstOrDefaultAsync();
            if(requestPerson is null) throw new BadHttpRequestException("Bad Request");
            if (requestPerson.AstronautDetail is null) throw new BadHttpRequestException("Bad Request");
            var person = new PersonAstronaut()
            {
                PersonId = requestPerson.Id,
                Name = requestPerson.Name,
                CurrentRank = requestPerson.AstronautDetail.CurrentRank,
                CurrentDutyTitle = requestPerson.AstronautDetail.CurrentDutyTitle,
                CareerStartDate = requestPerson.AstronautDetail.CareerStartDate,
                CareerEndDate = requestPerson.AstronautDetail.CareerEndDate
            };

            result.Person = person;

            result.AstronautDuties = requestPerson.AstronautDuties.OrderByDescending(ad => ad.DutyStartDate).Select(ad => new AstronautDutyDTO(ad)).ToList();
            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDutyDTO> AstronautDuties { get; set; } = new List<AstronautDutyDTO>();
    }
}
