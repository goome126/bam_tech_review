using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Dtos;

namespace StargateTests
{
    public class StargateAllTests: IClassFixture<StargateContextFixture>
    {
        // Important Test cases to cover

        // For Person Related
        // Attempting to create a person with a blank or whitespace name returns an error code.
        // Attempting to create a person with the same name as another person returns an error.
        // Attempting to create a person with a unique name returns success.
        // Searching by name for a person that doesn't exist returns nothing.
        // Searching for all people returns all people.
        // If no people exist searching for all people returns nothing.
        // When searching for a person by name or all people, ensure that people without astronaut details return null for astronaut related fields.
        // Ensure that if someone who has astronaut fields returns with them.

        // For Astronaut Duty Related
        // Ensure that no astronaut duties are returned for a person who isn't an astronaut
        // Ensure that if a person has astronaut duties, the controller returns them with their duties
        // Validate that if an invalid person is passed in, an error is returned.
        // Validate that when attempting to create an Astronaut Duty, it works when all of the fields are valid and it throws an error when any or all of them are invalid.
        // Validate that the astronauty dute changes the end date of the previous duty to be 1 day before the start date of the new duty when adding a new duty for a person.
        // A person's Career End Date is set correctly when a duty with the title RETIRED is received

        private readonly StargateContext _context;
        private readonly PersonController _personController;
        private readonly AstronautDutyController _astronautDutyController;
        private readonly Mock<IMediator> _mediator;
        private readonly GetPeopleHandler _peopleHandler;
        private readonly GetPersonByNameHandler _personByNameHandler;
       //private readonly Mock<StargateContext> _mockContext;

        public StargateAllTests(StargateContextFixture fixture)
        {
            _context = fixture.Context;
            _mediator = new Mock<IMediator>();
            /*_personController = new PersonController(_mediator.Object);
            _astronautDutyController = new AstronautDutyController(_mediator.Object);*/
            _peopleHandler = new GetPeopleHandler(_context);
            _personByNameHandler = new GetPersonByNameHandler(_context);
        }

        [Fact]
        public async void AllPersonsReturnedWhenCallingGetPeopleHandler()
        {
            // Arrange
            var examplePerson = new List<Person>()
            {
                new Person { Name = "John Doe" },
                new Person { Name = "Jane Doe" },
                new Person { Name = "John Smith" }
            };
            _context.People.AddRange(examplePerson);
            _context.SaveChanges();

            // Act
            var result = await _peopleHandler.Handle(new GetPeople { }, new CancellationToken());

            foreach (var person in result.People)
            {
                Assert.Contains(person.Name, examplePerson.Select(p => p.Name));
            }

            // Tear Down
            ClearPeopleDB();

        }

        [Fact]
        public async void NothingReturnedWhenNoPeopleInDatabase()
        {
            // Arrange
            // Do Nothing

            // Act
            var result = await _peopleHandler.Handle(new GetPeople { }, new CancellationToken());

            Assert.Empty(result.People);

            // Tear Down
            ClearPeopleDB();
        }

        [Theory]
        [InlineData("John Doe")]
        [InlineData("Jane Doe")]
        [InlineData("John Smith")]
        public async void PersonReturnedWhenSearchingByValidNameWithoutBeingAstronaut(string name)
        {
            // Arrange
            var examplePerson = new List<Person>()
            {
                new Person { Name = "John Doe" },
                new Person { Name = "Jane Doe" },
                new Person { Name = "John Smith" }
            };
            _context.People.AddRange(examplePerson);
            _context.SaveChanges();

            // Create PeopleAstronaut object to compare against
            var personToSearch = _context.People.Where(p => p.Name == name).FirstOrDefault();
            var personAstronaut = new PersonAstronaut()
            {
                PersonId = personToSearch.Id,
                Name = personToSearch.Name,
                CurrentRank = null,
                CurrentDutyTitle = null,
                CareerEndDate = null,
                CareerStartDate = null
            };

            // Act
            var result = await _personByNameHandler.Handle(new GetPersonByName { Name = name }, new CancellationToken());

            Assert.Equivalent(personAstronaut, result.Person);

            // Tear Down
            ClearPeopleDB();
        }

        [Theory]
        [InlineData("Carl")]
        [InlineData("Joseph Smith")]
        public async void SearchingForPersonWithIncorrectNameReturnsNothing(string name)
        {
            // Arrange
            var examplePerson = new List<Person>()
            {
                new Person { Name = "John Doe" },
                new Person { Name = "Jane Doe" },
                new Person { Name = "John Smith" }
            };
            _context.People.AddRange(examplePerson);
            _context.SaveChanges();

            // Act
            var result = await _personByNameHandler.Handle(new GetPersonByName { Name = name }, new CancellationToken());

            Assert.Null(result.Person);

            // Tear Down
            ClearPeopleDB();
        }

        [Theory]
        [InlineData("Carl")]
        [InlineData("Joseph Smith")]
        public async void SearchingForPersonWhenNoOneInDatabaseReturnsNothing(string name)
        {
            // Arrange
            // Do Nothing

            // Act
            var result = await _personByNameHandler.Handle(new GetPersonByName { Name = name }, new CancellationToken());

            Assert.Null(result.Person);

            // Tear Down
            ClearPeopleDB();
        }




        private void ClearPeopleDB()
        {
            _context.People.RemoveRange(_context.People);
            _context.SaveChanges();
        }

        private void ClearAstronautDutiesDB()
        {
            _context.AstronautDuties.RemoveRange(_context.AstronautDuties);
            _context.SaveChanges();
        }

        private void ClearAstronautDetailsDB()
        {
            _context.AstronautDetails.RemoveRange(_context.AstronautDetails);
            _context.SaveChanges();
        }
    }
}