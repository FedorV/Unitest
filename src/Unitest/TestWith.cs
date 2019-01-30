using System;

namespace Unitest
{
    /// <summary>
    /// Base unit test class that provides some basics for unit test classes
    /// </summary>
    /// <typeparam name="TFixture">Type of the fixture to be used with the class</typeparam>
    /// <typeparam name="TSystemUnderTest">Type of the system under test</typeparam>
    public class TestWith<TFixture, TSystemUnderTest>
        where TFixture : Fixture, new()
        where TSystemUnderTest : class
    {
        protected readonly TFixture Fixture;
        private TSystemUnderTest _sut = null;

        public TestWith(TFixture fixture, bool isolateTestRuns = true)
        {
            Fixture = fixture ?? throw new ArgumentException($"Fixture cannot be null. Fixture supposed to be created on test initialization.");
            if (isolateTestRuns)
                Fixture.Initialize();
        }

        /// <summary>
        /// System under test
        /// </summary>
        public TSystemUnderTest SUT => _sut ?? (_sut = Fixture.Resolve<TSystemUnderTest>());

        /// <summary>
        /// Syntactic sugar to access System under test instance, for tests readability 
        /// </summary>
        public TSystemUnderTest When => SUT;

        /// <summary>
        /// Syntactic sugar to access fixture instance, for tests readability 
        /// </summary>
        public virtual TFixture Given => Fixture;

        /// <summary>
        /// Syntactic sugar to access fixture instance, for tests readability 
        /// </summary>
        public virtual TFixture And => Fixture;

        /// <summary>
        /// Syntactic sugar to access fixture instance, for tests readability 
        /// </summary>
        public virtual TFixture Then => Fixture;

        /// <summary>
        /// Creates a mock you can use to setup dependencies
        /// </summary>
        /// <typeparam name="TMock">Type of dependency</typeparam>
        /// <returns>Dependency mock</returns>
        public TMock Mock<TMock>()
            where TMock: class
        {
            return Fixture.SubstituteFor<TMock>();
        }
    }
}