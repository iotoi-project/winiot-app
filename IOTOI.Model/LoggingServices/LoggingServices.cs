using System;
using System.Collections.Generic;
using System.Text;

namespace IOTOI.Model.LoggingServices
{
    using MetroLog;
    using MetroLog.Targets;

    public class LoggingServices
    {
        #region Properties
        public static LoggingServices Instance { get; }
        public static int RetainDays { get; } = 3;
        public static bool Enabled { get; set; } = true;
        #endregion


        #region Constructors
        static LoggingServices()
        {
            Instance = Instance ?? new LoggingServices();

            StreamingFileTarget st = new StreamingFileTarget(new CustomLayout());
            st.RetainDays = RetainDays;

#if DEBUG   
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, st);
#else
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new StreamingFileTarget { RetainDays = RetainDays });
#endif

        }
        #endregion

        #region Public Methods
        public void WriteLine<T>(string message, LogLevel logLevel = LogLevel.Trace, Exception exception = null)
        {
            if (Enabled)
            {
                var logger = LogManagerFactory.DefaultLogManager.GetLogger<T>();

                if (logLevel == LogLevel.Trace && logger.IsTraceEnabled)
                {
                    logger.Trace(message);
                }

                if (logLevel == LogLevel.Debug && logger.IsDebugEnabled)
                {
                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay.ToString()} {message}");
                    logger.Debug(message);
                }

                if (logLevel == LogLevel.Error && logger.IsErrorEnabled)
                {
                    logger.Error(message);
                }

                if (logLevel == LogLevel.Fatal && logger.IsFatalEnabled)
                {
                    logger.Fatal(message);
                }

                if (logLevel == LogLevel.Info && logger.IsInfoEnabled)
                {
                    logger.Info(message);
                }

                if (logLevel == LogLevel.Warn && logger.IsWarnEnabled)
                {
                    logger.Warn(message);
                }
            }
        }

        public void Write(ILogger logger, string message, LogLevel logLevel = LogLevel.Trace, Exception exception = null)
        {
            if (Enabled)
            {
                if (logLevel == LogLevel.Trace && logger.IsTraceEnabled)
                {
                    logger.Trace(message);
                }

                if (logLevel == LogLevel.Debug && logger.IsDebugEnabled)
                {
                    System.Diagnostics.Debug.WriteLine($"{DateTime.Now.TimeOfDay.ToString()} {message}");
                    logger.Debug(message);
                }

                if (logLevel == LogLevel.Error && logger.IsErrorEnabled)
                {
                    logger.Error(message);
                }

                if (logLevel == LogLevel.Fatal && logger.IsFatalEnabled)
                {
                    logger.Fatal(message);
                }

                if (logLevel == LogLevel.Info && logger.IsInfoEnabled)
                {
                    logger.Info(message);
                }

                if (logLevel == LogLevel.Warn && logger.IsWarnEnabled)
                {
                    logger.Warn(message);
                }
            }
        }
        #endregion


        public class CustomLayout : MetroLog.Layouts.Layout
        {
            /// <summary>
            /// Create a formatted string based on given informations
            /// </summary>
            /// <param name="context"><see cref="LogWriteContext"/></param>
            /// <param name="info"><see cref="LogEventInfo"/></param>
            /// <returns>Formatted string to log</returns>
            public override string GetFormattedString(LogWriteContext context, LogEventInfo info)
            {
                return $"{info.SequenceID}|{info.TimeStamp.LocalDateTime}|{info.Level}|{info.Logger}|{info.Message}|{info.Exception}";
            }
        }
    }
}
