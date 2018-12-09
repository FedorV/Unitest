using System;

namespace Unitest
{
    public class TestWith<TFixture, TSystemUnderTest>
        where TFixture : Fixture, new()
        where TSystemUnderTest : class
    {
        protected readonly TFixture Fixture;

        public TestWith(TFixture fixture)
        {
            Fixture = fixture ?? throw new ArgumentException($"Fixture cannot be null. Fixture supposed to be created on test initialization.");
        }

        public TSystemUnderTest SUT => Fixture.Resolve<TSystemUnderTest>();

        public TSystemUnderTest When => SUT;

        public virtual TFixture Given => Fixture;

        public virtual TFixture And => Fixture;

        public virtual TFixture Then => Fixture;

        public TMock Mock<TMock>()
            where TMock: class
        {
            return Fixture.SubstituteFor<TMock>();
        }
    }
}