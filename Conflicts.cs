using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Conflicts
{
  /// <summary>
  /// Class for running directory conflict search algorithm.
  /// </summary>
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
    } /* getFileName */

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
    } /* getFolder */

    /// <summary>
    /// 
    /// </summary>
    /// <param name="files"></param>
    /// <param name="path"></param>
    private static void getFiles(out string[] files, ConflictPath path)
    {
      if (path.PathOptions.subdirectories)
        files = Directory.GetFiles(path.Path, "*", SearchOption.AllDirectories);

      else
        files = Directory.GetFiles(path.Path, "*",
          SearchOption.TopDirectoryOnly);
    }

    /// <summary>
    /// Run the search algorithm.
    /// </summary>
    /// <param name="path1">The first path to search.</param>
    /// <param name="path2">The second path to search.</param>
    /// <returns>A list of conflicts. Conflicts are in the form of a string 
    /// array  with three elements: the first is the file name and the second 
    /// and  third are the paths in which the file was found in path1 and path2 
    /// respectively.</returns>
    public static List<String[]> run(ConflictPath path1, ConflictPath path2)
    {
      List<String[]> ret = new List<string[]>();
      object retLock = new object();

      string[] files1 = null;
      string[] files2 = null;

      string err = "";

      // Get the files in the directory and their subdirectories if specified
      if (path1.Path == "" || path2.Path == "")
        throw new DirectoryNotFoundException();

      Thread t = new Thread(() =>
      {
        try
        {
          getFiles(out files1, path1);
        }

        catch (UnauthorizedAccessException ex)
        {
          err = ex.Message;
        }
      });

      t.Start();
      if (err.CompareTo("") != 0) throw new UnauthorizedAccessException(err);

      //List<string> files22 = new List<string>(Directory.EnumerateDirectories(path2.Path));
      //StringBuilder sb1 = new StringBuilder();
      //foreach (string s in files22)
      //{
      //  sb1.Append(s).Append(Environment.NewLine);
      //}
      //MessageBox.Show(sb1.ToString(), "here2", MessageBoxButtons.OK);

      //List<string> EnumerateFiles = new List<string>(Directory.EnumerateFiles(path2.Path));
      //sb1.Clear();
      //foreach (string s in EnumerateFiles)
      //{
      //  sb1.AppendLine(s);
      //}
      //MessageBox.Show(sb1.ToString(), "here3", MessageBoxButtons.OK);

      getFiles(out files2, path2);

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

          // Loop through files in path2 for each file in path1
          for (int j = 0; j < len2; j++)
          {
            int cmp = String.Compare(filename, files2_filenames[j],
                                      StringComparison.OrdinalIgnoreCase);
            if (cmp == 0)
            {
              numConflicts += 1;

              string[] entry = { filename,
                                 getFolder(files1[index]), 
                                 getFolder(files2[j]) 
                               };

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

    public class ConflictPath
    {
      private string path;
      public string Path
      {
        get { return path; }
        set { this.path = value; }
      }

      private Options options;
      public Options PathOptions
      {
        get { return this.options; }
        set { this.options = value; }
      }

      public ConflictPath(string path, Options options)
      {
        this.path = path;
        this.options = options;
      }

      public struct Options
      {
        public bool subdirectories;
      }
    }
  } /* ConflictFinder */
} /* Conflicts */
