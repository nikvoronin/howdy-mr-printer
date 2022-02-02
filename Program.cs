#pragma warning disable CA1416 // Validate platform compatibility
using System.Management;

const string DefaultPrinterName = "Microsoft Print to PDF";

#region F word states... i.e. faulty states
DetectedErrorState[] F_DetectedErrorStates = new[] {
    DetectedErrorState.NoPaper,
    DetectedErrorState.NoToner,
    DetectedErrorState.DoorOpen,
    DetectedErrorState.Jammed,
    DetectedErrorState.Offline,
    DetectedErrorState.ServiceRequested,
    DetectedErrorState.OutputBinFull
};

ExtendedDetectedErrorState[] F_ExtendedDetectedErrorStates = new[] {
    ExtendedDetectedErrorState.NoPaper,
    ExtendedDetectedErrorState.NoToner,
    ExtendedDetectedErrorState.DoorOpen,
    ExtendedDetectedErrorState.Jammed,
    ExtendedDetectedErrorState.ServiceRequested,
    ExtendedDetectedErrorState.OutputBinFull,
    ExtendedDetectedErrorState.PaperProblem,
    ExtendedDetectedErrorState.CannotPrintPage,
    ExtendedDetectedErrorState.UserInterventionRequired,
    ExtendedDetectedErrorState.OutOfMemory,
    ExtendedDetectedErrorState.ServerUnknown
};

ExtendedStatus[] F_PrinterStatuses = new[] {
    ExtendedStatus.None,
    ExtendedStatus.Offline
};

PrinterState[] F_PrinterStates = new[] {
    PrinterState.Error,
    PrinterState.PaperJam,
    PrinterState.PaperOut,
    PrinterState.Offline,
    PrinterState.NoToner,
    PrinterState.Unk_PrinterIsOffline,
    PrinterState.Unk_OutOfPaper,
    PrinterState.Unk_Offline,
    PrinterState.Unk_OutOfPaper_LidOpen
};
#endregion

//💡 The args array can't be null. So, it's safe to access the Length property without null checking.
//🔗 https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/main-command-line
bool hasArgs = !(args.Length == 0 || string.IsNullOrWhiteSpace( args[0] ));
string printerName = hasArgs ? args[0] : DefaultPrinterName;

Console.WriteLine( $"Monitoring: {printerName}" );
while ( !Console.KeyAvailable ) {
    HealthCheck( printerName );
    await Task.Delay( 1000 );
}

void HealthCheck( string nick )
{
    bool ready = true;
    string query = $"SELECT * from Win32_Printer WHERE Name LIKE '%{nick}%'";

    using ManagementObjectSearcher searcher = new( query );
    using ManagementObjectCollection folks = searcher.Get();
    try {
        foreach ( ManagementObject buddy in folks ) {
            var whosthere = buddy.ValueOrNull( "DeviceId", "" );
            if ( whosthere.Contains( nick ) ) {
                ready = AreYouReady( buddy );
                break;
            };
        }
    }
    catch ( ManagementException ex ) {
        ready = false;
    }
}

bool AreYouReady( ManagementObject man )
{
    var state = man.ValueOrNull( "PrinterState", PrinterState.Idle );    
    var status = man.ValueOrNull( "PrinterStatus", ExtendedStatus.None );    
    var des = man.ValueOrNull( "DetectedErrorState", DetectedErrorState.Unknown );    
    var exdes = man.ValueOrNull( "ExtendedDetectedErrorState", ExtendedDetectedErrorState.Unknown );

    bool needtogodeeper = 
        ( status == ExtendedStatus.Other ) 
        && ( F_DetectedErrorStates.Contains( des )
            || F_ExtendedDetectedErrorStates.Contains( exdes ));

    bool fault =
        F_PrinterStates.Contains( state )
        || F_PrinterStatuses.Contains( status )
        || needtogodeeper;

    Console.WriteLine(
        $"{(fault ? "ERROR" : "Ready")} | State:{state} | Status:{status} | DetectedError:{des} | DetectedErrorEx:{exdes}" );

    return !fault;
}

enum PrinterStatus : ushort
{
    None = 0,
    Other = 1,
    Unknown = 2,
    Idle = 3,
    Printing = 4,
    Warmup = 5,
    StoppedPrinting = 6,
    Offline = 7
}

enum ExtendedStatus : ushort
{
    None = 0,
    Other = 1,
    Unknown = 2,
    Idle = 3,
    Printing = 4,
    WarmingUp = 5,
    StoppedPrinting = 6,
    Offline = 7,
    Paused = 8,
    Error = 9,
    Busy = 10,
    NotAvailable = 11,
    Waiting = 12,
    Processing = 13,
    Initialization = 14,
    PowerSave = 15,
    PendingDeletion = 16,
    IOActive = 17,
    ManualFeed = 18
}

enum DetectedErrorState : ushort
{
    Unknown = 0,
    Other = 1,
    NoError = 2,
    LowPaper = 3,
    NoPaper = 4,
    LowToner = 5,
    NoToner = 6,
    DoorOpen = 7,
    Jammed = 8,
    Offline = 9,
    ServiceRequested = 10,
    OutputBinFull = 11
}

enum ExtendedDetectedErrorState : ushort
{
    Unknown = 0,
    Other = 1,
    NoError = 2,
    LowPaper = 3,
    NoPaper = 4,
    LowToner = 5,
    NoToner = 6,
    DoorOpen = 7,
    Jammed = 8,
    ServiceRequested = 9,
    OutputBinFull = 10,
    PaperProblem = 11,
    CannotPrintPage = 12,
    UserInterventionRequired = 13,
    OutOfMemory = 14,
    ServerUnknown = 15
}

// MSDN marked the PrinterState as deprecated but anyway it is wide using
// PagePunt: A printer error in which printing is terminated and the page in process is ejected.
enum PrinterState : uint
{
    Idle = 0,
    Paused = 1,
    Error = 2,
    PendingDeletion = 3,
    PaperJam = 4,
    PaperOut = 5,
    ManualFeed = 6,
    PaperProblem = 7,
    Offline = 8,
    IOActive = 9,
    Busy = 10,
    Printing = 11,
    OutputBinFull = 12,
    NotAvailable = 13,
    Waiting = 14,
    Processing = 15,
    Initialization = 16,
    WarmingUp = 17,
    TonerLow = 18,
    NoToner = 19,
    PagePunt = 20,
    UserInterventionRequired = 21,
    OutOfMemory = 22,
    DoorOpen = 23,
    ServerUnknown = 24,
    PowerSave = 25,

    Unk_OutOfPaper = 144,
    Unk_LidOpen = 4194432,
    Unk_OutOfPaper_LidOpen = 4194448,
    Unk_Offline = 4096,
    Unk_Printing = 1024,
    Unk_PrinterIsOffline = 128
}

public static class Extensions
{
    public static T ValueOrNull<T>( this ManagementObject man, string valueName, T @default ) where T : notnull
    {
        try {
            return (T)man.GetPropertyValue( valueName );
        }
        catch {
            return @default;
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
