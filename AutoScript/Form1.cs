using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace AutoScript
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenProgram("OnStart_Id");
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Hide();
        }

        private void OpenProgram(string handler)
        {
            try
            {
                string xmlFile = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}{ConfigurationManager.AppSettings["xmlPath"].ToString()}";
                DataSet ds = new();
                ds.ReadXml(xmlFile);
                foreach (DataRow program in ds.Tables[1].Select(handler + " >= 0"))
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(program?["waitFor"].ToString()))
                        {
                            while (!Process.GetProcessesByName(program?["waitFor"].ToString()).Any())
                            {
                                Thread.Sleep(5000);
                            }
                        }
                        if (!string.IsNullOrEmpty(program?["useCmd"].ToString()) || (!string.IsNullOrEmpty(program?["name"].ToString()) && !Process.GetProcessesByName(program?["name"].ToString()).Where(c => !string.IsNullOrEmpty(c.MainWindowTitle)).Any()))
                        {
                            ProcessStartInfo info = new(program?["fileName"].ToString())
                            {
                                WindowStyle = ProcessWindowStyle.Minimized
                            };
                            if(!string.IsNullOrEmpty(program?["useCmd"].ToString()))
                            {
                                info.FileName = "cmd.exe";
                                info.Arguments = program?["fileName"].ToString() + " ; exit";
                                info.RedirectStandardOutput = true;
                                info.CreateNoWindow = true;
                                info.WorkingDirectory = @"C:\";
                            }
                            Process process = Process.Start(info);
                            if (ds.Tables[1].Columns.Contains("handleExit") && !string.IsNullOrEmpty(program?["handleExit"].ToString()))
                            {
                                process.EnableRaisingEvents = true;
                                process.Exited += new EventHandler(ClosingProgram);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "AutoScript", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "AutoScript", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClosingProgram(object sender, EventArgs e)
        {
            OpenProgram("OnStop_Id");
        }
    }
}
