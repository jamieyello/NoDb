namespace SlothSerializer.Inspector;

public static class Program {
    delegate void CliCommand(string[] args, ref int cli_pos);

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
            bb.ReadFromDisk(args[0]);
            Console.WriteLine(bb.GetDebugString());
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
    
}