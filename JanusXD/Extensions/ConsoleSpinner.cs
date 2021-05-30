using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JanusXD.Shell.Extensions
{
    public enum SpinnerType
    {
        Stroke,
        Ball,
        Cross,
        Pointer,
        Dots,
        Rail
    }

    public class ConsoleSpinner
    {
        private static Lazy<ConsoleSpinner> _instance =
            new Lazy<ConsoleSpinner>(() => new ConsoleSpinner());

        public static ConsoleSpinner Instance => _instance.Value;

        public SpinnerType CurrentType { get; private set; }

        static Dictionary<SpinnerType, string[]> Spinners = new Dictionary<SpinnerType, string[]>
        {
            { SpinnerType.Stroke, new string[] { "/", "-", "\\", "|" } },
            { SpinnerType.Ball, new string[] { ".", "o", "0", "o" } },
            { SpinnerType.Cross, new string[] { "+", "x", "+", "x" } },
            { SpinnerType.Pointer, new string[] { "V", "<", "^", ">" } },
            { SpinnerType.Dots, new string[] { ".   ", "..  ", "... ", "...." } },
            { SpinnerType.Rail, new string[] { "=>   ", "==>  ", "===> ", "====>" } }
        };


        string[] Frames { get; set; }
        int Current { get; set; }
        public int CursorX { get; private set; }
        public int CursorY { get; private set; }

        public int Delay { get; private set; } = 200;

        public bool IsActive { get; private set; }
        public string Message { get; private set; }
        CancellationTokenSource Switch { get; set; } = new CancellationTokenSource();

        private ConsoleSpinner()
        {
            SetOrigin();
        }

        #region Methods

        public void SetOrigin(int x = -1, int y = -1)
        {
            var pos = Console.GetCursorPosition();

            if (x < 0) x = pos.Left;
            if (y < 0) y = pos.Top;

            CursorX = x;
            CursorY = y;
        }

        public void SetMessage(string message)
        {
            Message = message;
        }

        public void Activate(int delay = 200, SpinnerType type = SpinnerType.Stroke, string message = null)
        {
            SetOrigin(Console.CursorLeft, Console.CursorTop);

            CurrentType = type;
            Delay = delay;
            Message = message;

            if (IsActive) return;

            Current = 0;
            Frames = Spinners[CurrentType];

            IsActive = true;
            Switch = new CancellationTokenSource();

            Task.Run(() => Render(Switch.Token));
        }

        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = true;
            Switch.Cancel();
        }

        void Render(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return;

                Console.CursorVisible = true;
                Console.SetCursorPosition(CursorX, CursorY);


                string output = "";
                //Console
                if (!string.IsNullOrWhiteSpace(Message))
                    output = $"{Message} ";

                output += Frames[Current];

                Console.WriteLine(output.PadRight(Console.BufferWidth - 1));

                Console.CursorVisible = false;

                if (token.IsCancellationRequested) return;


                if (++Current >= Frames.Length)
                    Current = 0;

                Thread.Sleep(Delay);
            }
        }


        //public void Update()
        //{
        //    Console.Write(Frames[Current]);
        //    Console.SetCursorPosition(CursorX, CursorY);

        //    if (++Current >= Frames.Length)
        //        Current = 0;
        //}
        #endregion

    }
}
