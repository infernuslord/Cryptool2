using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Collections;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class CT2Translate : Form
    {
        private ListViewColumnSorter lvwColumnSorter;
        List<string> allResources = new List<string>();
        //Dictionary<string, TranslatedResource> theResources = new Dictionary<string,TranslatedResource>();
        AllResources allres = new AllResources();

        public CT2Translate()
        {
            InitializeComponent();
            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = lvwColumnSorter;
            listView1.Columns.Clear();
            listView1.Columns.Add(new ColHeader("File", 10, HorizontalAlignment.Left, true));
            listView1.Columns.Add(new ColHeader("Key", 60, HorizontalAlignment.Left, true));
            listView1.Columns.Add(new ColHeader("Englisch", 110, HorizontalAlignment.Left, true));
            listView1.Columns.Add(new ColHeader("Deutsch", 110, HorizontalAlignment.Left, true));

            textBox1.Text = Properties.Settings.Default.Path;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open Cryptool Plugin Resource";
            //dlg.Filter = "Crytool Plugin Resource (*.resx)|*.resx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = dlg.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void PathButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog objDialog = new FolderBrowserDialog();
            objDialog.Description = "Startpfad wählen";
            objDialog.SelectedPath = textBox1.Text;
            DialogResult objResult = objDialog.ShowDialog(this);
            //if (objResult == DialogResult.OK)
            //    MessageBox.Show("Neuer Pfad : " + objDialog.SelectedPath);
            //else
            //    MessageBox.Show("Abbruch gewählt!");
            if (objResult == DialogResult.OK)
            {
                textBox1.Text = objDialog.SelectedPath;
            }
        }

        private void SearchResourcesButton_Click(object sender, EventArgs e)
        {
            logBox.Text = "";
            listView1.Items.Clear();
            treeView1.Nodes.Clear();

            Cursor.Current = Cursors.WaitCursor;
            TreeNode t = new TreeNode( textBox1.Text );
            int n = DirSearch( textBox1.Text, t );
            if( n>0 ) treeView1.Nodes.Add(t);
            Cursor.Current = Cursors.Default;

            toolStripStatusLabel1.Text = n + " files found";
        }

        private void AddAllResourcesButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            allres.Clear();
            allres.Add(allResources.ToArray());

            listView1.Items.Clear();
            listViewAdd();

            Cursor.Current = Cursors.Default;
        }

        int DirSearch(string sDir, TreeNode p)
        {
            int cnt=0;

            try
            {
                foreach (string f in Directory.GetFiles(sDir, "*.resx"))
                {
                    TreeNode t = p.Nodes.Add(f.Substring(f.LastIndexOf('\\') + 1));
                    t.Tag = f;
                    t.ToolTipText = f;
                    allResources.Add(f);
                    //allres.Add(f);
                    
                    cnt++;
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    TreeNode t = new TreeNode(d.Substring(d.LastIndexOf('\\') + 1));
                    int found = DirSearch(d, t);
                    if (found > 0)
                    {
                        p.Nodes.Add(t);
                        cnt += found;
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return cnt;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( listView1.SelectedItems.Count>0 ) {
                fileTextBox.Text = listView1.SelectedItems[0].SubItems[0].Text;
                keyTextBox.Text = listView1.SelectedItems[0].SubItems[1].Text;
                richTextBox2.Text = listView1.SelectedItems[0].SubItems[2].Text;
                richTextBox3.Text = listView1.SelectedItems[0].SubItems[3].Text;
            }
        }

        private void listViewAdd(string fname, TranslatedResource dict, string filter = ".*")
        {
            string lang1 = "en";
            string lang2 = "de";
            string emptystring = "-";
            List<ListViewItem> lvi = new List<ListViewItem>();

            try
            {
                foreach (KeyValuePair<string, TranslatedKey> pair in dict.TranslatedKey)
                {
                    bool matched = Regex.Match(pair.Key.ToString(), filter).Success;
                    if (pair.Value.Translations.ContainsKey(lang1))
                        matched |= Regex.Match(pair.Value.Translations[lang1].ToString(), filter).Success;
                    if (pair.Value.Translations.ContainsKey(lang2))
                        matched |= Regex.Match(pair.Value.Translations[lang2].ToString(), filter).Success;
                    if (!matched) continue;

                    ListViewItem item = new ListViewItem(new string[] {
                        fname,
                        pair.Key.ToString(),
                        pair.Value.Translations.ContainsKey(lang1) ? pair.Value.Translations[lang1] : emptystring,
                        pair.Value.Translations.ContainsKey(lang2) ? pair.Value.Translations[lang2] : emptystring
                    });
                    item.UseItemStyleForSubItems = false;
                    if (!pair.Value.Translations.ContainsKey(lang1)) item.SubItems[2].BackColor = Color.LightSalmon;
                    if (!pair.Value.Translations.ContainsKey(lang2)) item.SubItems[3].BackColor = Color.LightSalmon;
                    //listView1.Items.Add(item);
                    lvi.Add(item);
                }
                listView1.Items.AddRange(lvi.ToArray());    // viel schneller als einzelnes Hinzufügen!
            }
            catch (Exception e)
            {
            }
        }

        private void listViewAdd(string filter = ".*")
        {
            //foreach (string basename in theResources.Keys)
                //listViewAdd(basename, theResources[basename], filter);
            listView1.BeginUpdate();
            foreach (string basename in allres.Resources.Keys)
                listViewAdd(basename, allres.Resources[basename], filter);
            listView1.EndUpdate();
            toolStripStatusLabel1.Text = listView1.Items.Count + " items displayed";
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Nodes.Count == 0)
            {
                string sel = treeView1.SelectedNode.Tag.ToString();
                toolStripStatusLabel1.Text = sel;

                allres.Clear();
                allres.Add(sel);
                
                listView1.Items.Clear();
                listViewAdd();

                if (listView1.Items.Count > 0)
                    listView1.SelectedIndices.Add(0);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Path = textBox1.Text;
            Properties.Settings.Default.Save();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //listView1.AutoResizeColumn(e.Column, ColumnHeaderAutoResizeStyle.ColumnContent);
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                lvwColumnSorter.Order = (lvwColumnSorter.Order == SortOrder.Ascending)
                    ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            listView1.Items.Clear();
            listViewAdd(filterBox.Text);
            Cursor.Current = Cursors.Default;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode==null) return;
            //if( listView1.SelectedItems.Count==0 ) return;
            //string filename = listView1.SelectedItems[0].SubItems[0].Text;
            string filename = treeView1.SelectedNode.FullPath;
            logBox.Text += "Saving " + filename + "\n";
            //SaveResourceFile(listView1.SelectedItems[0].SubItems[0].Text);
            string basename = AllResources.getBasename(filename);
            string culture = AllResources.getCulture(filename);
            if( allres.Resources.ContainsKey(basename) )
                allres.Resources[basename].SaveAs(culture, "test");
        }

        private void saveInOneFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fname = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CT2resources.xml";
            allres.SaveXML(fname, textBox1.Text);
        }

        private void loadFromOneFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            string fname = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CT2resources.xml";

            try
            {
                allres.Clear();
                allres.LoadXML(fname);

                listView1.Items.Clear();
                listViewAdd();
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = fname + " not found!";
            }

            Cursor.Current = Cursors.Default;
        }

        private void ClearSearchButton_Click(object sender, EventArgs e)
        {
            filterBox.Text = "";
            SearchButton_Click(sender, e);
        }

        private void saveGermanAsTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            allres.SaveText(path+"\\CT2german.txt", "de");
        }

    }
}
