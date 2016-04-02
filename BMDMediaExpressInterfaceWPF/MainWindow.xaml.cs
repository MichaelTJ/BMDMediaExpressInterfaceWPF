using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace BMDMediaExpressInterfaceWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        globalKeyboardHook gkh = new globalKeyboardHook();
        bool isCheckingDocs;

        public MainWindow()
        {
            InitializeComponent();
            isCheckingDocs = false;
            StartCheckingForImages();
            gkh.HookedKeys.Add(Keys.Space);
            gkh.KeyDown += new System.Windows.Forms.KeyEventHandler(gkh_KeyDown);
            
        }
        void gkh_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Console.WriteLine("Down {0}", e.KeyCode.ToString());
            //e.Handled = true;


            SendKeys.SendWait("^g");
        }

        public void StartCheckingForImages()
        {
            isCheckingDocs = true;
            Thread newThread = new Thread(new ThreadStart(CheckMyDocs));
            newThread.Start();
        }

        public void StopCheckingForImages()
        {
            isCheckingDocs = false;

        }

        private void CheckMyDocs()
        {
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            List<string> oldImages = Directory.GetFiles(myDocs, "*.tga").ToList<string>();
            List<string> addedImages;
            while (isCheckingDocs)
            {
                List<string> newImages = Directory.GetFiles(myDocs, "*.tga").ToList<string>();
                addedImages = new List<string>();
                //Get newly added images each second
                foreach (string imagePath in newImages)
                {
                    if (oldImages.Contains(imagePath))
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("new image added: {0}", imagePath);
                        addedImages.Add(imagePath);
                        Thread.Sleep(1000);
                        
                    }
                }
                if (addedImages.Count > 0)
                {
                    NewImageEventArgs args = new NewImageEventArgs(addedImages);
                    OnNewImageAdded(args);
                }

                //Process added images

                oldImages = newImages;
                Thread.Sleep(2000);
                
            }

        }

        protected virtual void OnNewImageAdded(NewImageEventArgs e)
        {
            NewImageEventHandler handler = NewImage;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event NewImageEventHandler NewImage;

    }
    public class NewImageEventArgs : EventArgs
    {
        private List<string> imagePaths;

        public NewImageEventArgs(List<string> imagePaths)
        {
            this.imagePaths = imagePaths;
        }

        public List<string> GetPaths
        {
            get { return imagePaths; }
        }
    }

    public delegate void NewImageEventHandler(Object sender, NewImageEventArgs e);
}
