using System;
using FluentAssertions;

namespace Unitest
{
    public class TestWith<TFixture, TSystemUnderTest>
        where TFixture : Fixture
        where TSystemUnderTest : class
    {
        public TFixture Fixture { get; set; }

        public TSystemUnderTest SUT
        {
            get
            {
                if (Fixture == null)
                    Given();

                return Fixture.Resolve<TSystemUnderTest>();
            }
        }

        public virtual TFixture Given()
        {
            return Fixture ?? (Fixture = default(TFixture));
        }

        public virtual TFixture And()
        {
            if (Fixture == null)
                throw new InvalidOperationException("Expected Given to be called before And can be called. Fixture must be created first and Fixture supposed to be created inside Given.");

            return Fixture;
        }

        public void ShouldThrowException<TException>(Action action)
            where TException: Exception
        {
            action.Should().Throw<TException>();
        }
    }
}