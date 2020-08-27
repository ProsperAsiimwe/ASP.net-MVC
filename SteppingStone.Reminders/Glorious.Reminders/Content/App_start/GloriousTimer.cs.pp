using Glorious.Reminders.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(
  typeof(GloriousTimer), "Start")]

namespace Glorious.Reminders.Infrastructure
{
    public static class GloriousTimer
    {
        private static readonly Timer _timer = new Timer(OnTimerElapsed);
        private static readonly JobHelper _jobHost = new JobHelper();

        public static void Start()
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
        }

        private static void OnTimerElapsed(object sender)
        {
            _jobHost.DoWork(() => { /* What is it that you do around here */ });
        }
    }
}