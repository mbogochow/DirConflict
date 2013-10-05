///<author>Mike Bogochow</author>
///<description>
///Parses through the files of two directories and their subdirectories and 
///displays any filenames that are found within both of the directories or 
///their subdirectories.
/// </description>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Conflicts
{
  /// <summary>
  /// Logic for the main form
  /// </summary>
  public partial class MainForm : Form
  {
    /// <summary>
    /// 
    /// </summary>
    public MainForm()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Get the filename from a path
    /// </summary>
    /// <param name="path">The path to get the filename from</param>
    /// <returns>The filename</returns>
    private string getFilename(string path)
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
    private string getFolder(string path)
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
    /// Make a label with the given text
    /// </summary>
    /// <param name="text">The text to set the label's Text property to</param>
    /// <returns>The new label</returns>
    private Label makeLabel(string text)
    {
      Label lb = new Label();
      lb.Text = text;
      return lb;
    }

    /// <summary>
    /// Make a TextBox with the given text
    /// </summary>
    /// <param name="text">The text to set the TextBox's Text property 
    /// to</param>
    /// <returns>The new TextBox</returns>
    private TextBox makeTextBox(string text)
    {
      TextBox tb = new TextBox();
      tb.Dock = DockStyle.Fill;
      //tb.Multiline = true;
      tb.Text = text;

      return tb;
    }

    private int numConflicts = 0;

    private void update(string filename, string path1, string path2)
    {
      numConflicts += 1;
      TextBox a = makeTextBox(filename);
      TextBox b = makeTextBox(path1);
      TextBox c = makeTextBox(path2);

      tableLayoutPanel1.Controls.Add(a, 0, numConflicts);
      tableLayoutPanel1.Controls.Add(b, 1, numConflicts);
      tableLayoutPanel1.Controls.Add(c, 2, numConflicts);
    }

    /// <summary>
    /// Perform the algorithm on button click
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void button1_Click(object sender, EventArgs e)
    {
      tableLayoutPanel1.Controls.Clear();
      label3.Text = "";

      string path1 = textBox1.Text;
      string path2 = textBox2.Text;
      string[] files1;
      string[] files2;

      // Get the files in the directory and their subdirectories
      try
      {
        if (path1 == "" || path2 == "")
          throw new DirectoryNotFoundException();

        files1 = Directory.GetFiles(path1, "*", SearchOption.AllDirectories);
        files2 = Directory.GetFiles(path2, "*", SearchOption.AllDirectories);
      }

      catch (DirectoryNotFoundException ex)
      {
        label3.Text = ex.Message;
        return;
      }

      int len1 = files1.Length;
      int len2 = files2.Length;

      // Get the actual file names from the paths
      string[] files2_filenames = new string[files2.Length];
      for (int i = 0; i < len2; i++)
        files2_filenames[i] = getFilename(files2[i]);

      StringBuilder sb = new StringBuilder();
      int numConflicts = 0;

      tableLayoutPanel1.Controls.Add(makeLabel("File Name"), 0, 0);
      tableLayoutPanel1.Controls.Add(makeLabel("Folder 1"),  1, 0);
      tableLayoutPanel1.Controls.Add(makeLabel("Folder 2"),  2, 0);

      // Loop through all files in both sets looking for conflicting names
      for (int i = 0; i < len1; i++)
      {
        string filename = getFilename(files1[i]);
                                                                                
        for (int j = 0; j < len2; j++)
        {
          int cmp = String.Compare(filename, files2_filenames[j], 
                                   StringComparison.OrdinalIgnoreCase);
          if (cmp == 0)
          {
            numConflicts += 1;

            update(filename, files1[i], files2[j]);
          }
        }
      }
    } /* button click */

    private void getFiles(string path)
    {
      Directory.GetFiles(path, "*", SearchOption.AllDirectories);
    }
  } /* form1 */
} /* Conflicts */
