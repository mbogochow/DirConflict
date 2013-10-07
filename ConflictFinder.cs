using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DirConflict
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
      List<string> files1 = new List<string>(); // The files in path1
      List<string> files2 = new List<string>(); // The files in path2

      // Get the files in the directory and their subdirectories if specified
      if (path1.Path == "" || path2.Path == "")
        throw new DirectoryNotFoundException();

      // Setup threadpool objects
      ManualResetEvent[] doneEvents = new ManualResetEvent[2];
      FilesRetriever[] mGetFiles = new FilesRetriever[2];
      for (int i = 0; i < doneEvents.Length; i++)
      {
        doneEvents[i] = new ManualResetEvent(false);
        mGetFiles[i] = new FilesRetriever(doneEvents[i]);
      }

      // Get files from path1
      if (path1.PathOptions.subdirectories)
        ThreadPool.QueueUserWorkItem(mGetFiles[0].ThreadPoolCallback, 
          (object)path1.Path);

      else
        mGetFiles[0].getFiles(path1.Path);
      
      // Get files from path2
      if (path2.PathOptions.subdirectories)
        ThreadPool.QueueUserWorkItem(mGetFiles[1].ThreadPoolCallback, 
          (object)path2.Path);

      else
        mGetFiles[1].getFiles(path2.Path);

      // Wait until items have finished
      WaitHandle.WaitAll(doneEvents);

      // Get results
      files1 = mGetFiles[0].Files;
      files2 = mGetFiles[1].Files;

      return searchConflicts(files1, files2);
    } /* run */

    /// <summary>
    /// Search two lists of files for conflicts
    /// </summary>
    /// <param name="files1">First list of files</param>
    /// <param name="files2">Second list of files</param>
    /// <returns>Conflicting file names within the paths including the full 
    /// paths at which the conflicting file is located within each path.
    /// </returns>
    public static List<string[]> searchConflicts (List<string> files1, 
      List<string> files2)
    {
      // Get lengths
      int len1 = files1.Count;
      int len2 = files2.Count;

      List<String[]> ret = new List<string[]>(); // The list to be returned
      object retLock = new object(); // Lock for adding to ret

      // Get the actual file names from the paths
      string[] files2_filenames = new string[len2];
      for (int i = 0; i < len2; i++)
        files2_filenames[i] = getFilename(files2.ElementAt(i));

      // Use threads for searching for conflicting names
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
            }

            if (cmp == 0)
            { // Found a conflict
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

      // Wait for all searcher threads to finish
      for (int i = 0; i < len1; i++)
        threads[i].Join();

      return ret;
    }

    /// <summary>
    /// Path to be used by ConflictFinder.  Contains the path as a string as 
    /// well as options for how it should be processed.
    /// </summary>
    public class ConflictPath
    {
      /// <summary>The path</summary>
      private string path;
      /// <summary>The path</summary>
      public string Path
      {
        get { return path; }
        set { this.path = value; }
      }

      /// <summary>The options</summary>
      private Options options;
      /// <summary>The options</summary>
      public Options PathOptions
      {
        get { return this.options; }
        set { this.options = value; }
      }

      /// <summary>
      /// Construct a ConflictPath
      /// </summary>
      /// <param name="path">The path</param>
      /// <param name="options">The options for the path</param>
      public ConflictPath(string path, Options options)
      {
        this.path = path;
        this.options = options;
      }

      /// <summary>
      /// Represents options for ConflictPath
      /// </summary>
      public struct Options
      {
        /// <summary>
        /// Whether subdirectories of the path should be processed
        /// </summary>
        public bool subdirectories;
      }
    } /* ConflictPath */
  } /* ConflictFinder */
} /* Conflicts */
