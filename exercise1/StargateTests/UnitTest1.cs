using StargateAPI.Business;

namespace StargateTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(1, 1);

        }

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

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void NoPersonReturnedWhenSearchingForInvalidSearches(string searchName)
        {


        }

    }
}