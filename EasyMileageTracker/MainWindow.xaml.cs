﻿using System;
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
using Xceed.Words.NET;
using System.Drawing;
using System.Diagnostics;

namespace EasyMileageTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MileageLogContext db = new MileageLogContext();
        private MileageLog logEntryToEdit;
        public MainWindow()
        {
            InitializeComponent();
            Refresh();
        }

        /*
         * Method Name: Refresh
         * Description: 
         * Parameters: none
         * Returns: none
         */
        private void Refresh()
        {
            // Create new mileage log entry list object
            List<MileageLog> clientList = new List<MileageLog>();

            // Query database for all mileage log entries
            var query = from m in db.MileageLogs
                        orderby m.Date, m.StartTime
                        select m;

            // Loop through each item
            foreach (var item in query)
            {
                // Parse date, time
                item.Date = DateTime.Parse(item.Date).ToString("MMM d, yyyy");
                item.StartTime = DateTime.Parse(item.StartTime).ToString("t");
                clientList.Add(item);
            }

            // Refresh list
            dataGridMileageLogs.ItemsSource = clientList;
            dataGridMileageLogs.Items.Refresh();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            mdDialog1.IsOpen = false;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Get input data, parse date and time
            var date = DateTime.Parse(txtBoxDate.Text).ToString("yyyy-MM-dd");
            var time = DateTime.Parse(txtBoxTime.Text).ToString("HH:mm:ss");
            var startingPoint = txtBoxStartingPoint.Text;
            var destination = txtBoxDestination.Text;
            var purpose = "";
            if (btnBusiness.IsChecked == true)
            {
                purpose = "Business";
            } else if (btnPersonal.IsChecked == true)
            {
                purpose = "Personal";
            }
            var purposeDetails = txtBoxTripDetails.Text;
            var totalDistance = Convert.ToDouble(txtBoxTotalDistance.Text);

            // Create a new mileage log entry
            var mileageLog = new MileageLog { Date = date, StartTime = time, StartingPoint = startingPoint, Destination = destination, Purpose = purpose, PurposeDetails = purposeDetails, TotalDistance = totalDistance };
            db.MileageLogs.Add(mileageLog);
            db.SaveChanges();

            // Close modal window
            mdDialog1.IsOpen = false;

            // Refresh list
            Refresh();
        }


        /*
         * Method Name: btnEdit_Click
         * Description: 
         * Parameters: object sender, RoutedEventArgs e
         * Returns: none
         */
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)dataGridMileageLogs.ItemContainerGenerator.ContainerFromItem(dataGridMileageLogs.CurrentCell.Item);

            // selectedRow
            MileageLog selectedRow = (MileageLog)row.Item;

            var query = from m in db.MileageLogs
                        where m.Id == selectedRow.Id
                        select m;

            logEntryToEdit = query.FirstOrDefault();

            txtBoxEditDate.Text = DateTime.Parse(logEntryToEdit.Date).ToString("MMM d, yyyy");
            txtBoxEditTime.Text = DateTime.Parse(logEntryToEdit.StartTime).ToString("t");
            txtBoxEditStartingPoint.Text = logEntryToEdit.StartingPoint;
            txtBoxEditDestination.Text = logEntryToEdit.Destination;
            var purpose = logEntryToEdit.Purpose;
            if (purpose == "Business")
            {
                btnEditBusiness.IsChecked = true;
            }
            else if (purpose == "Personal")
            {
                btnEditPersonal.IsChecked = true;
            }
            txtBoxEditTripDetails.Text = logEntryToEdit.PurposeDetails;
            txtBoxEditTotalDistance.Text = logEntryToEdit.TotalDistance.ToString();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MileageLog selectedLog = (MileageLog)dataGridMileageLogs.SelectedItem;
            var query = from m in db.MileageLogs
                        where m.Id == selectedLog.Id
                        select m;

            var logToDelete = query.FirstOrDefault();
            db.MileageLogs.Remove(logToDelete);
            db.SaveChanges();

            // Refresh list
            Refresh();
        }


        /*
         * Method Name: btnSave_Click
         * Description: 
         * Parameters: object sender, RoutedEventArgs e
         * Returns: none
         */
        private void btnSaveEdit_Click(object sender, RoutedEventArgs e)
        {
            // Get input data, parse date and time
            var date = DateTime.Parse(txtBoxEditDate.Text).ToString("yyyy-MM-dd");
            var time = DateTime.Parse(txtBoxEditTime.Text).ToString("HH:mm:ss");
            var startingPoint = txtBoxEditStartingPoint.Text;
            var destination = txtBoxEditDestination.Text;
            var purposeDetails = txtBoxEditTripDetails.Text;
            var totalDistance = Convert.ToDouble(txtBoxEditTotalDistance.Text);

            logEntryToEdit.Date = date;
            logEntryToEdit.StartTime = time;
            logEntryToEdit.StartingPoint = startingPoint;
            logEntryToEdit.Destination = destination;
            var purpose = "";
            if (btnEditBusiness.IsChecked == true)
            {
                purpose = "Business";
            }
            else if (btnEditPersonal.IsChecked == true)
            {
                purpose = "Personal";
            }
            logEntryToEdit.Purpose = purpose;
            logEntryToEdit.PurposeDetails = purposeDetails;
            logEntryToEdit.TotalDistance = totalDistance;


            // Submit the changes to the database.
            try
            {
                db.SaveChanges();
                dialogEdit.IsOpen = false;
                Refresh();
            }
            catch (Exception ex)
            {
                // Provide for exceptions.
            }
        }


        /*
         * Method Name:
         * Description: 
         * Parameters: none
         * Returns: none
         */
        private void btnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            dialogEdit.IsOpen = false;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportWord();
        }

        private void ExportWord()
        {
            // Set file path
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string extension = ".docx";
            fileName += @"\example" + extension;

            // Create file
            var doc = DocX.Create(fileName);

            // Set the document margins
            doc.MarginLeft = 40f;
            doc.MarginRight = 40f;
            doc.MarginTop = 40;
            doc.MarginBottom = 40f;

            // Create Table with rows and columns  
            Xceed.Words.NET.Table t = doc.AddTable(dataGridMileageLogs.Items.Count + 1, 7);
            t.Alignment = Alignment.center;
            t.Design = TableDesign.TableGrid;

            // Set the header colours
            t.Rows[0].Cells[0].FillColor = System.Drawing.Color.LightGray;
            t.Rows[0].Cells[1].FillColor = System.Drawing.Color.LightGray;
            t.Rows[0].Cells[2].FillColor = System.Drawing.Color.LightGray;
            t.Rows[0].Cells[3].FillColor = System.Drawing.Color.LightGray;
            t.Rows[0].Cells[4].FillColor = System.Drawing.Color.LightGray;
            t.Rows[0].Cells[5].FillColor = System.Drawing.Color.LightGray;
            t.Rows[0].Cells[6].FillColor = System.Drawing.Color.LightGray;

            Formatting boldFormatting = new Formatting();
            boldFormatting.Bold = true;

            // Set the header titles
            t.Rows[0].Cells[0].Paragraphs.First().Append("Date", boldFormatting);
            t.Rows[0].Cells[1].Paragraphs.First().Append("Start Time", boldFormatting);
            t.Rows[0].Cells[2].Paragraphs.First().Append("Starting Point", boldFormatting);
            t.Rows[0].Cells[3].Paragraphs.First().Append("Destination", boldFormatting);
            t.Rows[0].Cells[4].Paragraphs.First().Append("Classification", boldFormatting);
            t.Rows[0].Cells[5].Paragraphs.First().Append("Details", boldFormatting);
            t.Rows[0].Cells[6].Paragraphs.First().Append("Total Kilometres Driven", boldFormatting);

            int count = 0;

            var itemsSource = dataGridMileageLogs.ItemsSource;

            if (itemsSource != null)
            {
                foreach (var item in itemsSource)
                {
                    var row = dataGridMileageLogs.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (row != null)
                    {
                        count++;
                        MileageLog logEntry = (MileageLog)row.Item;
                        t.Rows[count].Cells[0].Paragraphs.First().Append(logEntry.Date);
                        t.Rows[count].Cells[1].Paragraphs.First().Append(logEntry.StartTime);
                        t.Rows[count].Cells[2].Paragraphs.First().Append(logEntry.StartingPoint);
                        t.Rows[count].Cells[3].Paragraphs.First().Append(logEntry.Destination);
                        t.Rows[count].Cells[4].Paragraphs.First().Append(logEntry.Purpose);
                        t.Rows[count].Cells[5].Paragraphs.First().Append(logEntry.PurposeDetails);
                        t.Rows[count].Cells[6].Paragraphs.First().Append(logEntry.TotalDistance.ToString("F"));
                    }
                }
            }

            // Insert Table
            doc.InsertTable(t);

            // Save document
            doc.Save();

            // Start Word
            Process.Start("WINWORD.EXE", fileName);
        }
    }
}
