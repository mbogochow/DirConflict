using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private static void getAllFiles(ref List<string> files, string path)
    {
      try
      {
        files.AddRange(Directory.EnumerateFiles(path));
        //Debug.WriteLine("Count: " + files.Count);
      }

      catch (UnauthorizedAccessException ex)
      {
        //MessageBox.Show(ex.Message);
        return;
      }

      List<string> dirs = new List<string>(Directory.EnumerateDirectories(path));
      foreach (string dir in dirs)
      {
        getAllFiles(ref files, dir);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="files"></param>
    /// <param name="path"></param>
    private static void getFiles(ref List<string> files, string path)
    {
      try
      {
        files.AddRange(Directory.EnumerateFiles(path));
      }

      catch (UnauthorizedAccessException ex)
      {
        return;
      }
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

      List<string> files1 = new List<string>();
      List<string> files2 = new List<string>();

      string err = "";

      // Get the files in the directory and their subdirectories if specified
      if (path1.Path == "" || path2.Path == "")
        throw new DirectoryNotFoundException();

      Thread t = new Thread(() =>
      {
        try
        {
          if (path1.PathOptions.subdirectories)
            getAllFiles(ref files1, path1.Path);

          else
            getFiles(ref files1, path1.Path);
        }

        catch (UnauthorizedAccessException ex)
        {
          err = ex.Message;
        }
      });

      t.Start();
      if (err.CompareTo("") != 0) throw new UnauthorizedAccessException(err);

      if (path2.PathOptions.subdirectories)
        getAllFiles(ref files2, path2.Path);

      else
        getFiles(ref files2, path2.Path);

      t.Join();
      
      int len1 = files1.Count;
      int len2 = files2.Count;
      //MessageBox.Show(len1.ToString());
      // Get the actual file names from the paths
      string[] files2_filenames = new string[len2];
      for (int i = 0; i < len2; i++)
        files2_filenames[i] = getFilename(files2.ElementAt(i));

      StringBuilder sb = new StringBuilder();
      int numConflicts = 0;

      Thread[] threads = new Thread[len1];
      Object filesLock = new Object();

      // Loop through all files in both sets looking for conflicting names
      for (int i = 0; i < len1; i++)
      {
        threads[i] = new Thread((object data) => 
        {
          int index = (int)data;
          string filename;

          lock (filesLock)
          { 
            filename = getFilename(files1.ElementAt(index));
          }

          // Loop through files in path2 for each file in path1
          for (int j = 0; j < len2; j++)
          {
            int cmp;

            lock (filesLock)
            {
              cmp = String.Compare(filename, files2_filenames[j],
                                        StringComparison.OrdinalIgnoreCase);
              //Debug.WriteLine(filename + " " + files2_filenames[j] + " " + cmp);
            }
            if (cmp == 0)
            {
              numConflicts += 1;
              string[] entry = new string[3];

              lock (filesLock)
              {
                entry[0] = filename;
                entry[1] = getFolder(files1[index]);
                entry[2] = getFolder(files2[j]);
              }

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
      public
        string Path
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
