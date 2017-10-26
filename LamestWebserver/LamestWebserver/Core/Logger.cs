using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LamestWebserver.Synchronization;
using LamestWebserver.Collections;

namespace LamestWebserver.Core
{
    /// <summary>
    /// A simple Logger for LamestWebserver.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// The Main Logger for LamestWebserver.
        /// </summary>
        public readonly static Singleton<Logger> CurrentLogger = new Singleton<Logger>();

        /// <summary>
        /// Is LamestWebserverRunning in Debug Mode.
        /// </summary>
        public static bool LoggerDebugMode = false;

        /// <summary>
        /// The default minimum logging level. (should probably be ELoggingLevel.Warning by default)
        /// </summary>
        public static ELoggingLevel DefaultMinimumLoggingLevel = ELoggingLevel.Trace;

        /// <summary>
        /// The minimum logging level for this logger.
        /// </summary>
        public ELoggingLevel MinimumLoggingLevel = DefaultMinimumLoggingLevel;

        /// <summary>
        /// Set the Flags for the Output Sources
        /// </summary>
        public EOutputSource OutputSourceFlags
        {
            get
            {
                return _currentOutputSource;
            }
            set
            {
                if (_currentOutputSource == value) return;
                _currentOutputSource = value;
                CurrentLogger.Instance.RestartStream();
            }
        }

        /// <summary>
        /// Stream Writer to handle the writing from multiple streams
        /// </summary>
        private MultiStreamWriter _multiStreamWriter;

        /// <summary>
        /// Path for the Logging File
        /// </summary>
        public string FilePath
        {
            get
            {
                return _currentFilePath;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (_currentFilePath == value) return;
                _currentFilePath = value;
                RestartStream();
            }
        }
        /// <summary>
        /// Currently used File Path
        /// </summary>
        private string _currentFilePath = "LWS" + DateTime.Now.ToFileTime().ToString() + ".log";

        private UsableWriteLock _loggerWriteLock = new UsableWriteLock();

        private List<Stream> _streams = new List<Stream>();

        /// <summary>
        /// Currently used output source
        /// </summary>
        private EOutputSource _currentOutputSource = EOutputSource.File | EOutputSource.Console;

        /// <summary>
        /// If manually closed or opened check this value
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        /// Logging a message on logging level 'Trace'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void LogTrace(string msg) => CurrentLogger.Instance.Trace(msg);

        /// <summary>
        /// Logging a message on logging level 'Trace'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Trace(string msg)
        {
            if (MinimumLoggingLevel > ELoggingLevel.Trace)
                return;

            Log(ELoggingLevel.Trace, msg);
        }

        /// <summary>
        /// Logging a message on logging level 'Information'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void LogInformation(string msg) => CurrentLogger.Instance.Information(msg);

        /// <summary>
        /// Logging a message on logging level 'Information'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Information(string msg)
        {
            if (MinimumLoggingLevel > ELoggingLevel.Information)
                return;

            Log(ELoggingLevel.Information, msg);
        }

        /// <summary>
        /// Logging a message on logging level 'Warning'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void LogWarning(string msg) => CurrentLogger.Instance.Warning(msg);

        /// <summary>
        /// Logging a message on logging level 'Warning'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Warning(string msg)
        {
            if (MinimumLoggingLevel > ELoggingLevel.Warning)
                return;

            Log(ELoggingLevel.Warning, msg);
        }

        /// <summary>
        /// Logging a message on logging level 'Error'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void LogError(string msg) => CurrentLogger.Instance.Error(msg);

        /// <summary>
        /// Logging a message on logging level 'Error'.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Error(string msg)
        {
            if (MinimumLoggingLevel > ELoggingLevel.Error)
                return;

            Log(ELoggingLevel.Error, msg);
        }

        /// <summary>
        /// Logging a message on logging level 'DebugExcept'. The Exception will be thrown if Logger.LoggerDebugMode is true.
        /// </summary>
        /// <param name="exception">The exception, of which the message will be logged.</param>
        public static void LogDebugExcept(Exception exception) => CurrentLogger.Instance.DebugExcept(exception);

