using System;
using FluentAssertions;
using FluentAssertions.Specialized;

namespace Unitest
{
    public static class ShouldExtensions
    {
        public static ActionAssertions Then(this Action action)
        {
            return action.Should();
        }
    }
}