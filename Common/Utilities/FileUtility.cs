using System.IO;
using Common.Exceptions;

namespace Common.Utilities
{
    public class FileUtility
    {
        public static void DeleteFile(string fileName, string directoryName)
        {
            try
            {
                if (fileName != null)
                {
                    if (File.Exists(Path.Combine(directoryName, fileName)))
                    {
                        File.Delete(Path.Combine(directoryName, fileName));
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLog.SaveError(ex);
            }
        }
    }
}
