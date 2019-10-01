using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Data.SqlClient;

namespace SqlOrmLibrary
{
    /// <summary>
    /// clsDBControl의 요약 설명입니다.
    /// </summary>
    internal class clsSQLControl
    {
        private SqlConnection oConnect = null;
        private SqlCommand oCommand = null;
        private SqlTransaction oTransaction = null;

        public clsSQLControl(string sConectionInfo)
        {
            String sConnectInfo = sConectionInfo;
            if (sConnectInfo == null || sConnectInfo.Length < 1) throw new Exception("Web Config Connect Info ERROR");
            oConnect = new SqlConnection(sConnectInfo);
            oConnect.Open();
        }

        public clsSQLControl()
        {
        }

        public void SetDB(SqlConnection iConnect, SqlTransaction iTransaction)
        {
            oConnect = iConnect;
            oTransaction = iTransaction;
        }

        //~clsDBControl() //소멸자 정의
        //{
        //    this.Close();
        //}


        public bool ExcuteNonQuery(string sSql)
        {
            this.oCommand = new SqlCommand(sSql, oConnect);
            oCommand.Transaction = oTransaction;

            bool bResult = false;
            try
            {
                oCommand.ExecuteNonQuery();
                bResult = true;
            }
            catch (Exception e)
            {
                //clsMail.WriteLine(e.Message);
                bResult = false;
                throw e;
            }
            finally
            {
                this.CloseCommand();
            }

            return bResult;

        }
        public bool ExcuteNonQuerybyBinding(string sSql, string argumt)
        {
            this.oCommand = new SqlCommand(sSql, oConnect);
            this.oCommand.Parameters.AddWithValue("bind", argumt);

            oCommand.Transaction = oTransaction;
            bool bResult = false;
            try
            {
                oCommand.ExecuteNonQuery();
                bResult = true;
            }
            catch (Exception e)
            {
                bResult = false;
                throw e;

            }
            finally
            {
                this.CloseCommand();
            }

            return bResult;

        }

        //string으로 이루어진 객체만 가능
        public bool ExcuteNonQuerybyMapper(string sSql, dynamic obj, EntityMapperSQL oEntity)
        {

            Type t = obj.GetType(); //객체의 타입 가져오기
            //sql 구문 만들기 시작

            PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var removed_properties = from prt in Properties
                                     where !oEntity.except_members.Contains(prt.Name)
                                     select prt;

            Properties = removed_properties.ToArray();

            this.oCommand = new SqlCommand(sSql, oConnect);

            for (int i = 0; i < Properties.Length; i++)
            {

                string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                string sMemberName = Properties[i].Name.ToString();
                string object_member = string.Empty;
                string table_field = string.Empty;

                //Byte 타입의 경우
                if (oEntity.byte_type_members.Contains(sMemberName))
                {
                    table_field = Properties[i].Name.ToString().Substring(1);
                    
                    byte[] inputByte = Properties[i].GetValue(obj, null);
                    this.oCommand.Parameters.Add(table_field, SqlDbType.Binary, inputByte.Length).Value = inputByte;

                    continue;
                }

                if (firstofField == "_")
                {
                    table_field = Properties[i].Name.ToString().Substring(1);
                    object_member = Properties[i].GetValue(obj, null);
                }
                else
                {
                    table_field = Properties[i].Name.ToString();
                    object_member = Properties[i].GetValue(obj, null);
                    if (object_member != null)
                    {
                        object_member = object_member.Trim();
                    }
                    if (object_member == "" || object_member == null)
                    {
                        object_member = "'" + object_member + "'";
                    }
                }


                this.oCommand.Parameters.AddWithValue(table_field, object_member);
            }

            oCommand.Transaction = oTransaction;
            bool bResult = false;
            try
            {
                oCommand.ExecuteNonQuery();
                bResult = true;
            }
            catch (Exception e)
            {                
                bResult = false;
                throw e;
            }
            finally
            {
                this.CloseCommand();
            }

            return bResult;

        }

