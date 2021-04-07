using System.Threading;
using System.Threading.Tasks;

namespace CsprojToAsmdef.Cli.Domain.Services.DotNet
{
    public interface IDotNetTooling
    {
        Task SetMsbuildEnvironmentVariable(CancellationToken cancellationToken = default);
    }
}
