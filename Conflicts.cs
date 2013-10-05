using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Conflicts
{
  static class ConflictFinder
  {
    /// <summary>
    /// Get the filename from a path
    /// </summary>
    /// <param name="path">The path to get the filename from</param>
    /// <returns>The filename</returns>
    private static string getFilename(string path)
    {
      char[] delim = { '\\', '/' };
      string[] spl = path.Split(delim);
      return spl[spl.Length - 1];
    }

    /// <summary>
    /// Get the path without the last filename
    /// </summary>
    /// <param name="path">The path to parse</param>
    /// <returns>The path excluding the final filename</returns>
    private static string getFolder(string path)
    {
      char[] delim = { '\\', '/' };
      string[] spl = path.Split(delim);
      StringBuilder sb = new StringBuilder();
      int numFolders = spl.Length - 1;

      for (int i = 0; i < numFolders; i++)
        sb.Append(spl[i]).Append("/");

      return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path1"></param>
    /// <param name="path2"></param>
    /// <returns></returns>
    public static List<String[]> run(string path1, string path2)
    {
      List<String[]> ret = new List<string[]>();
      object retLock = new object();

      string[] files1 = null;
      string[] files2 = null;

      // Get the files in the directory and their subdirectories
      if (path1 == "" || path2 == "")
        throw new DirectoryNotFoundException();

      Thread t = new Thread(() =>
      {
        files1 = Directory.GetFiles(path1, "*", SearchOption.AllDirectories);
      });

      t.Start();
        
      files2 = Directory.GetFiles(path2, "*", SearchOption.AllDirectories);
      t.Join();
      
      int len1 = files1.Length;
      int len2 = files2.Length;

      // Get the actual file names from the paths
      string[] files2_filenames = new string[files2.Length];
      for (int i = 0; i < len2; i++)
        files2_filenames[i] = getFilename(files2[i]);

      StringBuilder sb = new StringBuilder();
      int numConflicts = 0;

      Thread[] threads = new Thread[len1];

      // Loop through all files in both sets looking for conflicting names
      for (int i = 0; i < len1; i++)
      {
        threads[i] = new Thread((object data) => 
        {
          int index = (int)data;
          string filename = getFilename(files1[index]);

          for (int j = 0; j < len2; j++)
          {
            int cmp = String.Compare(filename, files2_filenames[j],
                                      StringComparison.OrdinalIgnoreCase);
            if (cmp == 0)
            {
              numConflicts += 1;

              string[] entry = { filename, files1[index], files2[j] };

              lock (retLock)
              {
                ret.Add(entry);
              }
            }
          } /* j */
        });

        threads[i].Start((object)i);
      } /* i */

      for (int i = 0; i < len1; i++)
      {
        threads[i].Join();
      }

      return ret;
    } /* run */
  } /* ConflictFinder */
} /* Conflicts */
