using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Classcaller.Models;
using Microsoft.Extensions.Logging;

namespace Classcaller.Actions
{
    [ActionInfo("Classcaller.DisableHover", "禁用悬浮窗", "\uF486", false)]
    public class DisableHoverAction(ILogger<DisableHoverAction> logger) : ActionBase
    {
        private readonly ILogger<DisableHoverAction> _logger = logger;
        protected override async Task OnInvoke()
        {
            await base.OnInvoke();
            _logger.LogInformation("行动：禁用悬浮窗");
            Settings.Instance.Hover.IsEnable = false;
        }
        protected override async Task OnRevert()
        {
            await base.OnRevert();
            _logger.LogInformation("行动：恢复禁用悬浮窗");
            Settings.Instance.Hover.IsEnable = true;
        }
    }
}
