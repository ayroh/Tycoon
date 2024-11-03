using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enums;


namespace Utilities.Signals
{
    public static class Signals
    {

        public static Action OnMoneyUpdated = delegate { };

        public static Action<Quaternion> OnFaceCanvasToCamera = delegate { };
    }
}
