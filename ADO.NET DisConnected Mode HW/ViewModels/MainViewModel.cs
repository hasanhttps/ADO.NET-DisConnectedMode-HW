using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Windows.Controls;
using ADO.NET_DisConnected_Mode_HW.Commands;

namespace ADO.NET_DisConnected_Mode_HW.ViewModels;

public class MainViewModel {

    // Private Fields

    private DataSet? dataSet = null;
    private string? searchText = null;
    private DataGrid authorsDataGridView;
    private SqlDataAdapter? adapter = null;
    private DataRowView? selectedRow = null;
    private SqlConnection? connection = null;

    // Properties

    public DataRowView? SelectedRow { get => selectedRow;
        set {
            selectedRow = value;
        }
    }
    public ICommand? UpdateBtCommand { get; set; }
    public ICommand? InsertBtCommand { get; set; }
    public ICommand? DeleteBtCommand { get; set; }
    public string? SearchText { get => searchText;
        set {
            searchText = value;
            if (string.IsNullOrEmpty(searchText)) ReadDataDisConnectedMode();
            else SearchTextChanged();
        }
    }

    // Constructor

    public MainViewModel(DataGrid dataGrid) {

        authorsDataGridView = dataGrid;

        UpdateBtCommand = new RelayCommand(UpdateCommand);
        InsertBtCommand = new RelayCommand(InsertCommand);
        DeleteBtCommand = new RelayCommand(DeleteCommand);

        connection = new SqlConnection("Data Source=ASUSTUFGAMING\\SQLEXPRESS;Integrated Security=True;Initial Catalog=Library;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

        ReadDataDisConnectedMode();

        SetDeleteCommand();
        SetInsertCommand();
        SetUpdateCommand();
    }

    // Functions

    private void SetUpdateCommand() {
        SqlCommand updateCommand = new SqlCommand() {
            CommandText = "usp_UpdateBooks",
            Connection = connection,
            CommandType = CommandType.StoredProcedure,
        };

        updateCommand.Parameters.Add(new SqlParameter("@pId", SqlDbType.Int));
        updateCommand.Parameters["@pId"].SourceVersion = DataRowVersion.Original;
        updateCommand.Parameters["@pId"].SourceColumn = "Id";

        updateCommand.Parameters.Add(new SqlParameter("@pFirstName", SqlDbType.NVarChar));
        updateCommand.Parameters["@pFirstName"].SourceVersion = DataRowVersion.Original;
        updateCommand.Parameters["@pFirstName"].SourceColumn = "FirstName";

        updateCommand.Parameters.Add(new SqlParameter("@pLastName", SqlDbType.NVarChar));
        updateCommand.Parameters["@pLastName"].SourceVersion = DataRowVersion.Original;
        updateCommand.Parameters["@pLastName"].SourceColumn = "LastName";

        adapter!.UpdateCommand = updateCommand;
    }
    
    private void SetInsertCommand() {

        SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(adapter);
        adapter.UpdateCommand = null;
    }

    private void SetDeleteCommand() {
        try {
            SqlCommand deleteCommand = new SqlCommand($"DELETE FROM Authors WHERE Id = @Id", connection);
            adapter!.DeleteCommand = deleteCommand;
            adapter!.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "Id").SourceVersion = DataRowVersion.Original;
        }
        catch (Exception? ex)  { 
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReadDataDisConnectedMode() {
        try {
            string selectQuery = "SELECT * FROM Authors";
            adapter = new SqlDataAdapter(selectQuery, connection);

            dataSet = new DataSet();
            adapter.Fill(dataSet);

            authorsDataGridView.ItemsSource = dataSet.Tables["table"]!.DefaultView;
        }
        catch (Exception ex) {
            MessageBox.Show(ex.Message);
        }
    }

    private void UpdateCommand(object? param) {
        if (dataSet is not null) {
            try {
                MessageBox.Show(dataSet.Tables["table"].Rows[0]["FirstName"].ToString());
                adapter!.Update(dataSet);
            }
            catch(Exception ex) { }
        }
    }

    private void InsertCommand(object? param) {
        if (dataSet is not null) {
            DataTable dataTable = new DataTable();
            adapter!.Fill(dataTable);

            DataRow newRow = dataTable.NewRow();
            newRow["Id"] = selectedRow!["Id"];
            newRow["FirstName"] = selectedRow!["FirstName"];
            newRow["LastName"] = selectedRow!["LastName"];

            dataTable.Rows.Add(newRow);
            adapter!.Update(dataTable);
        }
    }

    private void DeleteCommand(object? param) {
        if (dataSet is not null) {
            DataTable dataTable = new DataTable();
            adapter!.Fill(dataTable);

            DataRow[] rowsToDelete = dataTable.Select($"Id = {SelectedRow!["Id"]}");
            foreach (DataRow row in rowsToDelete) {
                row.Delete();
            }
            selectedRow!.Delete();

            adapter.Update(dataTable);
        }
    }

    private void SearchTextChanged() {
        DataTable dataTable = new DataTable();
        adapter!.Fill(dataTable);

        DataRow[] searchedrows = dataTable.Select($"FirstName LIKE '{searchText}%'");

        DataTable newDataTable = new DataTable();

        newDataTable.Columns.Add("Id");
        newDataTable.Columns.Add("FirstName");
        newDataTable.Columns.Add("LastName");

        foreach (DataRow row in searchedrows) {
            DataRow newrow = newDataTable.NewRow();
            newrow["Id"] = row["Id"];
            newrow["FirstName"] = row["FirstName"];
            newrow["LastName"] = row["LastName"];

            newDataTable.Rows.Add(newrow);
        }

        authorsDataGridView.ItemsSource = newDataTable.DefaultView;
    }
}
