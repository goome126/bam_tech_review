using Microsoft.AspNetCore.Mvc;
using Moq;
using StargateAPI.Business;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Commands;

namespace StargateTests
{
    public class StargateAllTests: IClassFixture<StargateContextFixture>
    {
        // Important Test cases to cover

        // For Person Related

        // Done
        // Searching by name for a person that doesn't exist returns nothing.
        // Searching for all people returns all people.
        // If no people exist searching for all people returns nothing.
        // Attempting to create a person with a unique name returns success.
        // Attempting to create a person with a blank or whitespace name returns an error code.
        // Attempting to create a person with the same name as another person returns an error.


        // When searching for a person by name or all people, ensure that people without astronaut details return null for astronaut related fields.
        // Ensure that if someone who has astronaut fields returns with them.
        // Updating a person's name to a new name should succeed if the new name is unique.
        // Updating a person's name to a new name should fail if the new name is not unique.

        // For Astronaut Duty Related
        // Validate that if an invalid person is passed in, an error is returned.

        // Ensure that no astronaut duties are returned for a person who isn't an astronaut
        // Ensure that if a person has astronaut duties, the controller returns them with their duties
        // Validate that when attempting to create an Astronaut Duty, it works when all of the fields are valid and it throws an error when any or all of them are invalid.

        // Validate that the astronauty dute changes the end date of the previous duty to be 1 day before the start date of the new duty when adding a new duty for a person.
        // A person's Career End Date is set correctly when a duty with the title RETIRED is received

        private readonly StargateContext _context;
        private readonly PersonController _personController;
        private readonly AstronautDutyController _astronautDutyController;
        private readonly Mock<IMediator> _mediator;
        private readonly GetPeopleHandler _peopleHandler;
        private readonly GetPersonByNameHandler _personByNameHandler;
        private readonly CreatePersonHandler _createPersonHandler;
        private readonly UpdatePersonHandler _updatePersonHandler;
        private readonly CreateAstronautDutyHandler _createAstronautDutyHandler;
        private readonly GetAstronautDutiesByNameHandler _getAstronautDutiesByNameHandler;
       //private readonly Mock<StargateContext> _mockContext;

        public StargateAllTests(StargateContextFixture fixture)
        {
            _context = fixture.Context;
            _mediator = new Mock<IMediator>();
            //_mediator.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), new CancellationToken()));
            //_mediator.Setup(m => m.Send(It.IsAny<GetPeople>(), new CancellationToken()));
            /*_personController = new PersonController(_mediator.Object);
            _astronautDutyController = new AstronautDutyController(_mediator.Object);*/
            _peopleHandler = new GetPeopleHandler(_context);
            _personByNameHandler = new GetPersonByNameHandler(_context);
            _createPersonHandler = new CreatePersonHandler(_context);
            _updatePersonHandler = new UpdatePersonHandler(_context);
            _createAstronautDutyHandler = new CreateAstronautDutyHandler(_context);
            _getAstronautDutiesByNameHandler = new GetAstronautDutiesByNameHandler(_context);
        }

        #region Person Tests

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

        [Fact]
        public async void CreatePersonWithUniqueNameShouldSucceed()
        {
            // Arrange
            var person = new Person { Name = "John Doe" };

            // Act
            var result = await _createPersonHandler.Handle(new CreatePerson { Name = person.Name }, new CancellationToken());

            // Assert
            Assert.True(result.Success);
            Assert.Single(_context.People);

            // Tear Down
            ClearPeopleDB();
        }

        [Fact]
        public async void CreatePersonWithBlankNameShouldError()
        {
            // Arrange
            var person = new Person { Name = " " };

            try
            {
                // Act
                var result = await _createPersonHandler.Handle(new CreatePerson { Name = person.Name }, new CancellationToken());
            } catch (Exception ex)
            {

               // Assert
                Assert.Equal("Bad Request", ex.Message);
            }
            // Tear Down
            ClearPeopleDB();
        }

        [Fact]
        public async void CreatePersonWithSameNameShouldError()
        {
            // Arrange
            var notUniquePerson = new Person { Name = "Adam" };
            var result = await _createPersonHandler.Handle(new CreatePerson { Name = notUniquePerson.Name }, new CancellationToken());

            try
            {
                // Act
                var result2 = await _createPersonHandler.Handle(new CreatePerson { Name = notUniquePerson.Name }, new CancellationToken());
            }
            catch (Exception ex)
            {

                // Assert
                Assert.Equal("Bad Request", ex.Message);
            }
            // Tear Down
            ClearPeopleDB();
        }

