internal class Reader
{
    private static Thread inputThread;
    private static AutoResetEvent getInput, gotInput;
    private static string? input;
    private static bool reading = true;

    static Reader() => Initialise();

    private static void reader()
    {
        while (reading)
        {
            getInput.WaitOne();
            input = Console.ReadLine();
            gotInput.Set();
        }
    }

    private static void Initialise()
    {
        getInput = new AutoResetEvent(false);
        gotInput = new AutoResetEvent(false);
        inputThread = new Thread(reader);
        inputThread.IsBackground = true;
        reading = true;
        inputThread.Start();
    }

    // omit the parameter to read a line without a timeout
    public static string? ReadLine(int timeOutMillisecs = Timeout.Infinite)
    {
        if (!reading)
        {
            Initialise();
        }

        getInput.Set();
        var success = gotInput.WaitOne(timeOutMillisecs);
        if (success)
        {
            return input;
        }

        throw new TimeoutException("User did not provide input within the timelimit.");
    }

    public static void StopReading()
    {
        reading = false;
        inputThread.Interrupt();
        inputThread.Join();
    }
}