        //Batch 타입으로 입력
        public bool ExcuteNonQuerybyMapperList(List<string> sSql, dynamic obj, EntityMapperSQL oEntity)
        {

            this.oCommand = new SqlCommand();
            this.oCommand.Connection = oConnect;

            oCommand.Transaction = oTransaction;
            bool bResult = false;

            try
            {
                for (int k = 0; k < obj.Count; k++)
                {

                    Type t = obj[k].GetType(); //객체의 타입 가져오기                
                    //sql 구문 만들기 시작                
                    PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var removed_properties = from prt in Properties
                                             where !oEntity.except_members.Contains(prt.Name)
                                             select prt;
                    Properties = removed_properties.ToArray();
                    this.oCommand.CommandText = sSql[k];
                    this.oCommand.Parameters.Clear();
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                        string object_member = string.Empty;
                        string table_field = string.Empty;
                        string sMemberName = Properties[i].Name.ToString();

                        //Byte 타입의 경우
                        if (oEntity.byte_type_members.Contains(sMemberName))
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                            byte[] inputByte = Properties[i].GetValue(obj, null);
                            this.oCommand.Parameters.Add(table_field, SqlDbType.Binary, inputByte.Length).Value = inputByte;

                            continue;
                        }

                        if (firstofField == "_")
                        {

                            table_field = Properties[i].Name.ToString().Substring(1);
                            object_member = Properties[i].GetValue(obj[k], null);

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();
                            object_member = Properties[i].GetValue(obj[k], null);
                            if (object_member != null)
                            {
                                object_member = object_member.Trim();
                            }
                            if (object_member == "" || object_member == null)
                            {
                                object_member = "'" + object_member + "'";
                            }

                        }
                        this.oCommand.Parameters.AddWithValue(table_field, object_member);
                    }

                    oCommand.ExecuteNonQuery();
                }

                bResult = true;

            }

            catch (Exception e)
            {
                bResult = false;
                throw e;
            }
            finally
            {
                this.CloseCommand();
            }

