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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// Construct MainForm
    /// </summary>
    public MainForm()
    {
      InitializeComponent();
      
      string html = DirConflict.Properties.Resources.HTMLPage1;
      webBrowser1.DocumentText = html;
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
    private TextBox TextBoxFactory(string text)
    {
      TextBox tb = new TextBox();
      tb.Dock = DockStyle.Fill;
      tb.Text = text;

      return tb;
    }

    private delegate void setLabelText(Label label, string msg);
    private void labelTextSetter(Label label, string msg)
    {
      label.Text = msg;
    }

    /// <summary>
    /// Perform the algorithm on button click
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void button1_Click(object sender, EventArgs e)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      // Prepare form for new results
      label3.Text = "";

      List<String[]> res = null;
      int numConflicts = 0;
      string path1 = textBox1.Text;
      string path2 = textBox2.Text;

      // Run searching algorithm
      // Run on new thread since can't call WaitHandle.WaitAll on UI thread
      Thread t = new Thread(() =>
      {
        ConflictFinder.ConflictPath.Options options1 =
        new ConflictFinder.ConflictPath.Options();
        ConflictFinder.ConflictPath.Options options2 =
          new ConflictFinder.ConflictPath.Options();

        if (checkBox1.Checked) options1.subdirectories = true;
        if (checkBox2.Checked) options2.subdirectories = true;

        try
        {
          res = ConflictFinder.run(
            new ConflictFinder.ConflictPath(path1, options1),
            new ConflictFinder.ConflictPath(path2, options2)
            );
        }

        catch (Exception ex)
        {
          if (ex is DirectoryNotFoundException ||
              ex is UnauthorizedAccessException)
          { 
            object[] obj = {label3, ex.Message};
            label3.BeginInvoke(new setLabelText(labelTextSetter), obj);
            object[] obj2 = {label5, ""};
            label5.BeginInvoke(new setLabelText(labelTextSetter), obj2);
            return;
          }

          throw;
        }
      });

      t.Start();
      t.Join();
      
      if (res.Count > 0)
      {
        // Construct table from results
        const string header1 = "File Name";
        const string header2 = "Folder 1";
        const string header3 = "Folder 2";

        StringBuilder sb = new StringBuilder();
        sb.Append("<table>")
          .Append("<tr><th>").Append(header1)
          .Append("</th><th>").Append(header2)
          .Append("</th><th>").Append(header3)
          .Append("</th></tr>");
        foreach (String[] entry in res)
        {
          numConflicts += 1;

          sb.Append("<tr>");

          for (int i = 0; i < entry.Length; i++)
            sb.Append("<td>" + entry[i] + "</td>");

          sb.Append("</tr>");
        }
        sb.Append("</table>");

        webBrowser1.Document.Body.InnerHtml = sb.ToString();
      }

      else
        webBrowser1.Document.Body.InnerHtml = "";

      label5.Text = "Number of conflicts: " + numConflicts;

      stopwatch.Stop();
      long time = stopwatch.ElapsedMilliseconds;
      double dTime = (double)time / 1000;
      label3.Text = dTime.ToString() + " seconds";
    } /* button click */
  } /* form1 */
} /* Conflicts */
