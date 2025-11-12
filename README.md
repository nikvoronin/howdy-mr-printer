# Howdy Mr Printer

This is a small console app that monitors a Windows printer's health using WMI (Win32_Printer) and prints current status.

```log
Monitoring: Microsoft Print to PDF
Ready | State:Idle | Status:Idle | DetectedError:Unknown | DetectedErrorEx:Unknown
Ready | State:Idle | Status:Idle | DetectedError:Unknown | DetectedErrorEx:Unknown
...
Ready | State:17408 | Status:Printing | DetectedError:NoError | DetectedErrorEx:Unknown
Ready | State:Unk_Printing | Status:Printing | DetectedError:NoError | DetectedErrorEx:Unknown
Ready | State:Idle | Status:Idle | DetectedError:Unknown | DetectedErrorEx:Unknown
Ready | State:Idle | Status:Idle | DetectedError:Unknown | DetectedErrorEx:Unknown
...
Ready | State:Idle | Status:Idle | DetectedError:Unknown | DetectedErrorEx:Unknown
```

- Determines which printer to monitor (from command line or a default name).
- Every second:
    - Queries WMI for that printer.
    - Reads its state, status, and error flags.
    - Decides if the printer is `OK` or `FAULTY`.
    - Logs the result.
    - Await `Task.Delay(1000)` pauses for ~1 second between checks.
- Stops when you press a key.

To access WMI (Windows Management Instrumentation) the project `using System.Management` namespace methods. The program uses top-level statements (no explicit Main method).

## Build and Run

Build and run on Windows with .NET, for example:

```shell
dotnet run "My Printer Name"
```

or just:

```shell
dotnet run
```

If you don't specify a printer name in the arguments, it defaults to watching `Microsoft Print to PDF`. Find it in the following lines:

```csharp
const string DefaultPrinterName = "Microsoft Print to PDF";
```

## Querying WMI

`DoHealthCheck(string nick)` is where it talks to Windows:

It builds a WMI query for the printer:

```csharp
string query = $"SELECT * from Win32_Printer WHERE Name LIKE '%{nick}%'";
```

Uses ManagementObjectSearcher:

```csharp
using ManagementObjectSearcher searcher = new(query);
using ManagementObjectCollection folks = searcher.Get();
```

Then iterates over the printers returned:

- For each `buddy` in `folks`, it reads the printer's `DeviceId` via the extension method `ValueOrDefaultOf`.
- If `DeviceId` contains the nickname (`nick`), that's considered the matching printer.
- For that one, it calls `AreYouReady( buddy)` to decide if it is OK or in fault.

After the loop, it calls:

```csharp
GC.Collect();
GC.WaitForFullGCComplete(
    TimeSpan.FromMilliseconds(100));
```

which forces garbage collection and waits for a full GC. This is a bit aggressive, presumably to quickly free unmanaged resources related to WMI objects.
