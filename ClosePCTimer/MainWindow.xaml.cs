using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Serialization;

namespace ClosePCTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string save;
        string command;
        string path = "save.txt";

        public MainWindow()
        {
            InitializeComponent();

            OnStart();
        }

        public int StartProcess(ProcessStartInfo psi)
        {
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();

            return p.ExitCode;
        }

        private void LeftTimeMessage(string time)
        {
            TimeSpan timeSpan = DateTime.Parse(time).Subtract(DateTime.Now);
            switch (command)
            {
                case "s":
                    MessageBox.Show(time + "\nwill be closed" +
                        $"\n({timeSpan.TotalMinutes.ToString()}min left)");
                    break;
                case "r":
                    MessageBox.Show(time + "\nwill be restart" +
                        $"\n({timeSpan.TotalMinutes.ToString()}min left)");
                    break;
            }
        }

        public void OnStart()
        {
            string[] lines = File.ReadAllLines(path);

            if (lines != null && lines.Length != 0)
            {
                save = lines[0];
                command = lines[1];

                if (DateTime.Now > DateTime.Parse(save))
                {
                    File.WriteAllText(path, "");
                    save = "";
                    command = "";
                }
                else
                    LeftTimeMessage(save);
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(txtMin.Text))
            {
                try
                {
                    int sec = int.Parse(txtMin.Text.ToString()) * 60;

                    var psi = new ProcessStartInfo("shutdown", $"/{command} /t {sec}");
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;

                    int exitCode = StartProcess(psi);

                    if (exitCode == 1190)
                        MessageBox.Show("code : 1190\nyou have to cancel previous shutdown");
                    else if (exitCode == 0)
                    {
                        DateTime currTime = DateTime.Now.AddSeconds(sec);
                        File.WriteAllText(path, currTime.ToString() +
                            "\n" + command);

                        LeftTimeMessage(currTime.ToString());
                    }
                    else
                        MessageBox.Show("exit code: " + exitCode);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
                MessageBox.Show("Please enter a number");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo("shutdown", "-a");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            int exitCode = StartProcess(psi);

            if (exitCode == 1116)
                MessageBox.Show("code : 1116\nyou dont have any current shutdown timer set");
            else if (exitCode == 0)
            {
                File.WriteAllText(path, "");
                MessageBox.Show("canceled");
            }
            else
                MessageBox.Show("exit code: " + exitCode);

        }

        private void BtnTime_Click(object sender, RoutedEventArgs e)
        {
            string[] timeLeft = File.ReadAllLines(path);

            if (timeLeft != null && timeLeft.Length != 0)
                LeftTimeMessage(timeLeft[0]);

            else
                MessageBox.Show("Save File is Empty");
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
            if (regex.IsMatch(e.Text))
                MessageBox.Show("Please use only numbers");
        }

        private void RbShutDown_Checked(object sender, RoutedEventArgs e)
        {
            command = "s";
        }

        private void RbRestart_Checked(object sender, RoutedEventArgs e)
        {
            command = "r";
        }
    }
}
