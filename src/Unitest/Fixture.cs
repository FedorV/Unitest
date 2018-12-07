using System;
using System.Collections.Generic;
using AutofacContrib.NSubstitute;

namespace Unitest
{
    public class Fixture
    {
        public AutoSubstitute MockContext { get; }
        internal Dictionary<Type, object> Mocks { get; set; }

        protected Fixture()
        {
            MockContext = new AutoSubstitute();
            Mocks = new Dictionary<Type, object>();
        }

        public static TFixture Given<TFixture>()
            where TFixture : Fixture, new()
        {
            var fixture = new TFixture();
            fixture.Initialize();
            return fixture;
        }

        internal protected T SubstituteFor<T>()
            where T: class
        {
            var type = typeof(T);

            if (!Mocks.ContainsKey(type))
            {
                Mocks.Add(type, MockContext.SubstituteFor<T>());
            }

            return (T)Mocks[type];
        }

        public T Resolve<T>()
        {
            return MockContext.Resolve<T>();
        }

        public virtual void Initialize()
        {

        }
    }
}