using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CsprojToAsmdef.Tool
{
    [Command]
    public sealed class GetProjectProperties : ICommand
    {
        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync("Hello");
        }
    }
}
