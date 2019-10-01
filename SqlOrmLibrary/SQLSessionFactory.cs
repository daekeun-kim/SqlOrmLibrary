﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace SqlOrmLibrary
{
    public class SQLSessionFactory : IDisposable
    {
        private List<string> _UpdateOnlyField; //update시 load하지 않고 일부 필드에 대해서만 update 할 경우        
        private SqlConnection _Connect = null;
        private SqlTransaction _trans = null;
        private string _status;
        private string orm_sql = "";

        public SQLSessionFactory(string sConnectionString)
        {
            _UpdateOnlyField = new List<string>();
            _Connect = new SqlConnection(sConnectionString);
            _Connect.Open();
            _status = "Open";
        }

        public void Dispose()
        {
            stopBeginTransaction();
            Close();
        }

        public void Rollback()
        {
            if (_status == "BeginTransaction")
            {
                _trans.Rollback();
            }
            Close();
        }

        public void Commit()
        {
            _trans.Commit();
            Close();
        }

        private void Close()
        {
            if (_Connect != null)
            {
                if (_Connect.State != ConnectionState.Closed)
                {
                    _Connect.Close();
                }

                _Connect.Dispose();
                _Connect = null;
                _status = "Closed";
            }
        }

        private void startBeginTransaction()
        {
            orm_sql = "";

            if (_status != "BeginTransaction")
            {
                _trans = _Connect.BeginTransaction();
                _status = "BeginTransaction";
            }
        }

        private void stopBeginTransaction()
        {
            if (_status == "BeginTransaction")
            {
                _trans.Commit();
                _status = "Open";
            }
        }

        private void CompleteMethod<T>(T Target) where T : IORMInterface
        {
            Target.OnComplete(orm_sql);
        }


        private void CompleteMethodList<T>(List<T> Target) where T : IORMInterface
        {
            if (Target.Count > 0)
            {
                Target[0].OnComplete(orm_sql);
            }

        }

        private void ErrorMethod<T>(T Target, Exception e) where T : IORMInterface
        {
            Target.OnError(e, orm_sql);
        }


        private void ErrorMethodList<T>(List<T> Target, Exception e) where T : IORMInterface
        {
            if (Target.Count > 0)
            {
                Target[0].OnError(e, orm_sql);
            }

        }


        public bool Create<T>(T target) where T : IORMInterface
        {

            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

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

                if (isSucess)
                {
                    itTarget.AfterAdd();
                }

                orm_sql = mapper.sSql;
                CompleteMethod<T>(target);

            }
            catch (Exception ex)
            {
                isSucess = false;

                orm_sql = mapper.sSql;
                ErrorMethod<T>(target, ex);

                throw ex;
            }
            finally
            {

            }

            return isSucess;

        }

        //Parameterized SQL
        public bool _Create<T>(T target) where T : IORMInterface
        {
            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

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

            // Byte 타입 지정
            string[] aByteTypeField = itTarget.SetFieldforByteType();
            for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
            {
                string sByteTypeField = aByteTypeField[i2].Trim();
                mapper.byte_type_members.Add(sByteTypeField);
            }



            bool isORMAction = true;
            isORMAction = itTarget.BeforeAdd();

            try
            {

                if (isORMAction)
                {
                    isSucess = mapper.Create_Oracle(target, sTableName); //insert 구문 생성                    
                }

                if (isSucess)
                {
                    itTarget.AfterAdd();
                }

                orm_sql = mapper.sSql;
                CompleteMethod<T>(target);

            }
            catch (Exception ex)
            {
                isSucess = false;
                orm_sql = mapper.sSql;
                ErrorMethod<T>(target, ex);
                throw ex;
            }
            finally
            {

            }

            return isSucess;
        }

        public bool CreateListData<T>(List<T> targetList) where T : IORMInterface
        {
            startBeginTransaction();

            bool isSuccess = true;
            List<string> listQuery = new List<string>();

            try
            {

                EntityMapperSQL mapper = new EntityMapperSQL();
                mapper.oDB = new clsSQLControl();
                mapper.oDB.SetDB(_Connect, _trans);

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

                if (isSuccess)
                {
                    for (int j = 0; j < targetList.Count; j++)
                    {
                        IORMInterface itTarget = targetList[j] as IORMInterface;
                        itTarget.AfterAdd();
                    }

                    listQuery.ForEach(p => orm_sql += p.ToString() + ";");
                    CompleteMethodList<T>(targetList);


                }


            }
            catch (Exception e)
            {
                isSuccess = false;

                listQuery.ForEach(p => orm_sql += p.ToString() + ";");
                ErrorMethodList<T>(targetList, e);


                throw e;
            }

            return isSuccess;

        }


        public bool _CreateListData<T>(List<T> targetList) where T : IORMInterface
        {
            startBeginTransaction();

            bool isSuccess = true;

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

            try
            {


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

                    // Byte 타입 지정
                    string[] aByteTypeField = itTarget.SetFieldforByteType();
                    for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                    {
                        string sByteTypeField = aByteTypeField[i2].Trim();
                        mapper.byte_type_members.Add(sByteTypeField);
                    }

                    isORMAction = itTarget.BeforeAdd();

                    if (isORMAction == false)
                        break;

                }

                if (isORMAction)
                {
                    isSuccess = mapper.Create_Oracle_List(targetList, sBigTableName); // Create문 쿼리 생성                    
                }

                if (isSuccess)
                {
                    for (int j = 0; j < targetList.Count; j++)
                    {
                        IORMInterface itTarget = targetList[j] as IORMInterface;
                        itTarget.AfterAdd();
                    }

                    orm_sql = mapper.sSql;
                    CompleteMethodList<T>(targetList);

                }
            }
            catch (Exception e)
            {
                orm_sql = mapper.sSql;
                ErrorMethodList<T>(targetList, e);

                throw e;
            }


            return isSuccess;

        }

        public bool Edit<T>(T target) where T : IORMInterface
        {
            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

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


                if (isSucess)
                {
                    itTarget.AfterEdit();
                }

                orm_sql = mapper.sSql;
                CompleteMethod<T>(target);
            }
            catch (Exception ex)
            {
                isSucess = false;

                orm_sql = mapper.sSql;
                ErrorMethod<T>(target, ex);

                throw ex;
            }
            finally
            {

            }

            return isSucess;

        }

        public bool EditListData<T>(List<T> targetList) where T : IORMInterface
        {
            startBeginTransaction();
            List<string> listQuery = new List<string>();
            bool isSuccess = true;

            try
            {


                EntityMapperSQL mapper = new EntityMapperSQL();
                mapper.oDB = new clsSQLControl(); ;
                mapper.oDB.SetDB(_Connect, _trans);

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

                    // Byte 타입 지정
                    string[] aByteTypeField = itTarget.SetFieldforByteType();
                    for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                    {
                        string sByteTypeField = aByteTypeField[i2].Trim();
                        mapper.byte_type_members.Add(sByteTypeField);
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

                if (isSuccess)
                {
                    for (int j = 0; j < targetList.Count; j++)
                    {
                        IORMInterface itTarget = targetList[j] as IORMInterface;
                        itTarget.AfterEdit();
                    }

                    listQuery.ForEach(p => orm_sql += p.ToString() + ";");
                    CompleteMethodList<T>(targetList);
                }

                return isSuccess;
            }
            catch (Exception e)
            {
                listQuery.ForEach(p => orm_sql += p.ToString() + ";");
                ErrorMethodList<T>(targetList, e);


                throw e;
            }

        }

        public bool EditOnly<T>(T target) where T : IORMInterface
        {
            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

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

                if (isSucess)
                {
                    itTarget.AfterEdit();
                }

                _UpdateOnlyField.Clear();

                orm_sql = mapper.sSql;
                CompleteMethod<T>(target);

            }
            catch (Exception ex)
            {
                isSucess = false;

                orm_sql = mapper.sSql;

                ErrorMethod<T>(target, ex);

                throw ex;
            }
            finally
            {

            }

            return isSucess;

        }


        public bool EditOnlyListData<T>(List<T> targetList) where T : IORMInterface
        {
            startBeginTransaction();

            bool isSuccess = true;
            List<string> listQuery = new List<string>();

            try
            {

                EntityMapperSQL mapper = new EntityMapperSQL();
                mapper.oDB = new clsSQLControl();
                mapper.oDB.SetDB(_Connect, _trans);

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

                if (isSuccess)
                {
                    for (int j = 0; j < targetList.Count; j++)
                    {
                        IORMInterface itTarget = targetList[j] as IORMInterface;
                        itTarget.AfterEdit();
                    }

                    listQuery.ForEach(p => orm_sql += p.ToString() + ";");
                    CompleteMethodList<T>(targetList);

                }

                _UpdateOnlyField.Clear();

                return isSuccess;
            }
            catch (Exception e)
            {
                listQuery.ForEach(p => orm_sql += p.ToString() + ";");
                ErrorMethodList<T>(targetList, e);


                throw e;
            }

        }

        //Parameterized query
        public bool _Edit<T>(T target) where T : IORMInterface
        {
            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

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

            // Byte 타입 지정
            string[] aByteTypeField = itTarget.SetFieldforByteType();
            for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
            {
                string sByteTypeField = aByteTypeField[i2].Trim();
                mapper.byte_type_members.Add(sByteTypeField);
            }

            isORMAction = itTarget.BeforeEdit();

            try
            {
                if (isORMAction)
                {
                    isSucess = mapper.Save_Oracle(target, sTableName, where_stmt); // update문 쿼리 생성                            
                }

                if (isSucess)
                {
                    itTarget.AfterEdit();
                }

                orm_sql = mapper.sSql;
                CompleteMethod<T>(target);

            }
            catch (Exception ex)
            {
                isSucess = false;

                orm_sql = mapper.sSql;

                ErrorMethod<T>(target, ex);

                throw ex;

            }
            finally
            {


            }

            return isSucess;

        }

        //Parameterized query
        public bool _EditListData<T>(List<T> targetList) where T : IORMInterface
        {
            startBeginTransaction();

            bool isSuccess = true;

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);
            try
            {

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

                    // Byte 타입 지정
                    string[] aByteTypeField = itTarget.SetFieldforByteType();
                    for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                    {
                        string sByteTypeField = aByteTypeField[i2].Trim();
                        mapper.byte_type_members.Add(sByteTypeField);
                    }

                    isORMAction = itTarget.BeforeEdit();

                    if (isORMAction == false)
                        break;

                }

                if (isORMAction)
                {
                    isSuccess = mapper.Save_Oracle_List(targetList, lTableNames, lWhere); // update문 쿼리 생성
                }

                if (isSuccess)
                {
                    for (int j = 0; j < targetList.Count; j++)
                    {
                        IORMInterface itTarget = targetList[j] as IORMInterface;
                        itTarget.AfterEdit();
                    }

                    orm_sql = mapper.sSql;
                    CompleteMethodList<T>(targetList);
                }
            }
            catch (Exception e)
            {
                orm_sql = mapper.sSql;
                ErrorMethodList<T>(targetList, e);


                throw e;
            }

            return isSuccess;

        }

        //Parameterized query
        public bool _EditOnly<T>(T target) where T : IORMInterface
        {
            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

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

            // Byte 타입 지정
            string[] aByteTypeField = itTarget.SetFieldforByteType();
            for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
            {
                string sByteTypeField = aByteTypeField[i2].Trim();
                mapper.byte_type_members.Add(sByteTypeField);
            }

            try
            {
                if (isORMAction)
                {
                    isSucess = mapper.Save_Oracle(target, sTableName, where_stmt); // update문 쿼리 생성                            
                }


                if (isSucess)
                {
                    itTarget.AfterEdit();
                }

                _UpdateOnlyField.Clear();

                orm_sql = mapper.sSql;
                CompleteMethod<T>(target);

            }
            catch (Exception ex)
            {
                isSucess = false;

                orm_sql = mapper.sSql;

                ErrorMethod<T>(target, ex);

                throw ex;
            }
            finally
            {


            }

            return isSucess;

        }

        //Parameterized query
        public bool _EditOnlyListData<T>(List<T> targetList) where T : IORMInterface
        {
            startBeginTransaction();

            bool isSuccess = true;

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

            try
            {


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

                    // Byte 타입 지정
                    string[] aByteTypeField = itTarget.SetFieldforByteType();
                    for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                    {
                        string sByteTypeField = aByteTypeField[i2].Trim();
                        mapper.byte_type_members.Add(sByteTypeField);
                    }

                }

                if (isORMAction)
                {
                    isSuccess = mapper.Save_Oracle_List(targetList, lTableNames, lWhere); // update문 쿼리 생성
                }

                if (isSuccess)
                {
                    for (int j = 0; j < targetList.Count; j++)
                    {
                        IORMInterface itTarget = targetList[j] as IORMInterface;
                        itTarget.AfterEdit();
                    }
                }

                _UpdateOnlyField.Clear();

                orm_sql = mapper.sSql;
                CompleteMethodList<T>(targetList);

            }
            catch (Exception e)
            {
                orm_sql = mapper.sSql;
                ErrorMethodList<T>(targetList, e);


                throw e;
            }

            return isSuccess;

        }

        public bool DeleteListData<T>(List<T> targetList) where T : IORMInterface
        {
            startBeginTransaction();

            bool isSuccess = true;

            List<string> listQuery = new List<string>();
            bool isORMAction = true;

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

            try
            {


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

                if (isSuccess)
                {
                    for (int j = 0; j < targetList.Count; j++)
                    {
                        IORMInterface itTarget = targetList[j] as IORMInterface;
                        itTarget.AfterDelete();
                    }
                }
            }
            catch (Exception e)
            {
                listQuery.ForEach(p => orm_sql += p.ToString() + ";");
                ErrorMethodList<T>(targetList, e);


                throw e;
            }


            listQuery.ForEach(p => orm_sql += p.ToString() + ";");
            CompleteMethodList<T>(targetList);

            return isSuccess;

        }

        public bool Delete<T>(T target) where T : IORMInterface
        {
            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

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


                if (isSucess)
                {
                    itTarget.AfterDelete();

                    orm_sql = mapper.sSql;
                    CompleteMethod<T>(target);
                }

            }
            catch (Exception ex)
            {
                isSucess = false;

                orm_sql = mapper.sSql;

                ErrorMethod<T>(target, ex);

                throw ex;
            }
            finally
            {

            }



            return isSucess;

        }

        public bool ExcuteNonQuery(string sQuery)
        {
            startBeginTransaction();

            EntityMapperSQL mapper = new EntityMapperSQL();
            mapper.oDB = new clsSQLControl();
            mapper.oDB.SetDB(_Connect, _trans);

            string Sql = sQuery;

            bool isSucess = true;
            try
            {

                isSucess = mapper.oDB.ExcuteNonQuery(Sql); //Query 실행
                orm_sql = mapper.sSql;

            }
            catch (Exception ex)
            {
                isSucess = false;

                throw ex;
            }
            finally
            {

            }

            return isSucess;

        }

        public List<T> Load<T>(string sWhere = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            List<T> resultList = new List<T>();
            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {

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



                //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
                string[] aVirtualField = itTarget.SetVirtualField();
                for (int i2 = 0; i2 < aVirtualField.Length; i2++)
                {
                    string sNotEditField = aVirtualField[i2].Trim();
                    omapper.except_members.Add(sNotEditField);
                }

                //Byte 타입은 변환이 다름으로 지정
                string[] aByteTypeField = itTarget.SetFieldforByteType();
                for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                {
                    string sByteTypeField = aByteTypeField[i2].Trim();
                    omapper.byte_type_members.Add(sByteTypeField);
                }

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보
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

                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);

                //리스트 형태로 반환
                return resultList;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);

                throw e;
            }
        }

        public List<T> Load<T>(int ipage, int numberOfData, string sWhere = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            List<T> resultList = new List<T>();

            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {


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


                //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
                string[] aVirtualField = itTarget.SetVirtualField();
                for (int i2 = 0; i2 < aVirtualField.Length; i2++)
                {
                    string sNotEditField = aVirtualField[i2].Trim();
                    omapper.except_members.Add(sNotEditField);
                }

                //Byte 타입은 변환이 다름으로 지정
                string[] aByteTypeField = itTarget.SetFieldforByteType();
                for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                {
                    string sByteTypeField = aByteTypeField[i2].Trim();
                    omapper.byte_type_members.Add(sByteTypeField);
                }

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보

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

                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);
                //리스트 형태로 반환
                return resultList;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);

                throw e;
            }

        }

        public List<T> LoadRange<T>(int StartValue, int EndValue, string sWhere = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            List<T> resultList = new List<T>();

            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {


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


                //가상필드는 DB필드가 아니므로 트랜잭션에서 제외된다
                string[] aVirtualField = itTarget.SetVirtualField();
                for (int i2 = 0; i2 < aVirtualField.Length; i2++)
                {
                    string sNotEditField = aVirtualField[i2].Trim();
                    omapper.except_members.Add(sNotEditField);
                }

                //Byte 타입은 변환이 다름으로 지정
                string[] aByteTypeField = itTarget.SetFieldforByteType();
                for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                {
                    string sByteTypeField = aByteTypeField[i2].Trim();
                    omapper.byte_type_members.Add(sByteTypeField);
                }

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보

                omapper.Table_entity.Add(sTableName, _target); // 테이블 정보
                omapper.WhereCondition = sWhereStmt; // 조건절 
                omapper.LoadRange(StartValue, EndValue); //데이터 쿼리 실행

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

                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);
                //리스트 형태로 반환
                return resultList;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);

                throw e;
            }

        }

        public List<T> LoadSQL<T>(string sSql = "") where T : IORMInterface, new()
        {

            stopBeginTransaction();

            List<T> resultList = new List<T>();
            T _target = new T();

            try
            {
                IORMInterface itTarget = _target as IORMInterface;
                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();
                string sDefaultWhere = itTarget.SetQueryOption().Trim();


                EntityMapperSQL omapper = new EntityMapperSQL();
                omapper.oDB = new clsSQLControl();

                // 직접 query시 ROWID는 제외
                omapper.except_members.Add("_ROWID");

                string[] aByteTypeField = itTarget.SetFieldforByteType();
                for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                {
                    string sByteTypeField = aByteTypeField[i2].Trim();
                    omapper.byte_type_members.Add(sByteTypeField);
                }

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보
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

                orm_sql = sSql;
                CompleteMethod<T>(_target);

                //리스트 형태로 반환
                return resultList;
            }
            catch (Exception e)
            {
                orm_sql = sSql;
                ErrorMethod<T>(_target, e);
                throw e;
            }

        }

        public List<T> LoadSQL<T>(int ipage, int numberOfData, string sSql = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            List<T> resultList = new List<T>();

            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {

                IORMInterface itTarget = _target as IORMInterface;

                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                // 직접 query시 ROWID는 제외
                omapper.except_members.Add("_ROWID");

                string[] aByteTypeField = itTarget.SetFieldforByteType();
                for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                {
                    string sByteTypeField = aByteTypeField[i2].Trim();
                    omapper.byte_type_members.Add(sByteTypeField);
                }

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보

                omapper.Table_entity.Add(sTableName, _target); // 테이블 정보              
                omapper.Load(sSql, ipage, numberOfData); //데이터 쿼리 실행

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

                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);
                //리스트 형태로 반환
                return resultList;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);

                throw e;
            }

        }

        public List<T> LoadSQLRagne<T>(int StartValue, int EndValue, string sSql = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            List<T> resultList = new List<T>();

            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {

                IORMInterface itTarget = _target as IORMInterface;

                string sPrimaryField = itTarget.SetPrimaryField();
                string sTableName = itTarget.SetTableName();
                string sPrimaryValue = itTarget.SetPrimaryValue();

                // 직접 query시 ROWID는 제외
                omapper.except_members.Add("_ROWID");

                string[] aByteTypeField = itTarget.SetFieldforByteType();
                for (int i2 = 0; i2 < aByteTypeField.Length; i2++)
                {
                    string sByteTypeField = aByteTypeField[i2].Trim();
                    omapper.byte_type_members.Add(sByteTypeField);
                }

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보

                omapper.Table_entity.Add(sTableName, _target); // 테이블 정보              
                omapper.LoadRange(sSql, StartValue, EndValue); //데이터 쿼리 실행

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

                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);
                //리스트 형태로 반환
                return resultList;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);

                throw e;
            }

        }

        public DataTable LoadDataTable<T>(string sWhere = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {
                IORMInterface itTarget = _target as IORMInterface;
                string sTableName = itTarget.SetTableName();
                string sDefaultWhere = itTarget.SetQueryOption().Trim();

                if (sDefaultWhere == "")
                {
                    sDefaultWhere = " where 1=1 ";
                }

                string sWhereStmt = sDefaultWhere + sWhere;



                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보
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

                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);

                //리스트 형태로 반환
                return dtResult;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);
                throw e;
            }
        }

        public DataTable LoadDataTable<T>(int ipage, int numberOfData, string sWhere = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {


                IORMInterface itTarget = _target as IORMInterface;
                string sTableName = itTarget.SetTableName();
                string sDefaultWhere = itTarget.SetQueryOption().Trim();

                if (sDefaultWhere == "")
                {
                    sDefaultWhere = " where 1=1 ";
                }

                string sWhereStmt = sDefaultWhere + sWhere;


                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보
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


                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);

                //리스트 형태로 반환
                return dtResult;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);
                throw e;
            }
        }

        public DataTable LoadDataTableRange<T>(int StartValue, int EndValue, string sWhere = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            T _target = new T();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {


                IORMInterface itTarget = _target as IORMInterface;
                string sTableName = itTarget.SetTableName();
                string sDefaultWhere = itTarget.SetQueryOption().Trim();

                if (sDefaultWhere == "")
                {
                    sDefaultWhere = " where 1=1 ";
                }

                string sWhereStmt = sDefaultWhere + sWhere;


                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보
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

                DataTable dtResult = omapper.LoadDatableRange(StartValue, EndValue); //데이터 쿼리 실행


                orm_sql = omapper.sSql;
                CompleteMethod<T>(_target);

                //리스트 형태로 반환
                return dtResult;
            }
            catch (Exception e)
            {
                orm_sql = omapper.sSql;
                ErrorMethod<T>(_target, e);
                throw e;
            }
        }


        public DataTable LoadSQLDataTable(string sSQL)
        {
            stopBeginTransaction();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보

                DataTable dtResult = omapper.LoadDatable(sSQL); //데이터 쿼리 실행

                orm_sql = sSQL;


                //리스트 형태로 반환
                return dtResult;
            }
            catch (Exception e)
            {
                orm_sql = sSQL;
                throw e;
            }
        }

        public DataTable LoadSQLDataTable(int ipage, int numberOfData, string sSQL)
        {
            stopBeginTransaction();

            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보

                DataTable dtResult = omapper.LoadDatable(sSQL, ipage, numberOfData); //데이터 쿼리 실행


                orm_sql = sSQL;

                //리스트 형태로 반환
                return dtResult;
            }
            catch (Exception e)
            {
                orm_sql = sSQL;
                throw e;
            }
        }

        public DataTable LoadSQLDataTableRange(int StartValue, int EndValue, string sSQL)
        {
            stopBeginTransaction();



            EntityMapperSQL omapper = new EntityMapperSQL();
            omapper.oDB = new clsSQLControl();

            try
            {

                omapper.oDB.SetDB(_Connect, _trans); //데이터 베이스 정보


                DataTable dtResult = omapper.LoadDatableRange(sSQL, StartValue, EndValue); //데이터 쿼리 실행


                orm_sql = sSQL;


                //리스트 형태로 반환
                return dtResult;
            }
            catch (Exception e)
            {
                orm_sql = sSQL;
                throw e;
            }
        }



        public string GetTotalCount<T>(string sWhere = "") where T : IORMInterface, new()
        {
            stopBeginTransaction();

            T _target = new T();

            try
            {
                clsSQLControl oDBCon = new clsSQLControl(); //test server

                oDBCon.SetDB(_Connect, _trans); //데이터 베이스 정보

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

                orm_sql = sSql;
                CompleteMethod<T>(_target);

                return TotalCount;
            }
            catch (Exception e)
            {
                ErrorMethod<T>(_target, e);
                throw e;
            }

        }


        public string QuerySingleData<T>(string sQuery) where T : IORMInterface, new()
        {
            stopBeginTransaction();
            T _target = new T();

            try
            {
                clsSQLControl oDBCon = new clsSQLControl(); //test server
                oDBCon.SetDB(_Connect, _trans); //데이터 베이스 정보

                string sSql = sQuery;
                orm_sql = sSql;

                string TotalCount = oDBCon.QuerySingleData(sQuery);

                orm_sql = sSql;
                CompleteMethod<T>(_target);

                return TotalCount;
            }
            catch (Exception e)
            {
                ErrorMethod<T>(_target, e);
                throw e;
            }

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
        /// <summary>
        /// source 객체의 멤버 변수 중 destination 객체의 멤버 변수와 동일한 이름을 갖는 변수의 값을 
        /// destination 객체의 멤버 변수로 할당 시키는 함수
        /// </summary>
        /// <param name="source">source 객체</param>
        /// <param name="destination">destination</param>
        public static void CopyObject<T, U>(T source, U destination)
        {
            Type t = source.GetType(); //객체의 타입 가져오기
            Type u = destination.GetType(); //객체의 타입 가져오기
            //sql 구문 만들기 시작

            PropertyInfo[] tProperties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo[] uProperties = u.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < tProperties.Length; i++)
            {
                string tMemeberName = tProperties[i].Name.ToString();
                string object_member = string.Empty;
                string table_field = string.Empty;

                for (int k = 0; k < uProperties.Length; k++)
                {
                    string uMemberName = uProperties[k].Name.ToString();
                    string uobject_member = string.Empty;
                    string utable_field = string.Empty;

                    if (tMemeberName == uMemberName)
                    {

                        if (tProperties[i] != null)
                        {
                            if (tProperties[i].GetValue(source, null) != null)
                            {
                                uProperties[k].SetValue(destination, tProperties[i].GetValue(source, null).ToString(), null);
                            }
                        }

                    }

                }
            }
        }

    }
}