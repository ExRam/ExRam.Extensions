using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class OptionExtensionsTest
    {
        [Fact]
        public async Task Check()
        {
            Option<int>.None.IfNone(() => (Option<int>)37)
                .IfNone(38)
                .Should()
                .Be(37);

            (await Option<int>.None.IfNoneAsync(async () => 37))
                .Should().Be(37);

            (await Task.FromResult(Option<int>.None).IfNoneAsync(async () => 37))
                .Should().Be(37);

            (await Task.FromResult(Option<int>.None).IfNoneAsync(37))
                .Should().Be(37);

            (await Option<int>.None.IfNoneAsync(async () => (Option<int>)37))
                .IfNone(38)
                .Should()
                .Be(37);

            (await Task.FromResult(Option<int>.None).IfNoneAsync(async () => (Option<int>)37))
                .IfNone(38)
                .Should()
                .Be(37);

            (await Task.FromResult<Option<int>>(36).IfSomeAsync(async _ => { }))
                .Should().Be(Unit.Default);

            (await ((Option<int>)36).IfSomeAsync(async _ => { }))
                .Should().Be(Unit.Default);
        }
    }
}