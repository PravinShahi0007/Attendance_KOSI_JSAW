﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Attendance.Classes;

namespace Attendance.Forms
{
    public partial class frmMastEmpBasicData : Form
    {
        public string mode = "NEW";
        public string GRights = "XXXV";
        public string oldCode = "";
        public bool dupadhar = false;
        public string dupadharemp = string.Empty;
       
        public frmMastEmpBasicData()
        {
            InitializeComponent();
        }

        private void frmMastWrkGrp_Load(object sender, EventArgs e)
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


            if (string.IsNullOrEmpty(txtEmpUnqID.Text))
            {
                err = err + "Please Enter EmpUnqID " + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtEmpName.Text))
            {
                err = err + "Please Enter EmpName..." + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtFatherName.Text))
            {
                err = err + "Please Enter FatherName..." + Environment.NewLine;
            }
            
            if (string.IsNullOrEmpty(txtWrkGrpCode.Text))
            {
                err = err + "Please Enter WrkGrpCode " + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtWrkGrpDesc.Text))
            {
                err = err + "Please Enter WrkGrp Description" + Environment.NewLine;
            }
            if (string.IsNullOrEmpty(txtUnitCode.Text))
            {
                err = err + "Please Enter UnitCode " + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtUnitDesc.Text))
            {
                err = err + "Please Enter Unit Description" + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(txtAdharNo.Text))
            {
                err = err + "Please Enter Adhar No" + Environment.NewLine;
            }


            if(txtAdharNo.Text.Trim().ToString().Length < 12) {
                err = err + "Please Enter 12 digit Adhar No" + Environment.NewLine;
            }
        

            if(txtJoinDt.EditValue == null){
                err = err + "Please Enter JoinDate" + Environment.NewLine;
            }

            if(txtBirthDT.EditValue == null){
                err = err + "Please Enter BirthDate" + Environment.NewLine;
            }

            if (txtWrkGrpCode.Text.Trim() != "COMP")
            {
                if (txtValidFrom.EditValue == null)
                {
                    err = err + "Please Enter Valid From " + Environment.NewLine;
                }

                if (txtValidTo.EditValue == null)
                {
                    err = err + "Please Enter Valid To.." + Environment.NewLine;
                }

                if (txtValidFrom.DateTime == DateTime.MinValue)
                {
                    err = err + "Please Enter Valid From Date..." + Environment.NewLine;
                    return err;
                }


                if (txtValidTo.DateTime == DateTime.MinValue)
                {
                    err = err + "Please Enter Valid To Date..." + Environment.NewLine;
                    return err;
                }

                if (txtValidFrom.DateTime > txtValidTo.DateTime)
                {
                    err = err + "Valid From Date must be less than Valid To Date..." + Environment.NewLine;
                    return err;
                }


                if (txtValidFrom.DateTime < txtJoinDt.DateTime)
                {
                    err = err + "Valid From Date must be grator than Join Date..." + Environment.NewLine;
                    return err;
                }

                if (txtValidTo.DateTime < txtJoinDt.DateTime)
                {
                    err = err + "Valid To Date must be grator than Join Date..." + Environment.NewLine;
                    return err;
                }

                

            }
            else
            {
                txtValidFrom.EditValue = null;
                txtValidTo.EditValue = null;
            }
            

            if (txtJoinDt.DateTime == DateTime.MinValue)
            {
                err = err + "Please Enter Valid Join Date..." + Environment.NewLine;
                return err;
            }

            if (txtBirthDT.DateTime == DateTime.MinValue)
            {
                err = err + "Please Enter Valid Birth Date..." + Environment.NewLine;
                return err;
            }

            if (txtJoinDt.DateTime < txtBirthDT.DateTime)
            {
                err = err + "Birth Date must be less than JoinDate Date..." + Environment.NewLine;
                return err;
            }



            

            if(chkComp.Checked && chkCont.Checked)
            {
                err = err + "Please Select none or One of (Company/Contract)..." + Environment.NewLine;
                return err;
            }


            //check for duplicate adharno..
            DataSet ds = new DataSet();
            string sql = "select EmpUnqID,EmpName from MastEmp where CompCode ='" + txtCompCode.Text.Trim() + "' " +
                " and AdharNo = '" + txtAdharNo.Text.Trim() + "' and EmpUnqID not in ('" + txtEmpUnqID.Text.Trim() + "')";

            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {                      
                    dupadhar = true;
                    dupadharemp = dr["EmpUnqID"].ToString() + "," + dr["EmpName"].ToString() + Environment.NewLine;
                }
            }
            else
            {
                dupadhar = false;
                dupadharemp = string.Empty;
            }

            if (chkCont.Checked == true)
            {
                if (String.IsNullOrEmpty(txtBasic.Text))
                {
                    err = err + "Plase Enter Contract Employee Basic..." + Environment.NewLine;
                    return err;
                }
            }

            string blacklist = Utils.Helper.GetDescription("Select AdharNo from MastEmpBlackList where BlackList = 1 and AdharNo='" + txtAdharNo.Text.Trim() + "'", Utils.Helper.constr);
            if (!string.IsNullOrEmpty(blacklist))
            {
                err = err + "This Adhar Card is Black Listed..." + Environment.NewLine;
                return err;
            }

            if (string.IsNullOrEmpty(txtCostCode.Text.Trim().ToString()) || string.IsNullOrEmpty(txtCostDesc.Text.Trim().ToString()))
            {
                err = err + "CostCode Assignment is required..." + Environment.NewLine;
            }



            return err;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string err = DataValidate();
            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(mode == "NEW" && dupadhar )
            {
                string msg  ="This Adhar No is Already Registered with " + dupadharemp + Environment.NewLine
                    + " Are you sure to Insert this as New Employee ?" ;

                DialogResult qdr = MessageBox.Show(msg, "Question",MessageBoxButtons.YesNo,MessageBoxIcon.Question);

                if (qdr == DialogResult.No)
                {
                    return;
                }

            }

            Cursor.Current = Cursors.WaitCursor;
            GrpMain.Enabled = false;

            using (SqlConnection cn = new SqlConnection(Utils.Helper.constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        if(string.IsNullOrEmpty(txtBasic.Text.Trim()))
                        {
                            txtBasic.Text = "0";
                        }
                        
                        cn.Open();
                        cmd.Connection = cn;
                        string sql = "Insert into MastEmp (CompCode,WrkGrp,EmpUnqID,EmpName,FatherName," +
                            " UnitCode,MessCode,MessGrpCode,BirthDt,JoinDt,ValidFrom,ValidTo," +
                            " ADHARNO,IDPRF3,IDPRF3No,Sex,ContractFlg,PayrollFlg,OTFLG,Weekoff,Active,AddDt,AddID,Basic,CostCode) Values (" +
                            "'{0}','{1}','{2}','{3}','{4}' ," +
                            " '{5}',{6},{7},'{8}','{9}',{10},{11}," +
                            " '{12}','ADHARCARD','{13}','{14}','{15}','{16}','{17}','{18}','1',GetDate(),'{19}','{20}','{21}')";
 
                        sql = string.Format(sql, txtCompCode.Text.Trim().ToString(), txtWrkGrpCode.Text.Trim().ToString(),txtEmpUnqID.Text.Trim().ToString(),txtEmpName.Text.Trim().ToString(),txtFatherName.Text.Trim(),
                            txtUnitCode.Text.ToString(),((txtMessCode.Text.Trim() == "")? "null" :"'"+txtMessCode.Text.Trim()+"'"),
                            ((txtMessGrpCode.Text.Trim() == "")? "null" :"'"+txtMessGrpCode.Text.Trim()+"'"),
                            txtBirthDT.DateTime.ToString("yyyy-MM-dd"),txtJoinDt.DateTime.ToString("yyyy-MM-dd"),
                           ((txtWrkGrpCode.Text.Trim() == "COMP") ? "null" : "'" + txtValidFrom.DateTime.ToString("yyyy-MM-dd") + "'"),
                             ((txtWrkGrpCode.Text.Trim() == "COMP") ? "null" : "'" + txtValidTo.DateTime.ToString("yyyy-MM-dd") + "'"),
                             txtAdharNo.Text.Trim(),txtAdharNo.Text.Trim(),((Convert.ToBoolean(txtGender.EditValue))?1:0),
                            ((chkCont.Checked)?1:0),((chkComp.Checked)?1:0),((chkOTFlg.Checked)?1:0),txtWeekOff.Text.Trim(),
                            Utils.User.GUserID, txtBasic.Text.Trim(), txtCostCode.Text.Trim().ToString());

                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        //insert into costcode
                        sql = "Insert into MastCostCodeEmp (EmpUnqID,ValidFrom,CostCode,AddDt,AddID) Values ('" + txtEmpUnqID.Text.Trim().ToString() + "','" + txtJoinDt.DateTime.ToString("yyyy-MM-dd") + "','" + txtCostCode.Text.Trim().ToString() + "',GetDate(),'" + Utils.User.GUserID + "')";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        //new addition - 11/01/2022 - Contract employee must be blocked while new entry as this function is handled by thrid party.
                        if (txtWrkGrpCode.Text.Trim().ToUpper() == "CONT")
                        {
                            int tmaxid = Convert.ToInt32(Utils.Helper.GetDescription("Select isnull(Max(ID),0) + 1 from MastMachineUserOperation", Utils.Helper.constr));
                            string tEmpUnqID = txtEmpUnqID.Text.Trim().ToString();
                            
                            sql = "select MachineIP,IOFLG from readerconfig where canteenflg = 0  and [master] = 0 and compcode = '01' and Active = 1  ";
                            DataSet ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
                            bool hasRows = ds.Tables.Cast<DataTable>().Any(table => table.Rows.Count != 0);

                             if (hasRows)
                             {
                                 foreach (DataRow dr in ds.Tables[0].Rows)
                                 {

                                     sql = "insert into MastMachineUserOperation (ID,EmpUnqID,MachineIP,IOFLG,Operation,ReqDt,ReqBy,DoneFlg,AddDt,Remarks) Values ('" + tmaxid + "','" +
                                         tEmpUnqID + "','" + dr["MachineIP"].ToString() + "','" + dr["IOFLG"].ToString() + "','BLOCK',GetDate(),'" + Utils.User.GUserID + "',0,GetDate(),'Cont-Emp-New-Entry')";


                                     cmd.CommandText = sql;
                                     cmd.ExecuteNonQuery();

                                 }
                               
                             }

                             cmd.CommandText = "Update MastEmp Set PunchingBlocked = 1 where CompCode = '01' and EmpUnqID = '" + tEmpUnqID + "'";
                             cmd.ExecuteNonQuery();

                             cmd.CommandText = "Update EmpBioData Set Blocked = 1 where EmpUnqID = '" + tEmpUnqID + "'";
                             cmd.ExecuteNonQuery();

                             cmd.CommandText = "Insert into MastEmpApproval (EmpUnqID,AddDt,Approved,AddID) Values ('" + tEmpUnqID + "',GetDate(),0,'" + Utils.User.GUserID + "')";
                             cmd.ExecuteNonQuery();
                             
                             //SEND EMAIL NOTIFICATION. FOR NEW JOINING OF EMPLOYEE.
                             sendmail("", tEmpUnqID);

                        }



                        //createmuster
                        clsEmp t = new clsEmp();
                        string err2 = string.Empty;
                        if(t.GetEmpDetails(txtCompCode.Text.Trim(), txtEmpUnqID.Text.Trim()))
                        {
                            DateTime sFromDt, sToDt, sCurDt;
                            sCurDt = Convert.ToDateTime(Utils.Helper.GetDescription("Select GetDate()",Utils.Helper.constr));
                            if (txtJoinDt.DateTime.Year < sCurDt.Year)
                            {
                                sFromDt = Convert.ToDateTime(Utils.Helper.GetDescription("Select CalendarStartOfYearDate from dbo.F_TABLE_DATE(GetDate(),GetDate())", Utils.Helper.constr));
                                sToDt = Convert.ToDateTime(Utils.Helper.GetDescription("Select CalendarEndOfYearDate from dbo.F_TABLE_DATE(GetDate(),GetDate())", Utils.Helper.constr));
                            }
                            else
                            {
                                sFromDt = txtJoinDt.DateTime;
                                sToDt = Convert.ToDateTime(Utils.Helper.GetDescription("Select CalendarEndOfYearDate from dbo.F_TABLE_DATE('" + sFromDt.ToString("yyyy-MM-dd") + "','" + sFromDt.ToString("yyyy-MM-dd") + "')", Utils.Helper.constr));
                            }
                            

                            if (!t.CreateMuster(sFromDt, sToDt, out err2))
                            {
                                MessageBox.Show(err, "Error While Creating Muster Table", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            
                        }
                        if(string.IsNullOrEmpty(err2))
                            MessageBox.Show("Record saved...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBox.Show("Record saved with error please check muster table created...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        ResetCtrl();

                    }catch(Exception ex){
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            GrpMain.Enabled = true;

            Cursor.Current = Cursors.Default;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string err = DataValidate();
            bool WrkGrpChange = false;
            bool UnitCodeChange = false;
            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dupadhar)
            {
                string msg = "This Adhar No is Already Registered with " + dupadharemp + Environment.NewLine
                    + " Are you sure to Update this Adhar No ?";

                DialogResult qdr = MessageBox.Show(msg, "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (qdr == DialogResult.No)
                {
                    return;
                }

            }


            GrpMain.Enabled = false;

            Cursor.Current = Cursors.WaitCursor;
            
            using (SqlConnection cn = new SqlConnection(Utils.Helper.constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        cn.Open();
                        cmd.Connection = cn;

                        string sql = "insert into MastEmpHistory " +
                           " select 'Before Update Master Data, Action By " + Utils.User.GUserID + "', GetDate(), * from MastEmp where CompCode = '" + txtCompCode.Text.Trim() + "' " +
                           " and EmpUnqID ='" + txtEmpUnqID.Text.Trim() + "'";
                        
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();


                        clsEmp t = new clsEmp();
                        t.CompCode = txtCompCode.Text.Trim();
                        t.EmpUnqID = txtEmpUnqID.Text.Trim();
                        t.GetEmpDetails(t.CompCode, t.EmpUnqID);

                        //WrkGrp is Changed.. need to process in all tables..
                        if (t.WrkGrp != txtWrkGrpCode.Text.Trim())
                        {

                            try
                            {
                                sql = "Exec ChangeWrkGrp '" + t.EmpUnqID + "','" + txtWrkGrpCode.Text.Trim() + "';";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                                WrkGrpChange = true;
                            }
                            catch (Exception ex)
                            {

                                WrkGrpChange = false;
                                GrpMain.Enabled = true;
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Kindly Clear the Job Profile First,(EmpTypeCode,CatCode,GradeCode,DesgCode,DeptCode,StatCode)" + Environment.NewLine +
                                   "and try again..."
                               , "WrkGrp Change Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;
                            }


                        }
                        else if (t.UnitCode != txtUnitCode.Text.Trim())
                        {
                            try
                            {
                                sql = "Exec sp_ChangeUnitCode '" + t.EmpUnqID + "','" + txtUnitCode.Text.Trim() + "';";
                                cmd.CommandText = sql;
                                cmd.ExecuteNonQuery();
                                UnitCodeChange = true;
                            }
                            catch (Exception ex)
                            {

                                UnitCodeChange = false;
                                GrpMain.Enabled = true;
                                Cursor.Current = Cursors.Default;
                                MessageBox.Show("Kindly Clear the Job Profile First,(EmpTypeCode,CatCode,GradeCode,DesgCode,DeptCode,StatCode)" + Environment.NewLine +
                                   "and try again..."
                               , "UnitCode Change Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;
                            }
                        }
                        

                        if (string.IsNullOrEmpty(txtBasic.Text.Trim()))
                        {
                            txtBasic.Text = "0";
                        }

                        sql = "Update MastEmp set WrkGrp ='{0}',EmpName='{1}',FatherName = '{2}'," +
                            " UnitCode = '{3}',MessCode={4},MessGrpCode = {5},BirthDt ='{6}',JoinDt ='{7}',ValidFrom = {8},ValidTo = {9}," +
                            " ADHARNO = '{10}',IDPRF3No = '{11}',Sex='{12}',ContractFlg='{13}',PayrollFlg='{14}',OTFLG='{15}',Weekoff='{16}',UpdDt=GetDate(),UpdID ='{17}', Basic='{18}' Where " +
                            " CompCode ='{19}' and EmpUnqID = '{20}'";


                        sql = string.Format(sql,  txtWrkGrpCode.Text.Trim().ToString(), txtEmpName.Text.Trim().ToString(), txtFatherName.Text.Trim(),
                            txtUnitCode.Text.ToString(), ((txtMessCode.Text.Trim() == "") ? "null" : "'" + txtMessCode.Text.Trim() + "'"),
                            ((txtMessGrpCode.Text.Trim() == "") ? "null" : "'" + txtMessGrpCode.Text.Trim() + "'"),
                            txtBirthDT.DateTime.ToString("yyyy-MM-dd"), txtJoinDt.DateTime.ToString("yyyy-MM-dd"),
                            ((txtWrkGrpCode.Text.Trim() == "COMP")? "null" :"'" + txtValidFrom.DateTime.ToString("yyyy-MM-dd")+"'"), 
                             ((txtWrkGrpCode.Text.Trim() == "COMP")? "null" :"'" + txtValidTo.DateTime.ToString("yyyy-MM-dd")+"'"),
                             txtAdharNo.Text.Trim(), txtAdharNo.Text.Trim(), ((Convert.ToBoolean(txtGender.EditValue))?1:0),
                            ((chkCont.Checked) ? 1 : 0), ((chkComp.Checked) ? 1 : 0), ((chkOTFlg.Checked) ? 1 : 0), txtWeekOff.Text.Trim(),
                            Utils.User.GUserID,
                            txtBasic.Text.Trim(),
                            txtCompCode.Text.Trim(),txtEmpUnqID.Text.Trim()
                            
                            );

                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();


                        if(WrkGrpChange || UnitCodeChange){
                            MessageBox.Show("Employee Job Profile is Discarded.." + Environment.NewLine +
                                    "Please Fill the Employee Job Profile Again.."                                
                                , "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        MessageBox.Show("Record Updated...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetCtrl();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            GrpMain.Enabled = true;

            Cursor.Current = Cursors.Default;
        }

        private void ResetCtrl()
        {
            btnAdd.Enabled = false;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dupadhar = false;
            dupadharemp = string.Empty;

            object s = new object();
            EventArgs e = new EventArgs();
            txtCompCode.Text = "01";
            txtCompName.Text = "";
            txtCompCode_Validated(s, e);
            txtEmpUnqID.Text = "";
            txtEmpName.Text = "";
            txtFatherName.Text = "";
            txtAdharNo.Text = "";
            
            txtCostDesc.Text = "";
            txtCostCode.Text = "";

            txtWrkGrpCode.Text = "";
            txtWrkGrpDesc.Text = "";
            txtUnitCode.Text = "";
            txtUnitDesc.Text = "";
            txtMessGrpCode.Text = "";
            txtMessGrpDesc.Text = "";
            txtMessCode.Text = "";
            txtMessDesc.Text = "";
            txtWeekOff.Text = "";
            
            txtBirthDT.EditValue = null;
            txtJoinDt.EditValue = null;
            txtValidFrom.EditValue = null;
            txtValidTo.EditValue = null;
            txtBasic.Text = "";

            txtGender.EditValue = true;
            chkActive.Checked = false;
            chkComp.Checked = false;
            chkCont.Checked = false;
            chkOTFlg.Checked = false;
            GrpMain.Enabled = true;

            lblLeft.Visible = false;

            oldCode = "";
            mode = "NEW";
        }

        private void SetRights()
        {
            if ( txtEmpUnqID.Text.Trim() != "" && mode == "NEW" && GRights.Contains("A") )
            {
                btnAdd.Enabled = true;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
            else if (txtEmpUnqID.Text.Trim() != "" && mode == "OLD")
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

        private void txtWrkGrpCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompCode.Text.Trim() == "")
                return;
            
            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select WrkGrp,WrkGrpDesc From MastWorkGrp Where CompCode ='" + txtCompCode.Text.Trim() + "'";
                if (e.KeyCode == Keys.F1)
                {

                    obj = (List<string>)hlp.Show(sql, "WrkGrp", "WrkGrp", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
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

                    txtWrkGrpCode.Text = obj.ElementAt(0).ToString();
                    txtWrkGrpDesc.Text = obj.ElementAt(1).ToString();
                    
                    mode = "OLD";
                }
            }
        }

        private void txtWrkGrpCode_Validated(object sender, EventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtCompName.Text.Trim() == "")
            {
                
                return;
            }

            DataSet ds = new DataSet();
            string sql = "select * From MastWorkGrp where CompCode ='" + txtCompCode.Text.Trim() + "' and WrkGrp='" + txtWrkGrpCode.Text.Trim() + "'";
            
            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtCompCode.Text = dr["CompCode"].ToString();
                    txtWrkGrpCode.Text = dr["WrkGrp"].ToString();
                    txtWrkGrpDesc.Text = dr["WrkGrpDesc"].ToString();
                    
                    txtCompCode_Validated(sender,e);
                    
                }
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

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string err = DataValidate();
            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            GrpMain.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            using (SqlConnection cn = new SqlConnection(Utils.Helper.constr))
            {
                try
                {
                    cn.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(err, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                
                SqlTransaction tr = cn.BeginTransaction("DeleteEmp");
                
                
                try
                {
                   
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        cmd.Transaction = tr;

                        string sql = "insert into MastEmpHistory " +
                           " select 'Before Delete Master Data, Action By " + Utils.User.GUserID + "',GetDate(),* from MastEmp where CompCode = '" + txtCompCode.Text.Trim() + "' " +
                           " and EmpUnqID ='" + txtEmpUnqID.Text.Trim() + "' ";

                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from AttdData where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from MastEmpBio where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from LeaveEntry where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from MastEmpFamily  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from MastEmpExp  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from MastEmpEDU  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from MastEmpPPE  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from MastLeaveSchedule  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();


                        //cmd.CommandText = "Delete from MastShiftSchedule  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        ////cmd.CommandText = "Delete from ATTDLOG  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        ////cmd.ExecuteNonQuery();

                        //cmd.CommandText = "Delete from LeaveBal  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        //cmd.ExecuteNonQuery();

                        cmd.CommandText = "Delete from MastEmp  where EmpUnqID = '" + txtEmpUnqID.Text.Trim() + "'";
                        cmd.ExecuteNonQuery();

                        tr.Commit();

                        MessageBox.Show("Record Deleted Sucessfull...", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    tr.Rollback();

                    MessageBox.Show(err + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            }


            GrpMain.Enabled = true;
            Cursor.Current = Cursors.Default;
            ResetCtrl();
            SetRights();
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

        private void txtEmpUnqID_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompCode.Text.Trim() == "")
                return;

            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select EmpUnqID,EmpName,WrkGrp,CompCode From MastEmp Where CompCode ='" + txtCompCode.Text.Trim() + "' ";
                if (e.KeyCode == Keys.F1)
                {
                    obj = (List<string>)hlp.Show(sql, "EmpUnqID", "EmpUnqID", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
                    100, 300, 400, 600, 100, 100);
                }
                else
                {
                    obj = (List<string>)hlp.Show(sql, "EmpName", "EmpName", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
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

                    txtCompCode.Text = obj.ElementAt(3).ToString();
                    txtEmpUnqID.Text = obj.ElementAt(0).ToString();
                    txtEmpUnqID_Validated(sender, e);
                }
            }
        }

        private void txtEmpUnqID_Validated(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEmpUnqID.Text.Trim()) || string.IsNullOrEmpty(txtCompCode.Text.Trim()))
            {
                mode = "NEW";
                oldCode = "";
                return;
            }

            clsEmp t = new clsEmp();
            t.CompCode = txtCompCode.Text.Trim();
            t.EmpUnqID = txtEmpUnqID.Text.Trim();
            bool isold = t.GetEmpDetails(t.CompCode, t.EmpUnqID);

            if (isold)
            {
                
                DisplayData(t);
            }
            else
            {
                mode = "NEW";
                oldCode = "";
            }
            SetRights();
        }

        private void DisplayData(clsEmp cEmp)
        {

            txtEmpName.Text = cEmp.EmpName;
            txtFatherName.Text = cEmp.FatherName;
            txtGender.EditValue = cEmp.Gender;
            chkActive.Checked = cEmp.Active;
            chkComp.Checked = cEmp.PayrollFlg;
            chkCont.Checked = cEmp.ContFlg;
            chkOTFlg.Checked = cEmp.OTFLG;
           
            txtWrkGrpCode.Text = cEmp.WrkGrp;
            txtWrkGrpDesc.Text = cEmp.WrkGrpDesc;
            txtUnitCode.Text = cEmp.UnitCode;
            txtUnitDesc.Text = cEmp.UnitDesc;
            txtMessCode.Text = cEmp.MessCode;
            txtMessDesc.Text = cEmp.MessDesc;
            txtMessGrpCode.Text = cEmp.MessGrpCode;
            txtMessGrpDesc.Text = cEmp.MessGrpDesc;
            
            txtAdharNo.Text = cEmp.AdharNo;

            txtCostCode.Text = cEmp.CostCode;
            txtCostDesc.Text = cEmp.CostDesc;


            txtJoinDt.EditValue = cEmp.JoinDt;
            txtValidFrom.EditValue = cEmp.ValidFrom;
            txtValidTo.EditValue = cEmp.ValidTo;
            txtBirthDT.EditValue = cEmp.BirthDt;
            txtWeekOff.Text = cEmp.WeekOffDay;

            if (cEmp.Active)
            {
                lblLeft.Visible = false;
            }
            else
            {
                lblLeft.Visible = true;
            }

            txtBasic.Text = Utils.Helper.GetDescription("Select Basic from MastEmp where EmpUnqID ='" + txtEmpUnqID.Text.Trim() + "'", Utils.Helper.constr);

            mode = "OLD";
            oldCode = cEmp.EmpUnqID;
        }

        private void txtCostCode_Validated(object sender, EventArgs e)
        {
            if (txtCostCode.Text.Trim() == "")
            {
                return;
            }

            DataSet ds = new DataSet();
            string sql = "select * from MastCostCode where CostCode ='" + txtCostCode.Text.Trim() + "'";

            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtCostCode.Text = dr["CostCode"].ToString();
                    txtCostDesc.Text = dr["CostDesc"].ToString();

                }
            }
            else
            {
                txtCostCode.Text = "";
                txtCostDesc.Text = "";
            }

        }

        private void txtCostCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select CostCode,CostDesc From MastCostCode Where Active = 1";
                if (e.KeyCode == Keys.F1)
                {

                    obj = (List<string>)hlp.Show(sql, "CostCode", "CostCode", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
                   100, 300, 400, 600, 100, 100);
                }
                else
                {
                    obj = (List<string>)hlp.Show(sql, "CostDesc", "CostDesc", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
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

                    txtCostCode.Text = obj.ElementAt(0).ToString();
                    txtCostDesc.Text = obj.ElementAt(1).ToString();

                }
            }
        }

        private void txtUnitCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtWrkGrpCode.Text.Trim() == "")
                return;

            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select UnitCode,UnitName From MastUnit Where CompCode ='" + txtCompCode.Text.Trim() + "' and WrkGrp ='" + txtWrkGrpCode.Text.Trim() + "'";
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
            if (txtCompCode.Text.Trim() == "" || txtCompName.Text.Trim() == "")
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
            if (txtCompCode.Text.Trim() == "" || txtUnitCode.Text.Trim() == "")
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

        private void txtMessGrpCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtUnitCode.Text.Trim() == "")
                return;

            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F2)
            {
                List<string> obj = new List<string>();

                Help_F1F2.ClsHelp hlp = new Help_F1F2.ClsHelp();
                string sql = "";


                sql = "Select MessGrpCode,MessGrpDesc From MastMessGrp Where CompCode ='" + txtCompCode.Text.Trim() + "' " +
                    " and  UnitCode ='" + txtUnitCode.Text.Trim() + "'";
                if (e.KeyCode == Keys.F1)
                {

                    obj = (List<string>)hlp.Show(sql, "MessGrpCode", "MessGrpCode", typeof(string), Utils.Helper.constr, "System.Data.SqlClient",
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

                    txtMessGrpCode.Text = obj.ElementAt(0).ToString();
                    txtMessGrpDesc.Text = obj.ElementAt(1).ToString();

                }
            }
        }

        private void txtMessGrpCode_Validated(object sender, EventArgs e)
        {
            if (txtCompCode.Text.Trim() == "" || txtCompName.Text.Trim() == "" || txtUnitCode.Text.Trim() == "" || txtUnitDesc.Text.Trim() == "")
            {

                return;
            }

            

            DataSet ds = new DataSet();
            string sql = "select * From MastMessGrp where CompCode ='" + txtCompCode.Text.Trim() + "' " +
                    " and UnitCode ='" + txtUnitCode.Text.Trim() + "' and MessGrpCode= '" + txtMessGrpCode.Text.Trim() + "'";

            ds = Utils.Helper.GetData(sql, Utils.Helper.constr);
            bool hasRows = ds.Tables.Cast<DataTable>()
                           .Any(table => table.Rows.Count != 0);

            if (hasRows)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    txtCompCode.Text = dr["CompCode"].ToString();
                    txtUnitCode.Text = dr["UnitCode"].ToString();
                    txtMessGrpCode.Text = dr["MessGrpCode"].ToString();
                    txtMessGrpDesc.Text = dr["MessGrpDesc"].ToString();
                    txtCompCode_Validated(sender, e);
                    txtUnitCode_Validated(sender, e);
                    

                }
            }
            
        }

        private void frmMastEmpBasicData_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyData == Keys.Enter))
            {
                SelectNextControl(ActiveControl, true, true, true, true);
            }
        }

        private void chkCont_Validated(object sender, EventArgs e)
        {
            
        }

        private void sendmail(string mailtype, string tEmpUnqID)
        {
            string sql = "Select Config_Val from Mast_OtherConfig where Config_Key ='CONTNEWJOIN'";

            string tsubject = string.Empty;
            string tbody = string.Empty;
            string to = Utils.Helper.GetDescription(sql, Utils.Helper.constr);
            
            string partbody = string.Empty;

            //to = "anand.acharya@jindalsaw.com";
            string cc = Utils.Helper.GetDescription("Select Config_Val from Mast_OtherConfig Where Config_Key ='CONTNEWJOIN_CC'", Utils.Helper.constr);
            //string bcc = "anand.acharya@jindalsaw.com";
            string bcc = Utils.Helper.GetDescription("Select Config_Val from Mast_OtherConfig Where Config_Key ='CONTNEWJOIN_BCC'", Utils.Helper.constr); ;

           
            tsubject = "Notification : Approval Request for " + txtEmpName.Text.Trim().ToUpper() + " (" + tEmpUnqID + "," + txtWrkGrpCode.Text.Trim().ToUpper() + ")";
           

            string thead = "<html> " +
                    "<head>" +
                    "<style>" +
                    " table { " +
                        " font-family: arial, sans-serif; " +
                        " border-collapse: collapse; " +
                        " width: 100%; " +
                    "} " +

                    " td, th { " +
                    "    border: 1px solid #dddddd; " +
                    "    text-align: left; " +
                    "    padding: 8px; " +
                    "} " +

                    " tr:nth-child(even) { " +
                    "    background-color: #dddddd;" +
                    "}" +
                    "</style>" +
                    "</head>" +
                    "<body>";

            

            clsEmp cEmp = new clsEmp();
            cEmp.GetEmpDetails("01",tEmpUnqID);

            tbody = "Sir, <br/><p>&nbsp; Kindly Approve Employee Joining.</p> <br/> <br/> " +
                "<table>" +
                "<tr><td>EmpCode : </td><td>" + tEmpUnqID + "</td></tr>" +
                "<tr><td>EmpName : </td><td>" + cEmp.EmpName + "</td></tr>" +
                "<tr><td>WrkGrp : </td><td>" + cEmp.WrkGrp + " " + (cEmp.ContCode) + "</td></tr>" +
                "<tr><td>Action performed by</td><td>" + Utils.User.GUserID + "</td>" +
                "<tr><td>Reason</td><td>New joining of Cont. Employee</td>" +
                "<tr><td>Date And Time</td><td>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "</td></tr>" +
                "</table><br/><br/> " +
                 "*This is Auto-generated notification, do not reply on this e-mail id. </body></html>";


            string err = EmailHelper.Email(to, cc, bcc, thead + tbody, tsubject, Globals.G_DefaultMailID,
                        Globals.G_DefaultMailDisplayName, "", "");

            //MessageBox.Show(mailtype + " : Notification : Status : " + err, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
    }
}
