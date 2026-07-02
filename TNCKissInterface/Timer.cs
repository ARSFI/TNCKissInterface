using System;
using System.Timers;

namespace TNCKissInterface
{
    //
    // Wrapper class for the timers we use in this DLL.
    //
    // Copyright 2008, 2009 - Peter R. Woods (N6PRW)
    //
    public class Timer : IDisposable
    {
        Random timeRandom = new Random();
        Object syncLock = new Object();
        System.Timers.Timer timerT;
        ElapsedEventHandler timerHandler;
        String tName;

        public Boolean Enabled
        {
            get { lock (syncLock) { return timerT != null && timerT.Enabled; } }
        }

        public String timerName
        {
            get { return tName; }
        }

        public Timer(ElapsedEventHandler eHandler, String name)
        {
            timerT = new System.Timers.Timer();
            timerT.AutoReset = true;
            timerT.Enabled = false;
            timerT.Elapsed += timerT_Elapsed;
            timerHandler = eHandler;
            timerT.Elapsed += timerHandler;
            tName = name;
        }

        void timerT_Elapsed(object sender, ElapsedEventArgs e)
        {
        }
        public void SetTime(Int32 time)
        {
            SetTime(time, false);
        }

        public void SetTime(Int32 time, Boolean variable)
        {
            lock (syncLock)
            {
                if (timerT == null)
                    return;
                if (variable)
                {
                    //
                    // Add up to an attitional 50% to the time value to help avoid channel collisions
                    //
                    time += timeRandom.Next(time >> 1);
                }
                timerT.Interval = Convert.ToDouble(time);
            }
            Support.DbgPrint("Timer " + tName + " interval set to  msec: " + time.ToString());

        }

        public void Stop()
        {
            lock (syncLock)
            {
                if (timerT == null)
                    return;
                timerT.Stop();
            }
            Support.DbgPrint("Timer " + tName + " Stopped");
        }

        public void Start()
        {
            lock (syncLock)
            {
                if (timerT == null)
                    return;
                timerT.Start();
            }
            Support.DbgPrint("Timer " + tName + " Started");
        }

        public void Restart()
        {
            lock (syncLock)
            {
                Stop();
                Start();
            }
            Support.DbgPrint("Timer " + tName + " Restarted");
        }

        public void Dispose()
        {
            lock (syncLock)
            {
                if (timerT == null)
                    return;

                // 2026-07-02 W4PHS: These wrappers are created for each connection/provider.
                // Release the underlying System.Timers.Timer at terminal owner shutdown instead
                // of only stopping it and retaining its native resources and event handlers.
                timerT.Stop();
                timerT.Elapsed -= timerT_Elapsed;
                timerT.Elapsed -= timerHandler;
                try
                {
                    timerT.Dispose();
                }
                catch
                {
                }
                timerT = null;
                timerHandler = null;
            }
        }
    }
}
