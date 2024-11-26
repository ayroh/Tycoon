// Constants

using UnityEngine;

namespace Utilities.Constants
{
    public static class Constants
    {

        // Visitor
        public static float visitorForwardRotationTime = .25f;
        public static float visitorMoveSpeed = .04f;


        // Fixed Update Frame Interval
        public static float fixedUpdateFrameInterval = 0.02f;


        // Exhibition
        public static int LevelBaseDivider = 10;
        public static int MaximumExhibitionCapacity = 8;
        public static int InitialExhibitionCapacity = 3;
        public static int MinimumExhibitionTime = 10;
        public static int InitialExhibitionTime = 20;

        public static Vector3 ExhibitionSize = new Vector3(12f, 2f, 12f);

    }
}