using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DirConflict
{
  /// <summary>
  /// Retrieve files from a path
  /// </summary>
  class FilesRetriever
  {
    /// <summary>
    /// Files read from the path
    /// </summary>
    private List<string> files = new List<string>();
    /// <summary>
    /// Files read from the path
    /// </summary>
    public List<string> Files
    {
      get { return files; }
    }

    /// <summary>
    /// Event to be set upon completion when using ThreadPool
    /// </summary>
    private ManualResetEvent doneEvent;

    /// <summary>
    /// Construct the FilesRetriever.
    /// </summary>
    public FilesRetriever()
    {
      this.doneEvent = null;
    }

    /// <summary>
    /// Construct the FilesRetriever with the given ManualResetEvent.
    /// </summary>
    /// <param name="doneEvent"></param>
    public FilesRetriever(ManualResetEvent doneEvent)
    {
      this.doneEvent = doneEvent;
    }

    /// <summary>
    /// Callback to be used for ThreadPool
    /// </summary>
    /// <param name="data">The path to get files from</param>
    public void ThreadPoolCallback(object data) 
    {
      string path = (string)data;
      getAllFiles(path);

      if (doneEvent != null)
        doneEvent.Set();
    }

    /// <summary>
    /// Recursively retrieve files from the path as well as from all 
    /// subirectories within the path.
    /// </summary>
    /// <param name="path">The path to search</param>
    private void getAllFiles(string path)
    {
      try
      { // Get files
        files.AddRange(Directory.EnumerateFiles(path));
      }

      catch (Exception ex) // UnauthorizedAccessException
      {                    // DirectoryNotFoundException
        return;
      }

      // Get directories
      List<string> dirs = new List<string>(Directory.EnumerateDirectories(path));
      foreach (string dir in dirs)
      {
        getAllFiles(dir);
      }
    }

    /// <summary>
    /// Get files from the path and ignore any directories
    /// </summary>
    /// <param name="path">The path to search</param>
    public void getFiles(string path)
    {
      try
      { // Get files
        files.AddRange(Directory.EnumerateFiles(path));
      }

      catch (UnauthorizedAccessException ex)
      {
        return;
      }
    }
  }
}
