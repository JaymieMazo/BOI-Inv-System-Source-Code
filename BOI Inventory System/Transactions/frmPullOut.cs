﻿using System;
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
    public partial class frmPullOut : Form
    {
        string InvId, DocNo;
        public string End_User_Id,End_User_Unit;
        public frmPullOut()
        {
            InitializeComponent();
        }

        private void btnFindItem_Click(object sender, EventArgs e)
        {
            GlobalClass.GlobalInvItemId = "";

            frmItemsForPullOut frm_ItemsList = new frmItemsForPullOut();
            frm_ItemsList.ShowDialog();
                       
            if (!String.IsNullOrEmpty(GlobalClass.GlobalInvItemId))
            { func_Retrieve_Item(); btnFindUser.Enabled = false; txtReceivedBy.Focus(); }
            else { btnFindItem.Focus(); }
        }

        private void func_Retrieve_Item()
        {
            string RetrieveItems = "Select pk_Id,Description,New_Property_No,Serial_No from view_Inventory_Details where pk_Id = ' " + GlobalClass.GlobalInvItemId + "'";
            //close current connection
            SysCon.CloseConnection();
            //Open connection
            SysCon.SystemConnect.Open();

            SqlCommand ItemFinder = new SqlCommand(RetrieveItems, SysCon.SystemConnect);
            SqlDataReader ItemReader = ItemFinder.ExecuteReader();

            if (ItemReader.Read())
            {
                InvId = ItemReader[0].ToString();
                txtDescription.Text = ItemReader[1].ToString();
                txtPropertyNo.Text = ItemReader[2].ToString();
                txtSerialNo.Text = ItemReader[3].ToString();
            }
        }

        private void btnAddItem_Click(object sender, EventArgs e)
        {
            if (txtEndUser.Text == "")
            {
                MessageBox.Show("Select End User of the item to pull out.", "Item Pull out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnFindUser.Focus();
            }
            else if (txtDescription.Text == "")
            {
                MessageBox.Show("Choose item to pull-out.", "Item Pull out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnFindItem.Focus();
            }
            else if (txtReceivedBy.Text == "")
            {
                MessageBox.Show("Please indicate who received the item. ", "Item Pull out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReceivedBy.Focus();
            }
            else
            {
                func_AddtoDataGrid();
            }
        }

        private void func_AddtoDataGrid()
        {
            dgvItems.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            //Location_Id = "0";

            //additems to datagrid
            int CurrentRow = dgvItems.Rows.Add();

            dgvItems.Rows[CurrentRow].Cells[0].Value = GlobalClass.GlobalInvItemId;
            dgvItems.Rows[CurrentRow].Cells[1].Value = txtDescription.Text;
            dgvItems.Rows[CurrentRow].Cells[2].Value = txtPropertyNo.Text;
            dgvItems.Rows[CurrentRow].Cells[3].Value = txtSerialNo.Text;
            dgvItems.Rows[CurrentRow].Cells[4].Value = txtRemarks.Text;
            dgvItems.Rows[CurrentRow].Cells[5].Value = txtReceivedBy.Text;

            func_Reset_Details();
            btnFindUser.Enabled = false;
        }

        private void func_Reset_Details()
        {
            txtDescription.Text = "";
            txtPropertyNo.Text = "";
            txtSerialNo.Text = "";

           // txtRemarks.Text = "";
           // txtReceivedBy.Text = "";

            btnFindItem.Focus();
            btnFindItem.Enabled = true;
            btnFindUser.Enabled = false;
        }

        private void btnPullout_Click(object sender, EventArgs e)
        {

            if (dgvItems.Rows.Count == 0)
            {
                MessageBox.Show("Please select item/s to pull-out", "Item Pull-Out Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (txtReceivingEmployee.Text == "")
            {
                MessageBox.Show("Please indicate receiving employee name. ", "Item Pull-Out Record", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (cboReason.Text == "")
            {
                MessageBox.Show("Choose reason for Pull out.", "Item Pull out Record ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (btnPullout.Text == "Pull-Out")
                {
                    DialogResult mes = MessageBox.Show("Do you really want to pull-out listed item/s?", "Item Pull-Out Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    switch (mes)
                    {
                        case DialogResult.Yes:
                            {
                                func_Generate_Document_No(); //Generate Receipt no.
                                func_Save_Data(); //Save Pull out records
                                func_Print_RS(); //Print Pull out receipt
                                Reset_All();    //clear fields
                                break;
                            }

                        case DialogResult.No:
                            {
                                MessageBox.Show("All information you provide will be lost.", "Item Pull-Out", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                Reset_All();
                                break;
                            }
                    }
                }
            }
        }


        private void func_Save_Data()
        {
            //close connection
            SysCon.CloseConnection();
            //open connection
            SysCon.SystemConnect.Open();

            if (cboReason.Text == "FOR REPAIR")
            {
                string UpdateRecord = "Update tbl_Inventory_Details Set Status=@Status where pk_Id = @pk_Id";
                foreach (DataGridViewRow row in dgvItems.Rows)
                {
                    using (SqlCommand cmdUpdate = new SqlCommand(UpdateRecord, SysCon.SystemConnect))
                    {
                        cmdUpdate.Parameters.Clear();

                        cmdUpdate.Parameters.AddWithValue("@pk_Id", row.Cells[0].Value);
                        //cmdUpdate.Parameters.AddWithValue("@fk_OIC", GlobalClass.GlobalOICId);
                        cmdUpdate.Parameters.AddWithValue("@Status", "FOR REPAIR");

                        cmdUpdate.ExecuteNonQuery();
                    }
                }
            }
            else if (cboReason.Text == "FOR DISPOSAL")
            {
                string UpdateStatus = "Update tbl_Inventory_Details Set Status=@Status,fk_Accountable_Employee_Id=@fk_OIC,fk_End_User_Id= '',Document_No = '"+ DocNo +"' where pk_Id = @pk_Id";
                foreach (DataGridViewRow row in dgvItems.Rows)
                {
                    using (SqlCommand cmdUpdate = new SqlCommand(UpdateStatus, SysCon.SystemConnect))
                    {
                        cmdUpdate.Parameters.Clear();

                        cmdUpdate.Parameters.AddWithValue("@pk_Id", row.Cells[0].Value);
                        cmdUpdate.Parameters.AddWithValue("@fk_OIC", GlobalClass.GlobalOICId);
                        cmdUpdate.Parameters.AddWithValue("@Status", "FOR DISPOSAL");

                        cmdUpdate.ExecuteNonQuery();

                    }
                }
            }
            else if (cboReason.Text == "FOR RETURN TO SUPPLIER")
            {
                string UpdateStatus = "Update tbl_Inventory_Details Set Status=@Status,fk_Accountable_Employee_Id=@fk_OIC,fk_End_User_Id=@fk_OIC where pk_Id = @pk_Id";
                foreach (DataGridViewRow row in dgvItems.Rows)
                {
                    using (SqlCommand cmdUpdate = new SqlCommand(UpdateStatus, SysCon.SystemConnect))
                    {
                        cmdUpdate.Parameters.Clear();

                        cmdUpdate.Parameters.AddWithValue("@pk_Id", row.Cells[0].Value);
                        cmdUpdate.Parameters.AddWithValue("@fk_OIC", GlobalClass.GlobalOICId);
                        cmdUpdate.Parameters.AddWithValue("@Status", "RETURN TO SUPPLIER");

                        cmdUpdate.ExecuteNonQuery();
                    }
                }
            }
            else // (cboReason.Text == "For Transfer") //For Reassignment
            {
                string UpdateStat = "Update tbl_Inventory_Details Set Status=@Status,fk_Accountable_Employee_Id=@fk_OIC,fk_End_User_Id = '',Document_No ='' where pk_Id = @pk_Id";
                foreach (DataGridViewRow row in dgvItems.Rows)
                {
                    using (SqlCommand cmdUpdate = new SqlCommand(UpdateStat, SysCon.SystemConnect))
                    {
                        cmdUpdate.Parameters.Clear();

                        cmdUpdate.Parameters.AddWithValue("@pk_Id", row.Cells[0].Value);
                        cmdUpdate.Parameters.AddWithValue("@fk_OIC", GlobalClass.GlobalOICId);
                        cmdUpdate.Parameters.AddWithValue("@Status", "FOR REASSIGNMENT");

                        cmdUpdate.ExecuteNonQuery();

                    }
                }
            }

            string NewPullOutRecord = "Insert into tbl_Pull_Out_Record Values (@fk_Inv_Id,@Pull_Out_From,@End_User_Unit,@ReasonForPullOut,@Remarks,@ReceivedBy,@Date_Pull_Out,@Date_Received,@fk_OIC,@NotedBy,@RRP_No)";
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                using (SqlCommand cmd = new SqlCommand(NewPullOutRecord, SysCon.SystemConnect))
                {
                    {
                        cmd.Parameters.AddWithValue("@fk_Inv_Id", row.Cells[0].Value);
                        cmd.Parameters.AddWithValue("@Pull_Out_From", txtEndUser.Text);
                        cmd.Parameters.AddWithValue("@End_User_Unit", End_User_Unit);
                        cmd.Parameters.AddWithValue("@ReasonForPullOut", cboReason.Text);
                        cmd.Parameters.AddWithValue("@Remarks", cboReason.Text + '/' + row.Cells[4].Value);
                        cmd.Parameters.AddWithValue("@ReceivedBy", row.Cells[5].Value);
                        cmd.Parameters.AddWithValue("@Date_Pull_Out", dtPullOut.Text);
                        cmd.Parameters.AddWithValue("@Date_Received", dtReceived.Text);
                        cmd.Parameters.AddWithValue("@fk_OIC", GlobalClass.GlobalOICId);
                        cmd.Parameters.AddWithValue("@NotedBy", txtNotedBy.Text);
                        cmd.Parameters.AddWithValue("@RRP_No", DocNo);
                        cmd.ExecuteNonQuery();
                    }
                }
            }


            string ItemHistory = "Insert into tbl_Item_History Values (@fk_Inv_Id,@Date,@Document_No,@fk_End_User_Id,@Status,@Remarks)";
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                using (SqlCommand cmd = new SqlCommand(ItemHistory, SysCon.SystemConnect))
                {
                    {
                        cmd.Parameters.AddWithValue("@fk_Inv_Id", row.Cells[0].Value);
                        cmd.Parameters.AddWithValue("@Date", dtPullOut.Text);
                        cmd.Parameters.AddWithValue("@Document_No", DocNo);
                        cmd.Parameters.AddWithValue("@fk_End_User_Id", End_User_Id);
                        cmd.Parameters.AddWithValue("@Status", "Pulled Out");
                        cmd.Parameters.AddWithValue("@Remarks", cboReason.Text + '-' + txtRemarks.Text );
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            //Audit trail
            string user = "Insert into tbl_Audit_Trail Values (@Full_Name,@User_Name,@Date_Time,@Activity)";

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                using (SqlCommand cmd = new SqlCommand(user, SysCon.SystemConnect))
                {
                    {
                        cmd.Parameters.AddWithValue("@Full_Name", GlobalClass.GlobalName);
                        cmd.Parameters.AddWithValue("@User_Name", GlobalClass.GlobalUser);
                        cmd.Parameters.AddWithValue("@Date_Time", DateTime.Now.ToString());
                        cmd.Parameters.AddWithValue("@Activity", "Pull Out Item : " + row.Cells[1].Value + " ; Property No. : " + row.Cells[2].Value + " ; Serial No. : " + row.Cells[3].Value + " ; End User : " + txtEndUser.Text + " ; Reason for Pull out : " + cboReason.Text + " ; Date of Pull out : " + dtPullOut.Text);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            //close connection
            SysCon.SystemConnect.Close();

            MessageBox.Show("Item/s has been successfully pulled out!", "Items Pull out", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        private void func_Generate_Document_No()
        {
            int count = 0;
            string YearToday = DateTime.Now.Year.ToString();
            string MonthToday = DateTime.Now.ToString("MM");

            string YearMonth = YearToday + "-" + MonthToday;


            string strCount = "Select Series FROM tbl_Serial where Doc_Type = 'RS'";

            //Close existing connection
            SysCon.CloseConnection();

            SqlCommand comd = new SqlCommand(strCount, SysCon.SystemConnect);
            SysCon.SystemConnect.Open();

            count = Convert.ToInt32(comd.ExecuteScalar());

            count = (Convert.ToInt32(count) + 1);

            DocNo = "RS" + "-" + YearMonth + "-" + count.ToString("0000");
           // txtDocNo.Text = Document_No;

            //Update tbl_Serial
            string UpdateSerial = "Update tbl_Serial Set Series = '" + count + "' where Doc_Type = 'RS'";

            //close connection
            SysCon.CloseConnection();
            //open connection
            SysCon.SystemConnect.Open();

            //execute query
            SqlCommand cmdUpdateSerial = new SqlCommand();
            cmdUpdateSerial.CommandType = CommandType.Text;
            cmdUpdateSerial.CommandText = UpdateSerial;
            cmdUpdateSerial.Connection = SysCon.SystemConnect;
            cmdUpdateSerial.ExecuteNonQuery();

            //close connection
            SysCon.CloseConnection();

        }

        private void Reset_All()
        {
            func_Reset_Details();

            dgvItems.Rows.Clear();

            dtPullOut.Value = DateTime.Now;
            dtReceived.Value = DateTime.Now;

            txtReceivingEmployee.Text = "";
            txtNotedBy.Text = "";
            txtEndUser.Text = "";

            cboReason.Text = "";
            cboReason.SelectedIndex = -1;

            btnFindUser.Enabled = true;
            btnFindItem.Enabled = false;
            
            txtReceivedBy.Text = "";

            btnFindUser.Focus();

        }

        private void btnFindOIC_Click(object sender, EventArgs e)
        {
            frmOIC frm_OIC = new frmOIC();
            frm_OIC.ShowDialog();

            if (!String.IsNullOrEmpty(GlobalClass.GlobalOICId))
            { func_Retrieve_OIC(); dtReceived.Focus(); }
            else { txtReceivingEmployee.Text = ""; btnFindOIC.Focus(); }
        }

        private void func_Print_RS()
        {
            frmReportViewer PreviewDialog = new frmReportViewer("Return_Slip", "SELECT * FROM view_PullOutRecords where RRP_No = '" + DocNo + "' ");
            PreviewDialog.ShowDialog();
        }

       

        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult mes = MessageBox.Show("Do you really want to clear text fields?", "Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            switch (mes)
            {
                case DialogResult.Yes:
                    {
                        Reset_All(); 
                        break;
                    }
            }
        }
        private void btnFindUser_Click(object sender, EventArgs e)
        {
            frmEmployeesList frm_Employees = new frmEmployeesList();
            frm_Employees.ShowDialog();

            if (!String.IsNullOrEmpty(GlobalClass.GlobalEmployeeId))
            { func_Retrieve_User(); btnFindItem.Enabled = true; btnFindItem.Focus(); }
            else { btnFindUser.Focus(); }
        }
        private void func_Retrieve_User()
        {
            string RetrieveUser = "Select * from view_EmployeeDivision where pk_Employee_Id = ' " + GlobalClass.GlobalEmployeeId + "'";
            //close current connection
            SysCon.CloseConnection();
            //Open connection
            SysCon.SystemConnect.Open();

            SqlCommand UserFinder = new SqlCommand(RetrieveUser, SysCon.SystemConnect);
            SqlDataReader UserReader = UserFinder.ExecuteReader();

            if (UserReader.Read())
            {
                txtEndUser.Text = UserReader[2].ToString();
                End_User_Unit = UserReader[5].ToString();
            }
            End_User_Id = GlobalClass.GlobalEmployeeId;
            UserReader.Close();
            UserReader.Dispose();
        }


        private void frmPullOut_Load(object sender, EventArgs e)
        {
            Reset_All();
        }

        private void cboReason_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtReceivedBy_Leave(object sender, EventArgs e)
        {
            txtReceivedBy.BackColor = Color.White;
        }

        private void txtReceivedBy_MouseClick(object sender, MouseEventArgs e)
        {
            txtReceivedBy.BackColor = Color.Aquamarine;
        }

        private void txtRemarks_MouseClick(object sender, MouseEventArgs e)
        {
            txtRemarks.BackColor = Color.Aquamarine;
        }

        private void txtRemarks_Leave(object sender, EventArgs e)
        {
            txtRemarks.BackColor = Color.White;
        }

        private void txtNotedBy_MouseClick(object sender, MouseEventArgs e)
        {
            txtNotedBy.BackColor = Color.Aquamarine;
        }

        private void txtNotedBy_MouseLeave(object sender, EventArgs e)
        {
            txtNotedBy.BackColor = Color.White;
        }

        private void txtNotedBy_Leave(object sender, EventArgs e)
        {
            txtNotedBy.BackColor = Color.White;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void func_Retrieve_OIC()
        {
            string RetrieveOIC = "Select * from view_EmployeeDivision where pk_Employee_Id = ' " + GlobalClass.GlobalOICId + "'";
            //close current connection
            SysCon.CloseConnection();
            //Open connection
            SysCon.SystemConnect.Open();

            SqlCommand OICFinder = new SqlCommand(RetrieveOIC, SysCon.SystemConnect);
            SqlDataReader OICReader = OICFinder.ExecuteReader();

            if (OICReader.Read())
            {
                txtReceivingEmployee.Text = OICReader[2].ToString();
            }
            OICReader.Close();
            OICReader.Dispose();
        }
    }
}
