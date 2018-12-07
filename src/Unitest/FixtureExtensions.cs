namespace Unitest
{
    public static class FixtureExtensions
    {
        public static TFixture And<TFixture>(this TFixture fixture)
            where TFixture: Fixture
        {
            return fixture;
        }
    }
}