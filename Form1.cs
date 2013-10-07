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

namespace DirConflict
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

    /// <summary>
    /// Delegate for setting the text of a Control
    /// </summary>
    /// <param name="ctrl">The Control to set</param>
    /// <param name="msg">The value to set</param>
    private delegate void controlTextSetter(Control ctrl, string msg);
    /// <summary>
    /// Set the Text property of a Control
    /// </summary>
    /// <param name="ctrl">The Control to set</param>
    /// <param name="msg">The value to set</param>
    private void setControlText(Control ctrl, string msg)
    {
      ctrl.Text = msg;
    }

    /// <summary>
    /// Delegate for setting the inner HTML of a WebBrowser body.
    /// </summary>
    /// <param name="wb">The WebBrowser to set</param>
    /// <param name="html">The HTML to set the body of the WebBrowser to.
    /// </param>
    private delegate void innerHTMLSetter(WebBrowser wb, string html);
    /// <summary>
    /// Set the inner HTML of a WebBrowser body.
    /// </summary>
    /// <param name="wb">The WebBrowser to set</param>
    /// <param name="html">The HTML to set the body of the WebBrowser to.
    /// </param>
    private void setInnerHTML(WebBrowser wb, string html)
    {
      wb.Document.Body.InnerHtml = html;
    }

    /// <summary>
    /// Delegate for setting the boolean state of a control property.
    /// </summary>
    /// <param name="ctrl">The Control to set</param>
    /// <param name="val">The value to set the property to</param>
    private delegate void controlToggle(Control ctrl, bool val);
    /// <summary>
    /// Set the enabled state of the control to the value.
    /// </summary>
    /// <param name="ctrl">The control to set</param>
    /// <param name="val">The value to set the enabled state to</param>
    private void toggleControlEnabled(Control ctrl, bool val)
    {
      ctrl.Enabled = val;
    }

    /// <summary>
    /// The thread used for running Conflicts and processing results.
    /// </summary>
    private Thread processingThread = null;

    /// <summary>
    /// Perform the algorithm on button click
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void runButton_Click(object sender, EventArgs e)
    {
      // Keep track of how long it takes from start to display results
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();

      // Prepare form for new results
      label3.Text = "Running";
      timer1.Start();

      List<String[]> res = null;
      int numConflicts = 0;
      string path1 = textBox1.Text;
      string path2 = textBox2.Text;

      // Run searching algorithm
      // Run on new thread since can't call WaitHandle.WaitAll on UI thread and 
      // don't want to block on the UI thread anyway
      processingThread = new Thread(() =>
      {
        // Create new conflict finder paths to be passed to ConflictFinder
        ConflictFinder.ConflictPath.Options options1 =
          new ConflictFinder.ConflictPath.Options();
        ConflictFinder.ConflictPath.Options options2 =
          new ConflictFinder.ConflictPath.Options();

        if (checkBox1.Checked) options1.subdirectories = true;
        if (checkBox2.Checked) options2.subdirectories = true;

        try
        {
          // Run the search algorithm
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
            label3.BeginInvoke(new controlTextSetter(setControlText), 
              new object[]{ label3, ex.Message });
            label5.BeginInvoke(new controlTextSetter(setControlText), 
              new object[]{ label5, "" });
            return;
          }

          throw;
        }

        // Process if found any results
        if (res.Count > 0)
        {
          // Construct table from results
          const string header1 = "File Name";
          const string header2 = "Folder 1";
          const string header3 = "Folder 2";

          // Set header row
          StringBuilder sb = new StringBuilder();
          sb.Append("<table>")
            .Append("<tr><th>").Append(header1)
            .Append("</th><th>").Append(header2)
            .Append("</th><th>").Append(header3)
            .Append("</th></tr>");

          // Set data rows
          foreach (String[] entry in res)
          {
            numConflicts += 1;

            sb.Append("<tr>");

            for (int i = 0; i < entry.Length; i++)
              sb.Append("<td>" + entry[i] + "</td>");

            sb.Append("</tr>");
          }
          sb.Append("</table>");

          // Update the web browser
          webBrowser1.BeginInvoke(new innerHTMLSetter(setInnerHTML), 
            new object[]{ webBrowser1, sb.ToString() });
        }

        else // no results found, set to blank
          webBrowser1.BeginInvoke(new innerHTMLSetter(setInnerHTML), 
            new object[] { webBrowser1, "" });

        // Update number of reults found
        label5.BeginInvoke(new controlTextSetter(setControlText), 
          new object[]{ label5, "Number of conflicts: " + numConflicts });

        // Display total time it took
        stopwatch.Stop();
        long time = stopwatch.ElapsedMilliseconds;
        double dTime = (double)time / 1000;

        label3.BeginInvoke(new controlTextSetter(setControlText), 
          new object[]{ label3, dTime.ToString() + " seconds" });

        runButton.BeginInvoke(new controlToggle(toggleControlEnabled), 
          new object[] { runButton, true });

        timer1.Stop();
      });

      processingThread.Start();
      runButton.Enabled = false;
    } /* button click */

    /// <summary>
    /// Cancel the processing thread if it is running and set runButton to 
    /// enabled.
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void cancelButton_Click(object sender, EventArgs e)
    {
      if (processingThread != null && processingThread.IsAlive)
        processingThread.Abort();

      runButton.Enabled = true;
      timer1.Stop();
      label3.Text = "Canceled";
    } /* cancelButton_Click */

    private int tickCount = 0;
    private void timer1_Tick(object sender, EventArgs e)
    {
      label3.Text += ".";

      tickCount += 1;
      if (tickCount >= 4)
      {
        tickCount = 0;
        label3.Text = label3.Text.Trim('.');
      }
    } 

  } /* form1 */
} /* Conflicts */
