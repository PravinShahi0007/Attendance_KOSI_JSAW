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
    public partial class frmMastMessTime : Form
    {
        public string mode = "NEW";
        public string GRights = "XXXV";
        public string oldCode = "";

        public frmMastMessTime()
        {
            InitializeComponent();
        }

        private void frmMastMessTime_Load(object sender, EventArgs e)
        {
            ResetCtrl();
            GRights = Attendance.Classes.Globals.GetFormRights(this.Name);
            SetRights();
            
        }

        private string DataValidate()
        {
            string err = string.Empty;

            if (string.IsNullOrEmpty(txtCompCode.Text))
            {
                err = err + "Please Enter CompCode " + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtCompName.Text))
            {
                err = err + "Please Enter CompName..." + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtUnitCode.Text))
            {
                err = err + "Please Enter Unit Code" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtUnitDesc.Text))
            {
                err = err + "Please Enter Unit Name" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtMessCode.Text))
            {
                err = err + "Please Enter MessCode" + Environment.NewLine;
            }
            else
            {
                string input = txtMessCode.Text.Trim().ToString();
                bool t = Regex.IsMatch(input, @"^\d+$");
                if (!t)
                {
                    err = err + "Please Enter MessCode in Numeric Format..(001,123,012) " + Environment.NewLine;
                }
                

            }
            if (string.IsNullOrEmpty(txtMessDesc.Text))
            {
                err = err + "Please Enter Mess Name" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtFoodCode.Text))
            {
                err = err + "Please Enter FoodCode" + Environment.NewLine;
            }
            
            if (string.IsNullOrEmpty(txtFoodDesc.Text))
            {
                err = err + "Please Enter Food Name" + Environment.NewLine;
            }

            if(txtStartTime.EditValue == null)
            {
                err = err + "Please Enter Start Time" + Environment.NewLine;
            }

            if (txtEndTime.EditValue == null)
            {
                err = err + "Please Enter End Time" + Environment.NewLine;
            }

            return err;
        }

        private void ResetCtrl()
        {
            btnAdd.Enabled = false;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;

            
            object s = new object();
            EventArgs e = new EventArgs();
            txtCompCode.Text = "01";
            txtCompName.Text = "";
            txtCompCode_Validated(s, e);
           
            
            txtUnitCode.Text = "";
            txtUnitDesc.Text = "";
            txtMessCode.Text = "";
            txtMessDesc.Text = "";
            txtFoodCode.Text = "";
            txtFoodDesc.Text = "";
            txtStartTime.EditValue = null;
            txtEndTime.EditValue = null;
            oldCode = "";
            mode = "NEW";
        }
        
        private void SetRights()
        {
            if ( txtFoodCode.Text.Trim() != "" && mode == "NEW" && GRights.Contains("A") )
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
            else if (txtFoodCode.Text.Trim() != "" && mode == "OLD")
            {
                btnAdd.Enabled = false;
                if(GRights.Contains("U"))
                    btnUpdate.Enabled = true;
                if (GRights.Contains("D"))
                    btnDelete.Enabled = true;
            }

            if (GRights.Contains("XXXV"))
            {
                btnAdd.Enabled = false;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void txtCompCode_Validated(object sender, EventArgs e)
        {
            if (txtCompCode.Text.Trim() == "")
            {   
                return;
            }

            DataSet ds = new DataSet();
            string sql = "select * from MastComp where CompCode ='" + txtCompCode.Text.Trim() + "'";

            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtCompCode.Text = dr["CompCode"].ToString();
                    txtCompName.Text = dr["CompName"].ToString();        

                }
            }
            else
            {
                txtCompName.Text = "";
            }
            
        }

        private void txtCompCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1 )
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select CompCode,CompName From MastComp Where 1 = 1";
                if (e.KeyCode == Keys.F1)
                {

                    obj = (List<string>)hlp.Show(sql, "CompCode", "CompCode", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
                   100, 300, 400, 600, 100, 100);
                }

                if (obj.Count == 0)
                {

                    return;
                }
                else if (obj.ElementAt(0).ToString() == "0")
                {
                    return;
                }
                else if (obj.ElementAt(0).ToString() == "")
                {
                    return;
                }
                else
                {

                    txtCompCode.Text = obj.ElementAt(0).ToString();
                    txtCompName.Text = obj.ElementAt(1).ToString();

                }
            }
        }

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
                        string sql = "Insert into MastMessTime (CompCode,UnitCode,MessCode,FoodCode,StartTime,EndTime,AddDt,AddID) " +
                            " Values ('{0}','{1}','{2}','{3}','{4:yyyy-MM-dd HH:mm:ss}','{5:yyyy-MM-dd HH:mm:ss}',GetDate(),'{6}')";
                        sql = string.Format(sql, txtCompCode.Text.Trim().ToString(),
                            txtUnitCode.Text.Trim().ToString(),
                            txtMessCode.Text.Trim().ToString(),
                            txtFoodCode.Text.Trim().ToString().ToUpper(),
                            txtStartTime.Time,txtEndTime.Time,                            
                            Utils.User.GUserID);

                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Record saved...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetCtrl();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void btnUpdate_Click(object sender, EventArgs e)
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
                        string sql = "Update MastMessTime Set StartTime = '{0:yyyy-MM-dd HH:mm:ss}',EndTime = '{1:yyyy-MM-dd HH:mm:ss}', UpdDt = GetDate(), UpdID = '{2}' " +
                            " Where CompCode = '{3}' and UnitCode = '{4}' and MessCode = '{5}' and FoodCode = '{6}' ";

                        sql = string.Format(sql, txtStartTime.Time,txtEndTime.Time,
                             Utils.User.GUserID, txtCompCode.Text.Trim().ToString(), 
                             txtUnitCode.Text.Trim(), txtMessCode.Text.Trim(),
                             txtFoodCode.Text.Trim()
                           );

                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Record Updated...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetCtrl();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string err = DataValidate();
            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Not Implemented...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ResetCtrl();
            GRights = Attendance.Classes.Globals.GetFormRights(this.Name);
            SetRights();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtUnitCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompCode.Text.Trim() == "")
                return;

            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select UnitCode,UnitName From MastUnit Where CompCode ='" + txtCompCode.Text.Trim() + "' ";
                if (e.KeyCode == Keys.F1)
                {

                    obj = (List<string>)hlp.Show(sql, "UnitCode", "UnitCode", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
                   100, 300, 400, 600, 100, 100);
                }

                if (obj.Count == 0)
                {

                    return;
                }
                else if (obj.ElementAt(0).ToString() == "0")
                {
                    return;
                }
                else if (obj.ElementAt(0).ToString() == "")
                {
                    return;
                }
                else
                {

                    txtUnitCode.Text = obj.ElementAt(0).ToString();
                    txtUnitDesc.Text = obj.ElementAt(1).ToString();
                   
                }
            }
        }

        private void txtUnitCode_Validated(object sender, EventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtCompName.Text.Trim() == "" )
            {

                return;
            }

            txtUnitCode.Text = txtUnitCode.Text.Trim().ToString().PadLeft(3, '0');

            DataSet ds = new DataSet();
            string sql = "select * From MastUnit where CompCode ='" + txtCompCode.Text.Trim() + "' " +
                    " and  UnitCode ='" + txtUnitCode.Text.Trim() + "'";

            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtCompCode.Text = dr["CompCode"].ToString();
                    txtUnitCode.Text = dr["UnitCode"].ToString();
                    txtUnitDesc.Text = dr["UnitName"].ToString();
                    txtCompCode_Validated(sender, e);
                    
                }
            }
        }

        private void txtMessCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" ||  txtUnitCode.Text.Trim() == "")
                return;

            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select MessCode,MessDesc From MastMess Where CompCode ='" + txtCompCode.Text.Trim() + "' " + 
                    " and  UnitCode ='" + txtUnitCode.Text.Trim() + "'";
                if (e.KeyCode == Keys.F1)
                {

                    obj = (List<string>)hlp.Show(sql, "MessCode", "MessCode", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
                   100, 300, 400, 600, 100, 100);
                }

                if (obj.Count == 0)
                {

                    return;
                }
                else if (obj.ElementAt(0).ToString() == "0")
                {
                    return;
                }
                else if (obj.ElementAt(0).ToString() == "")
                {
                    return;
                }
                else
                {

                    txtMessCode.Text = obj.ElementAt(0).ToString();
                    txtMessDesc.Text = obj.ElementAt(1).ToString();

                }
            }
        }

        private void txtMessCode_Validated(object sender, EventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtCompName.Text.Trim() == "" || txtUnitCode.Text.Trim() == "" || txtUnitDesc.Text.Trim() == "")
            {

                return;
            }

            txtMessCode.Text = txtMessCode.Text.Trim().ToString().PadLeft(3, '0');

            DataSet ds = new DataSet();
            string sql = "select * From MastMess where CompCode ='" + txtCompCode.Text.Trim() + "' " +
                    " and UnitCode ='" + txtUnitCode.Text.Trim() + "' and MessCode ='" + txtMessCode.Text.Trim() + "'";
                    
            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtCompCode.Text = dr["CompCode"].ToString();
                    txtUnitCode.Text = dr["UnitCode"].ToString();
                    txtMessCode.Text = dr["MessCode"].ToString();
                    txtMessDesc.Text = dr["MessDesc"].ToString();
                    txtCompCode_Validated(sender, e);
                    txtUnitCode_Validated(sender, e);
                    
                }
            }

        }
        
        private void txtFoodCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtUnitCode.Text.Trim() == "")
                return;

            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";
                                
                if (e.KeyCode == Keys.F1)
                {
                    sql = "Select FoodCode,FoodDesc From MastMessFood Where CompCode ='" + txtCompCode.Text.Trim() + "' " +
                    " and  UnitCode ='" + txtUnitCode.Text.Trim() + "' and MessCode ='" + txtMessCode.Text.Trim() + "'";
                    obj = (List<string>)hlp.Show(sql, "FoodCode", "FoodCode", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
                   100, 300, 400, 600, 100, 100);
                }
                else
                {
                    sql = "Select FoodCode,FoodDesc From MastMessFood Where CompCode ='" + txtCompCode.Text.Trim() + "' " +
                    " and  UnitCode ='" + txtUnitCode.Text.Trim() + "' and MessCode = '" + txtMessCode.Text.Trim() + "'" ;
                    obj = (List<string>)hlp.Show(sql, "FoodDesc", "FoodDesc", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
                   100, 300, 400, 600, 100, 100);
                }

                if (obj.Count == 0)
                {

                    return;
                }
                else if (obj.ElementAt(0).ToString() == "0")
                {
                    return;
                }
                else if (obj.ElementAt(0).ToString() == "")
                {
                    return;
                }
                else
                {

                    txtFoodCode.Text = obj.ElementAt(0).ToString();
                    txtFoodDesc.Text = obj.ElementAt(1).ToString();

                }
            }
        }

        private void txtFoodCode_Validated(object sender, EventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtCompName.Text.Trim() == "" || 
                txtUnitCode.Text.Trim() == "" || txtUnitDesc.Text.Trim() == "" ||
                txtFoodCode.Text.Trim() == "" )
            {

                return;
            }

            txtMessCode.Text = txtMessCode.Text.Trim().ToString().PadLeft(3, '0');

            DataSet ds = new DataSet();
            string sql = "select a.*,b.FoodDesc From MastMessTime a , MastMessFood b where a.CompCode ='" + txtCompCode.Text.Trim() + "' " +
                    " and a.UnitCode ='" + txtUnitCode.Text.Trim() + "'"
                    + " and a.MessCode ='" + txtMessCode.Text.Trim() + "' "
                    + " and a.FoodCode ='" + txtFoodCode.Text.Trim() + "' "
                    + " and a.CompCode = b.CompCode "
                    + " and a.UnitCode = b.UnitCode "
                    + " and a.MessCode = b.MessCode "
                    + " and a.FoodCode = b.FoodCode ";

            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtCompCode.Text = dr["CompCode"].ToString();
                    txtUnitCode.Text = dr["UnitCode"].ToString();
                    txtMessCode.Text = dr["MessCode"].ToString();
                    txtFoodCode.Text = dr["FoodCode"].ToString();
                    txtFoodDesc.Text = dr["FoodDesc"].ToString();
                    txtStartTime.Time = Convert.ToDateTime(dr["StartTime"]);
                    txtEndTime.Time = Convert.ToDateTime(dr["EndTime"]);
                    txtCompCode_Validated(sender, e);
                    txtUnitCode_Validated(sender, e);
                    txtMessCode_Validated(sender, e);
                    
                    mode = "OLD";
                    oldCode = dr["FoodCode"].ToString();

                }
            }
            else
            {
                mode = "NEW";
                oldCode = "";
            }

            SetRights();
        }
    }
}
