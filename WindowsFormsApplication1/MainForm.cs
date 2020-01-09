using System;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;





namespace SlimCSV
{




    public partial class Form1 : Form
    {


        //this is what runs at initialization
        public Form1()
        {

            InitializeComponent();


            foreach (var encoding in Encoding.GetEncodings())
            {
                EncodingDropdown.Items.Add(encoding.Name);
            }

            try
            {
                EncodingDropdown.SelectedIndex = EncodingDropdown.FindStringExact("utf-8");
            }
            catch
            {
                EncodingDropdown.SelectedIndex = EncodingDropdown.FindStringExact(Encoding.Default.BodyName);
            }

            
            


            EnclosedInQuotesDropdown.SelectedIndex = 0;
            HeaderRowDropdown.SelectedIndex = 0;
         

        }






//   _____ _ _      _       _____ _             _     ____        _   _              
//  / ____| (_)    | |     / ____| |           | |   |  _ \      | | | |             
// | |    | |_  ___| | __ | (___ | |_ __ _ _ __| |_  | |_) |_   _| |_| |_ ___  _ __  
// | |    | | |/ __| |/ /  \___ \| __/ _` | '__| __| |  _ <| | | | __| __/ _ \| '_ \ 
// | |____| | | (__|   <   ____) | || (_| | |  | |_  | |_) | |_| | |_| || (_) | | | |
//  \_____|_|_|\___|_|\_\ |_____/ \__\__,_|_|   \__| |____/ \__,_|\__|\__\___/|_| |_|
                                                                              
                                                                                   




