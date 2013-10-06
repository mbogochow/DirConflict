using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Conflicts
{
  /// <summary>
  /// 
  /// </summary>
  class FilesRetriever
  {
    /// <summary>
    /// 
    /// </summary>
    private List<string> files = new List<string>();
    /// <summary>
    /// 
    /// </summary>
    public List<string> Files
    {
      get { return files; }
    }

    /// <summary>
    /// 
    /// </summary>
    private ManualResetEvent doneEvent;

    private ManualResetEvent[] doneEvents;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doneEvent"></param>
    public FilesRetriever(ManualResetEvent doneEvent)
    {
      this.doneEvent = doneEvent;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void ThreadPoolCallback(object data) 
    {
      string path = (string)data;
      getAllFiles(path);
      doneEvent.Set();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="files"></param>
    /// <param name="path"></param>
    private void getAllFiles(string path)
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

      catch (DirectoryNotFoundException ex)
      {
        return;
      }

      List<string> dirs = new List<string>(Directory.EnumerateDirectories(path));
      int dirCount = dirs.Count;
      if (dirCount > 1)
      {
        int waitCount = Math.Min(dirCount, 64);

        while (waitCount <= dirCount)
        {
          doneEvents = new ManualResetEvent[waitCount];
          FilesRetriever[] fr = new FilesRetriever[waitCount];
          
          for (int i = 0; i < waitCount; i++)
          {
            string dir = dirs.ElementAt(i);
            doneEvents[i] = new ManualResetEvent(false);
            fr[i] = new FilesRetriever(doneEvents[i]);
            ThreadPool.QueueUserWorkItem(fr[i].ThreadPoolCallback, (object)dir);
          }

          WaitHandle.WaitAll(doneEvents);
          for (int i = 0; i < fr.Length; i++)
          {
            files.AddRange(fr[i].Files);
          }

          dirCount -= waitCount;
          waitCount = Math.Min(dirCount, 64);
          if (waitCount <= 0) break;
        }
      }

      else if (dirCount != 0)
        getAllFiles(dirs.ElementAt(0));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="files"></param>
    /// <param name="path"></param>
    public void getFiles(string path)
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
  }
}
