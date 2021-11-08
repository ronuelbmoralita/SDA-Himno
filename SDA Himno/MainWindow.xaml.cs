using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Path = System.IO.Path;
using System.Diagnostics;

namespace SDA_Hymn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //sqlite connection

        readonly string sqliteConnectionString = @"Data Source=C:\SDA Himno\Database\SDATagalogHymnalDatabase.sqlite;Version=3;";

        public DateTime StartDate;
        public DateTime EndDate;
        public TimeSpan Difference;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            titleSearchFront.Focus();
            CreateFolder();
            CreateTable();
            DisplayTitle();
        }

        private void CreateFolder()
        {
            string root = @"C:\SDA Himno";
            if (Directory.Exists(root))
            {
                return;
            }
            else
            {
                Directory.CreateDirectory(root).Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                Directory.CreateDirectory(Path.Combine(root, @"Database"));
            }

            if (!File.Exists(@"C:\SDA Himno\Database\SDATagalogHymnalDatabase.sqlite"))
            {
                if (!File.Exists(@"C:\Program Files (x86)\OWL\SDA Himno\Database\SDATagalogHymnalDatabase.sqlite"))
                {
                    MessageBox.Show("File not found!", "Transaksyon Tracer", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                { 
                    File.Copy(@"C:\Program Files (x86)\OWL\SDA Himno\Database\SDATagalogHymnalDatabase.sqlite", @"C:\SDA Himno\Database\SDATagalogHymnalDatabase.sqlite");
                }
            }
        }

        //mouse cursor

        private void WaitCursor()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait; // set the cursor to loading spinner  
        }

        private void NormalCursor()
        {
            Mouse.OverrideCursor = null; // set the cursor back to normal
        }

        private void CreateTable()
        {
            if (!File.Exists(@"C:\SDA Himno\Database\SDATagalogHymnalDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile(@"C:\\Backup\\SDATagalogHymnalDatabase.sqlite");

                string sql = @"CREATE TABLE hymnalTable(
                               ID INTEGER PRIMARY KEY AUTOINCREMENT ,
                                title         TEXT      NULL,
                                content         TEXT      NULL
                            );";

                using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
        }


        private void CreateAlgoTable()
        {
            /*
            if (File.Exists(@"C:\Transaksyon Tracer\Database\TransaksyonTracerDatabase.sqlite"))
            {
                return;
            }
            else
            {   */
            string sql = @"CREATE TABLE trialBegin(
	                               NOS INTEGER PRIMARY KEY AUTOINCREMENT ,
                                   dateBegin         TEXT      NULL         
                            );

                                CREATE TABLE trialEnd(
	                               NOS INTEGER PRIMARY KEY AUTOINCREMENT ,
                                   dateEnd         TEXT      NULL         
                            );";

            using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    con.Open();
                     cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        void Clear(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {

                if (obj is TextBox textbox)
                    textbox.Text = string.Empty;

                if (obj is RadioButton radiobutton)
                    radiobutton.IsChecked = false;

                Clear(VisualTreeHelper.GetChild(obj, i));
            }
        }


        //display address

        private void DisplayTitle()
        {

            try
            {
                using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
                {
                    con.Open();
                    //address

                    DataTable dt = new DataTable();
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter("select distinct title from hymnalTable", con))
                    {
                        da.Fill(dt);

                        titleSearchFront.DisplayMemberPath = "title";
                        titleSearchFront.ItemsSource = dt.DefaultView;

                        titleSearchBack.DisplayMemberPath = "title";
                        titleSearchBack.ItemsSource = dt.DefaultView;
                        NormalCursor();
                        //idDocuments

                        /*DataTable dtIdDoc_Type = new DataTable();
                        da = new SQLiteDataAdapter("select distinct idDoc_Type from transactions", con);
                        da.Fill(dtIdDoc_Type);

                        input_idDoc.DisplayMemberPath = "idDoc_Type";
                        input_idDoc.ItemsSource = dtIdDoc_Type.DefaultView;
                        con.Close();  */
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        //

        private void BindTitleFront_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                /*DataView dv = db_transaction.ItemsSource as DataView;
                dv.RowFilter = "Convert(ID, 'System.String') like '%" + searchId.Text + "%'"; //where n is a column name of the DataTable
                */

                if (titleSearchFront.Text == string.Empty)
                {
                    Clear(this);
                }

                if (titleSearchFront.Text != string.Empty)
                {
                    using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
                    {
                        con.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("Select content from hymnalTable where title like @Name", con))
                        {
                            cmd.Parameters.AddWithValue("@Name", titleSearchFront.Text);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    //int id = (int)reader["iD"]; 
                                    //textBox1.Text = rdr.GetString(0);
                                    displayContent.Text = reader.GetString(0);
                                }
                            }
                        }
                    }
                }
            }

            catch (System.Exception)
            {
                throw;
            }
        }

        //

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult close = MessageBox.Show("Are you sure you want to exit?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (close == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }

        //

        private void TitleSearchFront_KeyUp(object sender, KeyEventArgs e)
        {
            if (titleSearchFront.Text == string.Empty)
            {
                this.displayContent.ScrollToHome();
                titleSearchFront.IsDropDownOpen = false;
            }
            else
            {
                this.displayContent.ScrollToHome();
                titleSearchFront.IsDropDownOpen = false;
            }
        }






        //grid menu


         /*
        private void DisplayHymnal()
        {
            using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
            {
                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    //da = new SQLiteDataAdapter("Select * From Student order by ID desc", con);
                    //da = new SQLiteDataAdapter("Select * From hymnalTable order by ID desc", con);
                    da = new SQLiteDataAdapter("Select * From hymnalTable order by ID desc", con);
                    ds = new DataSet();
                    con.Open();
                    da.Fill(ds, "hymnalTable");
                    db_hymnal.ItemsSource = ds.Tables["hymnalTable"].DefaultView;
                }
            }
        }
         */


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (titleSearchBack.Text == string.Empty || addContent.Text == string.Empty)
            {
                MessageBox.Show("Please don't leave the title or content empty!", "SDA Himno", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                try
                {
                    using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
                    {

                        using (SQLiteCommand cmd = con.CreateCommand())
                        {

                            con.Open();
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "insert into hymnalTable(title,content) values(@title,@content)";


                            cmd.Parameters.AddWithValue("@title", titleSearchBack.Text);
                            cmd.Parameters.AddWithValue("@content", addContent.Text);

                            using (SQLiteCommand command = new SQLiteCommand("Select count (*) from hymnalTable where ID = '" + id.Text + "'", con))
                            {
                                var result = command.ExecuteScalar();
                                int i = Convert.ToInt32(result);
                                if (i != 0)
                                {
                                    MessageBox.Show("Hymn title are already exist!", "Transaction Tracer", MessageBoxButton.OK, MessageBoxImage.Warning); ;
                                    Clear(this);
                                    return;
                                }
                                else
                                {

                                    cmd.ExecuteNonQuery();

                                    //AutoComplete();
                                    //display_transaction();
                                    //display_Id();
                                    Clear(this);

                                    //MessageBox.Show("Transaction successfully saved into Database! ", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                                    //DisplayHymnal();
                                    DisplayTitle();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        //

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (id.Text == string.Empty)
            {
                MessageBox.Show("Select song to edit!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else

                using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
                {
                    using (SQLiteCommand cmd = con.CreateCommand())
                    {
                        //wait_cursor();
                        con.Open();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "update hymnalTable set title=@title,content=@content where ID=" + id.Text;
                        cmd.Parameters.AddWithValue("@title", titleSearchBack.Text);
                        cmd.Parameters.AddWithValue("@content", addContent.Text);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Edited!", "SDA Himno", MessageBoxButton.OK, MessageBoxImage.Question);
                        //DisplayHymnal();
                        //display_transaction();
                        DisplayTitle();
                        Clear(this);
                        //normal_cursor();
                    }
                }
        }

        private void BindTitleBack_TextChanged(object sender, TextChangedEventArgs e)
        {
              try
            {
                /*DataView dv = db_transaction.ItemsSource as DataView;
                dv.RowFilter = "Convert(ID, 'System.String') like '%" + searchId.Text + "%'"; //where n is a column name of the DataTable
                */

                if (titleSearchBack.Text == string.Empty)
                {
                    Clear(this);
                }

                if (titleSearchBack.Text != string.Empty)
                {
                    using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
                    {
                        con.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand("Select ID, title, content from hymnalTable where title like @Name", con))
                        {
                            cmd.Parameters.AddWithValue("@Name", titleSearchBack.Text);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    //int i = (int)reader["ID"];
                                    //id.Text = i.ToString();
                                    //textBox1.Text = rdr.GetString(0); 
                                    id.Text = reader["ID"].ToString();
                                    titleSearchBack.Text = reader.GetString(1);
                                    addContent.Text = reader.GetString(2);

                                }
                            }
                        }
                    }
                }
            }

            catch (System.Exception)
            {
                throw;
            }
        }

        private void TitleSearchBack_KeyUp(object sender, KeyEventArgs e)
        {
            if (titleSearchBack.Text == string.Empty)
            {
                SaveButton.IsEnabled = true;
                titleSearchBack.IsDropDownOpen = false;
            }
            else
            {
                SaveButton.IsEnabled = false;
                //titleSearchBack.IsDropDownOpen = true;
                titleSearchBack.IsDropDownOpen = false;
            }
        }

        private void Db_hymnal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataGrid gd = (DataGrid)sender;
                if (gd.SelectedItem is DataRowView row_selected)
                {
                    //wait_cursor();
                    //edit_recieverFirstname.Focus();

                    id.Text = row_selected["ID"].ToString();
                    titleSearchBack.Text = row_selected["title"].ToString();
                    addContent.Text = row_selected["content"].ToString();

                    //normal_cursor();
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Clear(this);
            //DisplayHymnal();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (id.Text == string.Empty)
            {
                MessageBox.Show("No item selected, please select into table!", "SDA Himno", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                string var;
                var = "Track number " + id.Text;
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete?  " + var, "SDA Himno", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)

                // Do this

                {
                    using (SQLiteConnection con = new SQLiteConnection(sqliteConnectionString))
                    {
                        using (SQLiteCommand cmd = con.CreateCommand())
                        {
                            con.Open();
                            cmd.CommandType = CommandType.Text;
                            //cmd.CommandText = "alter table transactions AUTO_INCREMENT = 1";
                            //cmd.CommandText = "truncate table transactions";
                            //cmd.CommandText = "delete from [transactions]";
                            cmd.CommandText = "delete from hymnalTable where ID=@id";
                            cmd.Parameters.AddWithValue("@id", id.Text);
                            cmd.ExecuteNonQuery();
                            //DisplayHymnal();
                            Clear(this);
                        }
                    }
                }
                else
                {

                    //DisplayHymnal();
                    return;
                }
            }
        }

        private void ListViewItem_Support(object sender, MouseButtonEventArgs e)
        {
            openTechnical.IsOpen = true;
        }

        private void TitleSearchFront_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.displayContent.ScrollToHome();
        }

        private void ListViewItem_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult close = MessageBox.Show("Are you sure you want to exit?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (close == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }

        private void ButtonClose_Click_1(object sender, RoutedEventArgs e)
        {
            titleSearchFront.Focus();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            titleSearchBack.Focus();
        }

        private void ButtonCloseListItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult close = MessageBox.Show("Are you sure you want to exit?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (close == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }
    }
}
