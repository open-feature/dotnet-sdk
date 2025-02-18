using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OpenFeature.Constant;
using OpenFeature.Hooks;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.Tests.Hooks
{
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]

    public class LoggingHookTests
    {
        [Fact]
        public async Task BeforeAsync_Without_EvaluationContext_Generates_Debug_Log()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var hook = new LoggingHook(logger, includeContext: false);

            // Act
            await hook.BeforeAsync(context);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;

            Assert.Equal(LogLevel.Debug, record.Level);
            Assert.Equal(
                """
                Before Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False

                """,
                record.Message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task BeforeAsync_Without_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var hook = new LoggingHook(logger, includeContext: false);

            // Act
            await hook.BeforeAsync(context);

            // Assert
            var record = logger.LatestRecord;

            Assert.Equal(
                """
                Before Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False

                """,
                record.Message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task BeforeAsync_With_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", "value")
                .Set("key_2", false)
                .Set("key_3", 1.531)
                .Set("key_4", 42)
                .Set("key_5", DateTime.Parse("2025-01-01T11:00:00.0000000Z"))
                .Build();

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.BeforeAsync(context);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;
            Assert.Equal(LogLevel.Debug, record.Level);

            Assert.Multiple(
                () => Assert.Contains("key_1:value", record.Message),
                () => Assert.Contains("key_2:False", record.Message),
                () => Assert.Contains("key_3:1.531", record.Message),
                () => Assert.Contains("key_4:42", record.Message),
                () => Assert.Contains("key_5:2025-01-01T11:00:00.0000000+00:00", record.Message)
            );
        }

        [Fact]
        public async Task BeforeAsync_With_No_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            // Act
            var hook = new LoggingHook(logger, includeContext: true);

            await hook.BeforeAsync(context);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;
            Assert.Equal(LogLevel.Debug, record.Level);

            Assert.Equal(
                """
                Before Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False
                Context:

                """,
                record.Message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task ErrorAsync_Without_EvaluationContext_Generates_Error_Log()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var hook = new LoggingHook(logger, includeContext: false);

            var exception = new Exception("Error within hook!");

            // Act
            await hook.ErrorAsync(context, exception);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;

            Assert.Equal(LogLevel.Error, record.Level);
        }

        [Fact]
        public async Task ErrorAsync_Without_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var hook = new LoggingHook(logger, includeContext: false);

            var exception = new Exception("Error within hook!");

            // Act
            await hook.ErrorAsync(context, exception);

            // Assert
            var record = logger.LatestRecord;

            Assert.Equal(
                """
                Error during Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False

                """,
                record.Message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task ErrorAsync_With_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", " ")
                .Set("key_2", true)
                .Set("key_3", 0.002154)
                .Set("key_4", -15)
                .Set("key_5", DateTime.Parse("2099-01-01T01:00:00.0000000Z"))
                .Build();

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var hook = new LoggingHook(logger, includeContext: true);

            var exception = new Exception("Error within hook!");

            // Act
            await hook.ErrorAsync(context, exception);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;
            Assert.Equal(LogLevel.Error, record.Level);

            Assert.Multiple(
                () => Assert.Contains("key_1: ", record.Message),
                () => Assert.Contains("key_2:True", record.Message),
                () => Assert.Contains("key_3:0.002154", record.Message),
                () => Assert.Contains("key_4:-15", record.Message),
                () => Assert.Contains("key_5:2099-01-01T01:00:00.0000000+00:00", record.Message)
            );
        }

        [Fact]
        public async Task ErrorAsync_With_No_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var hook = new LoggingHook(logger, includeContext: true);

            var exception = new Exception("Error within hook!");

            // Act
            await hook.ErrorAsync(context, exception);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;
            Assert.Equal(LogLevel.Error, record.Level);

            Assert.Equal(
                """
                Error during Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False
                Context:

                """,
                record.Message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task AfterAsync_Without_EvaluationContext_Generates_Debug_Log()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: false);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;
            Assert.Equal(LogLevel.Debug, record.Level);
        }

        [Fact]
        public async Task AfterAsync_Without_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: false);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            var record = logger.LatestRecord;

            Assert.Equal(
                """
                After Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False

                """,
                record.Message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task AfterAsync_With_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", "")
                .Set("key_2", false)
                .Set("key_3", double.MinValue)
                .Set("key_4", int.MaxValue)
                .Set("key_5", DateTime.MinValue)
                .Build();

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;
            Assert.Equal(LogLevel.Debug, record.Level);

            // .NET Framework uses G15 formatter on double.ToString
            // .NET uses G17 formatter on double.ToString
#if NET462
            var expectedMaxDoubleString = "-1.79769313486232E+308";
#else
            var expectedMaxDoubleString = "-1.7976931348623157E+308";
#endif
            Assert.Multiple(
                () => Assert.Contains("key_1:", record.Message),
                () => Assert.Contains("key_2:False", record.Message),
                () => Assert.Contains($"key_3:{expectedMaxDoubleString}", record.Message),
                () => Assert.Contains("key_4:2147483647", record.Message),
                () => Assert.Contains("key_5:0001-01-01T00:00:00.0000000", record.Message)
            );
        }

        [Fact]
        public async Task AfterAsync_With_No_EvaluationContext_Generates_Correct_Log_Message()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, EvaluationContext.Empty);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            Assert.Equal(1, logger.Collector.Count);

            var record = logger.LatestRecord;
            Assert.Equal(LogLevel.Debug, record.Level);

            Assert.Equal(
                """
                After Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False
                Context:

                """,
                record.Message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public void Create_LoggingHook_Without_Logger()
        {
            Assert.Throws<ArgumentNullException>(() => new LoggingHook(null!, includeContext: true));
        }

        [Fact]
        public async Task With_Structure_Type_In_Context_Returns_Qualified_Name_Of_Value()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", Structure.Builder().Set("inner_key_1", false).Build())
                .Build();

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            var record = logger.LatestRecord;

            // Raw string literals will convert tab to spaces (the File index style)
            var message = NormalizeLogRecord(record);

            Assert.Equal(
                """
                After Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False
                Context:
                    key_1:OpenFeature.Model.Value

                """,
                message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task Without_Domain_Returns_Missing()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata(null, "1.0.0");
            var providerMetadata = new Metadata("provider");
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", true)
                .Build();

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            var record = logger.LatestRecord;
            var message = NormalizeLogRecord(record);

            Assert.Equal(
                """
                After Flag Evaluation Domain:missing
                ProviderName:provider
                FlagKey:test
                DefaultValue:False
                Context:
                    key_1:True

                """,
                message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task Without_Provider_Returns_Missing()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata(null);
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", true)
                .Build();

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            var record = logger.LatestRecord;
            var message = NormalizeLogRecord(record);

            Assert.Equal(
                """
                After Flag Evaluation Domain:client
                ProviderName:missing
                FlagKey:test
                DefaultValue:False
                Context:
                    key_1:True

                """,
                message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task Without_DefaultValue_Returns_Missing()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", true)
                .Build();

            var context = new HookContext<string>("test", null!, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var details = new FlagEvaluationDetails<string>("test", "true", ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            var record = logger.LatestRecord;
            var message = NormalizeLogRecord(record);

            Assert.Equal(
                """
                After Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:missing
                Context:
                    key_1:True

                """,
                message,
                ignoreLineEndingDifferences: true
            );
        }

        [Fact]
        public async Task Without_EvaluationContextValue_Returns_Nothing()
        {
            // Arrange
            var logger = new FakeLogger<LoggingHookTests>();

            var clientMetadata = new ClientMetadata("client", "1.0.0");
            var providerMetadata = new Metadata("provider");
            var evaluationContext = EvaluationContext.Builder()
                .Set("key_1", (string)null!)
                .Build();

            var context = new HookContext<bool>("test", false, FlagValueType.Object, clientMetadata,
                providerMetadata, evaluationContext);

            var details = new FlagEvaluationDetails<bool>("test", true, ErrorType.None, reason: null, variant: null);

            var hook = new LoggingHook(logger, includeContext: true);

            // Act
            await hook.AfterAsync(context, details);

            // Assert
            var record = logger.LatestRecord;
            var message = NormalizeLogRecord(record);

            Assert.Equal(
                """
                After Flag Evaluation Domain:client
                ProviderName:provider
                FlagKey:test
                DefaultValue:False
                Context:
                    key_1:

                """,
                message,
                ignoreLineEndingDifferences: true
            );
        }

        private static string NormalizeLogRecord(FakeLogRecord record)
        {
            // Raw string literals will convert tab to spaces (the File index style)
            const int tabSize = 4;

            return record.Message.Replace("\t", new string(' ', tabSize));
        }
    }
}
