namespace Unitest
{
    public static class FixtureExtensions
    {
        /// <summary>
        /// Syntactic sugar to access Fixture instance, for tests readability 
        /// </summary>
        public static TFixture And<TFixture>(this TFixture fixture)
            where TFixture: Fixture
        {
            return fixture;
        }
    }
}