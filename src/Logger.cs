
class Logger
{
    private string level;
    private int log_lev;
    private (bool, System.ConsoleColor) switch_log_lev(string level, bool init = false)
    {
        switch (level) 
        {
            case "DEBUG":
                if (init){
                    this.log_lev = 0;
                    return(false, ConsoleColor.Black);
                }                    
                else {
                    return (this.log_lev <= 0, ConsoleColor.Blue);
                }                    
                    
            case "INFO":
                if (init){
                    this.log_lev = 1;
                    return(false, ConsoleColor.Black);
                }                    
                else {
                    return (this.log_lev <= 1, ConsoleColor.DarkGreen);
                }  

            case "WARNING":
                if (init){
                    this.log_lev = 2;
                    return(false, ConsoleColor.Black);
                }                    
                else {
                    return (this.log_lev <= 2, ConsoleColor.DarkYellow);
                }  
            case "ERROR":
                if (init){
                    this.log_lev = 3;
                    return(false, ConsoleColor.Black);
                }                    
                else {
                    return (this.log_lev <= 3, ConsoleColor.Red);
                }  
            default:
                throw new ArgumentException("LEVEL NOT SUPPORTED. AVAILABLE -> DEBUG, INFO, WARNING, ERROR");

        }

    }
    public Logger(string level){
        this.level = level;
        var _tup = switch_log_lev(level, true);        
    }

    public void log_mex(string level, string mex)
    {   
        var _tup = switch_log_lev(level, false);
        if (!_tup.Item1){
            return;
        }
        Console.BackgroundColor = ConsoleColor.Magenta;
        string time = DateTimeOffset.Now.ToString();
        Console.Write($"{time}");
        Console.ResetColor();
        Console.Write(" - ");
        Console.BackgroundColor = _tup.Item2;
        Console.Write($"{level}");
        Console.ResetColor();
        Console.WriteLine($" - {mex}");
    }

    public void log_mex_noline(string level, string mex)
    {   
        var _tup = switch_log_lev(level, false);
        if (!_tup.Item1){
            return;
        }
        Console.BackgroundColor = ConsoleColor.Magenta;
        string time = DateTimeOffset.Now.ToString();
        Console.Write($"{time}");
        Console.ResetColor();
        Console.Write(" - ");
        Console.BackgroundColor = _tup.Item2;
        Console.Write($"{level}");
        Console.ResetColor();
        Console.Write($" - {mex}");
    }
}