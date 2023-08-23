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
using Word = Microsoft.Office.Interop.Word;



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
            dataGridView1.Columns.Add("[Id службы]", "id");
            dataGridView1.Columns.Add("[Id больничного]", "id");
            dataGridView1.Columns.Add("Название", "Служба");
            dataGridView1.Columns.Add("Примечание", "Примечание");
            dataGridView1.Columns.Add("Название", "Больничные");
            dataGridView1.Columns.Add("Примечание", "Примечание");
            dataGridView1.Columns.Add("Количество", "Количество");
            dataGridView1.Columns.Add("Дата", "Дата");
            dataGridView1.Columns.Add("IsNew", String.Empty);
            //this.dataGridView1.Columns["[Id службы]"].Visible = false;
            this.dataGridView1.Columns["IsNew"].Visible = false;
        }

        private void ClearFields() 
        {
            textBox_SL.Text = "";
            textBox_Prim1.Text = "";
            textBox_Bol.Text = "";
            textBox_Prim2.Text = "";
            textBox_Kol.Text = "";
            textBox_Date.Text = "";

        }

        private void ReadSingleRow(DataGridView dgw, IDataRecord record)
        {
            dgw.Rows.Add(record.GetInt32(0), record.GetInt32(1), record.GetString(2), record.GetString(3), record.GetString(4), record.GetString(5), record.GetInt32(6), record.GetDateTime(7), RowState.ModifiedNew);
        }

        private void RefreshDataGrid(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string queryString = $"SELECT Службы.[Id службы], [Виды больничных].[Id больничного], Службы.Название, Службы.Примечание, [Виды больничных].Название, [Виды больничных].Примечание,[Службы, виды больничных].[Количество], " +
                $"[Службы, виды больничных].Дата FROM Службы INNER JOIN[Службы, виды больничных] ON[Службы, виды больничных].[Id службы] = Службы.[Id службы]" +
                $" inner join[Виды больничных] ON[Виды больничных].[Id больничного] = [Службы, виды больничных].[Id больничного] ";

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

                textBox_SL.Text = row.Cells[2].Value.ToString();
                textBox_Prim1.Text = row.Cells[3].Value.ToString();
                textBox_Bol.Text = row.Cells[4].Value.ToString();
                textBox_Prim2.Text = row.Cells[5].Value.ToString();
                textBox_Kol.Text = row.Cells[6].Value.ToString();
                textBox_Date.Text = row.Cells[7].Value.ToString();
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshDataGrid(dataGridView1);
            ClearFields();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Add_Form addform = new Add_Form();
            addform.Show();
        }

        private void Search(DataGridView dgw)
        {
            dgw.Rows.Clear();

            string searchString = $"SELECT [Виды больничных].[Id больничного], Службы.Название, Службы.Примечание, [Виды больничных].Название, [Виды больничных].Примечание,[Службы, виды больничных].[Количество], " +
                $"[Службы, виды больничных].Дата FROM Службы INNER JOIN[Службы, виды больничных] ON[Службы, виды больничных].[Id службы] = Службы.[Id службы]" +
                $" inner join[Виды больничных] ON[Виды больничных].[Id больничного] = [Службы, виды больничных].[Id больничного]" +
                $" where concat ([Id больничного]Службы.Название, Службы.Примечание, [Виды больничных].Название, [Виды больничных].Примечание, Количество, Дата) like '%" + textBox_search.Text + "%'";

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
            dataGridView1.Rows[index].Cells[7].Value = RowState.Deleted;
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
                var rowState = (RowState)dataGridView1.Rows[index].Cells[8].Value;

                if (rowState == RowState.Existed)
                    continue;

                if (rowState == RowState.Deleted)
                {

                    var id = Convert.ToInt32(dataGridView1.Rows[index].Cells[0].Value);
                    var deleteQuery = $"delete from Службы where [Id службы] = {id}; delete from [Виды больничных] where [Id больничного] = {id}; delete from [Службы, виды больничных] where [Id Службы] = {id} and [Id больничного] = {id}";

                    var command = new SqlCommand(deleteQuery, dataBase.getConnection());
                    command.ExecuteNonQuery();

                }

                if (rowState == RowState.Modified)
                {
                    var id = dataGridView1.Rows[index].Cells[0].Value.ToString();
                    var SL = dataGridView1.Rows[index].Cells[1].Value.ToString();
                    var Prim1 = dataGridView1.Rows[index].Cells[2].Value.ToString();
                    var Bol = dataGridView1.Rows[index].Cells[3].Value.ToString();
                    var Prim2 = dataGridView1.Rows[index].Cells[4].Value.ToString();
                    var Kol = dataGridView1.Rows[index].Cells[5].Value.ToString();
                    var Date = dataGridView1.Rows[index].Cells[6].Value.ToString();
                    

                    var changeQuery = $"update Службы set Службы.Название = '{SL}' where [Id службы] = '{id}'"; 
                        //$" [Виды больничных], [Службы, виды больничных] set Службы.Название = '{SL}', Службы.Примечание = '{Prim1}', [Виды больничных].Название = '{Bol}', [Виды больничных].Примечание = '{Prim2}', Количество = '{Kol}', Дата = '{Date}' where id = '{id}';";

                    //$"update Службы, [Виды больничных], [Службы, виды больничных] set Службы.Название = '{SL}', Службы.Примечание = '{Prim1}', [Виды больничных].Название = '{Bol}', [Виды больничных].Примечание = '{Prim2}', Количество = '{Kol}', Дата = '{Date}' where id = '{id}';";

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

            var SL = textBox_SL.Text;
            var Prim1 = textBox_Prim1.Text;
            var Bol = textBox_Bol;
            var Prim2 = textBox_Prim2.Text;
            int Kol;
            var Date = textBox_Date.Text;
            

            if (dataGridView1.Rows[selectedRowIndex].Cells[0].Value.ToString() != string.Empty)
            {
                if (int.TryParse(textBox_Kol.Text, out Kol))
                {
                    dataGridView1.Rows[selectedRowIndex].SetValues(SL, Prim1, Bol, Prim2, Kol, Date);
                    dataGridView1.Rows[selectedRowIndex].Cells[7].Value = RowState.Modified;
                }
                else
                {
                    MessageBox.Show("Количество должно быть записано в числовом формате!");
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

        private void textBox_SL_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        //protected void btnexportreport_Click(object sender, EventArgs e)
        //{
        //    ExportGridToExcel();
        //}

        //private void ExportGridToExcel()
        //{

        //    Response.AddHeader("content-disposition", "attachment;filename=Excelsheet" + DateTime.Now.Ticks.ToString() + ".xls");
        //    Response.Charset = "";
        //    Response.ContentType = "application/vnd.xls";
        //    StringWriter StringWriter = new System.IO.StringWriter();
        //    HtmlTextWriter HtmlTextWriter = new HtmlTextWriter(StringWriter);

        //    gvdisplay.RenderControl(HtmlTextWriter);


        //    Response.Write(StringWriter.ToString());
        //    Response.End();
        //    Response.Clear();


        //}
        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "Word Documents (*.docx)|*.docx";
            sfd.FileName = "Save.docx";

            if (sfd.ShowDialog() == DialogResult.OK)
            {

                Export_Data_To_Word(dataGridView1, sfd.FileName);
            }
        }
        public void Export_Data_To_Word(DataGridView DGV, string filename)
        {
            if (DGV.Rows.Count != 0)
            {
                int RowCount = DGV.Rows.Count;
                int ColumnCount = DGV.Columns.Count;
                Object[,] DataArray = new object[RowCount + 1, ColumnCount + 1];

                //add rows
                int r = 0;
                for (int c = 0; c <= ColumnCount - 1; c++)
                {
                    for (r = 0; r <= RowCount - 1; r++)
                    {
                        DataArray[r, c] = DGV.Rows[r].Cells[c].Value;
                    } //end row loop
                } //end column loop

                Word.Document oDoc = new Word.Document();
                oDoc.Application.Visible = true;

                //page orintation
                oDoc.PageSetup.Orientation = Word.WdOrientation.wdOrientPortrait;


                dynamic oRange = oDoc.Content.Application.Selection.Range;
                string oTemp = "";
                for (r = 0; r <= RowCount - 1; r++)
                {
                    for (int c = 0; c <= ColumnCount - 1; c++)
                    {
                        oTemp = oTemp + DataArray[r, c] + "\t";

                    }
                }

                //table format
                oRange.Text = oTemp;

                object Separator = Word.WdTableFieldSeparator.wdSeparateByTabs;
                object ApplyBorders = true;
                object AutoFit = true;
                object AutoFitBehavior = Word.WdAutoFitBehavior.wdAutoFitContent;

                oRange.ConvertToTable(ref Separator, ref RowCount, ref ColumnCount,
                                      Type.Missing, Type.Missing, ref ApplyBorders,
                                      Type.Missing, Type.Missing, Type.Missing,
                                      Type.Missing, Type.Missing, Type.Missing,
                                      Type.Missing, ref AutoFit, ref AutoFitBehavior, Type.Missing);

                oRange.Select();

                oDoc.Application.Selection.Tables[1].Select();
                oDoc.Application.Selection.Tables[1].Rows.AllowBreakAcrossPages = 0;
                oDoc.Application.Selection.Tables[1].Rows.Alignment = 0;
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.InsertRowsAbove(1);
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                //header row style
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Bold = 1;
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Name = "Tahoma";
                oDoc.Application.Selection.Tables[1].Rows[1].Range.Font.Size = 14;

                //add header row manually
                for (int c = 0; c <= ColumnCount - 1; c++)
                {
                    oDoc.Application.Selection.Tables[1].Cell(1, c + 1).Range.Text = DGV.Columns[c].HeaderText;
                }

                //table style 
                oDoc.Application.Selection.Tables[1].set_Style("Grid Table 4 - Accent 5");
                oDoc.Application.Selection.Tables[1].Rows[1].Select();
                oDoc.Application.Selection.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                //header text
                foreach (Word.Section section in oDoc.Application.ActiveDocument.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Fields.Add(headerRange, Word.WdFieldType.wdFieldPage);
                    headerRange.Text = "your header text";
                    headerRange.Font.Size = 16;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                }

                //save the file
                oDoc.SaveAs2(filename);

                //NASSIM LOUCHANI
            }
        }
    }
}
