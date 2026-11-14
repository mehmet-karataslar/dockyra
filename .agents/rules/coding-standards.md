---
trigger: always_on
---

# Dockyra C# & WPF Coding Standards

1. **Language & Framework**:
   - Target `.NET 10` (`net10.0` for platform-independent libraries; `net10.0-windows` for `WindowsIntegration`, `Presentation.Wpf`, and `Desktop`).
   - Keep `<Nullable>enable</Nullable>` active across all projects and resolve nullable warnings properly.
2. **Async & Concurrency**:
   - Suffix asynchronous methods with `Async` and accept a `CancellationToken cancellationToken = default`.
   - Never block the WPF UI thread with `.Result` or `.Wait()`.
3. **Error Handling**:
   - Use `OperationResult` and `OperationResult<T>` from `Dockyra.Contracts` for expected operational failures (e.g., Win32 focus denial, invalid launch path, validation errors).
   - Never swallow exceptions silently (`catch { }`). Log technical details via `ILogger<T>` and surface user-friendly messages.
4. **Resource Disposal**:
   - Implement `IDisposable` (or `IAsyncDisposable`) on any service holding Win32 hooks, timers, event subscriptions, or database handles.
5. **Design Tokens & Localization**:
   - Never hardcode colors, margins, or UI strings directly inside XAML Views or ViewModels. Use `DesignTokens/` and localization resource keys.

