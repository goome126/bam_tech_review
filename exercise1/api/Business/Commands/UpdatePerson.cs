using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string OldName { get; set; }
        public required string NewName { get; set; }
    }

    public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
    {
        private readonly StargateContext _context;
        public UpdatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public Task Process(UpdatePerson request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.NewName)) throw new BadHttpRequestException("Bad Request");

            var updatedPerson = _context.People.FirstOrDefault(z => z.Name == request.OldName);
            if (updatedPerson is null) throw new BadHttpRequestException("Bad Request");

            // Check if anyone already has the same name they're trying to update to.
            var person = _context.People.FirstOrDefault(z => z.Name == request.NewName);
            if (person is not null) throw new BadHttpRequestException("Bad Request");

            return Task.CompletedTask;
        }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            // Don't want to consider whitespace as valid names for people
            if (string.IsNullOrWhiteSpace(request.NewName)) throw new BadHttpRequestException("Bad Request");

            var updatedPerson = _context.People.FirstOrDefault(z => z.Name == request.OldName);
            //if (updatedPerson is null) throw new BadHttpRequestException("Bad Request");

            // Check if anyone already has the same name they're trying to update to.
            var person = _context.People.FirstOrDefault(z => z.Name == request.NewName);
            //if (person is not null) throw new BadHttpRequestException("Bad Request");

            updatedPerson.Name = request.NewName;

            await _context.SaveChangesAsync();

            return new UpdatePersonResult()
            {
                Id = updatedPerson.Id,
                Name = updatedPerson.Name
            };

        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
