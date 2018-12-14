using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Specialized;

namespace Unitest
{
    /// <summary>
    /// Some more of the syntactic sugar to make tests more look like Gherkin language
    /// </summary>
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