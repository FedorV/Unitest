using System;
using System.Threading.Tasks;
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

        public static AsyncFunctionAssertions Then(this Func<Task> action)
        {
            return action.Should();
        }
    }
}