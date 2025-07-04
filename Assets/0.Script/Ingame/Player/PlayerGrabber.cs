using Fusion.XR.Shared.Grabbing;

namespace Ingame.Player
{
    public class PlayerGrabber : Grabber
    {
        public System.Action<Grabbable> onGrabbed;
        public System.Action<Grabbable> onUngrabbed;

        public override void Grab(Grabbable grabbable)
        {
            base.Grab(grabbable);
            onGrabbed?.Invoke(grabbable);
        }

        public override void Ungrab(Grabbable grabbable)
        {
            base.Ungrab(grabbable);
            onUngrabbed?.Invoke(grabbable);
        }
    }
}