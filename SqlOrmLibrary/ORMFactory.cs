using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data;

namespace SqlOrmLibrary
{
    public class ORMFactory<T> where T : new()
    {
        private T _target;
        private string _DBServerName;
        private List<string> _UpdateOnlyField; //update시 load하지 않고 일부 필드에 대해서만 update 할 경우

        public ORMFactory()
        {
            _target = new T();
            IORMInterface itTarget = _target as IORMInterface;
            _DBServerName = itTarget.SetDBconnectionInfo();
            _UpdateOnlyField = new List<string>();
        }

        public ORMFactory(string DBServerNum)
        {
            _target = new T();
            _DBServerName = DBServerNum;
            _UpdateOnlyField = new List<string>();
        }

        public ORMFactory(T target, string DBServerNum)
        {
            _target = target;
            _DBServerName = DBServerNum;
            _UpdateOnlyField = new List<string>();

        }

        public void Put(T target)
        {
            _target = target;
        }


        public bool Create(T target)
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            bool isSucess = true;

            IORMInterface itTarget = target as IORMInterface;
            string sTableName = itTarget.SetTableName();
            mapper.except_members.Add("_ROWID");

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            bool isORMAction = true;
            isORMAction = itTarget.BeforeAdd();

            try
            {
                string sql = "";
                if (isORMAction)
                {
                    string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                    Type Class = target.GetType(); //객체의 타입 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string table_field = table_field = Properties[i].Name.ToString();

                        if (arDateTimeFields.Contains(table_field))
                        {
                            object sRowValue = Properties[i].GetValue(target, null);
                            string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                            Properties[i].SetValue(target, sInputDate, null);
                        }

                    }

                    sql = mapper.Create(target, sTableName); //insert 구문 생성 
                    isSucess = mapper.oDB.ExcuteNonQuery(sql);
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();
                if (isSucess)
                {
                    itTarget.AfterAdd();
                }

            }

            return isSucess;

        }

        public bool Create()
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            bool isSucess = true;

            IORMInterface itTarget = _target as IORMInterface;
            string sTableName = itTarget.SetTableName();
            mapper.except_members.Add("_ROWID");

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            bool isORMAction = true;
            isORMAction = itTarget.BeforeAdd();

