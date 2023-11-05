using AOSharp.Clientless;
using AOSharp.Clientless.Logging;
using AOSharp.Common.GameData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MalisImpDispenser
{
    public class Timer
    {
        public float TimeLimit;
        internal float TimeRemains;
        private bool _started = false;
        private bool _finished = false;

        public Timer(float timeLimitInSeconds)
        {
            TimeLimit = timeLimitInSeconds;
            TimeRemains = timeLimitInSeconds;
        }

        public void Start()
        {
            _started = true;
        }

        public bool HasStarted() => _started;


        public bool HasExpired()
        {
            if (_finished)
                return true;

            if (!_started)
                return false;

            if (TimeRemains <= 0)
            {
                _finished = true;

                return true; // Timer has expired
            }

            return false; // Timer is still running
        }

        public void Tick(int intervalInSeconds)
        {
            if (!_started)
                return;

            TimeRemains -= intervalInSeconds;

            if (TimeRemains < 0.0f)
            {
                TimeRemains = 0.0f; // Ensure the time remaining does not go negative
                _finished = true;
            }
        }
    }
}