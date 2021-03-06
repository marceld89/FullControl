﻿using System;
using System.Drawing;
using System.Windows.Forms;
using TBotCore;
using TBotCore.Database;

//Using for testingmethods
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace TBot {
    public partial class TBot : Form {
        private Twitch _twitch;
        private DB _dataBase;

        IrcInfo ircInfo = new IrcInfo();

        //TODO: Move to Twitch.cs etc...
        SpotifyCore spCore = new SpotifyCore();

        public TBot(IrcInfo _ircInfo, bool IsSpotifyEnabled)
        {
            this.ircInfo = _ircInfo;

            InitializeComponent();

#if true
            Initilize(IsSpotifyEnabled);
#else
            DoTest();
#endif
        }

        #region Testing methods
        private void DoTest()
        {
#if false
            string commandgotbychatline = "!currentsong";
            DataRow[] result = _commands._currentCustomCommandsDatatable.Select("Command = '" + commandgotbychatline + "'");
            if(result.Length == 1)
                txtChat.AppendLine(result[0][0] + " - " + result[0][1]);
#else
            //https://api.twitch.tv/kraken/users/mrrrrcl/follows/channels/hardlydifficult
            txtChat.Text = isFollowingMyStream("marceld89");
#endif
        }
        private string isFollowingMyStream(string username)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(@"https://api.twitch.tv/kraken/users/" + username + "/follows/channels/hardlydifficult");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Method = "GET";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var streamReader = new StreamReader(httpResponse.GetResponseStream());
                return streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                TBotCore.Log.Logging.Log(ex.Message, TBotCore.Log.Logging.Loglevel.Warning);
                TBotCore.Log.Logging.Log("User is not following the stream!", TBotCore.Log.Logging.Loglevel.Warning);
                return "User is not following the stream!";
            }
        }
        #endregion

        #region Custom methods
        #region Public methods
        //Write to Connect-tab
        public void LogToConnect(string message)
        {
            Invoke(new Action(() =>
            {
                txtLog.AppendLine(DateTime.Now.ToShortTimeString() + " " + message);
            }));
        }

        //Write to Chat-tab
        public void LogToChat(string nick, string message)
        {
            Invoke(new Action(() =>
            {
                txtChat.AppendText("[" + DateTime.Now.ToShortTimeString() + "] ");
                txtChat.SelectionFont = new Font(txtChat.Font, FontStyle.Bold);
                //Todo: Maybe add custom coloring to this
                if (nick == "BOT")
                {
                    txtChat.AppendText(nick + ": ", Color.Green);
                }
                else if (nick != ircInfo.Channel)
                {
                    txtChat.AppendText(nick + ": ", Color.Gray);
                }
                else {
                    txtChat.AppendText(nick + ": ", Color.Red);
                }
                txtChat.SelectionFont = new Font(txtChat.Font, FontStyle.Regular);
                txtChat.ForeColor = Color.Black;
                txtChat.AppendLine(message);
            }));
        }

        //Write to Command-tab
        public void LogToCommand(string nick, string message)
        {
            Invoke(new Action(() =>
            {
                txtCommand.AppendText("[" + DateTime.Now.ToShortTimeString() + "] ");
                txtCommand.SelectionFont = new Font(txtCommand.Font, FontStyle.Bold);
                //Todo: Maybe add custom coloring to this
                if (nick == "BOT")
                {
                    txtCommand.AppendText(nick + ": ", Color.Green);
                }
                else if (nick != ircInfo.Channel)
                {
                    txtCommand.AppendText(nick + ": ", Color.Gray);
                }
                else {
                    txtCommand.AppendText(nick + ": ", Color.Red);
                }
                txtCommand.SelectionFont = new Font(txtCommand.Font, FontStyle.Regular);
                txtCommand.ForeColor = Color.Black;
                txtCommand.AppendLine(message);
            }));
        }

        //Write to logfile
        public void LogToLog(bool enabled, string Path, string message)
        {
            if (enabled)
                System.IO.File.AppendAllText(Path, DateTime.Now.ToShortTimeString() + " " + message + "\r\n");
        }

        //Write to formtitle
        public void UpdateFormText(string text)
        {
            Invoke(new Action(() =>
            {
                this.Text = text;
            }));
        }
        #endregion

        #region Private methods
        private void Initilize(bool SpotifyEnabled)
        {
            _dataBase = new DB();

            this.txtSongrequest.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            enableLogToolStripMenuItem.Checked = _dataBase.getLogEnabled();
            enableSpotifyAutosongchangeToolStripMenuItem.Checked = _dataBase.getSpotifyAutoSongChangeEnabled();
            _twitch = new Twitch(this, ircInfo, _dataBase, SpotifyEnabled);
        }
        #endregion
        #endregion

        #region Form and Control actions
        //Form events
        private void SpotiBoti_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            try
            {
                if (!_twitch.ircClient.IrcCloseConnection())
                {
                    _twitch.twitchThread.Abort();
                    _twitch.spotifyThread.Abort();
                }
            }
            catch (Exception)
            {
            }
        }

        //ToolStrip events
        private void customCommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new CustomCommands().ShowDialog();
        }
        private void genericCommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new GenericCommands().ShowDialog();
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void enableLogToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _dataBase.setLog(enableLogToolStripMenuItem.Checked);
        }
        private void enableLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (enableLogToolStripMenuItem.Checked)
            {
                enableLogToolStripMenuItem.Checked = false;
                enableLogToolStripMenuItem.CheckState = CheckState.Unchecked;
                _dataBase.setLog(enableLogToolStripMenuItem.Checked);
            }
            else {
                enableLogToolStripMenuItem.Checked = true;
                enableLogToolStripMenuItem.CheckState = CheckState.Checked;
                _dataBase.setLog(enableLogToolStripMenuItem.Checked);
            }
        }
        private void enableSpotifyAutosongchangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (enableSpotifyAutosongchangeToolStripMenuItem.Checked)
            {
                enableSpotifyAutosongchangeToolStripMenuItem.Checked = false;
                enableSpotifyAutosongchangeToolStripMenuItem.CheckState = CheckState.Unchecked;
                _dataBase.setSpotifyAutoSongChange(enableSpotifyAutosongchangeToolStripMenuItem.Checked);
            }
            else {
                enableSpotifyAutosongchangeToolStripMenuItem.Checked = true;
                enableSpotifyAutosongchangeToolStripMenuItem.CheckState = CheckState.Checked;
                _dataBase.setSpotifyAutoSongChange(enableSpotifyAutosongchangeToolStripMenuItem.Checked);
            }
        }
        private void enableSpotifyAutosongchangeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            _dataBase.setSpotifyAutoSongChange(enableSpotifyAutosongchangeToolStripMenuItem.Checked);
        }

        //TabControl events
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                UpdateFormText("SpotiBoti");
            }
        }

        #endregion

        private void txtSongrequest_DrawItem(object sender, DrawItemEventArgs e) {

            SongrequestListBoxItem item = txtSongrequest.Items[e.Index] as SongrequestListBoxItem;
            if(item != null) {
                e.DrawBackground();
                Graphics g = e.Graphics;
                g.FillRectangle(new SolidBrush(Color.Gray), e.Bounds);
                g.DrawString(
                    item.Message,
                    new Font(txtSongrequest.Font, FontStyle.Regular),
                    new SolidBrush(item.ItemColor),
                    0,
                    e.Index * txtSongrequest.ItemHeight);
                
                e.DrawFocusRectangle();

            } else {

            }
        }
    }
}