using System;

namespace Common.Utilities
{
    public static class ProgressClass
    {
        public static string GetClass(int i)
        {
            Random rnd = new Random();
          var index=rnd.Next(1, 3);
            switch (i)
            {
                case 1: { return "success"; }
                case 2: { return "primary"; }
                case 3: { return "yellow"; }
                case 4: { return "warning"; }
                case 5: { return "striped"; }
                case 6: { return "danger"; }
                case 7: { return "info"; }
                default:
                    return "success";
             
            }

            //var cls = new string[] { "info", "striped", "warning"};
            //return cls[index-1];
        }
    }
}
