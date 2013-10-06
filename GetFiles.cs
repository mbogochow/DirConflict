using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Conflicts
{
  class FilesRetriever
  {
    private List<string> files = new List<string>();
    public List<string> Files
    {
      get { return files; }
    }

    private ManualResetEvent doneEvent;

    public FilesRetriever(ManualResetEvent doneEvent)
    {
      this.doneEvent = doneEvent;
    }

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

      List<string> dirs = new List<string>(Directory.EnumerateDirectories(path));
      foreach (string dir in dirs)
      {
        getAllFiles(dir);
      }
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
