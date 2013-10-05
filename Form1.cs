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
    /// 
    /// </summary>
    public MainForm()
    {
      InitializeComponent();

      tableLayoutPanel1.Controls.Add(makeLabel("File Name"), 0, 0);
      tableLayoutPanel1.Controls.Add(makeLabel("Folder 1"), 1, 0);
      tableLayoutPanel1.Controls.Add(makeLabel("Folder 2"), 2, 0);
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

    private int numConflicts = 0;

    private void makeRow(TextBox[] textBoxes)
    {
      numConflicts += 1;
      for (int i = 0; i < 3; i++)
        tableLayoutPanel1.Controls.Add(textBoxes[i], i, numConflicts);
    }
    public delegate void InvokeDelegate(TextBox[] textBoxes);

    private void update(string[] data)
    {
      TextBox[] textBoxes = new TextBox[3];
      for (int i = 0; i < 3; i++)
        textBoxes[i] = TextBoxFactory(data[i]);

      Object[] pars = new Object[1];
      pars[0] = textBoxes;
      tableLayoutPanel1.Invoke(new InvokeDelegate(makeRow), pars);
    }

    /// <summary>
    /// Perform the algorithm on button click
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void button1_Click(object sender, EventArgs e)
    {
      tableLayoutPanel1.Controls.Clear();
      numConflicts = 0;
      label3.Text = "";

      List<String[]> res;
      string path1 = textBox1.Text;
      string path2 = textBox2.Text;

      try
      {
        res = ConflictFinder.run(path1, path2);
      }
      catch (DirectoryNotFoundException ex)
      {
        label3.Text = ex.Message;
        label5.Text = "";
        return;
      }

      int count = res.Count;
      Thread[] threads = new Thread[count];
      for (int i = 0; i < count; i++)
      {
        threads[i] = new Thread((object data) =>
        {
          update(res.ElementAt((int)data));
        });

        threads[i].Start((object)i);
      }

      label5.Text = "Number of conflicts: " + numConflicts;
    } /* button click */

    private void getFiles(string path)
    {
      Directory.GetFiles(path, "*", SearchOption.AllDirectories);
    }
  } /* form1 */
} /* Conflicts */
