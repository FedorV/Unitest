﻿using System;
using System.Collections.Generic;
using AutofacContrib.NSubstitute;

namespace HockyTest
{
    public class Fixture
    {
        public AutoSubstitute MockContext { get; }
        private Dictionary<Type, object> Mocks { get; set; }

        protected Fixture()
        {
            MockContext = new AutoSubstitute();
            Mocks = new Dictionary<Type, object>();
        }

        protected T SubstituteFor<T>()
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
    }

    public static class FixtureForExtensions
    {
        public static TFixture And<TFixture>(this TFixture fixture)
            where TFixture: Fixture
        {
            return fixture;
        }
    }
}