        /// <summary>
        /// Logging a message on logging level 'DebugExcept'. The Exception will be thrown if Logger.LoggerDebugMode is true.
        /// </summary>
        /// <param name="exception">The exception, of which the message will be logged.</param>
        public void DebugExcept(Exception exception)
        {
            if (MinimumLoggingLevel > ELoggingLevel.DebugExcept)
                return;

            Log(ELoggingLevel.DebugExcept, "{" + exception.GetType() + "} " + exception.Message);

            if (LoggerDebugMode)
                throw exception;
        }

        /// <summary>
        /// Logging a message on logging level 'DebugExcept'. The Exception will be thrown if Logger.LoggerDebugMode is true.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to throw.</param>
        public static void LogDebugExcept(string message, Exception exception) => CurrentLogger.Instance.DebugExcept(message, exception);

        /// <summary>
        /// Logging a message on logging level 'DebugExcept'. The Exception will be thrown if Logger.LoggerDebugMode is true.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to throw.</param>
        public void DebugExcept(string message, Exception exception)
        {
            if (MinimumLoggingLevel > ELoggingLevel.DebugExcept)
                return;

            Log(ELoggingLevel.DebugExcept, "{" + exception.GetType() + "} " + message);

            if (LoggerDebugMode)
                throw exception;
        }

        /// <summary>
        /// Logging a message on logging level 'DebugExcept'. An Exception will be thrown if Logger.LoggerDebugMode is true.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void LogDebugExcept(string msg) => CurrentLogger.Instance.DebugExcept(msg);

        /// <summary>
        /// Logging a message on logging level 'DebugExcept'. The Exception will be thrown if Logger.LoggerDebugMode is true.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void DebugExcept(string msg)
        {
            if (MinimumLoggingLevel > ELoggingLevel.DebugExcept)
                return;

            Log(ELoggingLevel.DebugExcept, msg);

            if (LoggerDebugMode)
                throw new Exception(msg);
        }

        /// <summary>
        /// Logging a message on logging level 'Except'. The Exception will be thrown.
        /// </summary>
        /// <param name="exception">The exception, of which the message will be logged.</param>
        public static void LogExcept(Exception exception) => CurrentLogger.Instance.Except(exception);

        /// <summary>
        /// Logging a message on logging level 'Except'. The Exception will be thrown.
        /// </summary>
        /// <param name="exception">The exception, of which the message will be logged.</param>
        public void Except(Exception exception)
        {
            if (MinimumLoggingLevel > ELoggingLevel.Except)
                return;

            Log(ELoggingLevel.Except, "{" + exception.GetType() + "} " + exception.Message);

            throw exception;
        }

        /// <summary>
        /// Logging a message on logging level 'Except'. The Exception will be thrown.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to throw.</param>
        public static void LogExcept(string message, Exception exception) => CurrentLogger.Instance.Except(message, exception);

        /// <summary>
        /// Logging a message on logging level 'Except'. The Exception will be thrown.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to throw.</param>
        public void Except(string message, Exception exception)
        {
            if (MinimumLoggingLevel > ELoggingLevel.Except)
                return;

            Log(ELoggingLevel.Except, "{" + exception.GetType() + "} " + message);

            throw exception;
        }

        /// <summary>
        /// Logging a message on logging level 'Except'.  An exception will be thrown.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void LogExcept(string msg) => CurrentLogger.Instance.Except(msg);

        /// <summary>
        /// Logging a message on logging level 'Except'. The Exception will be thrown.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void Except(string msg)
        {
            if (MinimumLoggingLevel > ELoggingLevel.Except)
                return;

            Log(ELoggingLevel.Except, msg);

            throw new Exception(msg);
        }

        /// <summary>
        /// Logging a message on logging level 'CrashAndBurn'. The logger will be closed, a dump file will be written and the application will be exited with error code -1.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public static void LogCrashAndBurn(string msg) => CurrentLogger.Instance.CrashAndBurn(msg);

        /// <summary>
        /// Add some Custom Streams.
        /// Call RestartStream to let the changes take action.
        /// </summary>
        /// <param name="stream"></param>
        public void AddCustomStream(Stream stream) => _streams.Add(stream);

        /// <summary>
        /// Remove some Custom Streams.
        /// Call RestartStream to let the changes take action.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool RemoveCustomStream(Stream stream) => _streams.Remove(stream);

