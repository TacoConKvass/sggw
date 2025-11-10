using Console = System.Console;
using Enum = System.Enum;
using Reader = System.IO.StreamReader;
using File = System.IO.File;
using CharToIndexMap = System.Collections.Generic.Dictionary<char, int>;

namespace Szyfry
{
    public static class Program
    {
        public enum Mode
        {
            encode,
            decode,
        }

        public class Args
        {
            public int shift;
            public string input_file_name;
            public Mode mode;
            public string alfabet;
            public CharToIndexMap char_to_index;
        }

        public static void Main(string[] args) {
            var opts = ParseArgs(args);
            if (opts == null) return;

            using (var input_file = new Reader(opts.input_file_name))
            {

                switch (opts?.mode)
                {
                    case Mode.encode:
                        Encode(input_file, opts.shift, opts.alfabet, opts.char_to_index);
                        break;
                    case Mode.decode:
                        Decode(input_file, opts.shift, opts.alfabet, opts.char_to_index);
                        break;
                }
            }
        }

        public static void Encode(Reader file, int shift, string alfabet, CharToIndexMap index_map) {
            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                foreach (char c in line)
                {
                    if (!ShiftIndex(c, alfabet, index_map, shift, out int index))
                    {
                        Console.Write(c);
                        continue;
                    }

                    char output = alfabet[index];
                    if (char.IsUpper(c))
                    {
                        output = char.ToUpper(output);
                    }

                    Console.Write(output);
                }
                Console.Write('\n');
            }
        }

        public static void Decode(Reader file, int shift, string alfabet, CharToIndexMap index_map) {
            while (!file.EndOfStream) {
                string line = file.ReadLine();
                foreach (char c in line) {
                    if (!ShiftIndex(c, alfabet, index_map, -shift, out int index))
                    {
                        Console.Write(c);
                        continue;
                    }

                    char output = alfabet[index];
                    if (char.IsUpper(c)) {
                        output = char.ToUpper(output);
                    }

                    Console.Write(output);
                }
                Console.Write('\n');
            }
        }

        public static bool ShiftIndex(char c, string alfabet, CharToIndexMap index_map, int shift, out int index) {
            if (!index_map.TryGetValue(char.ToLower(c), out index)) return false;

            index += shift;
            if (index < 0) index += alfabet.Length;
            if (index >= alfabet.Length) index -= alfabet.Length;
            return true;
        }

        public static Args ParseArgs(string[] args) {
            if (args.Length < 3) {
                ShowUsage();
                return null;
            }
            
            if (!Enum.TryParse(args[0], ignoreCase: true, out Mode mode)) {
                Console.WriteLine($"Unknown mode: {args[0]}");
                ShowUsage();
                return null;
            }

            var input_file_name = args[1];
            if (!File.Exists(input_file_name)) {
                Console.WriteLine($"File {input_file_name} doesn't exist");
                ShowUsage();
                return null;
            }

            if (!int.TryParse(args[2], out int shift)) {
                Console.WriteLine($"Shift \"{args[2]}\" is not a number");
                ShowUsage();
            }
            
            if (!File.Exists("alfabet.txt")) {
                Console.WriteLine($"File alfabet.txt doesn't exist. Please create one");
                return null;
            }
            
            string alfabet = "";
            using (var reader = new Reader("alfabet.txt")) {
                while (!reader.EndOfStream)
                    alfabet += reader.ReadLine();
            }

            var char_to_index = new CharToIndexMap();
            for (int i = 0; i < alfabet.Length; i++) {
                char_to_index.Add(char.ToLower(alfabet[i]), i);
            }
            
            return new Args { shift = shift % alfabet.Length, mode = mode, input_file_name = input_file_name, alfabet = alfabet, char_to_index = char_to_index };
        }

        public static void ShowUsage() => Console.WriteLine("\nUsage: dotnet Szyfry.dll (encode|decode) filename shift");
    }
}
