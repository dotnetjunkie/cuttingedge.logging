using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;

using CuttingEdge.Logging.Tests.Common;
using CuttingEdge.Logging.Web;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CuttingEdge.Logging.Tests.Unit.Web
{
    [TestClass]
    public class AspNetExceptionLoggingModuleTests
    {
#if DEBUG // This test code only runs in debug mode
        [TestMethod]
        public void Dispose_Always_Succeeds()
        {
            // Arrange
            var module = new FakeAspNetExceptionLoggingModule();
            
            // Act
            module.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Init_WithNullArgument_ThrowsException()
        {
            // Arrange
            var module = new FakeAspNetExceptionLoggingModule();

            // Act
            module.Init(null);
        }

        [TestMethod]
        public void Init_WithValidArgument_Succeeds()
        {
            // Arrange
            var module = new FakeAspNetExceptionLoggingModule();
            var context = new HttpApplication();

            // Act
            module.Init(context);
        }

        [TestMethod]
        public void Error_AfterInitialization_CallsLog()
        {
            // Arrange
            var module = new FakeAspNetExceptionLoggingModule();
            var context = new HttpApplication();

            // Act
            module.Init(context);
            RaisErrorOnContext(context, new Exception());

            // Assert
            Assert.IsTrue(module.IsLogCalled);
        }

        [TestMethod]
        public void Error_WithoutInitialization_DoesNotCallLog()
        {
            // Arrange
            var moduleUnderTest = new FakeAspNetExceptionLoggingModule();
            var context = new HttpApplication();

            // Act
            RaisErrorOnContext(context, new Exception());

            // Assert
            Assert.IsFalse(moduleUnderTest.IsLogCalled);
        }

        [TestMethod]
        public void Error_WithoutException_DoesNotLog()
        {
            // Arrange
            var expectedNumberOfLogEntries = 0;
            var moduleUnderTest = new FakeAspNetExceptionLoggingModule();
            var context = new HttpApplication();
            moduleUnderTest.Init(context);

            using (var scope = new LoggingProviderScope())
            {
                // Act
                RaisErrorOnContext(context, null);

                // Assert
                Assert.AreEqual(expectedNumberOfLogEntries, scope.LoggedEntries.Count);
            }
        }

        [TestMethod]
        public void Error_WithException_LogsException()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            var context = new HttpApplication();
            var expectedException = new Exception();
            moduleUnderTest.Init(context);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                RaisErrorOnContext(context, expectedException);
                var actualException = scope.LoggedEntries.First().Exception;

                // Assert
                Assert.AreEqual(expectedException, actualException);
            }
        }

        [TestMethod]
        public void Error_WithException_LogsEventWithSeverityOfError()
        {
            // Arrange
            var expectedSeverity = LoggingEventType.Error;
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            var context = new HttpApplication();

            moduleUnderTest.Init(context);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                RaisErrorOnContext(context, new Exception());

                // Assert
                Assert.AreEqual(expectedSeverity, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void Error_WithThreadAbortException_DoesNotLogException()
        {
            // Arrange
            var expectedNumberOfLogEntries = 0;
            var moduleUnderTest = new FakeAspNetExceptionLoggingModule();
            var context = new HttpApplication();
            var expectedException = CreateThreadAbortException();
            moduleUnderTest.Init(context);

            using (var scope = new LoggingProviderScope())
            {
                // Act
                RaisErrorOnContext(context, expectedException);

                // Assert
                Assert.AreEqual(expectedNumberOfLogEntries, scope.LoggedEntries.Count);
            }
        }

        [TestMethod]
        public void Error_WithHttpUnhandledExceptionWithInnerException_LogsInnerException()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            var context = new HttpApplication();
            var expectedException = new InvalidOperationException();
            var exceptionForError = new HttpUnhandledException("some message", expectedException);
            moduleUnderTest.Init(context);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                RaisErrorOnContext(context, exceptionForError);
                var actualException = scope.LoggedEntries.First().Exception;

                // Assert
                Assert.AreEqual(expectedException, actualException);
            }
        }

        [TestMethod]
        public void LogUnhandledException_EventArgsWithoutException_DoesNotLog()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            Exception exceptionToLog = null;
            var eventArgs = new UnhandledExceptionEventArgs(exceptionToLog, false);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                moduleUnderTest.LogUnhandledException(null, eventArgs);

                // Assert
                Assert.AreEqual(0, scope.LoggedEntries.Count());
            }
        }

        [TestMethod]
        public void LogUnhandledException_EventArgsWithNonExceptionObject_DoesNotLog()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            object exceptionToLog = new object();
            var eventArgs = new UnhandledExceptionEventArgs(exceptionToLog, false);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                moduleUnderTest.LogUnhandledException(null, eventArgs);

                // Assert
                Assert.AreEqual(0, scope.LoggedEntries.Count());
            }
        }

        [TestMethod]
        public void LogUnhandledException_EventArgsWithException_DoesLog()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            object exceptionToLog = new InvalidOperationException();
            var eventArgs = new UnhandledExceptionEventArgs(exceptionToLog, false);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                moduleUnderTest.LogUnhandledException(null, eventArgs);

                // Assert
                Assert.AreEqual(1, scope.LoggedEntries.Count());
            }
        }

        [TestMethod]
        public void LogUnhandledException_EventArgsWithHttpUnhandledException_DoesLog()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            object exceptionToLog = new HttpUnhandledException();
            var eventArgs = new UnhandledExceptionEventArgs(exceptionToLog, false);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                moduleUnderTest.LogUnhandledException(null, eventArgs);

                // Assert
                Assert.AreEqual(1, scope.LoggedEntries.Count());
            }
        }

        [TestMethod]
        public void LogUnhandledException_EventArgsWithThreadAbortException_DoesLog()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            ThreadAbortException exceptionToLog = CreateThreadAbortException();
                
            var eventArgs = new UnhandledExceptionEventArgs(exceptionToLog, false);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                moduleUnderTest.LogUnhandledException(null, eventArgs);

                // Assert
                Assert.AreEqual(1, scope.LoggedEntries.Count());
            }
        }

        [TestMethod]
        public void LogUnhandledException_EventArgsWithException_LogsCriticalEvent()
        {
            // Arrange
            var expectedSeverity = LoggingEventType.Critical;
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            object exceptionToLog = new Exception();

            var eventArgs = new UnhandledExceptionEventArgs(exceptionToLog, false);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                moduleUnderTest.LogUnhandledException(null, eventArgs);

                // Assert
                Assert.AreEqual(expectedSeverity, scope.LoggedEntries.First().Severity);
            }
        }

        [TestMethod]
        public void Error_WithHttpUnhandledExceptionWithoutInnerException_LogsHttpUnhandledException()
        {
            // Arrange
            var moduleUnderTest = new TestAspNetExceptionLoggingModule();
            var context = new HttpApplication();
            var expectedException = new HttpUnhandledException("some message");
            moduleUnderTest.Init(context);

            using (var scope = new LoggingProviderScope(ScopeOption.AllowOnlyASingleEntryToBeLogged))
            {
                // Act
                RaisErrorOnContext(context, expectedException);
                var actualException = scope.LoggedEntries.First().Exception;

                // Assert
                Assert.AreEqual(expectedException, actualException);
            }
        }

        private static void RaisErrorOnContext(HttpApplication context, Exception exception)
        {
            const string RaiseErrorWithoutContextMethodName = "RaiseErrorWithoutContext";

            var applicationType = typeof(HttpApplication);

            var method = applicationType.GetMethod(RaiseErrorWithoutContextMethodName, 
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(method, "Method '{0}' could not be found in type {1}", 
                RaiseErrorWithoutContextMethodName, applicationType.FullName);

            // This is a bit tricky, because raising an error on a HttpApplication is only possible by using
            // reflection.
            method.Invoke(context, new[] { exception });
        }

        private static ThreadAbortException CreateThreadAbortException()
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var defaultConstructor = (
                from constructor in typeof(ThreadAbortException).GetConstructors(flags)
                where constructor.GetParameters().Length == 0
                select constructor).First();

            return (ThreadAbortException)defaultConstructor.Invoke(null);
        }

        private sealed class FakeAspNetExceptionLoggingModule : AspNetExceptionLoggingModule
        {
            public bool IsLogCalled { get; private set; }

            internal override void RegisterAppDomainUnhandledException()
            {
                // Do not register anything. This prevents many events from being hooked to the app domain.
            }

            internal override void Log(object sender, EventArgs e)
            {
                this.IsLogCalled = true;
            }
        }

        private sealed class TestAspNetExceptionLoggingModule : AspNetExceptionLoggingModule
        {
            public bool IsLogCalled { get; private set; }

            internal override void RegisterAppDomainUnhandledException()
            {
                // Do not register anything. This prevents many events from being hooked to the app domain.
            }
        }
#endif
    }
}