        private void StartButton_Click(object sender, EventArgs e)
        {



            if (BgWorker.IsBusy)
            {
                BgWorker.CancelAsync();
                return;
            }


            int number_of_columns = ColumnNameCheckedListbox.CheckedIndices.Count;

            if (number_of_columns < 1)
            {
                MessageBox.Show("You must choose at least one column to keep.", "No columns selected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            

                //validate the subfolder numbers



                this.Enabled = false;

            
                FolderBrowser.Description = "Please choose the OUTPUT location for your files";

                FolderBrowser.SelectedPath = Path.GetDirectoryName(openFileDialog.FileName);


                if (FolderBrowser.ShowDialog() != DialogResult.Cancel)
                {


                    BgWorkerInformation BgData = new BgWorkerInformation();

                    BgData.InputFile = FilenameDisplayBox.Text;
                    BgData.OutputLocation = FolderBrowser.SelectedPath.ToString();
                    BgData.HasHeaders = HeaderRowDropdown.SelectedItem.ToString();
                    BgData.Delimiters = DelimiterTextBox.Text.ToString();
                    BgData.UsingQuotes = EnclosedInQuotesDropdown.SelectedItem.ToString();

                    List<int> CheckedIndices = new List<int>();
                    foreach (Object item in ColumnNameCheckedListbox.CheckedItems)
                        CheckedIndices.Add(ColumnNameCheckedListbox.Items.IndexOf(item));

                    BgData.KeepCols = CheckedIndices.ToArray();
                    BgData.NumberOfColumns = BgData.KeepCols.Length;

                        

                

                    
                        
                    DisableButtons();

                    StartButton.Text = "Cancel";

                    BgWorker.RunWorkerAsync(BgData);

                }

                

            

            this.Enabled = true;
           
        }





        //  _                     _   _____        _          ____        _   _              
        // | |                   | | |  __ \      | |        |  _ \      | | | |             
        // | |     ___   __ _  __| | | |  | | __ _| |_ __ _  | |_) |_   _| |_| |_ ___  _ __  
        // | |    / _ \ / _` |/ _` | | |  | |/ _` | __/ _` | |  _ <| | | | __| __/ _ \| '_ \ 
        // | |___| (_) | (_| | (_| | | |__| | (_| | || (_| | | |_) | |_| | |_| || (_) | | | |
        // |______\___/ \__,_|\__,_| |_____/ \__,_|\__\__,_| |____/ \__,_|\__|\__\___/|_| |_|
        //                                                                                   


        private void GeneratePreviewButton_Click(object sender, EventArgs e)
        {

            ColumnNameCheckedListbox.Items.Clear();

            FilenameDisplayBox.Text = "No file selected...";

            FilenameLabel.Text = "Clearing old preview... (This might take a while for previews with a large number of columns.)";
            FilenameLabel.Invalidate();
            FilenameLabel.Update();
            FilenameLabel.Refresh();

            dataGridView1.DataSource = null;
            FilenameLabel.Text = "Ready to load a data file preview.";

            openFileDialog.Title = "Please select you data file...";

                DialogResult InputFileDialog = openFileDialog.ShowDialog();

                if (InputFileDialog != DialogResult.Cancel)
                {

                    DisableButtons();
                    string InputFile = openFileDialog.FileName;

                    FilenameDisplayBox.Text = InputFile;

                    FilenameDisplayBox.Focus();
                    // Move the caret to the end of the text box
                    FilenameDisplayBox.Select(FilenameDisplayBox.Text.Length, 0);


                BgWorkerInformation BgData = new BgWorkerInformation();

                    BgData.InputFile = FilenameDisplayBox.Text;
                    BgData.HasHeaders = HeaderRowDropdown.SelectedItem.ToString();
                    BgData.Delimiters = DelimiterTextBox.Text.ToString();
                    BgData.UsingQuotes = EnclosedInQuotesDropdown.SelectedItem.ToString();

                    LoadCSVPreview_BGWorker.RunWorkerAsync(BgData);

                    

                }
            else
            {
                FilenameDisplayBox.Text = "No file selected...";
                StartButton.Enabled = false;
                ReloadCSVButton.Enabled = false;
            }
        }





        //  _____      _                 _   _____        _          ______ _ _      
        // |  __ \    | |               | | |  __ \      | |        |  ____(_) |     
        // | |__) |___| | ___   __ _  __| | | |  | | __ _| |_ __ _  | |__   _| | ___ 
        // |  _  // _ \ |/ _ \ / _` |/ _` | | |  | |/ _` | __/ _` | |  __| | | |/ _ \
        // | | \ \  __/ | (_) | (_| | (_| | | |__| | (_| | || (_| | | |    | | |  __/
        // |_|  \_\___|_|\___/ \__,_|\__,_| |_____/ \__,_|\__\__,_| |_|    |_|_|\___|




        private void ReloadCSVButton_Click(object sender, EventArgs e)
        {

            ColumnNameCheckedListbox.Items.Clear();

            FilenameLabel.Text = "Clearing old preview... (This might take a while for previews with a large number of columns.)";
            FilenameLabel.Invalidate();
            FilenameLabel.Update();
            FilenameLabel.Refresh();

            dataGridView1.DataSource = null;
            FilenameLabel.Text = "Ready to load a data file preview.";

            if (FilenameDisplayBox.Text != "No file selected...")
            {

                DisableButtons();
                BgWorkerInformation BgData = new BgWorkerInformation();

                BgData.InputFile = FilenameDisplayBox.Text;
                BgData.HasHeaders = HeaderRowDropdown.SelectedItem.ToString();
                BgData.Delimiters = DelimiterTextBox.Text.ToString();
                BgData.UsingQuotes = EnclosedInQuotesDropdown.SelectedItem.ToString();

                LoadCSVPreview_BGWorker.RunWorkerAsync(BgData);
            }
        }







        //   _____                           _         _____                _               
        //  / ____|                         | |       |  __ \              (_)              
        // | |  __  ___ _ __   ___ _ __ __ _| |_ ___  | |__) | __ _____   ___  _____      __
        // | | |_ |/ _ \ '_ \ / _ \ '__/ _` | __/ _ \ |  ___/ '__/ _ \ \ / / |/ _ \ \ /\ / /
        // | |__| |  __/ | | |  __/ | | (_| | ||  __/ | |   | | |  __/\ V /| |  __/\ V  V / 
        //  \_____|\___|_| |_|\___|_|  \__,_|\__\___| |_|   |_|  \___| \_/ |_|\___| \_/\_/  
        //                                                                                                 






        private void LoadCSVPreview_BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {


            //here, we're basically unpacking and redefining all of the core information that was
            //passed to the background worker. it's a bit redundant and not super efficient, but the
            //loss of efficiency is more than made up for by the gains in readability

            BgWorkerInformation BgData = (BgWorkerInformation) e.Argument;

            Encoding SelectedEncoding = null;

            string InputFile = BgData.InputFile;
            bool HasHeaders = Convert.ToBoolean(BgData.HasHeaders);
            string[] Delimiters = BgData.Delimiters.ToCharArray().Select(c => c.ToString()).ToArray(); ;
            bool UsingQuotes = Convert.ToBoolean(BgData.UsingQuotes);




            this.Invoke((MethodInvoker)delegate ()
            {
                SelectedEncoding = Encoding.GetEncoding(EncodingDropdown.SelectedItem.ToString());
            });


            // a data table we'll use to hold the parsed data
            DataTable dt = new DataTable();


            try
            {
                // create the parser
                using (TextFieldParser parser = new TextFieldParser(InputFile, SelectedEncoding))
                {
                    // set the parser variables
                    parser.TrimWhiteSpace = true;
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(Delimiters);
                    parser.HasFieldsEnclosedInQuotes = UsingQuotes;

                    int LineNumber = 0;

                    bool firstLine = true;

                    //report what we're working on
                    FilenameLabel.Invoke((MethodInvoker)delegate
                    {
                        FilenameLabel.Text = "Preparing to read data file for preview...";
                    });




                    while (!parser.EndOfData)
                    {

                        

                        //report what we're working on
                        FilenameLabel.Invoke((MethodInvoker)delegate
                        {
                            FilenameLabel.Text = "Loading data file for preview... Data Row #" + LineNumber.ToString();
                        });


                        //Processing row
                        string[] fields = parser.ReadFields();

                        LineNumber++;

                        // get the column headers
                        if (firstLine)
                        {

                            firstLine = false;

                            if (HasHeaders)
                            {
                                foreach (var val in fields)
                                {
                                    dt.Columns.Add(val);
                                }
                                LineNumber--;
                                continue;
                            }
                            else
                            {
                                for (int i = 1; i <= fields.Length; i++)
                                {
                                    dt.Columns.Add("v" + i.ToString());
                                }

                            }
                        }


                        // get the row data
                        dt.Rows.Add(fields);

                        if (LineNumber > 999)
                        {
                            break;
                        }

                    }

                }

                e.Result = dt;

                if (dt.Columns.Count < 1 || dt.Rows.Count < 1)
                {
                    MessageBox.Show("Your spreadsheet file could not be properly parsed" + "\r\n" +
                                "with the current settings. SlimCSV could not find any" + "\r\n" +
                               "distinct columns and/or rows in your data file. This is" + "\r\n" +
                               "most often caused by using the wrong delimiter(s).", "Data Parse Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            catch
            {
                //what to do if there's an error
                e.Result = false;
            }

        }

        private void LoadCSVPreview_BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            FilenameLabel.Text = "Please wait while preview is being generated... (This might take a while for files with a large number of columns.)";
            FilenameLabel.Invalidate();
            FilenameLabel.Update();
            FilenameLabel.Refresh();
            Application.DoEvents();

            //bind the results to the datagridview
            try { 
                dataGridView1.DataSource = e.Result;
                foreach (var column in dataGridView1.Columns) ((DataGridViewTextBoxColumn)column).MaxInputLength = 2147483647;
                EnableButtons();
                ReloadCSVButton.Enabled = true;
                StartButton.Enabled = true;
                foreach (DataGridViewColumn column in dataGridView1.Columns) ColumnNameCheckedListbox.Items.Add(column.Name);

                MessageBox.Show("Your data file preview has been loaded." + "\r\n\r\n" + 
                                "If your preview window appears to be empty, you most likely need to edit your settings under the \"Options for Reading Data File\" section.", "Preview Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch
            {

                ReloadCSVButton.Enabled = false;
                StartButton.Enabled = false;

                MessageBox.Show("Your spreadsheet file could not be properly parsed" + "\r\n" +
                                "with the current settings. Please make sure that the" + "\r\n" +
                               "file is not open elsewhere, check your settings, and" + "\r\n" + 
                               "try again.", "Data Parse Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            FilenameLabel.Text = "Finished creating dataset preview.";

        }













        // __          __   _ _          ____        _               _     ______ _ _           
        // \ \        / /  (_) |        / __ \      | |             | |   |  ____(_) |          
        //  \ \  /\  / / __ _| |_ ___  | |  | |_   _| |_ _ __  _   _| |_  | |__   _| | ___  ___ 
        //   \ \/  \/ / '__| | __/ _ \ | |  | | | | | __| '_ \| | | | __| |  __| | | |/ _ \/ __|
        //    \  /\  /| |  | | ||  __/ | |__| | |_| | |_| |_) | |_| | |_  | |    | | |  __/\__ \
        //     \/  \/ |_|  |_|\__\___|  \____/ \__,_|\__| .__/ \__,_|\__| |_|    |_|_|\___||___/
        //                                              | |                                     
        //                                              |_|                                     





        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            //here, we're basically unpacking and redefining all of the core information that was
            //passed to the background worker. it's a bit redundant and not super efficient, but the
            //loss of efficiency is more than made up for by the gains in readability

            BgWorkerInformation BgData = (BgWorkerInformation)e.Argument;

            Encoding SelectedEncoding = null;

            string InputFile = BgData.InputFile;
            bool HasHeaders = Convert.ToBoolean(BgData.HasHeaders);
            string[] Delimiters = BgData.Delimiters.ToCharArray().Select(c => c.ToString()).ToArray(); ;
            bool UsingQuotes = Convert.ToBoolean(BgData.UsingQuotes);

            bool DumpOutputAsTXT = false;
            



            this.Invoke((MethodInvoker)delegate ()
            {
                SelectedEncoding = Encoding.GetEncoding(EncodingDropdown.SelectedItem.ToString());
                DumpOutputAsTXT = DumpAsTextCheckbox.Checked;
            });

            string OutputFile = BgData.OutputLocation + Path.DirectorySeparatorChar + "_SLIM_" + Path.GetFileName(InputFile);
            if (DumpOutputAsTXT) OutputFile += ".txt";

            try { 

                // create the parser
                using (TextFieldParser parser = new TextFieldParser(InputFile, SelectedEncoding))
                {

                
                        // set the parser properties
                        parser.TrimWhiteSpace = true; //trim the whitespace to make sure that files/folder names don't end with a space, which will break the program
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(Delimiters);
                        parser.HasFieldsEnclosedInQuotes = UsingQuotes;


                        bool firstLine = true;
                        ulong LineNumber = 0;
                        ulong FileNumber = 0;
                        ulong LastFileNumberforFolderCreation = 0;
                        ulong FolderNumber = 0;

                        //report what we're working on
                        FilenameLabel.Invoke((MethodInvoker)delegate
                        {
                            FilenameLabel.Text = "Preparing to write output files...";
                        });




                    using (FileStream fileStream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, SelectedEncoding))
                    {


                        //Loop through each row of the dataset
                        while (!parser.EndOfData && !BgWorker.CancellationPending)
                        {

                            //parse out the row
                            string[] fields = parser.ReadFields();

                            LineNumber++;

                            //report what row we're working on
                            if (LineNumber % 10 == 0)
                            {
                                FilenameLabel.Invoke((MethodInvoker)delegate
                                    {
                                        FilenameLabel.Text = "Currently writing row #" + LineNumber.ToString();
                                    });
                            }





                            //prepare our output to write
                            string[] output_array = new string[BgData.NumberOfColumns];

                            for (int i = 0; i < BgData.NumberOfColumns; i++)

                            { 

                                if (UsingQuotes && DumpOutputAsTXT == false)
                                {
                                    output_array[i] = '"' + fields[BgData.KeepCols[i]].Replace("\"", "\"\"") + '"';
                                }
                                else
                                {
                                    output_array[i] = fields[BgData.KeepCols[i]];
                                }

                            }

                            if (DumpOutputAsTXT)
                            {
                                streamWriter.WriteLine(string.Join("\r\n", output_array));
                            }
                            else
                            {
                                streamWriter.WriteLine(string.Join(Delimiters[0], output_array));
                            }
                            


                            //write our output


                            if (e.Cancel)
                            {
                                break;
                            }



                        }

                    }

                }

                e.Result = null;

            }
            catch
            {
                MessageBox.Show("SlimCSV has encountered an error while processing your file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Result = "error";
            }



            


        }


        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StartButton.Text = "Start!";
            EnableButtons();
            if (e.Result == null)
            {
                FilenameLabel.Text = "Finished!  :)";
                MessageBox.Show("SlimCSV has finished writing your output.", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                FilenameLabel.Text = "Finished with an error  :(";
            }

        }




















        //borrowed from here:
        //https://stackoverflow.com/a/17546909
        private static DialogResult ShowInputDialog(ref string input, string DialogName)
        {
            System.Drawing.Size size = new System.Drawing.Size(300, 70);
            Form inputBox = new Form();

            inputBox.StartPosition = FormStartPosition.CenterParent;

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            inputBox.ClientSize = size;
            inputBox.Text = DialogName;

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }




        //validate regexes
        private static bool ValidateRegex(string RegexInBox)
        {

            bool IsRegexValid = true;

            try
            {
                Regex NewlineClean = new Regex(RegexInBox, RegexOptions.Compiled);
            }
            catch
            {
                IsRegexValid = false;
                MessageBox.Show("Your regular expression (RegEx) does not appear to be" + "\r\n" +
                                "a valid construction. Please review and revise your entry.", "RegEx Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            return IsRegexValid;
        }






        //this override makes sure that we don't get errors thrown for fillweight violations.
        //it also sets the basic column width, which is useful for when it only finds one column
        //and looks like nothing is there with default settings
        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.FillWeight = 1;
            e.Column.Width = 100;
        }


        public void DisableButtons()
        {
            LoadCSVButton.Enabled = false;
            ReloadCSVButton.Enabled = false;

            DelimiterTextBox.Enabled = false;
            EnclosedInQuotesDropdown.Enabled = false;
            HeaderRowDropdown.Enabled = false;
            EncodingDropdown.Enabled = false;

            DumpAsTextCheckbox.Enabled = false;


        }

        public void EnableButtons()
        {
            LoadCSVButton.Enabled = true;
            ReloadCSVButton.Enabled = true;
            
            DelimiterTextBox.Enabled = true;
            EnclosedInQuotesDropdown.Enabled = true;
            HeaderRowDropdown.Enabled = true;
            EncodingDropdown.Enabled = true;

            DumpAsTextCheckbox.Enabled = true;

        }




        public class BgWorkerInformation
        {
            public string InputFile { get; set; }
            public string OutputLocation { get; set; }
            public string HasHeaders { get; set; }
            public string Delimiters { get; set; }
            public string UsingQuotes { get; set; }

            

            public int[] KeepCols { get; set; }
            public int NumberOfColumns { get; set; }

        }



        private void SubfolderCountTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
           
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
                 
        }
    }












    

}
