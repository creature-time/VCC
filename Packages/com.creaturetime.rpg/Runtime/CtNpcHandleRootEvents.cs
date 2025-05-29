
using VRC.SDK3;

namespace CreatureTime
{
    public class CtNpcHandleRootEvents : CtLoggerUdonScript
    {
        public override void OnControllerColliderHitPlayer(ControllerColliderPlayerHit hit)
        {
            LogDebug($"Collided with {hit.player.displayName}");
        }
    }
}