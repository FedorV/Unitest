using System;
using Xunit;

namespace Unitest
{
    public class TestWith<TFixture, TSystemUnderTest> : IClassFixture<TFixture>
        where TFixture : Fixture, new()
        where TSystemUnderTest : class
    {
        protected readonly TFixture _fixture;

        public TestWith(TFixture fixture)
        {
            _fixture = fixture;
        }

        public TSystemUnderTest When => SUT;

        public TSystemUnderTest SUT
        {
            get
            {
                return _fixture == null 
                    ? Given.Resolve<TSystemUnderTest>() 
                    : _fixture.Resolve<TSystemUnderTest>();
            }
        }

        public virtual TFixture Given
        {
            get
            {
                if (_fixture == null)
                {
                    throw new InvalidOperationException("The Fixture expected to be created in constructor prior to Calling Given.");
                }

                return _fixture;
            }
        }

        public virtual TFixture And
        {

            get
            {
                if (_fixture == null)
                {
                    throw new InvalidOperationException("Expected Fixture to be created before And can be called. Fixture supposed to be created on test initialization.");
                }

                return _fixture;
            }
        }

        public virtual TFixture Then
        {
            get
            {
                if (_fixture == null)
                {
                    throw new InvalidOperationException("Expected Fixture to be created before Then can be called. Fixture supposed to be created on test initialization.");
                }

                return _fixture;
            }
        }

        public TMock Mock<TMock>()
            where TMock: class
        {
            return _fixture.SubstituteFor<TMock>();
        }
    }
}