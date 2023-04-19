using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Test_boln
{
    enum RowState 
    {
        Existed,
        New,
        Modified,
        ModifiedNew,
        Deleted
    }
    public partial class Form1 : Form
    {
        DataBase dataBase = new DataBase();

        int selectedRow;
        public Form1()
        {
            InitializeComponent();
        }

        private void CreateColumns()
        {
            dataGridView1.Columns.Add("id", "id");
            dataGridView1.Columns.Add("Служба", "Служба");
            dataGridView1.Columns.Add("Больничные", "Больничные");
            dataGridView1.Columns.Add("Коронавирус", "Коронавирус");
            dataGridView1.Columns.Add("Уход", "Уход");
            dataGridView1.Columns.Add("Дата", "Дата");
            dataGridView1.Columns.Add("IsNew", String.Empty);
            this.dataGridView1.Columns["id"].Visible = false;
            this.dataGridView1.Columns["IsNew"].Visible = false;
        }

        private void ClearFields() 
        {
            textBox_ID.Text = "";
            textBox_SL.Text = "";
            textBox_Bol.Text = "";
            textBox_Kor.Text = "";
            textBox_Uh.Text = "";
            textBox_Date.Text = "";

        }

        private void ReadSingleRow(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0), record.GetString(1), record.GetInt32(2), record.GetInt32(3), record.GetInt32(4), record.GetString(5), RowState.ModifiedNew);
        }

        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string queryString = $"select * from Boln";

            SqlCommand command = new SqlCommand(queryString, dataBase.getConnection());

            dataBase.openConnection();

            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ReadSingleRow(dgw, reader);
            }
            reader.Close();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            CreateColumns();
            RefreshDataGrid(dataGridView1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            deleteRow();
            ClearFields();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];

                textBox_ID.Text = row.Cells[0].Value.ToString();
                textBox_SL.Text = row.Cells[1].Value.ToString();
                textBox_Bol.Text = row.Cells[2].Value.ToString();
                textBox_Kor.Text = row.Cells[3].Value.ToString();
                textBox_Uh.Text = row.Cells[4].Value.ToString();
                textBox_Date.Text = row.Cells[5].Value.ToString();
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView1);
            ClearFields();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Add_Form addfrm = new Add_Form();
            addfrm.Show();
        }

        private void Search(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string searchString = $"select * from Boln where concat (id, Служба, Больничные, Коронавирус, Уход, Дата) like '%" + textBox_search.Text + "%'";

            SqlCommand com = new SqlCommand(searchString, dataBase.getConnection());

            dataBase.openConnection();

            SqlDataReader read = com.ExecuteReader();

            while (read.Read())
            {
                ReadSingleRow(dgw, read);
            }

            read.Close();
        }

        private void deleteRow()
        {
            int index = dataGridView1.CurrentCell.RowIndex;

            dataGridView1.Rows[index].Visible = false;

            //if (dataGridView1.Rows[index].Cells[0].Value.ToString() == string.Empty)
            //{
            //    dataGridView1.Rows[index].Cells[6].Value = RowState.Deleted;
            //    return;
            //}
            dataGridView1.Rows[index].Cells[6].Value = RowState.Deleted;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Update();
        }
        private new void Update()
        {
            dataBase.openConnection();

            for (int index = 0; index < dataGridView1.Rows.Count; index++)
            {
                var rowState = (RowState)dataGridView1.Rows[index].Cells[6].Value;

                if (rowState == RowState.Existed)
                    continue;

                if (rowState == RowState.Deleted)
                {

                    var id = Convert.ToInt32(dataGridView1.Rows[index].Cells[0].Value);
                    var deleteQuery = $"delete from Boln where id = {id}";

                    var command = new SqlCommand(deleteQuery, dataBase.getConnection());
                    command.ExecuteNonQuery();

                }

                if (rowState == RowState.Modified)
                {
                    var id = dataGridView1.Rows[index].Cells[0].Value.ToString();
                    var SL = dataGridView1.Rows[index].Cells[1].Value.ToString();
                    var Bol = dataGridView1.Rows[index].Cells[2].Value.ToString();
                    var Kor = dataGridView1.Rows[index].Cells[3].Value.ToString();
                    var Uh = dataGridView1.Rows[index].Cells[4].Value.ToString();
                    var Date = dataGridView1.Rows[index].Cells[5].Value.ToString();
                    

                    var changeQuery = $"update Boln set Служба = '{SL}', Больничные = '{Bol}', Коронавирус = '{Kor}', Уход = '{Uh}', Дата = '{Date}' where id = '{id}'";

                    var command = new SqlCommand(changeQuery, dataBase.getConnection());
                    command.ExecuteNonQuery();
                }
            }

            dataBase.closeConnection();
        }
        private void textBox_search_TextChanged(object sender, EventArgs e)
        {
            Search(dataGridView1);
        }

        private void Change()
        {
            var selectedRowIndex = dataGridView1.CurrentCell.RowIndex;

            var ID = textBox_ID.Text;
            var SL = textBox_SL.Text;
            int Bol;
            var Kor = textBox_Kor.Text;
            var Uh = textBox_Uh.Text;
            var Date = textBox_Date.Text;
            

            if (dataGridView1.Rows[selectedRowIndex].Cells[0].Value.ToString() != string.Empty)
            {
                if (int.TryParse(textBox_Bol.Text, out Bol))
                {
                    dataGridView1.Rows[selectedRowIndex].SetValues(ID, SL, Bol, Kor, Uh, Date);
                    dataGridView1.Rows[selectedRowIndex].Cells[6].Value = RowState.Modified;
                }
                else
                {
                    MessageBox.Show("Больничные должны иметь числовой формат!");
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Change();
            ClearFields();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            ClearFields();
        }
    }
}
