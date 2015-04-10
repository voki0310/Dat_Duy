using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PhotoAlbum;

namespace MyPhotos
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            NewAlbum();
            mnuView.DropDown = ctxMenuPhoto;
        }

        
        private void ProcessPhoto(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem item = e.ClickedItem;
            string enumVal = item.Tag as string;
            if (enumVal != null)
            {
                pbxPhoto.SizeMode = (PictureBoxSizeMode)Enum.Parse(typeof(PictureBoxSizeMode), enumVal);
            }
        }

        private void mnuImage_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripDropDownItem parent = (ToolStripDropDownItem)sender;
            if (parent != null)
            {
                string enumVal = pbxPhoto.SizeMode.ToString();
                //duyệt qua các item con trong Image
                foreach (ToolStripMenuItem item in parent.DropDownItems)
                {
                    //ẩn hiện item khi có ảnh
                    item.Enabled = (pbxPhoto.Image != null);
                    item.Checked = item.Tag.Equals(enumVal);
                }
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetStatusStrip()
        {
            if (pbxPhoto.Image != null)
            {
                sttInfo.Text = Manager.Current.FileName;
                sttImageSize.Text = String.Format("{0:#}x{1:#}", pbxPhoto.Image.Width, pbxPhoto.Image.Height);
                sttAlbumPos.Text = String.Format(" {0:0}/{1:0} ", Manager.Index + 1, Manager.Album.Count);
            }
            else
            {
                sttInfo.Text = null;
                sttImageSize.Text = null;
                sttAlbumPos.Text = null;
            }
        }

        private  AlbumManager manager;
        private AlbumManager Manager
        {
            get { return manager; }
            set { manager = value; }
        }

        private void DisplayAlbum()
        {
            pbxPhoto.Image = Manager.CurrentImage;
            SetStatusStrip();
            SetTitleBar();
        }

        private void mnuFileNew_Click(object sender, EventArgs e)
        {
            NewAlbum();
        }

        private void NewAlbum()
        {
            if (Manager == null || SaveAndCloseAlbum())
            {

                Manager = new AlbumManager();
                DisplayAlbum();
            }
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            // Allow user to select a new album
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open Album";
            dlg.Filter = "Album files (*.abm)|*.abm|" + "All files (*.*)|*.*";
            dlg.InitialDirectory = AlbumManager.DefaultPath;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string path = dlg.FileName;
                if (!SaveAndCloseAlbum())
                    return;
                try
                {
                    // Open the new album
                    // TODO: handle invalid album file
                    Manager = new AlbumManager(path);
                }
                catch (AlbumStorageException aex)
                {
                    string msg = String.Format("Unable to open album file {0}\n({1})",
                        path, aex.Message);
                    MessageBox.Show(msg, "Unable to Open");
                    Manager = new AlbumManager();
                }
                DisplayAlbum();
            }
            dlg.Dispose();
        }

        private void SaveAlbum(string name)
        {
            try
            {
                Manager.Save(name, true);
            }
            catch (AlbumStorageException aex)
            {
                string msg = String.Format("Unable to save album {0} ({1})\n\n"
                    + "Do you wish to save the album " + "under an alternate name?",
                    name, aex.Message);
                DialogResult result = MessageBox.Show(msg, "Unable to Save",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Yes)
                    SaveAsAlbum();
            }
        }
        private void SaveAlbum()
        {
            if (String.IsNullOrEmpty(Manager.FullName))
                SaveAsAlbum();
            else
            {
                // Save the album under the existing name
                SaveAlbum(Manager.FullName);
            }
        }

        private void mnuFileSave_Click(object sender, EventArgs e)
        {
            SaveAlbum();
        }

        private void mnuFileSaveAs_Click(object sender, EventArgs e)
        {
            SaveAsAlbum();
        }

        private void SaveAsAlbum()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save Album";
            dlg.DefaultExt = "abm";
            dlg.Filter = "Album files (*.abm)|*.abm|"
                + "All files|*.*";
            dlg.InitialDirectory
              = AlbumManager.DefaultPath;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SaveAlbum(dlg.FileName);
                // Update title bar to include new name
                SetTitleBar();
            }
            dlg.Dispose();
        }

        private void SetTitleBar()
        {
            Version ver = new Version(Application.ProductVersion);
            string name = Manager.FullName;
            this.Text = String.Format("{2} - MyPhotos {0:0}.{1:0}",
                ver.Major, ver.Minor,
                String.IsNullOrEmpty(name)? "Untitled" : name);
        }

        private void mnuEditAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Add Photos";
            dlg.Multiselect = true;
            dlg.Filter = "Image Files (JPEG, GIF, BMP, etc.)|"
                  + "*.jpg;*.jpeg;*.gif;*.bmp;"
                  + "*.tif;*.tiff;*.png|"
                  + "JPEG files (*.jpg;*.jpeg)|*.jpg;*.jpeg|"
                  + "GIF files (*.gif)|*.gif|"
                  + "BMP files (*.bmp)|*.bmp|"
                  + "TIFF files (*.tif;*.tiff)|*.tif;*.tiff|"
                  + "PNG files (*.png)|*.png|"
                  + "All files (*.*)|*.*";
            dlg.InitialDirectory = Environment.CurrentDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string[] files = dlg.FileNames;
                int index = 0;
                foreach (string s in files)
                {
                    Photo photo = new Photo(s);
                    // Add the file (if not already present)
                    index = Manager.Album.IndexOf(photo);
                    if (index < 0)
                        Manager.Album.Add(photo);
                    else
                        photo.Dispose();  // photo already there
                }
                Manager.Index=Manager.Album.Count-1;
            }
            dlg.Dispose();
            DisplayAlbum();
        }

        private void mnuEditRemove_Click(object sender, EventArgs e)
        {
            if (Manager.Album.Count > 0)
            {
                Manager.Album.RemoveAt(Manager.Index);
                DisplayAlbum();
            }
        }

        private void mnuNext_Click(object sender, EventArgs e)
        {
            if (Manager.Index < Manager.Album.Count - 1)
            {
                Manager.Index++;
                DisplayAlbum();
            }
        }

        private void mnuPrevious_Click(object sender, EventArgs e)
        {
            if (Manager.Index > 0)
            {
                Manager.Index--;
                DisplayAlbum();
            }
        }

        private void ctxMenuPhoto_Opening(object sender, CancelEventArgs e)
        {
             mnuNext.Enabled = (Manager.Index < Manager.Album.Count - 1);
             mnuPrevious.Enabled = (Manager.Index > 0);
        }

        private bool SaveAndCloseAlbum()
        {
            if (Manager.Album.HasChanged)
            {
                // Offer to save the current album
                string msg;
                if (String.IsNullOrEmpty(Manager.FullName))
                    msg = "Do you wish to save your changes?";
                else
                    msg = String.Format("Do you wish to "+ "save your changes to \n{0}?",
                        Manager.FullName);
                    DialogResult result = MessageBox.Show(this, msg,"Save Changes?",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                 if (result == DialogResult.Yes)
                    SaveAlbum();
                else if (result == DialogResult.Cancel)
                    return false;  // do not close
            }
            // Close the album and return true
            if (Manager.Album != null)
                Manager.Album.Dispose();
            Manager = new AlbumManager();
            SetTitleBar();
            return true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (SaveAndCloseAlbum() == false)
                e.Cancel = true;
            else
                e.Cancel = false;
             base.OnFormClosing(e);
        }
        
    }
}

