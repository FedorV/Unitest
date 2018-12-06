using System;
using FluentAssertions;
using Xunit;

namespace Unitest
{
    public class TestWith<TFixture>: IClassFixture<TFixture>
        where TFixture : Fixture, new()
    {
        protected readonly TFixture _fixture;

        public TestWith(TFixture fixture)
        {
            _fixture = fixture;
        }

        //public TSystemUnderTest SUT
        //{
        //    get
        //    {
        //        if (Fixture == null)
        //            Given();

        //        return Fixture.Resolve<TSystemUnderTest>();
        //    }
        //}

        public TSUT SUT<TSUT>()
        {
            return _fixture.Resolve<TSUT>();
        }

        public virtual TFixture Given()
        {
            return Fixture.Given<TFixture>();
        }

        public virtual TFixture And()
        {
            if (_fixture == null)
                throw new InvalidOperationException("Expected Given to be called before And can be called. Fixture must be created first and Fixture supposed to be created inside Given.");

            return _fixture;
        }

        public void ShouldThrowException<TException>(Action action)
            where TException: Exception
        {
            action.Should().Throw<TException>();
        }
    }
}