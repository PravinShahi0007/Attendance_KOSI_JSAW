using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Attendance.Forms
{
    public partial class frmMastEmpJoinApproval : Form
    {
        public string mode = "NEW";
        public string GRights = "XXXV";
        public string oldCode = "";

        public frmMastEmpJoinApproval()
        {
            InitializeComponent();
        }

        private void frmMastEmpJoinApproval_Load(object sender, EventArgs e)
        {
            ResetCtrl();
            GRights = Attendance.Classes.Globals.GetFormRights(this.Name);
           
        }

        private string DataValidate()
        {
            string err = string.Empty;

            if (string.IsNullOrEmpty(txtEmpUnqID.Text))
            {
                err = err + "Please Enter Employee Code " + Environment.NewLine;
            }

           
            if (string.IsNullOrEmpty(txtWrkGrpCode.Text))
            {
                err = err + "Please Enter WrkGrpCode " + Environment.NewLine;
            }          

            if (string.IsNullOrEmpty(txtRemarks.Text.Trim()))
            {
                err = err + "Please Enter Remarks" + Environment.NewLine;
            }
            

            return err;
        }

        //approved
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string err = DataValidate();
            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection cn = new SqlConnection(Utils.Helper.constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        cn.Open();
                        cmd.Connection = cn;
                        string sql = "Update MastEmp Set PunchingBlocked = 0 where EmpUnqID ='" + txtEmpUnqID.Text.Trim().ToString() + "'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        sql = "Update EmpBioData Set Blocked = 0 where EmpUnqID ='" + txtEmpUnqID.Text.Trim().ToString() + "'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        sql = "Update MastEmpApproval set Approved = 1, ApprovedBy ='" + Utils.User.GUserID + "', Remark ='" + txtRemarks.Text.Trim().ToString() + "', ApprovedDt = GetDAte() Where EmpUnqID='" + txtEmpUnqID.Text.Trim().ToString() + "'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Approved...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetCtrl();

                    }catch(Exception ex){
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void ResetCtrl()
        {
            btnAdd.Enabled = false;
            txtWrkGrpCode.Text = "";
            txtEmpUnqID.Text = "";
            txtEmpName.Text = "";
            txtRemarks.Text = "";
            lblStatus.Text = "Status : ";
        }

      
        private void btnCancel_Click(object sender, EventArgs e)
        {
            ResetCtrl();
            GRights = Attendance.Classes.Globals.GetFormRights(this.Name);
            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    
        private void txtEmpUnqID_Validated(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEmpUnqID.Text.Trim()))
            {
                btnAdd.Enabled = false;
                return;
            }

            DataSet ds = new DataSet();
            string sql = "select * From MastEmpApproval where EmpUnqID='" + txtEmpUnqID.Text.Trim() + "'";

            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtEmpUnqID.Text = dr["EmpUnqID"].ToString();
                    txtEmpName.Text = Utils.Helper.GetDescription("select EmpName from mastEmp where Empunqid = '" + txtEmpUnqID.Text.Trim() + "'", Utils.Helper.constr);
                    txtWrkGrpCode.Text = Utils.Helper.GetDescription("select WrkGrp from mastEmp where Empunqid = '" + txtEmpUnqID.Text.Trim() + "'", Utils.Helper.constr);
                    
                    txtRemarks.Text = dr["Remark"].ToString();
                    bool approved = Convert.ToBoolean(dr["Approved"]);

                    if (approved)
                    {
                        btnAdd.Enabled = false;                        
                        lblStatus.Text = "Status : Already Approved by " + dr["ApprovedBy"].ToString() + ", Dt:" + Convert.ToDateTime(dr["ApprovedDt"]).ToString("yyyy-MM-dd HH:mm:ss");
                    }else
                    {
                        btnAdd.Enabled = true;
                        lblStatus.Text = "Status : Pending for Approval";
                    }

                    if (GRights != "AUDV")
                    {
                        btnAdd.Enabled = false;
                        lblStatus.Text = "Status : you are not authorised..";
                    }
                }
            }
            else
            {
                lblStatus.Text = "Status : Employee not found in approval process.";
                btnAdd.Enabled = false;
            }


            
        }



    }
}