            try
            {
                string sql = "";
                if (isORMAction)
                {
                    string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                    Type Class = _target.GetType(); //객체의 타입 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string table_field = table_field = Properties[i].Name.ToString();

                        if (arDateTimeFields.Contains(table_field))
                        {
                            object sRowValue = Properties[i].GetValue(_target, null);
                            string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                            Properties[i].SetValue(_target, sInputDate, null);
                        }

                    }


                    sql = mapper.Create(_target, sTableName); //insert 구문 생성
                    isSucess = mapper.oDB.ExcuteNonQuery(sql);
                }

            }
            catch (Exception ex)
            {
                isSucess = false;
            }
            finally
            {
                mapper.oDB.Close();
                if (isSucess)
                {
                    itTarget.AfterAdd();
                }
            }

            return isSucess;
        }

        //Parameterized SQL
        public bool _Create(T target)
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            bool isSucess = true;

            IORMInterface itTarget = target as IORMInterface;
            string sTableName = itTarget.SetTableName();
            mapper.except_members.Add("_ROWID");

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            bool isORMAction = true;
            isORMAction = itTarget.BeforeAdd();

            try
            {

                if (isORMAction)
                {
                    isSucess = mapper.Create_Oracle(target, sTableName); //insert 구문 생성                    
                }

            }
            catch (Exception ex)
            {
                isSucess = false;
            }
            finally
            {
                mapper.oDB.Close();
                if (isSucess)
                {
                    itTarget.AfterAdd();
                }
            }

            return isSucess;
        }

        //Parameterized SQL
        public bool _Create()
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            bool isSucess = true;

            IORMInterface itTarget = _target as IORMInterface;
            string sTableName = itTarget.SetTableName();
            mapper.except_members.Add("_ROWID");

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            bool isORMAction = true;
            isORMAction = itTarget.BeforeAdd();

            try
            {

                if (isORMAction)
                {
                    isSucess = mapper.Create_Oracle(_target, sTableName); //insert 구문 생성                    
                }

            }
            catch (Exception ex)
            {
                isSucess = false;
            }
            finally
            {
                mapper.oDB.Close();
                if (isSucess)
                {
                    itTarget.AfterAdd();
                }
            }

            return isSucess;
        }


        public bool CreateListData(List<T> targetList)
        {
            List<string> listQuery = new List<string>();
            bool isSuccess = true;

            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);

            bool isORMAction = true;

            for (int i = 0; i < targetList.Count; i++)
            {
                string Sql = "";

                IORMInterface itTarget = targetList[i] as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                mapper.except_members.Clear();

                mapper.except_members.Add("_ROWID");

                //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
                string[] aVirtualField = itTarget.SetVirtualField();
                for (int i2 = 0; i2 < aVirtualField.Length; i2++)
                {
                    string sNotEditField = aVirtualField[i2].Trim();
                    mapper.except_members.Add(sNotEditField);
                }

                isORMAction = itTarget.BeforeAdd();

                string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                Type Class = targetList[i].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int k = 0; k < Properties.Length; k++)
                {

                    string table_field = table_field = Properties[k].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[k].GetValue(targetList[i], null);
                        string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                        Properties[k].SetValue(targetList[i], sInputDate, null);
                    }

                }

                if (isORMAction)
                {
                    Sql = mapper.Create(targetList[i], sTableName); // Create문 쿼리 생성
                    listQuery.Add(Sql);
                }

            }

            if (listQuery.Count > 0)
            {
                isSuccess = mapper.oDB.ExcuteNonQuery(listQuery.ToArray());
            }

            mapper.oDB.Close();

            if (isSuccess)
            {
                for (int j = 0; j < targetList.Count; j++)
                {
                    IORMInterface itTarget = targetList[j] as IORMInterface;
                    itTarget.AfterAdd();
                }
            }

            return isSuccess;

        }


        public bool _CreateListData(List<T> targetList)
        {
            bool isSuccess = true;

            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);

            bool isORMAction = true;
            string sBigTableName = "";

            for (int i = 0; i < targetList.Count; i++)
            {

                IORMInterface itTarget = targetList[i] as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                sBigTableName = sTableName;

                mapper.except_members.Clear();

                mapper.except_members.Add("_ROWID");

                //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
                string[] aVirtualField = itTarget.SetVirtualField();
                for (int i2 = 0; i2 < aVirtualField.Length; i2++)
                {
                    string sNotEditField = aVirtualField[i2].Trim();
                    mapper.except_members.Add(sNotEditField);
                }

                isORMAction = itTarget.BeforeAdd();

                if (isORMAction == false)
                    break;

            }

            if (isORMAction)
            {
                isSuccess = mapper.Create_Oracle_List(targetList, sBigTableName); // Create문 쿼리 생성
            }

            mapper.oDB.Close();

            if (isSuccess)
            {
                for (int j = 0; j < targetList.Count; j++)
                {
                    IORMInterface itTarget = targetList[j] as IORMInterface;
                    itTarget.AfterAdd();
                }
            }

            return isSuccess;

        }

        public bool Edit(T target)
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;
            bool isORMAction = true;

            IORMInterface itTarget = target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();


            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            mapper.except_members.Add("_ROWID");

            string[] arNotEditField = itTarget.SetFieldforNotEdit();

            for (int i = 0; i < arNotEditField.Length; i++)
            {
                string sNotEditField = arNotEditField[i].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            isORMAction = itTarget.BeforeEdit();

            try
            {
                string sql = "";
                if (isORMAction)
                {
                    string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                    Type Class = target.GetType(); //객체의 타입 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string table_field = table_field = Properties[i].Name.ToString();

                        if (arDateTimeFields.Contains(table_field))
                        {
                            object sRowValue = Properties[i].GetValue(target, null);
                            string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                            Properties[i].SetValue(target, sInputDate, null);
                        }

                    }


                    sql = mapper.Save(target, sTableName, where_stmt); // update문 쿼리 생성        
                    isSucess = mapper.oDB.ExcuteNonQuery(sql);
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();

                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        public bool Edit()
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;


            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();

            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            mapper.except_members.Add("_ROWID");

            string[] arNotEditField = itTarget.SetFieldforNotEdit();

            for (int i = 0; i < arNotEditField.Length; i++)
            {
                string sNotEditField = arNotEditField[i].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            bool isORMAction = true;
            isORMAction = itTarget.BeforeEdit();

            try
            {
                string sql = "";
                if (isORMAction)
                {

                    string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                    Type Class = _target.GetType(); //객체의 타입 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string table_field = table_field = Properties[i].Name.ToString();

                        if (arDateTimeFields.Contains(table_field))
                        {
                            object sRowValue = Properties[i].GetValue(_target, null);
                            string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                            Properties[i].SetValue(_target, sInputDate, null);
                        }

                    }

                    sql = mapper.Save(_target, sTableName, where_stmt); // update문 쿼리 생성        
                    isSucess = mapper.oDB.ExcuteNonQuery(sql);
                }


            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();
                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        public bool EditListData(List<T> targetList)
        {
            List<string> listQuery = new List<string>();
            bool isSuccess = true;

            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);

            bool isORMAction = true;

            for (int i = 0; i < targetList.Count; i++)
            {
                string Sql = "";
                string where_stmt = "";

                IORMInterface itTarget = targetList[i] as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                if (itTarget.isCombinedPrimaryKey())
                {
                    where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
                }
                else
                {
                    where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
                }

                mapper.except_members.Clear();
                mapper.except_members.Add("_ROWID");

                string[] arNotEditField = itTarget.SetFieldforNotEdit();

                for (int j = 0; j < arNotEditField.Length; j++)
                {
                    string sNotEditField = arNotEditField[j].Trim();
                    mapper.except_members.Add(sNotEditField);
                }

                //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
                string[] aVirtualField = itTarget.SetVirtualField();
                for (int i2 = 0; i2 < aVirtualField.Length; i2++)
                {
                    string sNotEditField = aVirtualField[i2].Trim();
                    mapper.except_members.Add(sNotEditField);
                }

                isORMAction = itTarget.BeforeEdit();

                string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                Type Class = targetList[i].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int k = 0; k < Properties.Length; k++)
                {

                    string table_field = table_field = Properties[k].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[k].GetValue(targetList[i], null);
                        string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                        Properties[k].SetValue(targetList[i], sInputDate, null);
                    }

                }

                if (isORMAction)
                {
                    Sql = mapper.Save(targetList[i], sTableName, where_stmt); // update문 쿼리 생성
                    listQuery.Add(Sql);
                }

            }

            if (listQuery.Count > 0)
            {
                isSuccess = mapper.oDB.ExcuteNonQuery(listQuery.ToArray());
            }

            mapper.oDB.Close();

            if (isSuccess)
            {
                for (int j = 0; j < targetList.Count; j++)
                {
                    IORMInterface itTarget = targetList[j] as IORMInterface;
                    itTarget.AfterEdit();
                }
            }

            return isSuccess;

        }

        public bool EditOnly(T target)
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;
            bool isORMAction = true;

            IORMInterface itTarget = target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();


            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            isORMAction = itTarget.BeforeEdit();

            try
            {
                string sql = "";
                if (isORMAction)
                {
                    string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                    Type Class = target.GetType(); //객체의 타입 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string table_field = table_field = Properties[i].Name.ToString();

                        if (arDateTimeFields.Contains(table_field))
                        {
                            object sRowValue = Properties[i].GetValue(target, null);
                            string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                            Properties[i].SetValue(target, sInputDate, null);
                        }

                        mapper.except_members.Add(table_field);
                    }

                    foreach (string item in _UpdateOnlyField)
                    {
                        mapper.except_members.RemoveAll(p => p.ToString() == item);
                    }


                    sql = mapper.Save(target, sTableName, where_stmt); // update문 쿼리 생성        
                    isSucess = mapper.oDB.ExcuteNonQuery(sql);
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();

                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        public bool EditOnly()
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;
            bool isORMAction = true;

            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();


            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            isORMAction = itTarget.BeforeEdit();

            try
            {
                string sql = "";
                if (isORMAction)
                {
                    string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                    Type Class = _target.GetType(); //객체의 타입 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    for (int i = 0; i < Properties.Length; i++)
                    {

                        string table_field = table_field = Properties[i].Name.ToString();

                        if (arDateTimeFields.Contains(table_field))
                        {
                            object sRowValue = Properties[i].GetValue(_target, null);
                            string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                            Properties[i].SetValue(_target, sInputDate, null);
                        }

                        mapper.except_members.Add(table_field);
                    }

                    foreach (string item in _UpdateOnlyField)
                    {
                        mapper.except_members.RemoveAll(p => p.ToString() == item);
                    }


                    sql = mapper.Save(_target, sTableName, where_stmt); // update문 쿼리 생성        
                    isSucess = mapper.oDB.ExcuteNonQuery(sql);
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();

                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        public bool EditOnlyListData(List<T> targetList)
        {
            List<string> listQuery = new List<string>();
            bool isSuccess = true;

            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);

            bool isORMAction = true;

            for (int i = 0; i < targetList.Count; i++)
            {
                string Sql = "";
                string where_stmt = "";

                IORMInterface itTarget = targetList[i] as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                if (itTarget.isCombinedPrimaryKey())
                {
                    where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
                }
                else
                {
                    where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
                }

                isORMAction = itTarget.BeforeEdit();

                mapper.except_members.Clear();

                string[] arDateTimeFields = itTarget.SetFieldforDatetime();
                Type Class = targetList[i].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int k = 0; k < Properties.Length; k++)
                {

                    string table_field = table_field = Properties[k].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[k].GetValue(targetList[i], null);
                        string sInputDate = " to_date('" + sRowValue.ToString() + "','yyyy-MM-dd HH24:MI:SS') ";
                        Properties[k].SetValue(targetList[i], sInputDate, null);
                    }

                    mapper.except_members.Add(table_field);

                }

                foreach (string item in _UpdateOnlyField)
                {
                    mapper.except_members.RemoveAll(p => p.ToString() == item);
                }

                if (isORMAction)
                {
                    Sql = mapper.Save(targetList[i], sTableName, where_stmt); // update문 쿼리 생성
                    listQuery.Add(Sql);
                }

            }

            if (listQuery.Count > 0)
            {
                isSuccess = mapper.oDB.ExcuteNonQuery(listQuery.ToArray());
            }

            mapper.oDB.Close();

            if (isSuccess)
            {
                for (int j = 0; j < targetList.Count; j++)
                {
                    IORMInterface itTarget = targetList[j] as IORMInterface;
                    itTarget.AfterEdit();
                }
            }

            return isSuccess;

        }

        //Parameterized query
        public bool _Edit(T target)
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;
            bool isORMAction = true;

            IORMInterface itTarget = target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();


            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            mapper.except_members.Add("_ROWID");

            string[] arNotEditField = itTarget.SetFieldforNotEdit();

            for (int i = 0; i < arNotEditField.Length; i++)
            {
                string sNotEditField = arNotEditField[i].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            isORMAction = itTarget.BeforeEdit();

            try
            {
                if (isORMAction)
                {
                    isSucess = mapper.Save_Oracle(target, sTableName, where_stmt); // update문 쿼리 생성                            
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();

                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        //Parameterized query
        public bool _Edit()
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;


            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();

            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            mapper.except_members.Add("_ROWID");

            string[] arNotEditField = itTarget.SetFieldforNotEdit();

            for (int i = 0; i < arNotEditField.Length; i++)
            {
                string sNotEditField = arNotEditField[i].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                mapper.except_members.Add(sNotEditField);
            }

            bool isORMAction = true;
            isORMAction = itTarget.BeforeEdit();

            try
            {
                if (isORMAction)
                {
                    isSucess = mapper.Save_Oracle(_target, sTableName, where_stmt); // update문 쿼리 생성                                                
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();
                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        //Parameterized query
        public bool _EditListData(List<T> targetList)
        {
            bool isSuccess = true;

            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);

            bool isORMAction = true;

            List<string> lTableNames = new List<string>();
            List<string> lWhere = new List<string>();


            for (int i = 0; i < targetList.Count; i++)
            {
                string where_stmt = "";

                IORMInterface itTarget = targetList[i] as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                if (itTarget.isCombinedPrimaryKey())
                {
                    where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
                }
                else
                {
                    where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
                }

                lTableNames.Add(sTableName);
                lWhere.Add(where_stmt);

                mapper.except_members.Clear();
                mapper.except_members.Add("_ROWID");

                string[] arNotEditField = itTarget.SetFieldforNotEdit();

                for (int j = 0; j < arNotEditField.Length; j++)
                {
                    string sNotEditField = arNotEditField[j].Trim();
                    mapper.except_members.Add(sNotEditField);
                }

                //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
                string[] aVirtualField = itTarget.SetVirtualField();
                for (int i2 = 0; i2 < aVirtualField.Length; i2++)
                {
                    string sNotEditField = aVirtualField[i2].Trim();
                    mapper.except_members.Add(sNotEditField);
                }

                isORMAction = itTarget.BeforeEdit();

                if (isORMAction == false)
                    break;

            }

            if (isORMAction)
            {
                isSuccess = mapper.Save_Oracle_List(targetList, lTableNames, lWhere); // update문 쿼리 생성
            }

            mapper.oDB.Close();

            if (isSuccess)
            {
                for (int j = 0; j < targetList.Count; j++)
                {
                    IORMInterface itTarget = targetList[j] as IORMInterface;
                    itTarget.AfterEdit();
                }
            }

            return isSuccess;

        }

        //Parameterized query
        public bool _EditOnly(T target)
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;
            bool isORMAction = true;

            IORMInterface itTarget = target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();


            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            isORMAction = itTarget.BeforeEdit();

            Type Class = target.GetType(); //객체의 타입 가져오기
            PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int k = 0; k < Properties.Length; k++)
            {
                string table_field = table_field = Properties[k].Name.ToString();
                mapper.except_members.Add(table_field);
            }

            foreach (string item in _UpdateOnlyField)
            {
                mapper.except_members.RemoveAll(p => p.ToString() == item);
            }

            try
            {
                if (isORMAction)
                {
                    isSucess = mapper.Save_Oracle(target, sTableName, where_stmt); // update문 쿼리 생성                            
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();

                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        //Parameterized query
        public bool _EditOnly()
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string where_stmt = "";
            bool isSucess = true;


            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();

            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            bool isORMAction = true;
            isORMAction = itTarget.BeforeEdit();

            Type Class = _target.GetType(); //객체의 타입 가져오기
            PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int k = 0; k < Properties.Length; k++)
            {
                string table_field = table_field = Properties[k].Name.ToString();
                mapper.except_members.Add(table_field);
            }

            foreach (string item in _UpdateOnlyField)
            {
                mapper.except_members.RemoveAll(p => p.ToString() == item);
            }

            try
            {
                if (isORMAction)
                {
                    isSucess = mapper.Save_Oracle(_target, sTableName, where_stmt); // update문 쿼리 생성                                                
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

            }
            finally
            {
                mapper.oDB.Close();
                if (isSucess)
                {
                    itTarget.AfterEdit();
                }
            }

            return isSucess;

        }

        //Parameterized query
        public bool _EditOnlyListData(List<T> targetList)
        {
            bool isSuccess = true;

            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);

            bool isORMAction = true;
            List<string> lTableNames = new List<string>();
            List<string> lWhere = new List<string>();

            for (int i = 0; i < targetList.Count; i++)
            {
                string where_stmt = "";

                IORMInterface itTarget = targetList[i] as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                if (itTarget.isCombinedPrimaryKey())
                {
                    where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
                }
                else
                {
                    where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
                }

                isORMAction = itTarget.BeforeEdit();

                lTableNames.Add(sTableName);
                lWhere.Add(where_stmt);


                if (isORMAction == false)
                    break;

                mapper.except_members.Clear();

                Type Class = targetList[i].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                for (int k = 0; k < Properties.Length; k++)
                {
                    string table_field = table_field = Properties[k].Name.ToString();
                    mapper.except_members.Add(table_field);
                }

                foreach (string item in _UpdateOnlyField)
                {
                    mapper.except_members.RemoveAll(p => p.ToString() == item);
                }

            }

            if (isORMAction)
            {
                isSuccess = mapper.Save_Oracle_List(targetList, lTableNames, lWhere); // update문 쿼리 생성
            }

            mapper.oDB.Close();

            if (isSuccess)
            {
                for (int j = 0; j < targetList.Count; j++)
                {
                    IORMInterface itTarget = targetList[j] as IORMInterface;
                    itTarget.AfterEdit();
                }
            }

            return isSuccess;

        }

        public bool DeleteListData(List<T> targetList)
        {
            List<string> listQuery = new List<string>();
            bool isSuccess = true;
            bool isORMAction = true;

            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);

            for (int i = 0; i < targetList.Count; i++)
            {


                string Sql = "";
                string where_stmt = "";

                IORMInterface itTarget = targetList[i] as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                if (itTarget.isCombinedPrimaryKey())
                {
                    where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
                }
                else
                {
                    where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
                }

                isORMAction = itTarget.BeforeDelete();

                if (isORMAction)
                {
                    Sql = mapper.Delete(sTableName, where_stmt); // update문 쿼리 생성
                    listQuery.Add(Sql);
                }


            }

            if (listQuery.Count > 0)
            {
                isSuccess = mapper.oDB.ExcuteNonQuery(listQuery.ToArray());
            }

            mapper.oDB.Close();

            if (isSuccess)
            {
                for (int j = 0; j < targetList.Count; j++)
                {
                    IORMInterface itTarget = targetList[j] as IORMInterface;
                    itTarget.AfterDelete();
                }
            }

            return isSuccess;

        }

        public bool Delete(T target)
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string Sql = "";
            string where_stmt = "";
            bool isSucess = true;
            bool isORMAction = true;


            IORMInterface itTarget = target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();

            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            isORMAction = itTarget.BeforeDelete();

            try
            {
                if (isORMAction)
                {
                    Sql = mapper.Delete(sTableName, where_stmt); // update문 쿼리 생성
                    isSucess = mapper.oDB.ExcuteNonQuery(Sql); // update문 실행
                }

            }
            catch (Exception ex)
            {
                isSucess = false;
            }
            finally
            {
                mapper.oDB.Close();

                if (isSucess)
                {
                    itTarget.AfterDelete();
                }

            }

            return isSucess;

        }

        public bool Delete()
        {
            EntityMapper mapper = new EntityMapper();
            mapper.oDB = new clsDBControl_new(_DBServerName);
            string Sql = "";
            string where_stmt = "";
            bool isSucess = true;
            bool isORMAction = true;

            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();

            if (itTarget.isCombinedPrimaryKey())
            {
                where_stmt = " where " + itTarget.SetWhereQueryForCombinedPrimaryKey();
            }
            else
            {
                where_stmt = " where " + sPrimaryField + " ='" + sPrimaryValue + "'";// update문 조건
            }

            isORMAction = itTarget.BeforeDelete();

            try
            {
                if (isORMAction)
                {
                    Sql = mapper.Delete(sTableName, where_stmt); // update문 쿼리 생성
                    isSucess = mapper.oDB.ExcuteNonQuery(Sql); // update문 실행
                }

            }
            catch (Exception ex)
            {
                isSucess = false;
            }
            finally
            {
                mapper.oDB.Close();

                if (isSucess)
                {
                    itTarget.AfterDelete();
                }
            }

            return isSucess;

        }
        public List<T> Load(string sWhere = "")
        {
            List<T> resultList = new List<T>();

            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();
            string sDefaultWhere = itTarget.SetQueryOption().Trim();

            if (sDefaultWhere == "")
            {
                sDefaultWhere = " where 1=1 ";
            }

            string sWhereStmt = sDefaultWhere + sWhere;

            EntityMapper omapper = new EntityMapper();

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                omapper.except_members.Add(sNotEditField);
            }

            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보
            omapper.WhereCondition = sWhereStmt; // 조건절 
            omapper.Load(); //데이터 쿼리 실행

            // 데이터 타입 형 변환
            for (int i = 0; i < omapper.Result[0].Count; i++)
            {
                resultList.Add((T)omapper.Result[0][i]);
            }

            // load 완료후 
            for (int j = 0; j < resultList.Count; j++)
            {
                IORMInterface itResult = resultList[j] as IORMInterface;

                //date type 변환
                string[] arDateTimeFields = itResult.SetFieldforDatetime();
                Type Class = resultList[j].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < Properties.Length; i++)
                {

                    string table_field = table_field = Properties[i].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[i].GetValue(resultList[j], null);
                        string sInputDate = "";
                        if (sRowValue != null && sRowValue.ToString() != "")
                        {
                            sInputDate = DateTime.ParseExact(sRowValue.ToString(), "yyyy-MM-dd tt h:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal).ToString("yyyy/MM/dd HH:mm:ss");
                        }

                        Properties[i].SetValue(resultList[j], sInputDate, null);
                    }

                }

                itResult.AfterLoad();
            }

            //리스트 형태로 반환
            return resultList;
        }

        public List<T> Load(int ipage, int numberOfData, string sWhere = "")
        {
            List<T> resultList = new List<T>();

            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();
            string sDefaultWhere = itTarget.SetQueryOption().Trim();

            if (sDefaultWhere == "")
            {
                sDefaultWhere = " where 1=1 ";
            }

            string sWhereStmt = sDefaultWhere + sWhere;

            EntityMapper omapper = new EntityMapper();

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                omapper.except_members.Add(sNotEditField);
            }

            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보
            omapper.WhereCondition = sWhereStmt; // 조건절 
            omapper.Load(ipage, numberOfData); //데이터 쿼리 실행

            // 데이터 타입 형 변환
            for (int i = 0; i < omapper.Result[0].Count; i++)
            {
                resultList.Add((T)omapper.Result[0][i]);
            }

            // load 완료후 
            for (int j = 0; j < resultList.Count; j++)
            {
                IORMInterface itResult = resultList[j] as IORMInterface;

                //date type 변환
                string[] arDateTimeFields = itResult.SetFieldforDatetime();
                Type Class = resultList[j].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < Properties.Length; i++)
                {

                    string table_field = table_field = Properties[i].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[i].GetValue(resultList[j], null);
                        string sInputDate = "";
                        if (sRowValue != null && sRowValue.ToString() != "")
                        {
                            sInputDate = DateTime.ParseExact(sRowValue.ToString(), "yyyy-MM-dd tt h:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal).ToString("yyyy/MM/dd HH:mm:ss");
                        }

                        Properties[i].SetValue(resultList[j], sInputDate, null);
                    }

                }

                itResult.AfterLoad();
            }

            //리스트 형태로 반환
            return resultList;
        }


        public List<T> LoadSQL(string sSql = "")
        {
            List<T> resultList = new List<T>();

            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();
            string sDefaultWhere = itTarget.SetQueryOption().Trim();


            EntityMapper omapper = new EntityMapper();

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                omapper.except_members.Add(sNotEditField);
            }

            // 직접 query시 ROWID는 제외
            omapper.except_members.Add("_ROWID");

            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보            
            omapper.Load(sSql); //데이터 쿼리 실행


            // 데이터 타입 형 변환
            for (int i = 0; i < omapper.Result[0].Count; i++)
            {
                resultList.Add((T)omapper.Result[0][i]);
            }

            // load 완료후 
            for (int j = 0; j < resultList.Count; j++)
            {
                IORMInterface itResult = resultList[j] as IORMInterface;

                //date type 변환
                string[] arDateTimeFields = itResult.SetFieldforDatetime();
                Type Class = resultList[j].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < Properties.Length; i++)
                {

                    string table_field = table_field = Properties[i].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[i].GetValue(resultList[j], null);
                        string sInputDate = DateTime.ParseExact(sRowValue.ToString(), "yyyy-MM-dd tt h:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal).ToString("yyyy/MM/dd HH:mm:ss");
                        Properties[i].SetValue(resultList[j], sInputDate, null);
                    }

                }

                itResult.AfterLoad();
            }

            //리스트 형태로 반환
            return resultList;
        }

        public DataTable LoadDataTable(string sWhere = "")
        {
            IORMInterface itTarget = _target as IORMInterface;
            string sTableName = itTarget.SetTableName();
            string sDefaultWhere = itTarget.SetQueryOption().Trim();

            if (sDefaultWhere == "")
            {
                sDefaultWhere = " where 1=1 ";
            }

            string sWhereStmt = sDefaultWhere + sWhere;

            EntityMapper omapper = new EntityMapper();

            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보
            omapper.WhereCondition = sWhereStmt; // 조건절 
            omapper.except_members.Add("_ROWID");

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                omapper.except_members.Add(sNotEditField);
            }

            DataTable dtResult = omapper.LoadDatable(); //데이터 쿼리 실행

            //리스트 형태로 반환
            return dtResult;
        }

        public DataTable LoadDataTable(int ipage, int numberOfData, string sWhere = "")
        {
            IORMInterface itTarget = _target as IORMInterface;
            string sTableName = itTarget.SetTableName();
            string sDefaultWhere = itTarget.SetQueryOption().Trim();

            if (sDefaultWhere == "")
            {
                sDefaultWhere = " where 1=1 ";
            }

            string sWhereStmt = sDefaultWhere + sWhere;

            EntityMapper omapper = new EntityMapper();

            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보
            omapper.WhereCondition = sWhereStmt; // 조건절 
            omapper.except_members.Add("_ROWID");

            //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
            string[] aVirtualField = itTarget.SetVirtualField();
            for (int i2 = 0; i2 < aVirtualField.Length; i2++)
            {
                string sNotEditField = aVirtualField[i2].Trim();
                omapper.except_members.Add(sNotEditField);
            }

            DataTable dtResult = omapper.LoadDatable(ipage, numberOfData); //데이터 쿼리 실행

            //리스트 형태로 반환
            return dtResult;
        }

        public string GetTotalCount(string sWhere = "")
        {
            clsDBControl_new oDBCon = new clsDBControl_new(_DBServerName); //test server

            IORMInterface itTarget = _target as IORMInterface;
            string sPrimaryField = itTarget.SetPrimaryField();
            string sTableName = itTarget.SetTableName();
            string sPrimaryValue = itTarget.SetPrimaryValue();

            string sDefaultWhere = itTarget.SetQueryOption().Trim();

            if (sDefaultWhere == "")
            {
                sDefaultWhere = " where 1=1 ";
            }

            string sWhereStmt = sDefaultWhere + sWhere;


            string sSql = "select count(*) from " + sTableName + " " + sWhereStmt;
            string TotalCount = oDBCon.QuerySingleData(sSql);
            return TotalCount;

        }

        public void AddUpdateOnlyField(string inField)
        {
            _UpdateOnlyField.Add(inField);
        }

        public void RemoveUpdateOnlyField(string inField)
        {
            _UpdateOnlyField.RemoveAll(p => p.ToString() == inField);
        }

        public bool hasUpdateOnlyField()
        {
            bool isUpdate = false;

            if (_UpdateOnlyField.Count > 0)
            {
                isUpdate = true;
            }
            else
            {
                isUpdate = false;
            }

            return isUpdate;

        }

    }
}