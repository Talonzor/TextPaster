using System;

using System.Windows.Forms;

using System.IO;

using Shortcut;
using System.Text.RegularExpressions;

/*
 * TODO:
 * Generate Report from Copy as a feature.
 * - Show all Errors/Warnings
 * - Connected Ports
 * - Loaded SWFS
 * [ X ] Missing Movieclips
 * - Client Version
 * - Server Version
 * 
 * Pick Path so Backup File/Log
 * - Set your own File name Syntax (1pt)
 * - 
 * 
 * Bind Multiple Keys to Features
 * - Special Key for Log analisys
 *      - Could be with "Detection" and suggest conversion with Popup
 *  
 * 
 */
namespace TextPasteSTuff
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _hotkeyBinder.Bind(Modifiers.Control | Modifiers.Shift, Keys.V).To(HotkeyCallback);
        }
        // Declare and instantiate the HotkeyBinder.
        private readonly HotkeyBinder _hotkeyBinder = new HotkeyBinder();
        /* Define all Global Vars needed for the Callback from nowhere */
        public static NotifyIcon g_notify;
        public static string lastFile = "";
        public static string baseFilePath = Application.StartupPath;
        public static TextBox g_debugBox;
        public static CheckBox g_analysisCheckbox;
        public static CheckBox g_checkBox1;
        public static String networkPath = @"\\gamepoint.local\nlhag\Software\TextPaster\";



        // Declare the callback that you would like Shortcut to invoke when 
        // the specified system-wide hotkey is pressed.
        private static void HotkeyCallback()
        {
            if (Form1.checkClipboard())
            {
                String totalText = "";
                if (g_analysisCheckbox.Checked)
                {
                    if (g_checkBox1.Checked)
                    {
                        /* Include Rich HTML debug */
                        g_debugBox.AppendText(Environment.NewLine + "-- Making HTML report! --" + Environment.NewLine);
                        //Console.WriteLine(Clipboard.GetText());
                        totalText += Form1.createHTMLLogReport(Clipboard.GetText());
                        Form1.convertClipboard(totalText, true);
                        g_notify.ShowBalloonTip(20000, "Converted to File!", "Click here to Open backup file path. (It was also added to your clipboard)", ToolTipIcon.Info);
                    } else {
                        /* Make Plaintext Debug */
                        g_debugBox.AppendText(Environment.NewLine);
                        g_debugBox.AppendText("Including a Report");
                        totalText += Form1.createLogReport(Clipboard.GetText());
                        totalText += Clipboard.GetText();
                        Form1.convertClipboard(totalText, false);
                        g_notify.ShowBalloonTip(20000, "Converted to File!", "Click here to Open backup file path. (It was also added to your clipboard)", ToolTipIcon.Info);
                    }
                }
            }
        }

        private static String listLoadedResources(string clipboardText)
        {
            String missingMcPattern = "add resource:(.*)[,\"]*";
            MatchCollection result = Regex.Matches(clipboardText, missingMcPattern);
            String returnString = "----- Loaded Resources -----\r\n";
            //returnString += "Total Missing : [ " + result.Count = " ] ";
            bool found = false;
            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                // This will print the number of captured groups in this match
                returnString += data[1] + Environment.NewLine;
                found = true;
            }
            if (found)
            {
                return returnString;
            }
            else
            {
                return "--No Loaded Resources found -- " + Environment.NewLine;
            }
        }

        private static String findMissingMc(string clipboardText)
        {
            String missingMcPattern = "ImageLoader: Resource with such name was not found:(.*)[,\"\n]?[\\n]+";
            MatchCollection result = Regex.Matches(clipboardText, missingMcPattern);
            String returnString = "----- Missing Resources -----\r\n";
            //returnString += "Total Missing : [ " + result.Count = " ] ";
            bool found = false;
            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                returnString += data[1] + Environment.NewLine; ;
                found = true;
            }
            found = true;
            if (found) {
                return returnString;
            } else {
                return Environment.NewLine + "-- No Missing Resources Found -- " + Environment.NewLine;
            }
        }

        private static String findVersions(string clipboardText)
        {
            String missingMcPattern = "Version of the (.*) (.*)";
            MatchCollection result = Regex.Matches(clipboardText, missingMcPattern);
            String returnString = Environment.NewLine + "----- Client & Server Versions -----" + Environment.NewLine;
            //returnString += "Total Missing : [ " + result.Count = " ] ";
            bool found = false;
            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                returnString += data[0] + Environment.NewLine;
                found = true;
            }
            found = true;
            if (found)
            {
                return returnString;
            }
            else
            {
                return "-- No versions found" + Environment.NewLine;
            }
        }

        private static String findExceptions(string clipboardText)
        {
            /*
            05.04.2017 17:09:37:801 [WARN] [002053952] [8732] [001] [0000] [Exception] S_SET_JACKPOT [id=405.0,value=21000] in state 'Game Queue' <Error: IllegalArgumentException>
            Error: IllegalArgumentException
	            at com.gamepoint.client.state::State/processMsg()
	            at com.gamepoint.client.state::BasicStateCommon/processMsg()
	            at com.gamepoint.client.state::State/processMsg()
	            at com.gamepoint.client.state::BasicStateGameQueue/processMsg()
	            at com.gamepoint.client::ClientListener/processMessage()
	            at com.gamepoint.client::ClientListener/run()
	            at com.gamepoint.client::Client/onEnterFrame()
            */
            String pattern = "\\[Exception\\](.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)";
            MatchCollection result = Regex.Matches(clipboardText, pattern);
            String returnString = "----- Exceptions -----\r\n";
            //returnString += "Total Missing : [ " + result.Count = " ] ";
            bool found = false;
            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                returnString += data[0] + Environment.NewLine;
                found = true;
            }
            String pattern2 = "Uncaught Error(.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)\\n(.*)";
            MatchCollection result2 = Regex.Matches(clipboardText, pattern2);
            String returnString2 = "----- Exceptions -----\r\n";
            //returnString += "Total Missing : [ " + result.Count = " ] ";

            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                returnString += data[0] + Environment.NewLine;
                found = true;
            }
            if (found)
            {
                return returnString + returnString2;
            }
            else
            {
                return Environment.NewLine + "-- NO Exceptions Found --" + Environment.NewLine;
            }
        }

        private static string createLogReport(string clipboardText)
        {

            Console.WriteLine("Creating a Log Report");
            String totalReport = "------ REPORT ------" + Environment.NewLine;
            
            String missingMcResult = findMissingMc(clipboardText);
            String loadedResourceResult  = listLoadedResources(clipboardText);
            String missingParams = listMissingParams(clipboardText);
            String foundExceptions = findExceptions(clipboardText);
            String versions = findVersions(clipboardText);
            String joinedPorts = listJoinedPorts(clipboardText);
            String ingamecom = findIngamecom(clipboardText);

            totalReport += joinedPorts + versions + missingParams + missingMcResult + loadedResourceResult + foundExceptions + ingamecom;
            totalReport = totalReport.Replace("\r", "");
            totalReport += Environment.NewLine + "---- END REPORT ##P-Man##" + Environment.NewLine;
            return totalReport;

        }

        private static string createHTMLLogReport(string clipboardText)
        {

            Console.WriteLine("Creating HTML Log Report");
            String template = File.ReadAllText("report_template.html");

            String missingMcResult = findMissingMc(clipboardText);
            String loadedResourceResult = listLoadedResources(clipboardText);
            String missingParams = listMissingParams(clipboardText);
            String foundExceptions = findExceptions(clipboardText);
            String versions = findVersions(clipboardText);
            String joinedPorts = listJoinedPorts(clipboardText);
            String ingamecom = findIngamecom(clipboardText);

            template = template.Replace("%errorData%", foundExceptions.Replace(Environment.NewLine, "<br>"));
            template = template.Replace("%loadedResources%", loadedResourceResult.Replace(Environment.NewLine, "<br>"));
            template = template.Replace("%misc%", ingamecom.Replace(Environment.NewLine, "<br>") + joinedPorts.Replace(Environment.NewLine, "<br>") + versions.Replace(Environment.NewLine, "<br>") + missingMcResult.Replace(Environment.NewLine, "<br>"));
            
            return template;

        }

        private static String findIngamecom(string clipboardText)
        {
            /*
             * "27.10.2017 16:08:42:511 [DEBUG] AGetClient https://www.gamepoint.com/scripts/gameserver/ingamecom.php?params=PAYiHw9WSwmISt2uGArzb%2BDwX%2FTdpiLMbJnDShO4irunnaazx8UMn%2B1VLzfJLtO8mcbuGnY2fBaIOwerzE0frFwIyzJHUHWNJDSylcfxsXNr6EA7fJjUBJAbAoiSZdzpIXhzpc8Y90GtQDgwplURGKWOODa7sUDW7vhTuUg7Nht3eskiigODJjXb0R9%2FKTXuIulJqhTNKgXK8SJsyuQOWhOuBtV9yKpcmejP%2Fp7Ql4z125AjqhPdZFjyWwaVsHJDvZrj2DjAVuJU1fYcmNlUgV5KKqKAPi7bQeAcY6J5QY0PGCYJvbDUOMBFmQJFo6AZtkguNNLuBOorAtrFjBc4lUS1upZvwpwjNiHblAhX5C4LxKFPBLg9M%2F30jO%2FLoHGGFx3t59mLFOrSqGa2okfz6G0B9JXdIQ3nfhfVS3Yy0jXbDHDGKG2OKw%2BqnkTv7w9qlsvHwcxtb7VR3hLMxzj%2Blc3SEn%2BehGqTDmW1SJ%2BIOZWgTfWTMdoE2x4DBtn8WUsRgU%2FwrtPxV7jfMuu%2Bqo5hK9bOI1kKtt5eD3eUjUze8%2BQtcFi8TSRBKhbUnWX1KvFFf0amlrxF9emgnOtAOcOOkfMYR1LFkInBvWJJbhyVOAzNwmfi6RPPevhi%2BuddYpRt5uJfUCTaI%2Bfl4zBDlTGyMsyg0fxCJi5R",
             */

            String pattern = "\\[DEBUG\\] AGetClient (.*)";
            MatchCollection result = Regex.Matches(clipboardText, pattern);
            String returnString = "----- Ingamecom calls -----" + Environment.NewLine;
            //returnString += "Total Missing : [ " + result.Count = " ] ";
            bool found = false;
            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                returnString += data[0] + Environment.NewLine;
                found = true;
            }
            if (found)
            {
                return returnString;
            }
            else
            {
                return Environment.NewLine + "-- NO Ingamecom Found --" + Environment.NewLine;
            }
        }

        private static string listJoinedPorts(string Data)
        {
            //01.09.2017 12:02:45:257[INFO] run Dobbelfeest : 6435
            String pattern = "run (.*) : ([1-9]{4})";
            MatchCollection result = Regex.Matches(Data, pattern);
            String returnString = "----- Joined Room(s) -----" + Environment.NewLine;
            //returnString += "Total Missing : [ " + result.Count = " ] ";
            bool found = false;
            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                returnString += "Initial Port : " + data[0] + Environment.NewLine;
                found = true;
            }
            if (found)
            {
                return returnString;
            }
            else
            {
                return "-- No Initial ports found" + Environment.NewLine;
            }

        }

        private static string listMissingParams(string Data)
        {
            /* "25.09.2017 11:53:59:614 [DEBUG] Param DISCONNECT_DURING_LOADING@common-nl not found. Default value: false" */
            String pattern = "\\[DEBUG\\] Param (.*) not found\\. Default value\\: (.*)";
            MatchCollection result = Regex.Matches(Data, pattern);
            String returnString = "----- Missing params -----" + Environment.NewLine;
            //returnString += "Total Missing : [ " + result.Count = " ] ";
            bool found = false;
            foreach (Match match in result)
            {
                GroupCollection data = match.Groups;
                returnString += "" + data[0] + Environment.NewLine;
                found = true;
            }
            if (found)
            {
                return returnString;
            }
            else
            {
                return "-- No missing params found" + Environment.NewLine;
            }

        }

        private static bool appendClipboard(string Data)
        {
            Data.Trim(Environment.NewLine.ToCharArray());
            string clipboardData = Clipboard.GetText();
            Clipboard.SetText(clipboardData + Data);
            return true;
        }

        private static bool convertClipboard(string clipboardData, Boolean html)
        {
            Console.WriteLine("Converting Clipboard!");
            DataObject m_data = new DataObject();
            m_data.SetText(clipboardData, TextDataFormat.Text);
            String filePath = "";
            if (html == false)
            {
                filePath = baseFilePath + "\\File - " + DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss") + ".txt";
            } else
            {
                filePath = baseFilePath + "\\File - " + DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss") + ".html";
            }
            File.WriteAllText(filePath, m_data.GetText());
            System.Collections.Specialized.StringCollection a = new System.Collections.Specialized.StringCollection();
            a.Add(filePath);
            Clipboard.SetFileDropList(a);
            lastFile = filePath;
            g_debugBox.AppendText(Environment.NewLine);
            g_debugBox.AppendText("Converted Text to File : " + Environment.NewLine + "-------------------" + Environment.NewLine + filePath + Environment.NewLine + "-------------------" + Environment.NewLine);
            return true;
        }

        private static bool checkClipboard()
        {
            string clipboardData = Clipboard.GetText();
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                g_debugBox.AppendText(Environment.NewLine + "Text Detected" + Environment.NewLine);
                return true;
            } else
            {
                g_debugBox.AppendText(Environment.NewLine + "Source is not Text" + Environment.NewLine);
                return false;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            HotkeyCallback();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Add it to global scope, i think...
            g_analysisCheckbox = this.analyseCheckbox;
            g_checkBox1 = this.checkBox1;
            g_debugBox = this.debugBox;
            g_notify = this.notifyIcon1;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                this.ShowInTaskbar = false;
                notifyIcon1.ShowBalloonTip(20000, "Minimized but still active", 
                    "Press Ctrl+Shift+V To Convert text to a file. After that Paste with Ctrl-V (or as you normally would)",
                    ToolTipIcon.Info);

            }
        }

        private bool addToStartup()
        {
            try
            {
                //System.IO.File.Copy(Application.ExecutablePath, Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "TextPaster.lnk");
                Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\TextPaster.lnk");
                Console.WriteLine(Application.ExecutablePath);
                return true;
            }
            catch {
                Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                g_debugBox.AppendText(Environment.NewLine + "Could not add to Startup Folder" + Environment.NewLine);
                return false;
            }
        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            bool worked = this.addToStartup();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.openLastFile();
        }

        private void openLastFile()
        {
            if(lastFile == "")
            {
                lastFile = baseFilePath;
            }
            /* Opens Explorer with your Backup File selected */
            Console.WriteLine(lastFile);
            String args = "/select, \"" + lastFile + "\"";
            System.Diagnostics.Process.Start("explorer.exe", args);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.openLastFile();
        }

        private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            this.openLastFile();
        }
        /*
         * Method used to Copy-Lang files; 
         *
        */
        private void button4_Click(object sender, EventArgs e)
        {
            string[] langListNon = new String[] { "en", "fr", "de", "it", "nl" };
            string[] langListCopy = new String[] { "no", "sv", "da", "ru", "pl", "tr" };
            //string leadLang = "en";

            OpenFileDialog theDialog = new OpenFileDialog();
            /* Set last opened Directory */
            theDialog.RestoreDirectory = true;

            theDialog.Title = "Open SWF File";
            theDialog.Filter = "SWF Files|*.swf";
            //theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = theDialog.SafeFileName;
                string fullPath = theDialog.FileName;
                Console.WriteLine(fileName);

                String pattern = "(.*)(_en_)(.*)";
                MatchCollection result = Regex.Matches(fileName, pattern);
                bool found = false;
                foreach (Match match in result)
                {
                    foreach (String lang in langListCopy)
                    {
                        string copyFilePath = Path.GetDirectoryName(fullPath) + "/" + match.Groups[1] + "_" + lang.ToString() + "_" + match.Groups[3];
                        Console.WriteLine("Writing to : " + copyFilePath);
                        File.Copy(fullPath, copyFilePath, true);
                    }
                    found = true;
                }
                if (found == false)
                {
                    g_debugBox.AppendText(Environment.NewLine + "Selected File Does not contain _en_");
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