        /// <summary>
        /// Clear all Custom Streams.
        /// Call RestartStream to let the changes take action.
        /// </summary>
        public void ClearCustomStreams() => _streams.Clear();

        /// <summary>
        /// Logging a message on logging level 'CrashAndBurn'. The logger will be closed, a dump file will be written and the application will be exited with error code -1.
        /// </summary>
        /// <param name="msg">The message to log.</param>
        public void CrashAndBurn(string msg)
        {
            if (MinimumLoggingLevel > ELoggingLevel.CrashAndBurn)
                return;

            Log(ELoggingLevel.CrashAndBurn, msg);

            try
            {
                Close();
            }
            catch { }

            try
            {
                MiniDump.Write();
            }
            catch { }

            Environment.Exit(-1);
        }

        private void Log(ELoggingLevel loggingLevel, string msg)
        {
            if (_currentOutputSource != EOutputSource.None && _multiStreamWriter != null && !_multiStreamWriter.IsDisposed)
            {
                using (_loggerWriteLock.LockWrite())
                {
                    _multiStreamWriter.WriteLine($"[{loggingLevel.ToString()} \\\\\\\\ {msg}]");
                }
            }
        }

        /// <summary>
        /// Closes and Flushes the Logger stream.
        /// If you want to restart the stream manually use 'Restart'.
        /// </summary>
        public void Close()
        {
            using (_loggerWriteLock.LockWrite())
            {
                _multiStreamWriter.Dispose();
            }
        }

        /// <summary>
        /// Opens and initializes the steam to write to.
        /// If you want to restart the stream manually use 'Restart'.
        /// </summary>
        protected void Open()
        {
            using (_loggerWriteLock.LockWrite())
            {
                if (_currentOutputSource != EOutputSource.None)
                {
                    CreateMultiStreamWriter();
                }
            }
        }

        /// <summary>
        /// Atomic operation to close and open the Logger save by a mutex.
        /// </summary>
        public void RestartStream()
        {
            using (_loggerWriteLock.LockWrite())
            {
                Close();
                Open();
            }
        }

        private void CreateMultiStreamWriter()
        {
            List<Stream> streamsToApply = new List<Stream>();
            
            if ((_currentOutputSource & EOutputSource.Console) == EOutputSource.Console)
                streamsToApply.Add(Console.OpenStandardOutput());

            if ((_currentOutputSource & EOutputSource.File) == EOutputSource.File)
                streamsToApply.Add(File.Open(_currentFilePath, FileMode.Append, FileAccess.Write));

            streamsToApply.AddRange(_streams);
            _multiStreamWriter = new MultiStreamWriter(streamsToApply);
        }

        /// <summary>
        /// Creates a new Logger instance.
        /// </summary>
        public Logger()
        {
            Open();
        }

        /// <summary>
        /// Destructor for the logger closes the stream.
        /// </summary>
        ~Logger()
        {
            Close();
        }

        /// <summary>
        /// Represents the different LoggingLevels.
        /// </summary>
        public enum ELoggingLevel : byte
        {
            /// <summary>
            /// Quits the Application writing a CrashDump.
            /// </summary>
            CrashAndBurn = 6,

            /// <summary>
            /// Throws an Exception.
            /// </summary>
            Except = 5,

            /// <summary>
            /// Throws an Exception if Logger.LoggerDebugMode is true.
            /// </summary>
            DebugExcept = 4,

            /// <summary>
            /// A major Error.
            /// </summary>
            Error = 3,

            /// <summary>
            /// A warning message.
            /// </summary>
            Warning = 2,

            /// <summary>
            /// General Information.
            /// </summary>
            Information = 1,

            /// <summary>
            /// Debugging Information.
            /// </summary>
            Trace = 0
        }

        /// <summary>
        /// Flags for output Sources
        /// </summary>
        [Flags]
        public enum EOutputSource : byte
        {
            /// <summary>
            /// No Logging output at all.
            /// </summary>
            None = 0,

            /// <summary>
            /// Write To Console.
            /// </summary>
            Console = 1,

            /// <summary>
            /// Write into File.
            /// </summary>
            File = 2,
        }
    }
}