        #endregion


        #region Astronaut Duty Tests
        [Theory]
        [InlineData("Steve","Sargent","Space Explorer", "2024-09-14T00:00:00Z")]
        public async void CreateAstronautDutyWithValidFieldsShouldSucceed(string personName,string rank, string title, string startDate)
        {
            // Arrange
            var person = new Person { Name = personName };

            // Act
            var result = await _createPersonHandler.Handle(new CreatePerson { Name = person.Name }, new CancellationToken());
            var result2 = await _createAstronautDutyHandler.Handle(new CreateAstronautDuty { Name = person.Name, Rank = rank, DutyTitle = title, DutyStartDate = DateTime.Parse(startDate) }, new CancellationToken());

            // Assert
            Assert.True(result.Success);
            Assert.True(result2.Success);
            Assert.Single(_context.AstronautDuties);

            // Tear Down
            ClearPeopleDB();
            ClearAstronautDutiesDB();
        }

        [Theory]
        [InlineData("", "Sargent", "Space Explorer", "2024-09-14T00:00:00Z")]
        public async void CreateAstronautDutyWithInvalidFieldsShouldError(string personName, string rank, string title, string startDate)
        {
            // Arrange
            var person = new Person { Name = personName };

            // Act
            
            try
            {
                var result = await _createPersonHandler.Handle(new CreatePerson { Name = person.Name }, new CancellationToken());
                var result3 = await _createAstronautDutyHandler.Handle(new CreateAstronautDuty { Name = person.Name, Rank = rank, DutyTitle = title, DutyStartDate = DateTime.Parse(startDate) }, new CancellationToken());
            }
            catch (Exception ex)
            {
                // Assert
                Assert.Equal("Bad Request", ex.Message);
            }

            // Tear Down
            ClearPeopleDB();
            ClearAstronautDutiesDB();
        }

        [Theory]
        [InlineData("Steve Harrington", "Sargent", "Space Explorer", "2024-09-14T00:00:00Z")]
        public async void CreateAstronautDutyMultipleTimesInARowShouldSuceed(string personName, string rank, string title, string startDate)
        {
            // Arrange
            var person = new Person { Name = personName };

            // Act

            try
            {
                var result = await _createPersonHandler.Handle(new CreatePerson { Name = person.Name }, new CancellationToken());
                var result3 = await _createAstronautDutyHandler.Handle(new CreateAstronautDuty { Name = person.Name, Rank = rank, DutyTitle = title, DutyStartDate = DateTime.Parse(startDate) }, new CancellationToken());
                var result4 = await _createAstronautDutyHandler.Handle(new CreateAstronautDuty { Name = person.Name, Rank = rank, DutyTitle = title, DutyStartDate = DateTime.Parse(startDate).AddDays(1) }, new CancellationToken());
                Assert.True(result.Success);
                Assert.True(result3.Success);
                Assert.True(result4.Success);
                Assert.True(_context.AstronautDuties.Count() == 2);
            }
            catch (Exception ex)
            {
                // Assert
                
            }

            // Tear Down
            ClearPeopleDB();
            ClearAstronautDutiesDB();
        }


        [Theory]
        [InlineData("Steve Harrington", "Sargent", "Space Explorer", "2024-09-14T00:00:00Z")]
        public async void CreateAstronautDutyMultipleTimesInARowShouldSetPreviousDutyEndDate(string personName, string rank, string title, string startDate)
        {
            // Arrange
            var person = new Person { Name = personName };

            // Act

            try
            {
                var result = await _createPersonHandler.Handle(new CreatePerson { Name = person.Name }, new CancellationToken());
                var result3 = await _createAstronautDutyHandler.Handle(new CreateAstronautDuty { Name = person.Name, Rank = rank, DutyTitle = title, DutyStartDate = DateTime.Parse(startDate) }, new CancellationToken());
                var result4 = await _createAstronautDutyHandler.Handle(new CreateAstronautDuty { Name = person.Name, Rank = rank, DutyTitle = title, DutyStartDate = DateTime.Parse(startDate).AddDays(1) }, new CancellationToken());
                Assert.True(result3.Success);
                Assert.True(result4.Success);
                Assert.True(_context.AstronautDuties.Count() == 2);
            }
            catch (Exception ex)
            {
                // Assert

            }

            // Tear Down
            ClearPeopleDB();
            ClearAstronautDutiesDB();
        }


        #endregion


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