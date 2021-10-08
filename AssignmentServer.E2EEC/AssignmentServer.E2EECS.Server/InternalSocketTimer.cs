using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssignmentServer.E2EECS.Server
{
    public class InternalSocketTimer
    {
        private readonly Timer timer;
        private readonly int idleTimeMax;
        private int idleTime;

        public Action OnTimerTimeout;

        public InternalSocketTimer(int idleMax = 300)
        {
            idleTimeMax = idleMax;
            idleTime = 0;
            timer = new Timer(OnTimerTick, this, 0, 1000);
        }

        private void OnTimerTick(object tim)
        {
            if (tim is not InternalSocketTimer timerObj)
                return;

            Interlocked.Increment(ref idleTime);

            if (idleTime > idleTimeMax)
            {
                OnTimerTimeout?.Invoke();
                timer.Dispose();
            }
        }

        public void ResetIdleTime()
        {
            Interlocked.Exchange(ref idleTime, 0);
        }
    }
}
