using ClassIsland.Core.Abstractions.Automation;
using ClassIsland.Core.Attributes;
using Classcaller.Models;
using Microsoft.Extensions.Logging;

namespace Classcaller.Actions
{
    [ActionInfo("Classcaller.EnableHover", "启用悬浮窗", "\uF484", false)]
    public class EnableHoverAction(ILogger<EnableHoverAction> logger) : ActionBase
    {
        private readonly ILogger<EnableHoverAction> _logger = logger;
        protected override async Task OnInvoke()
        {
            await base.OnInvoke();
            _logger.LogInformation("行动：启用悬浮窗");
            Settings.Instance.Hover.IsEnable = true;
        }
        protected override async Task OnRevert()
        {
            await base.OnRevert();
            _logger.LogInformation("行动：恢复启用悬浮窗");
            Settings.Instance.Hover.IsEnable = false;
        }
    }
}
