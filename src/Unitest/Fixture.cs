using System;
using System.Collections.Generic;
using AutofacContrib.NSubstitute;

namespace Unitest
{
    /// <summary>
    /// Test fixture that suppose to contain test setup and some of the helper methods like VerifyTransactionCommitted 
    /// </summary>
    public class Fixture
    {
        /// <summary>
        ///  Expose the context to allow some non trivial scenarios
        /// </summary>
        public AutoSubstitute MockContext { get; private set; }

        /// <summary>
        /// List of mock for the current context to not re-define already used ones
        /// </summary>
        internal Dictionary<Type, object> Mocks { get; set; }

        protected Fixture()
        {
            MockContext = new AutoSubstitute();
            Mocks = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Syntactic sugar to access Fixture instance, for tests readability 
        /// </summary>
        public static TFixture Given<TFixture>()
            where TFixture : Fixture, new()
        {
            var fixture = new TFixture();
            fixture.Initialize(); // do common setup
            return fixture;
        }

        /// <summary>
        /// Resolves a type from mock context, mainly designed to use to build SUT
        /// </summary>
        public T Resolve<T>()
        {
            return MockContext.Resolve<T>();
        }

        /// <summary>
        /// Provides an instance of type T to include in the mock context
        /// </summary>
        public void Provide<T>(T instance)
            where T : class
        {
            MockContext.Provide(instance);
        }

        /// <summary>
        /// Overwrite this to provide common setup for many unit tests
        /// e.g. to mock HttpContext or other common dependency
        /// </summary>
        public virtual void Initialize()
        {
            MockContext = new AutoSubstitute();
            Mocks = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Adds a mocked dependency of a type TMock
        /// </summary>
        /// <typeparam name="TMock">Type of mocking dependency</typeparam>
        /// <returns>Mocked dependency</returns>
        internal protected TMock SubstituteFor<TMock>()
            where TMock : class
        {
            var type = typeof(TMock);

            if (!Mocks.ContainsKey(type))
            {
                Mocks.Add(type, MockContext.SubstituteFor<TMock>());
            }

            return (TMock)Mocks[type];
        }
    }
}