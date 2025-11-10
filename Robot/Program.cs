using Bot = CieploZimnoBib.CieploZimno;
using Console = System.Console;
using CommandMap = System.Collections.Generic.Dictionary<char, Robot.Direction>;
using Convert = System.Convert;

namespace Robot
{
    enum Direction
    {
        Forward = 0b00,
        Left = 0b01,
        Right = 0b10,
        Back = 0b11
    }

    public static class Program
    {
        static readonly CommandMap commands = new CommandMap() {
            { 'f', Direction.Forward },
            { 'b', Direction.Back },
            { 'l', Direction.Left },
            { 'r', Direction.Right },
            { 'F', Direction.Forward },
            { 'B', Direction.Back },
            { 'L', Direction.Left },
            { 'R', Direction.Right },
        };

        public static void Main(string[] args)
        {
            byte distance = 63;
            short battery = 150;
            while (distance != 0 && battery != 0)
            {
                short command = 0;
                Log($"\nBattery: {battery}\tDistance: {distance}");

                var direction = AskDirection();
                command = (short)((ushort)command | (byte)direction);

                var move_distance = AskDistance();
                command = (short)((ushort)command | (move_distance << 2));

                short answer = Bot.Poszukiwanie(command);
                battery = (short)((short)(answer << 7) >> 7);
                distance = (byte)(answer >> 9);
            }

            if (distance == 0) Log($"\nTarget reached! Battery left: {battery}");
            else Log($"\nRan out of battery {distance} units from the target!");
        }

        static Direction AskDirection()
        {
            Log("Rotate: [F]orward, [B]ack, [L]eft, [R]ight");
            char direction = ' ';
            while (direction == ' ')
            {
                direction = Console.ReadKey().KeyChar;
                if (commands.TryGetValue(direction, out Direction move_direction))
                {
                    Log($"\r{move_direction}\n");
                    return move_direction;
                }

                Log($"\rSupplied direction is not valid {direction}\r");
                direction = ' ';
            }
            return Direction.Forward;
        }

        static byte AskDistance()
        {
            Log("Choose distance: 1-32");
            sbyte move_distance;
            string read = "";
            while (true)
            {
                try
                {
                    read = Console.ReadLine();
                    move_distance = (sbyte)(Convert.ToSByte(read) - 1);
                    if (move_distance >= 0 && move_distance <= 31) break;
                }
                catch { };
                Log($"Distance {read} is invalid");
            }
            return (byte)move_distance;
        }

        static void Log(object o) => Console.WriteLine(o);
    }
}
