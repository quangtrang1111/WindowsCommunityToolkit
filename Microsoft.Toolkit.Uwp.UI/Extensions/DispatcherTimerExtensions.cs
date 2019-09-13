﻿using System;
using System.Collections.Concurrent;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Uwp.UI.Extensions
{
    /// <summary>
    /// Set of extention methods for using <see cref="DispatcherTimer"/>.
    /// </summary>
    public static class DispatcherTimerExtensions
    {
        private static ConcurrentDictionary<DispatcherTimer, Action> _debounceInstances = new ConcurrentDictionary<DispatcherTimer, Action>();

        /// <summary>
        /// <para>Used to debounce (rate-limit) an event.  The action will be postponed and executed after the interval has elapsed.  At the end of the interval, the function will be called with the arguments that were passed most recently to the debounced function.</para>
        /// <para>Use this method to control the timer instead of calling Start/Interval/Stop manually.</para>
        /// <para>A scheduled debounce can still be stopped by calling the stop method on the timer instance.</para>
        /// <para>Each timer can only have one debounced function limited at a time.</para>
        /// </summary>
        /// <param name="timer">Timer instance, only one debounced function can be used per timer.</param>
        /// <param name="action">Action to execute at the end of the interval.</param>
        /// <param name="interval">Interval to wait before executing the action.</param>
        /// <param name="immediate">Determines if the action execute on the leading edge instead of trailing edge.</param>
        /// <example>
        /// <code>
        /// private DispatcherTimer _typeTimer = new DispatcherTimer();
        ///
        /// _typeTimer.Debounce(async () =>
        ///     {
        ///         // Only executes this code after 0.3 seconds have elapsed since last trigger.
        ///     }, TimeSpan.FromSeconds(0.3));
        /// </code>
        /// </example>
        public static void Debounce(this DispatcherTimer timer, Action action, TimeSpan interval, bool immediate = false)
        {
            // Check and stop any existing timer
            var timeout = timer.IsEnabled;
            if (timeout)
            {
                timer.Stop();
            }

            // Reset timer parameters
            timer.Tick -= Timer_Tick;
            timer.Interval = interval;

            if (immediate)
            {
                // If we're in immediate mode then we only execute if the timer wasn't running beforehand
                if (!timeout)
                {
                    action.Invoke();
                }
            }
            else
            {
                // If we're not in immediate mode, then we'll execute when the current timer expires.
                timer.Tick += Timer_Tick;

                // Store/Update function
                _debounceInstances.AddOrUpdate(timer, action, (k, v) => v);
            }

            // Start the timer to keep track of the last call here.
            timer.Start();
        }

        private static void Timer_Tick(object sender, object e)
        {
            // This event is only registered/run if we weren't in immediate mode above
            if (sender is DispatcherTimer timer)
            {
                timer.Tick -= Timer_Tick;
                timer.Stop();

                if (_debounceInstances.TryRemove(timer, out Action action))
                {
                    action?.Invoke();
                }
            }
        }
    }
}
