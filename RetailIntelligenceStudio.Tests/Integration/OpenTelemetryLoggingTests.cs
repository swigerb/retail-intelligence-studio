using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Collections.Concurrent;

namespace RetailIntelligenceStudio.Tests.Integration;

/// <summary>
/// Tests to verify OpenTelemetry logging configuration works correctly.
/// </summary>
public class OpenTelemetryLoggingTests
{
    /// <summary>
    /// Test exporter that captures log records for verification.
    /// </summary>
    private class InMemoryLogExporter : BaseExporter<LogRecord>
    {
        public ConcurrentBag<LogRecordData> ExportedLogs { get; } = new();

        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            foreach (var logRecord in batch)
            {
                // Capture the log data before the batch is disposed
                ExportedLogs.Add(new LogRecordData
                {
                    LogLevel = logRecord.LogLevel,
                    CategoryName = logRecord.CategoryName,
                    FormattedMessage = logRecord.FormattedMessage,
                    EventId = logRecord.EventId,
                    Timestamp = logRecord.Timestamp
                });
            }
            return ExportResult.Success;
        }
    }

    /// <summary>
    /// Data class to hold captured log record information.
    /// </summary>
    public class LogRecordData
    {
        public LogLevel LogLevel { get; init; }
        public string? CategoryName { get; init; }
        public string? FormattedMessage { get; init; }
        public EventId EventId { get; init; }
        public DateTime Timestamp { get; init; }
    }

    [Fact]
    public void OpenTelemetryLogging_WhenConfigured_ExportsLogRecords()
    {
        // Arrange
        var exporter = new InMemoryLogExporter();
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.AddProcessor(new SimpleLogRecordExportProcessor(exporter));
                });
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("TestCategory");

        // Act
        logger.LogInformation("🎯 Test message with emoji");
        logger.LogWarning("⚠️ Warning message");
        logger.LogError("❌ Error message");

        // Force flush to ensure logs are exported
        Thread.Sleep(100);

        // Assert
        exporter.ExportedLogs.Should().NotBeEmpty("logs should be exported to OpenTelemetry");
        exporter.ExportedLogs.Should().HaveCountGreaterOrEqualTo(3);
        
        var infoLog = exporter.ExportedLogs.FirstOrDefault(l => l.LogLevel == LogLevel.Information);
        infoLog.Should().NotBeNull();
        infoLog!.FormattedMessage.Should().Contain("Test message with emoji");
        infoLog.CategoryName.Should().Be("TestCategory");

        var warningLog = exporter.ExportedLogs.FirstOrDefault(l => l.LogLevel == LogLevel.Warning);
        warningLog.Should().NotBeNull();
        warningLog!.FormattedMessage.Should().Contain("Warning message");

        var errorLog = exporter.ExportedLogs.FirstOrDefault(l => l.LogLevel == LogLevel.Error);
        errorLog.Should().NotBeNull();
        errorLog!.FormattedMessage.Should().Contain("Error message");
    }

    [Fact]
    public void OpenTelemetryLogging_WithIncludeFormattedMessage_CapturesFullMessage()
    {
        // Arrange
        var exporter = new InMemoryLogExporter();
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.AddProcessor(new SimpleLogRecordExportProcessor(exporter));
                });
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("FormattedMessageTest");

        // Act - Log with structured parameters
        var roleName = "MarketAnalyst";
        var phase = "Executing";
        logger.LogInformation("📊 [{RoleName}] Phase: {Phase}", roleName, phase);

        Thread.Sleep(100);

        // Assert
        exporter.ExportedLogs.Should().NotBeEmpty();
        var log = exporter.ExportedLogs.First();
        
        // FormattedMessage should contain the interpolated values
        log.FormattedMessage.Should().Contain("MarketAnalyst");
        log.FormattedMessage.Should().Contain("Executing");
        log.FormattedMessage.Should().Contain("📊");
    }

    [Fact]
    public void OpenTelemetryLogging_WithScopes_CapturesScopeInformation()
    {
        // Arrange
        var exporter = new InMemoryLogExporter();
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.AddProcessor(new SimpleLogRecordExportProcessor(exporter));
                });
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("ScopeTest");

        // Act - Log within a scope
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["DecisionId"] = "decision-123",
            ["RoleName"] = "ExecutiveRecommendation"
        }))
        {
            logger.LogInformation("🏆 Processing decision");
        }

        Thread.Sleep(100);

        // Assert
        exporter.ExportedLogs.Should().NotBeEmpty();
        var log = exporter.ExportedLogs.First();
        log.FormattedMessage.Should().Contain("Processing decision");
    }

    [Fact]
    public void OpenTelemetryLogging_WithoutIncludeFormattedMessage_DoesNotCaptureFullMessage()
    {
        // Arrange
        var exporter = new InMemoryLogExporter();
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddOpenTelemetry(options =>
                {
                    // Explicitly set to false
                    options.IncludeFormattedMessage = false;
                    options.AddProcessor(new SimpleLogRecordExportProcessor(exporter));
                });
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("NoFormattedMessageTest");

        // Act
        logger.LogInformation("This message should not appear in FormattedMessage");

        Thread.Sleep(100);

        // Assert
        exporter.ExportedLogs.Should().NotBeEmpty();
        var log = exporter.ExportedLogs.First();
        
        // FormattedMessage should be null when IncludeFormattedMessage is false
        log.FormattedMessage.Should().BeNull();
    }

    [Fact]
    public void OpenTelemetryLogging_MultipleLogLevels_AllExported()
    {
        // Arrange
        var exporter = new InMemoryLogExporter();
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddOpenTelemetry(options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.AddProcessor(new SimpleLogRecordExportProcessor(exporter));
                });
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("MultiLevelTest");

        // Act
        logger.LogTrace("Trace level");
        logger.LogDebug("🔍 Debug level");
        logger.LogInformation("ℹ️ Info level");
        logger.LogWarning("⚠️ Warning level");
        logger.LogError("❌ Error level");
        logger.LogCritical("🔥 Critical level");

        Thread.Sleep(100);

        // Assert
        exporter.ExportedLogs.Should().HaveCount(6);
        exporter.ExportedLogs.Select(l => l.LogLevel).Should().Contain(LogLevel.Trace);
        exporter.ExportedLogs.Select(l => l.LogLevel).Should().Contain(LogLevel.Debug);
        exporter.ExportedLogs.Select(l => l.LogLevel).Should().Contain(LogLevel.Information);
        exporter.ExportedLogs.Select(l => l.LogLevel).Should().Contain(LogLevel.Warning);
        exporter.ExportedLogs.Select(l => l.LogLevel).Should().Contain(LogLevel.Error);
        exporter.ExportedLogs.Select(l => l.LogLevel).Should().Contain(LogLevel.Critical);
    }
}
