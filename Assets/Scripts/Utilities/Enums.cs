// ENUM

namespace Utilities.Enums
{
    public enum GameState { Play, Menu }
    public enum VisitorState { Patrol, Idle, WaitingInLine, Visiting, GoingToLine }
    public enum VisitorAnimationState { Standing, Walking, Running }
    public enum ExhibitionState { Started, Waiting, Locked }
    public enum ExhibitionType { City, Pirate, Cowboy, Church }
    public enum GuideState { Waiting, Guiding }
    public enum GuideAnimationState { Standing, Walking, Running }
    public enum UpgradeType { Exhibition, Capacity, Time }

}
