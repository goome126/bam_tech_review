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
        // Making sure that attempting to create a person with a blank or whitespace name returns an error code.
        // Making sure that attempting to create a person with the same name as another person returns an error.

    }
}