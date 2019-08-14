using System;

namespace BlakeServerSide.Data
{
    public class Logger
    {
        public void Info(params object[] data)
        {
            foreach (var d in data)
            {
                Console.WriteLine(d);
            }
        }

        public void Warn(params object[] data)
        {
            foreach (var d in data)
            {
                Console.WriteLine(d);
            }
        }
    }
}
