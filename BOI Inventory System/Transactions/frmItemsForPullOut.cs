using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.Sql;
using System.Data.SqlClient;

namespace BOI_Inventory_System
{
    public partial class frmItemsForPullOut : Form
    {
        public frmItemsForPullOut()
        {
            InitializeComponent();
        }

        private void frmItemsForPullOut_Load(object sender, EventArgs e)
        {
            func_Load_All_Items();

            cboSearchCriteria.Focus();
            cboSearchCriteria.SelectedIndex = 0;

            lblcount.Text = dgvItems.RowCount.ToString();

            cboSearchCriteria.SelectedIndex = 0;
        }

        private void func_Load_All_Items()
        {
            //Close current connection
            SysCon.CloseConnection();

            SysCon.SystemConnect.Open();

            string AllExItems = "Select * from view_Inventory_Details where Status = 'Assigned' and  fk_End_User_Id = '"+ GlobalClass.GlobalEmployeeId +"'";

            SqlDataAdapter AllItemsAdapter = new SqlDataAdapter(AllExItems, SysCon.SystemConnect);

            string srctbl = "view_Inventory_Details";

            DataSet ItemsData = new DataSet();

            AllItemsAdapter.Fill(ItemsData, srctbl);

            dgvItems.DataSource = ItemsData.Tables["view_Inventory_Details"];

            dgvItems.RowHeadersWidth = 5;

            dgvItems.Columns[0].Visible = false;
            dgvItems.Columns[1].Visible = false;
            dgvItems.Columns[2].Visible = false;
            dgvItems.Columns[3].Visible = false;
            dgvItems.Columns[4].Visible = false;
            dgvItems.Columns[22].Visible = false;
            dgvItems.Columns[26].Visible = false;
            dgvItems.Columns[29].Visible = false;
            dgvItems.Columns[30].Visible = false;
            dgvItems.Columns[32].Visible = false;
            dgvItems.Columns[35].Visible = false;
            dgvItems.Columns[36].Visible = false;
            dgvItems.Columns[37].Visible = false;

            //dgvItems.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            SysCon.SystemConnect.Close();

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (cboSearchCriteria.Text == "Item Description")
            {

                ((DataTable)dgvItems.DataSource).DefaultView.RowFilter = "Description LIKE '%" + txtSearch.Text + "%'";

                if (dgvItems.RowCount <= 0)
                {
                    MessageBox.Show("Item description does not exists!");
                    txtSearch.Text = "";

                }
            }

            if (cboSearchCriteria.Text == "Property Number")
            {

                ((DataTable)dgvItems.DataSource).DefaultView.RowFilter = "Old_Property_No LIKE '%" + txtSearch.Text + "%'";

                if (dgvItems.RowCount <= 0)
                {
                    MessageBox.Show("Property Number does not exists! ");
                    txtSearch.Text = "";

                }
            }

            if (cboSearchCriteria.Text == "New Property Number")
            {

                ((DataTable)dgvItems.DataSource).DefaultView.RowFilter = "New_Property_No LIKE '%" + txtSearch.Text + "%'";

                if (dgvItems.RowCount <= 0)
                {
                    MessageBox.Show("Property Number does not exists! ");
                    txtSearch.Text = "";

                }
            }

            if (cboSearchCriteria.Text == "Serial No.")
            {

                ((DataTable)dgvItems.DataSource).DefaultView.RowFilter = "Serial_No LIKE '%" + txtSearch.Text + "%'";

                if (dgvItems.RowCount <= 0)
                {
                    MessageBox.Show("Serial No. does not exists! ");
                    txtSearch.Text = "";

                }
            }

            lblcount.Text = dgvItems.RowCount.ToString();
        }

        private void cboSearchCriteria_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearch.Text = "";
        }

        private void dgvItems_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvItems.RowCount <= 0)
            {
                MessageBox.Show("No items to select..");
            }
            else
            {

                GlobalClass.GlobalInvItemId = dgvItems.CurrentRow.Cells[0].Value.ToString();
                //MessageBox.Show(GlobalClass.GlobalInvItemId);
                this.Close();

            }
        }

        private void txtSearch_MouseClick(object sender, MouseEventArgs e)
        {
            txtSearch.BackColor = Color.Aquamarine;
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            txtSearch.BackColor = Color.White;
        }
    }
}