            return bResult;
        }

        //Batch 타입으로 insert 입력
        //한 개의 싱글 쿼리로 paremeterized 쿼리 날리기
        public bool ExcuteNonQuerybyMapperListWithSingleStatement(string sSql, dynamic obj, EntityMapperSQL oEntity)
        {

            this.oCommand = new SqlCommand(sSql, oConnect);

            oCommand.Transaction = oTransaction;
            this.oCommand.CommandText = sSql;

            bool bResult = false;
            try
            {
                for (int k = 0; k < obj.Count; k++)
                {

                    Type t = obj[k].GetType(); //객체의 타입 가져오기                
                    //sql 구문 만들기 시작                
                    PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var removed_properties = from prt in Properties
                                             where !oEntity.except_members.Contains(prt.Name)
                                             select prt;
                    Properties = removed_properties.ToArray();


                    this.oCommand.Parameters.Clear();
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                        string object_member = string.Empty;
                        string table_field = string.Empty;
                        string sMemberName = Properties[i].Name.ToString();

                        //Byte 타입의 경우
                        if (oEntity.byte_type_members.Contains(sMemberName))
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                            byte[] inputByte = Properties[i].GetValue(obj[k], null);
                            this.oCommand.Parameters.Add(table_field, SqlDbType.Binary, inputByte.Length).Value = inputByte;

                            continue;
                        }
                        if (firstofField == "_")
                        {

                            table_field = Properties[i].Name.ToString().Substring(1);
                            object_member = Properties[i].GetValue(obj[k], null);

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();
                            object_member = Properties[i].GetValue(obj[k], null);
                            if (object_member != null)
                            {
                                object_member = object_member.Trim();
                            }
                            if (object_member == "" || object_member == null)
                            {
                                object_member = "'" + object_member + "'";
                            }

                        }
                        this.oCommand.Parameters.AddWithValue(table_field, object_member);
                    }

                    oCommand.ExecuteNonQuery();
                }

                bResult = true;

            }

            catch (Exception e)
            {
                bResult = false;
                throw e;
            }
            finally
            {
                this.CloseCommand();
            }

            return bResult;
        }

        public bool ExcuteNonQuery(string[] sSql)
        {
            bool bResult = false;
            this.oCommand = new SqlCommand();

            oCommand.Connection = oConnect;
            this.oCommand.Transaction = oTransaction;

            try
            {
                for (int i = 0; i < sSql.Length; i++)
                {
                    if ((sSql[i] != null) && (sSql[i].Trim().Length > 0))
                    {
                        oCommand.CommandText = sSql[i];
                        oCommand.ExecuteNonQuery();
                    }
                }

                bResult = true;
            }
            catch (Exception ex)
            {
                bResult = false;
                throw ex;
            }
            finally
            {
                this.CloseCommand();
            }

            return bResult;
        }
        public bool ExecuteScalar(String sSql)
        {
            bool bResult = false;

            oCommand = new SqlCommand(sSql, oConnect);

            if (oCommand.ExecuteScalar() != null)
            {
                bResult = true;
            }
            else
            {
                bResult = false;
            }

            this.CloseCommand();

            return bResult;

        }
        public SqlDataReader QueryDataReader(string sSql)
        {
            oCommand = new SqlCommand(sSql, oConnect);

            System.Data.SqlClient.SqlDataReader reader = oCommand.ExecuteReader();

            return reader;
        }
        public DataSet QueryDataSet(string sSql)
        {

            DataSet oDataSet = new DataSet();

            SqlDataAdapter oDataAdapter = new SqlDataAdapter();
            oCommand = new SqlCommand(sSql, oConnect);

            oDataAdapter.SelectCommand = oCommand;
            oDataAdapter.Fill(oDataSet);

            this.CloseCommand();
            return oDataSet;
        }
        public DataTable QueryDataTable(string sSql)
        {

            DataSet oDataSet = new DataSet();

            SqlDataAdapter oDataAdapter = new SqlDataAdapter();
            oCommand = new SqlCommand(sSql, oConnect);

            oDataAdapter.SelectCommand = oCommand;
            oDataAdapter.Fill(oDataSet);
            this.CloseCommand();
            return oDataSet.Tables[0];
        }
        //added by skhan, 2010.08.17
        public DataSet QueryDataSetByBinding(string sSql, string[] aParam)
        {

            DataSet oDataSet = new DataSet();

            SqlDataAdapter oDataAdapter = new SqlDataAdapter();
            oCommand = new SqlCommand(sSql, oConnect);

            //Binding arg count
            int iCountofBind = Regex.Matches(sSql, "[?]").Count;



            //Binding
            SqlParameter odp = new SqlParameter();

            for (int i = 0; i < iCountofBind; i++)
            {
                odp = new SqlParameter("@param" + i.ToString(), aParam[0].ToString());
                oCommand.Parameters.Add(odp);
            }

            oDataAdapter.SelectCommand = oCommand;
            oDataAdapter.Fill(oDataSet);

            this.CloseCommand();
            return oDataSet;
        }
        public string QuerySingleData(string sSql)
        {
            oCommand = new SqlCommand(sSql, oConnect);

            Object oResult = oCommand.ExecuteScalar();
            String sResult = "";
            if (oResult != null)
            {
                sResult = oResult.ToString();
            }

            oCommand.CommandText = "commit";
            oCommand.ExecuteScalar();
            oCommand.CommandText = "";
            this.CloseCommand();

            return sResult;
        }
        public void CloseCommand()
        {
            try
            {
                oCommand.Cancel();
                oCommand.Dispose();
                oCommand = null;

                return;
            }
            catch
            {
                return;
            }
        }
        public void Close()
        {
            if (oCommand != null)
            {
                oCommand.Cancel();
                oCommand.Dispose();
                oCommand = null;
            }

            if (oConnect.State == ConnectionState.Open)
            {
                oConnect.Close();
            }

            oConnect.Dispose();
            oConnect = null;
        }


        public static clsDBControl_new oDB { get; set; }
    }
}