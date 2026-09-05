using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared;
using Classcaller.Services.ClasscallerService;
using Microsoft.Extensions.Logging;

namespace Classcaller.Actions
{
    [ActionInfo("Classcaller.Call", "随机点名", "\uECF9", false)]
    public class CallAction(ILogger<CallAction> logger) : ActionBase
    {
        private readonly ILogger<CallAction> _logger = logger;
        private readonly ClasscallerService _islandCallerService = IAppHost.GetService<ClasscallerService>();
        protected override async Task OnInvoke()
        {
            await base.OnInvoke();
            _logger.LogInformation("行动：随机点名");
            _islandCallerService.ShowRandomStudent(1);
        }
    }
}
