using System.Text;

namespace SlothSerializer.Inspector;

/// <summary>
/// This is a simple CLI tool used to inspect a <see cref="BitBuilderBuffer"/> 
/// that's been written to the disk.
/// </summary>
/// dev notes: I add this tool to the OS path, just hard-coding whatever this
/// happens to build to, so that I can use it from the CLI. In linux you can use
/// an alias like "ss-inspect", on Windows I'm not so sure you can do that without
/// re-naming the executable.
public static class Program {
    delegate void CliCommand(string[] args, ref int cli_pos);
    const ConsoleColor HEADER_COLOR = ConsoleColor.Yellow;

    static readonly Dictionary<string, CliCommand> commands = new() {
        {"--help", Help},
        {"-h", Help},
    };

    public static void Main(string[] args) {
        if (args.Length == 0) { 
            int i = 0;
            Help(args, ref i);
            return;
        }
        
        if (args[0][0] != '-') {
            if (!File.Exists(args[0])) throw new FileNotFoundException();

            var bb = new BitBuilderBuffer();
            PrintRawBinary(args[0]);
            bb.ReadFromDisk(args[0]);
            Console.ForegroundColor = HEADER_COLOR;
            Console.WriteLine("Parsed data;");
            Console.ResetColor();
            Console.WriteLine(bb.DebugString);
            return;
        }

        for (int i = 0; i < args.Length; i++) {
            if (commands.TryGetValue(args[i], out var command)) command(args, ref i);
            else throw new ArgumentException($"Unexpected parameter {args[i]}.");
        }
    }

    static void Help(string[] args, ref int cli_pos) =>
        Console.WriteLine(
            $"Usage: ss-inspect [path-to-file] [arguments]\n" +
            $"\n" +
            $"Arguments;\n" +
            $"  --help (-h):    Display available commands.\n"
        );

    static void PrintRawBinary(string file_path) {
        Console.ForegroundColor = HEADER_COLOR;
        Console.WriteLine("RAW binary (header included);");
        Console.ResetColor();

        using var fs = new FileStream(file_path, FileMode.Open);
        var buffer = new byte[8];
        while (fs.Length - fs.Position > 8) {
            fs.Read(buffer);
            var value = BitConverter.ToUInt64(buffer);
            Console.WriteLine(Convert.ToString((long)value, 2).PadLeft(64, '0'));
        }

        StringBuilder last_line = new();
        while (fs.Length != fs.Position) {
            last_line.Append(Convert.ToString(fs.ReadByte(), 2).PadLeft(8, '0'));
        }
        Console.WriteLine($"{last_line.ToString().PadRight(64, '-')}\n");
    }
}