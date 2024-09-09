namespace StargateTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(1, 1);

        }

        [Fact]
        public void FailTest()
        {
            Assert.Equal(2, 1);
        }

        // Important Test cases to cover

        // For Person Controller
        // Attempting to create a person with a blank or whitespace name returns an error code.
        // Attempting to create a person with the same name as another person returns an error.
        // Attempting to create a person with a unique name returns success.
        // Searching by name for a person that doesn't exist returns nothing.
        // Searching for all people returns all people.
        // If no people exist searching for all people returns nothing.


    }
}