
using Core.Types;

namespace Level.Room
{
    public enum ExitRestrictionType
    {
        None = 0,
        AlwaysBlocked,
        NeedItemX,
        NeedItemY,
    }

    [System.Serializable]
    public class ExitInfo
    {
        public RoomConfig ConnectedTo;
        public Cardinals ExitDirection;
        public ExitRestrictionType Restriction;
        public int FromIdx;
        public int ToIdx;
        public int Level;
    }
}