using System;
using Xunit;

namespace ExRam.Framework.Tests
{
    public class ExceptionExtensionsTest
    {
        [Fact]
        public void ExceptionMessages_are_concatenated_by_GetSafeMessage()
        {
            var inner = new InvalidOperationException();
            var outer = new ArgumentNullException("Eine Message", inner);

            Assert.Equal(outer.Message + " ---> " + inner.Message, outer.GetSafeMessage());
        }

        [Fact]
        public void GetSafeMessage_can_be_called_on_null_reference()
        {
            Exception ex = null;
            Assert.Equal(string.Empty, ex.GetSafeMessage());
        }
    }
}
